Shader "Custom/VolumeDiffraction"
{
    Properties
    {
        // 圆孔半径（体积横向范围为 [-_ApertureRadius, _ApertureRadius]）
        _ApertureRadius("Aperture Radius", Float) = 0.5
        // 波长（例如 500nm = 0.0000005）
        _Lambda("Wavelength", Float) = 0.0000005
        // 最大传播距离（体积 z 方向：0 到 _ZMax）
        _ZMax("Max Propagation Distance", Float) = 1.0
        // 射线步进数（体渲染采样数，数值越大图像越平滑，但性能越低）
        _Steps("Raymarch Steps", Float) = 64
        // 体积边界（假设体积在本地坐标中 x,y ∈ [-_ApertureRadius, _ApertureRadius]，z ∈ [0, _ZMax]）
        _VolumeBounds("Volume Bounds", Vector) = (0.5, 0.5, 1.0, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Shader 属性
            float _ApertureRadius;
            float _Lambda;
            float _ZMax;
            float _Steps;
            float4 _VolumeBounds; // x,y: 横向半范围；z: 传播深度

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            // 顶点着色器：传递世界坐标
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // 修正后的近似计算一阶贝塞尔函数 J0(x)
            float BesselJ0(float x)
            {
                float ax = abs(x);
                if (ax < 8.0)
                {
                    float y = x * x;
                    float ans1 = 57568490574.0 
                        + y * (-13362590354.0 
                        + y * (651619640.7 
                        + y * (-11214424.18 
                        + y * (77392.33017 
                        + y * (-184.9052456)))));
                    float ans2 = 57568490411.0 
                        + y * (1029532985.0 
                        + y * (9494680.718 
                        + y * (59272.64853 
                        + y * (267.8532712 
                        + y * 1.0))));
                    return ans1 / ans2;
                }
                else
                {
                    float z = 8.0 / ax;
                    float y = z * z;
                    float xx = ax - 0.785398164; // pi/4
                    float ans1 = 1.0 
                        + y * (-0.1098628627e-2 
                        + y * (0.2734510407e-4 
                        + y * (-0.2073370639e-5 
                        + y * 0.2093887211e-6)));
                    float ans2 = -0.1562499995e-1 
                        + y * (0.1430488765e-3 
                        + y * (-0.6911147651e-5 
                        + y * (0.7621095161e-6 
                        - y * 0.934935152e-7)));
                    float factor = sqrt(0.636619772 / ax) * (cos(xx) * ans1 - z * sin(xx) * ans2);
                    return factor;
                }
            }

            // 根据横向径向坐标 r 和传播距离 z，计算衍射场的强度（积分近似）
            float ComputeDiffractionIntensity(float r, float z)
            {
                // 波数 k = 2π/λ
                float k = 6.283185307 / _Lambda;
                const int N = 32;  // 积分离散步数
                float dr = _ApertureRadius / N;
                float Ereal = 0.0;
                float Eimag = 0.0;
                for (int n = 0; n < N; n++)
                {
                    float rSample = (n + 0.5) * dr;
                    // 相位项：exp[i * (k/(2z))*(rSample² + r²)]
                    float phase = (k / (2.0 * z)) * (rSample * rSample + r * r);
                    // 贝塞尔函数项：J0( (k * r * rSample) / z )
                    float argBessel = (k * r * rSample) / z;
                    float j0 = BesselJ0(argBessel);
                    Ereal += rSample * cos(phase) * j0;
                    Eimag += rSample * sin(phase) * j0;
                }
                float2 accum = float2(Ereal, Eimag) * (2.0 * 3.14159265 * dr);
                float amplitude = length(accum);
                return amplitude * amplitude; // 强度 ~ |E|²
            }

            // 计算射线与一个轴对齐包围盒（体积）的交点，假设盒子在本地坐标中：x,y ∈ [-_VolumeBounds.xy, _VolumeBounds.xy]，z ∈ [0, _VolumeBounds.z]
            bool RayBoxIntersection(float3 ro, float3 rd, out float tEnter, out float tExit)
            {
                float3 boxMin = -float3(_VolumeBounds.x, _VolumeBounds.y, 0.0);
                float3 boxMax = float3(_VolumeBounds.x, _VolumeBounds.y, _VolumeBounds.z);
                float3 invDir = 1.0 / rd;
                float3 t0s = (boxMin - ro) * invDir;
                float3 t1s = (boxMax - ro) * invDir;
                float3 tSmalls = min(t0s, t1s);
                float3 tBigs = max(t0s, t1s);
                tEnter = max(max(tSmalls.x, tSmalls.y), tSmalls.z);
                tExit = min(min(tBigs.x, tBigs.y), tBigs.z);
                return tExit >= max(tEnter, 0.0);
            }

            // 体渲染片元着色器（基于射线步进）
            float4 frag(v2f i) : SV_Target
            {
                float3 rayOrigin = _WorldSpaceCameraPos;
                float3 rayDir = normalize(i.worldPos - rayOrigin);
                float tEnter, tExit;
                if (!RayBoxIntersection(rayOrigin, rayDir, tEnter, tExit))
                    discard;

                float dt = (tExit - tEnter) / _Steps;
                float t = tEnter;
                float4 colorAcc = float4(0, 0, 0, 0);
                for (int s = 0; s < (int)_Steps; s++)
                {
                    float3 pos = rayOrigin + rayDir * t;
                    // 将世界坐标转换到体积局部坐标（假设体积 Mesh 的本地坐标定义了 x,y ∈ [-_ApertureRadius, _ApertureRadius]，z ∈ [0, _ZMax]）
                    float3 localPos = mul(unity_WorldToObject, float4(pos, 1)).xyz;
                    // 计算横向径向距离：r = sqrt(x²+y²)
                    float r = length(localPos.xy);
                    // z 分量归一化到 [0, _ZMax]（避免 z=0 导致除零）
                    float z = saturate(localPos.z / _ZMax) * _ZMax;
                    z = max(z, 0.0001);
                    
                    float intensity = ComputeDiffractionIntensity(r, z);
                    
                    // 将 intensity 由蓝到红映射（低 intensity 为蓝，高 intensity 为红）
                    float3 sampleColor = lerp(float3(0, 0, 1), float3(1, 0, 0), saturate(intensity));
                    // 设定一个较低的 alpha 以实现半透明效果
                    float sampleAlpha = saturate(intensity * 0.2);
                    float4 sample = float4(sampleColor, sampleAlpha);
                    
                    // 前向混合累加
                    colorAcc.rgb = colorAcc.rgb + (1.0 - colorAcc.a) * sample.rgb * sample.a;
                    colorAcc.a = colorAcc.a + (1.0 - colorAcc.a) * sample.a;
                    
                    t += dt;
                    if (colorAcc.a >= 0.95) break;
                }
                return colorAcc;
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}
