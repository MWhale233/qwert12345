Shader "Custom/VolumeFresnel" {
    Properties {
        _VolumeTex ("Volume Texture", 3D) = "" {}
        _Density ("密度", Range(0,5)) = 1.0
        _MaxSteps ("最大步数", Int) = 128
        _MaxDepth ("最大深度", Float) = 5.0
        _XYScale ("横向范围", Float) = 3.0
        _WaveLength ("波长 (nm)", Float) = 632.8
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            sampler3D _VolumeTex;
            float _Density;
            int _MaxSteps;
            float _MaxDepth;
            float _XYScale;
            float _WaveLength;

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                return o;
            }

            bool RayBoxIntersect(float3 origin, float3 dir, out float tMin, out float tMax) {
                float3 boxMin = float3(-_XYScale, -_XYScale, 0);
                float3 boxMax = float3(_XYScale, _XYScale, _MaxDepth);
                
                float3 invDir = 1.0 / (dir + 1e-6); // 防止除零
                float3 t0 = (boxMin - origin) * invDir;
                float3 t1 = (boxMax - origin) * invDir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                tMin = max(max(tmin.x, tmin.y), tmin.z);
                tMax = min(min(tmax.x, tmax.y), tmax.z);
                return tMax > max(tMin, 0.0);
            }

            float3 WavelengthToRGB(float wavelength) {
                // 波长到RGB转换（近似可见光谱）
                float3 rgb = float3(0,0,0);
                wavelength = clamp(wavelength, 380.0, 780.0);
                
                // 红色通道
                rgb.r = smoothstep(580.0, 780.0, wavelength) 
                      * smoothstep(780.0, 640.0, wavelength);
                
                // 绿色通道
                rgb.g = smoothstep(480.0, 580.0, wavelength) 
                      * smoothstep(580.0, 490.0, wavelength);
                
                // 蓝色通道
                rgb.b = smoothstep(380.0, 490.0, wavelength) 
                      * smoothstep(490.0, 440.0, wavelength);
                
                return saturate(rgb * 1.5);
            }

            fixed4 frag (v2f i) : SV_Target {
                float3 rayStart = i.worldPos;
                float3 rayDir = normalize(i.viewDir);
                
                float tMin, tMax;
                if (!RayBoxIntersect(rayStart, rayDir, tMin, tMax)) discard;

                float3 accum = float3(0,0,0);
                float alpha = 0.0;
                float stepSize = _MaxDepth / _MaxSteps;
                float3 wavelengthColor = WavelengthToRGB(_WaveLength);

                [loop]  // 根据D3D11要求明确循环类型
                for(int j=0; j<_MaxSteps; ++j) {
                    float t = tMin + j * stepSize;
                    if(t > tMax || alpha > 0.99) break;

                    float3 pos = rayStart + rayDir * t;
                    
                    // 修正后的UVW坐标计算
                    float3 uvw = float3(
                        (pos.x / (2.0 * _XYScale) + 0.5),
                        (pos.y / (2.0 * _XYScale) + 0.5),
                        saturate(pos.z / _MaxDepth)
                    );

                    if(any(uvw < 0.0)) continue;    // ← 补全括号
                    if(any(uvw > 1.0)) continue;    // ← 修正多余括号
                    
                    float intensity = tex3Dlod(_VolumeTex, float4(uvw, 0)).r;
                    
                    // 物理颜色计算
                    float3 color = wavelengthColor * intensity;
                    
                    // 体积透射率计算
                    float transmittance = exp(-_Density * alpha * stepSize);
                    accum += color * transmittance * stepSize;
                    alpha += (1.0 - alpha) * intensity * stepSize;
                }

                // HDR到LDR转换
                accum = 1.0 - exp(-accum * 2.0);
                return fixed4(accum, alpha);
            }
            ENDCG
        }
    }
    FallBack Off
}