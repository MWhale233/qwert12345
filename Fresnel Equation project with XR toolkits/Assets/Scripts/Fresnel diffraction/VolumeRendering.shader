Shader "Custom/VolumeRendering" {
    Properties {
        _VolumeTex ("Volume Texture", 3D) = "" {}
        _StepSize ("步长", Range(0.001, 0.1)) = 0.01
        _Density ("密度", Range(0, 10)) = 1.0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler3D _VolumeTex;
            float _StepSize;
            float _Density;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 rayOrigin : TEXCOORD0;
                float3 rayDir : TEXCOORD1;
            };

            // 在顶点着色器中计算射线的起点和方向：
            // 将相机位置转换到物体空间，得到射线起点；再由物体空间顶点计算射线方向。
            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // 获取相机在世界空间的位置，并转换到物体空间
                float3 worldCamPos = _WorldSpaceCameraPos;
                float4 objCamPos = mul(unity_WorldToObject, float4(worldCamPos, 1.0));
                o.rayOrigin = objCamPos.xyz;

                // 将当前顶点转换到物体空间
                float4 objPos = mul(unity_WorldToObject, v.vertex);
                o.rayDir = normalize(objPos.xyz - o.rayOrigin);
                return o;
            }

            // 计算射线与物体包围盒（这里假定体数据位于物体空间内的立方体 [-1,1]^3）的交点
            bool RayCubeIntersect(float3 rayOrigin, float3 rayDir, out float tEnter, out float tExit) {
                float3 boxMin = float3(-1, -1, -1);
                float3 boxMax = float3(1, 1, 1);
                float3 t0 = (boxMin - rayOrigin) / rayDir;
                float3 t1 = (boxMax - rayOrigin) / rayDir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                tEnter = max(max(tmin.x, tmin.y), tmin.z);
                tExit = min(min(tmax.x, tmax.y), tmax.z);
                return tExit >= max(tEnter, 0.0);
            }

            // 片元着色器：对体积数据进行 Ray Marching 采样，前向合成（Front-to-Back Compositing）
            fixed4 frag (v2f i) : SV_Target {
                float tEnter, tExit;
                if (!RayCubeIntersect(i.rayOrigin, i.rayDir, tEnter, tExit))
                    discard;

                // 确保 tEnter 不小于 0
                tEnter = max(tEnter, 0.0);
                float t = tEnter;
                float3 accumColor = float3(0, 0, 0);
                float accumAlpha = 0.0;
                float3 pos;

                
                // 沿射线方向以 _StepSize 步长进行采样
                const int maxSteps = 256; // 固定最大迭代次数
                int step = 0;
                for(; t < tExit && step < maxSteps; t += _StepSize, step++) {
                    pos = i.rayOrigin + i.rayDir * t;
                    // 将物体空间坐标映射到纹理坐标 [0,1]（假设体数据覆盖 [-1,1]^3）
                    float3 uvw = (pos + 1.0) * 0.5;
                    float sample = tex3D(_VolumeTex, uvw).r;
                    
                    // 简单传递函数：将采样值映射为颜色和透明度
                    // 这里将采样值直接视为灰度，乘以 _Density 和步长作为 alpha
                    float alpha = saturate(sample * _Density * _StepSize);
                    float3 color = float3(sample, sample, sample);

                    // 前向合成公式
                    accumColor += (1 - accumAlpha) * color * alpha;
                    accumAlpha += (1 - accumAlpha) * alpha;
                    
                    // 如果透明度接近不透明，则提前退出
                    if (accumAlpha >= 0.95)
                        break;
                }
                
                return fixed4(accumColor, accumAlpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
