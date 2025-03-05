Shader "Unlit/VolumeDataRendering"
{
    // Properties
    // {
    //     _MainTex ("Texture", 3D) = "white" {}
    //     _Alpha ("Alpha", float) = 0.02
    //     _StepSize ("Step Size", float) = 0.01
    // }
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "white" {}
        _Alpha ("Alpha", float) = 0.02
        _StepSize ("Step Size", float) = 0.01

        [Toggle(_CLIPPING)] _Clipping("Enable Clipping", Float) = 1
        _ClipPlane("Clip Plane", Vector) = (0,1,0,0) // (normal.x, normal.y, normal.z, distance)
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


            // 在CGPROGRAM添加
            #pragma shader_feature _CLIPPING
            float4 _ClipPlane;

            #include "UnityCG.cginc"

            // 最大光线追踪样本数
            #define MAX_STEP_COUNT 128

            // 允许的浮点数误差
            #define EPSILON 0.00001f

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
    
                UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
            };

            struct v2f
            {
                float3 objectVertex : TEXCOORD0;
                float3 vectorToSurface : TEXCOORD1;


                float4 vertex : SV_POSITION;
                
                UNITY_VERTEX_OUTPUT_STEREO //Insert
            };

            // sampler3D _MainTex;
            // float4 _MainTex_ST;
            sampler3D _VolumeTex;


            float _Alpha;
            float _StepSize;

            v2f vert (appdata v)
            {
                v2f o;
                
                UNITY_SETUP_INSTANCE_ID(v); //Insert
                UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

                // 对象空间中的顶点将成为光线追踪的起点
                o.objectVertex = v.vertex;

                // 计算世界空间中从摄像机到顶点的矢量
                float3 worldVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.vectorToSurface = worldVertex - _WorldSpaceCameraPos;

                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); //Insert

            float4 BlendUnder(float4 color, float4 newColor)
            {
                color.rgb += (1.0 - color.a) * newColor.a * newColor.rgb;
                color.a += (1.0 - color.a) * newColor.a;
                return color;
            }

            // fixed4 frag(v2f i) : SV_Target
            // {
            //     // 开始在对象的正面进行光线追踪
            //     float3 rayOrigin = i.objectVertex;

            //     // 使用摄像机到对象表面的矢量获取射线方向
            //     float3 rayDirection = mul(unity_WorldToObject, float4(normalize(i.vectorToSurface), 1));

            //     float4 color = float4(0, 0, 0, 0);
            //     float3 samplePosition = rayOrigin;

            //     // 穿过对象空间进行光线追踪
            //     for (int i = 0; i < MAX_STEP_COUNT; i++)
            //     {
            //         // 仅在单位立方体边界内累积颜色
            //         if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
            //         {
            //             // float4 sampledColor = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f));

            //             // 修改后（正确匹配Unity的0-1纹理空间）
            //             sampledColor = tex3D(_VolumeTex, samplePosition + 0.5f);


            //             sampledColor.a *= _Alpha;
            //             color = BlendUnder(color, sampledColor);
            //             samplePosition += rayDirection * _StepSize;
            //         }
            //     }

            //     return color;
            // }
            fixed4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert

                // 转换到世界空间
                float3 worldPos = mul(unity_ObjectToWorld, float4(i.objectVertex, 1)).xyz;
                // 平面剪切计算
                #if _CLIPPING

                    float distance = dot(_ClipPlane.xyz, worldPos) + _ClipPlane.w;
                    clip(distance > 0 ? -1 : 1); // 保留平面下方
                #endif

                // 校正采样坐标（假设物体中心在原点且尺寸为1x1x1）
                float3 startPos = i.objectVertex + 0.5f; // 映射到 [0,1] 纹理空间
                
                // 光线步进逻辑
                float3 rayDir = normalize(i.vectorToSurface);
                float4 finalColor = float4(0,0,0,0);
                
                for(int step=0; step<MAX_STEP_COUNT; step++)
                {
                    if(any(startPos < 0) || any(startPos > 1)) break;
                    
                    float4 sample = tex3D(_VolumeTex, startPos);
                    sample.a *= _Alpha;
                    
                    // 使用预乘Alpha混合
                    finalColor.rgb = finalColor.rgb * (1 - sample.a) + sample.rgb * sample.a;
                    finalColor.a = finalColor.a + (1 - finalColor.a) * sample.a;
                    
                    startPos += rayDir * _StepSize;
                }
                return finalColor;
            }
            ENDCG
        }
    }
}