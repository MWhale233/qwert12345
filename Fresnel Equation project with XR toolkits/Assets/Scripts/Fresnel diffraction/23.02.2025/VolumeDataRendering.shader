Shader "Custom/VolumeRendering"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "white" {}
        _StepSize ("Step Size", Range(0.001, 0.1)) = 0.01
        _DensityScale ("Density Scale", Range(0.1, 10)) = 1.0
        _AlphaScale ("Alpha Scale", Range(0.01, 1)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.5

            #include "UnityCG.cginc"

            sampler3D _VolumeTex;
            float _StepSize;
            float _DensityScale;
            float _AlphaScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 localCamPos : TEXCOORD1;
                float3 localRayDir : TEXCOORD2;
            };

            bool RayAABBIntersection(float3 origin, float3 dir, out float entry, out float exit)
            {
                float3 t0 = (0.0 - origin) / dir;
                float3 t1 = (1.0 - origin) / dir;
                float3 tmin = min(t0, t1);
                float3 tmax = max(t0, t1);
                
                entry = max(max(tmin.x, tmin.y), tmin.z);
                exit = min(min(tmax.x, tmax.y), tmax.z);
                return exit > entry && exit > 0;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localCamPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                o.localRayDir = normalize(mul(unity_WorldToObject, float4(o.worldPos - _WorldSpaceCameraPos, 0)).xyz);
                return o;
            }

            fixed4 frag (v2f input) : SV_Target // 修改参数名避免冲突
            {
                float3 rayDir = normalize(input.localRayDir);
                
                float entry, exit;
                if(!RayAABBIntersection(input.localCamPos, rayDir, entry, exit))
                    discard;
                
                entry = max(entry, 0);
                float3 startPos = input.localCamPos + rayDir * entry;
                float3 endPos = input.localCamPos + rayDir * exit;
                float maxDistance = distance(startPos, endPos);
                
                float accumDensity = 0;
                float3 currentPos = startPos;
                int numSteps = (int)(maxDistance / _StepSize);
                
                // 修改循环变量名并修正条件判断
                for(int stepIdx = 0; stepIdx < numSteps; stepIdx++)
                {
                    // 修正条件判断的括号
                    if(any(currentPos < 0)) break; 
                    if(any(currentPos > 1)) break;
                    
                    float density = tex3D(_VolumeTex, currentPos).r * _DensityScale;
                    accumDensity += density * _StepSize;
                    
                    if(accumDensity > 20) break;
                    
                    currentPos += rayDir * _StepSize;
                }
                
                float alpha = 1 - exp(-accumDensity * _AlphaScale);
                return fixed4(alpha.xxx, alpha);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}