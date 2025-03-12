Shader "Unlit/VolumeDataRendering_Optimized"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "white" {}
        _Alpha ("Alpha", Range(0,1)) = 0.02
        _StepSize ("Step Size", Range(0.001,0.1)) = 0.01
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            // 减少最大步数到64
            #define MAX_STEP_COUNT 64
            #define EPSILON 0.00001f

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float3 objectVertex : TEXCOORD0;
                float3 rayDirection : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler3D _VolumeTex;
            float _Alpha;
            float _StepSize;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.objectVertex = v.vertex.xyz;
                float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.rayDirection = normalize(worldVertex - _WorldSpaceCameraPos);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 startPos = i.objectVertex + 0.5f;
                float3 rayDir = normalize(mul(unity_WorldToObject, float4(i.rayDirection, 0)).xyz);
                
                float4 finalColor = float4(0,0,0,0);
                float3 samplePos = startPos;
                
                // 固定循环次数，移除动态break
                for(int step = 0; step < MAX_STEP_COUNT; step++)
                {
                    // 增加边界保护
                    samplePos = clamp(samplePos, 0.0, 1.0);
                    
                    // 使用更高效的纹理采样方式
                    float4 sample = tex3Dlod(_VolumeTex, float4(samplePos, 0));
                    sample.a *= _Alpha;
                    
                    // 简化混合计算
                    finalColor = finalColor * (1 - sample.a) + sample * sample.a;
                    
                    samplePos += rayDir * _StepSize * (1.0 - finalColor.a);
                    
                    // 提前终止判断
                    if(finalColor.a > 0.99) break;
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
}