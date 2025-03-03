Shader "Custom/FresnelToFraunhofer"
{
    Properties
    {
        // 圆孔半径
        _ApertureRadius("Aperture Radius", Float) = 0.5
        // 光波波长（单位自行决定，比如米），例如 0.0000005 = 500 nm
        _Lambda("Wavelength", Float) = 0.0000005
        // 可视化的最大传播距离 zMax
        _ZMax("Max Propagation Distance", Float) = 1.0

        // 你也可以加一些色彩控制等
        //_ColorMap("Color Map", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            //-------------- 数据结构 --------------
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            //-------------- Shader 属性 --------------
            float _ApertureRadius; // 孔径半径
            float _Lambda;         // 波长
            float _ZMax;           // z方向最大距离

            //-------------- 顶点着色器 --------------
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            //-------------- 贝塞尔函数 J0 近似 --------------
            // 参考自一些经典数值近似（如 Abramowitz & Stegun），
            // 仅作演示，误差在某些区间可能会较大。
            float BesselJ0(float x)
            {
                float ax = abs(x);
                if (ax < 8.0)
                {
                    // 多项式近似 (A&S 9.1.12)
                    float y = x*x;
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
                    // 大 x 时的渐进行为近似
                    float z  = 8.0 / ax;
                    float y  = z*z;
                    float xx = ax - 0.785398164; // (pi/4)
                    float ans1 = 1.0 + y * (-0.1098628627e-2
                        + y * (0.2734510407e-4
                        + y * (-0.2073370639e-5
                        + y * 0.2093887211e-6)));
                    float ans2 = -0.1562499995e-1
                        + y * (0.1430488765e-3
                        + y * (-0.6911147651e-5
                        + y * (0.7621095161e-6
                        - y * 0.934935152e-7)));
                    float factor = sqrt(0.636619772 / ax) *
                                   (cos(xx)*ans1 - z*sin(xx)*ans2);
                    return factor;
                }
            }

            //-------------- 片元着色器 --------------
            float4 frag (v2f i) : SV_Target
            {
                // 将 uv.x 从 [-_ApertureRadius, _ApertureRadius]，
                //    uv.y 从 [0, _ZMax]
                float x = lerp(-_ApertureRadius, _ApertureRadius, i.uv.x);
                float z = lerp(1e-5, _ZMax, i.uv.y); // 避免 z=0

                // 波数 k = 2π / λ
                float k = 6.283185307 /*2π*/ / _Lambda;

                //------------------------------------------------
                // 数值积分：计算在距离 z 处、横向坐标为 x 的场强
                // 理论公式(轴对称)：
                //   E(ρ,z) ∝ ∫[r=0..a] r' * e^{i(k/(2z)) (r'^2 + ρ^2)} * J0((k ρ r')/z) dr'
                // 其中还需乘 2π(积分 dφ)，这里用简单离散近似。
                //------------------------------------------------

                const int N = 32;   // 离散积分步数 (示例用 32，越大越精细但越耗性能)
                float dr = _ApertureRadius / N;
                
                float Ereal = 0.0;
                float Eimag = 0.0;

                for (int n = 0; n < N; n++)
                {
                    // r' 取样
                    float rSample = (n + 0.5) * dr;

                    // 相位项：exp{i [k/(2z)] (r'^2 + x^2)}
                    float phase = (k/(2.0*z)) * (rSample*rSample + x*x);

                    // J0( (k*x*r')/z )
                    float argBessel = (k * x * rSample) / z;
                    float j0 = BesselJ0(argBessel);

                    // 累加实部/虚部
                    Ereal += rSample * cos(phase) * j0;
                    Eimag += rSample * sin(phase) * j0;
                }

                // 整个积分还需乘 2π (对方位角 φ 的积分) 和 dr
                float2 accum = float2(Ereal, Eimag) * (2.0 * 3.14159265 * dr);

                // 幅度
                float amplitude = length(accum);

                // 强度 ~ |E|^2
                float intensity = amplitude * amplitude;

                //------------------------------------------------
                // 可视化映射：简单将 intensity 映射到 [0,1] 然后做个蓝到红渐变
                //------------------------------------------------
                // 可加 log/intensityScale 等更丰富的调色
                float scaleFactor = 1.0; // 可自行调整
                float val = intensity * scaleFactor;

                // clamp 到 [0,1]
                float clamped = saturate(val);

                // 线性插值：蓝(0,0,1) -> 红(1,0,0)
                float3 color = lerp(float3(0,0,1), float3(1,0,0), clamped);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}
