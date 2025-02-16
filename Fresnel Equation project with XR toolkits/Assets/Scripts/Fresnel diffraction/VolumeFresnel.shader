// VolumeFresnel.shader
Shader "Custom/VolumeFresnel" {
    Properties {
        _ApertureRadius ("孔径半径", Range(0.1, 1)) = 0.5
        _WaveLength ("波长 (nm)", Range(300, 700)) = 500
        _MaxDepth ("最大深度", Float) = 10
        _Density ("密度", Range(0, 10)) = 2
        _Absorption ("吸收率", Range(0, 1)) = 0.2
    }

    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #define PI 3.141592653589793

            struct appdata {
                float4 vertex : POSITION;
            };

            struct v2f {
                float3 worldPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 rayDir : TEXCOORD1;
            };

            float _ApertureRadius, _WaveLength, _MaxDepth, _Density, _Absorption;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.rayDir = ObjSpaceViewDir(v.vertex);
                return o;
            }

            // 菲涅尔积分计算
            float fresnelIntegral(float3 pos) {
                float k = 2 * PI / (_WaveLength * 1e-9);
                float sum = 0;
                const int samples = 32; //32

                for(int x = -samples/2; x < samples/2; x++) {
                    for(int y = -samples/2; y < samples/2; y++) {
                        float2 aperturePos = float2(x,y)/samples * 2;
                        if(length(aperturePos) > _ApertureRadius) continue;

                        float3 delta = pos - float3(aperturePos, 0);
                        float dist = length(delta);
                        float phase = k * (dist - pos.z);
                        
                        sum += cos(phase) / dist;
                    }
                }
                return sum * sum * 0.01;
            }

            fixed4 frag (v2f i) : SV_Target {
                // 光线步进参数
                float3 rayStart = i.worldPos;
                float3 rayDir = normalize(i.rayDir);
                float stepSize = 0.05;
                float transmittance = 1.0;
                float3 light = 0;

                // 光线步进循环
                for(float t=0; t<_MaxDepth; t+=stepSize){
                    float3 pos = rayStart + rayDir * t;
                    if(any(abs(pos) > 1)) break; // 超出立方体范围

                    // 计算当前点光强
                    float intensity = fresnelIntegral(pos);
                    
                    // 体积光照模型
                    float density = intensity * _Density * stepSize;
                    light += transmittance * density;
                    transmittance *= exp(-density * _Absorption);
                }

                // 颜色映射
                float3 color = float3(
                    1 - exp(-light.x * 2),
                    exp(-light.y * 0.5),
                    exp(-light.z * 0.2)
                );
                return fixed4(color, transmittance);
            }

            
            ENDCG
        }
    }
}