Shader "Custom/LightField"
{
    Properties
    {
        _WaveLength ("波长 (nm)", Range(300,700)) = 632.8
        _Aperture ("孔径半径", Range(0.01,1.5)) = 0.0001
        _MaxDistance ("最大距离", Float) = 10.0
        _RayStep ("光线步长", Range(0.0001,0.001)) = 0.0001
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float _WaveLength;
            float _Aperture;
            float _MaxDistance;
            float _RayStep;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(ObjSpaceViewDir(v.vertex));
                return o;
            }

            float computeIntensity(float3 pos)
            {
                float k = 2.0f * 3.14159265f / (_WaveLength * 1e-9f);
                float sum = 0.0f;
                const int samples = 16;

                for(int x=0; x<samples; x++)
                {
                    float u = (x + 0.5f)/samples * 2.0f - 1.0f;
                    for(int y=0; y<samples; y++)
                    {
                        float v = (y + 0.5f)/samples * 2.0f - 1.0f;
                        if(u*u + v*v > 1.0f) continue;

                        float3 delta = pos - float3(u*_Aperture, v*_Aperture, 0);
                        float dist = length(delta);
                        sum += cos(k * (dist - pos.z)) / (dist + 1e-6f);
                    }
                }
                return (sum * sum) / (samples*samples) * 1000.0f;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 rayDir = normalize(i.viewDir);
                float3 accum = 0;
                float alpha = 0;

                for(float t=0.0f; t<_MaxDistance; t+=_RayStep)
                {
                    float3 pos = i.worldPos + rayDir * t;
                    float intensity = computeIntensity(pos);

                    // 颜色映射
                    float3 color = float3(
                        sin(intensity * 0.5f),
                        cos(intensity * 0.3f),
                        intensity * 0.2f
                    );

                    // 体积混合
                    accum += color * (1.0f - alpha) * _RayStep;
                    alpha += (1.0f - alpha) * intensity * _RayStep;
                    if(alpha > 0.95f) break;
                }
                return fixed4(accum, alpha);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}