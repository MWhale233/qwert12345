// 3DFresnelDiffraction.shader
Shader "Custom/3DFresnel" {
    Properties {
        _ApertureRadius("Aperture Radius", Range(0.01, 0.3)) = 0.1 //Range(0.01, 0.5)) = 0.1
        _WaveLength("Wavelength (nm)", Range(300, 700)) = 500
        _MaxDistance("Max Z Distance", Float) = 10
        _IntensityScale("Intensity Scale", Float) = 1
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float3 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _ApertureRadius;
            float _WaveLength;
            float _MaxDistance;
            float _IntensityScale;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            float3 complexMultiply(float3 a, float3 b) {
                return float3(
                    a.x*b.x - a.y*b.y - a.z*b.z,
                    a.x*b.y + a.y*b.x,
                    a.x*b.z + a.z*b.x
                );
            }

            float4 frag (v2f i) : SV_Target {
                float k = 2 * 3.14159265 / (_WaveLength * 1e-9);
                float3 sum = float3(0, 0, 0);
                const int SAMPLES = 32;

                // 孔径平面坐标计算
                for(int x = -SAMPLES; x < SAMPLES; x++) {
                    for(int y = -SAMPLES; y < SAMPLES; y++) {
                        float2 apertureUV = float2(x, y) / SAMPLES;
                        float r = length(apertureUV);
                        
                        if(r < _ApertureRadius) {
                            // 三维传播计算
                            float3 delta = i.worldPos - float3(apertureUV, 0);
                            float distance = length(delta);
                            
                            // 菲涅尔相位项
                            float phase = k * (distance - i.worldPos.z);
                            float3 wave = float3(
                                cos(phase) / distance,
                                sin(phase) / distance,
                                0
                            );
                            
                            sum += wave;
                        }
                    }
                }
                
                float intensity = (sum.x*sum.x + sum.y*sum.y) * _IntensityScale;
                float zFactor = saturate(i.worldPos.z / _MaxDistance);
                return float4(intensity * (1 - zFactor), intensity * zFactor, 0, 1);
            }
            ENDCG
        }
    }
}