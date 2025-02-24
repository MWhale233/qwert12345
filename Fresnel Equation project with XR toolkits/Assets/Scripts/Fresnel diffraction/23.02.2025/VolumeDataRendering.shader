Shader "Custom/VolumeDataRendering"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "" {}
        _StepSize ("Step Size", Float) = 0.005
        _NumSteps ("Number of Steps", Int) = 100
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite Off
            Cull Off
            Fog { Mode Off }

            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler3D _VolumeTex;
            float _StepSize;
            int _NumSteps;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float3 rayOrigin: TEXCOORD0;
                float3 rayDir   : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // 假设体数据位于 [0,1]^3 内，使用物体中心作为射线目标
                float3 boxCenter = float3(0.5, 0.5, 0.5);
                o.rayOrigin = _WorldSpaceCameraPos;
                o.rayDir = normalize(boxCenter - _WorldSpaceCameraPos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float accum = 0;
                // 从摄像机位置沿射线方向采样，使用固定次数的采样步骤
                float3 pos = i.rayOrigin;
                
                // 固定循环次数，避免动态退出循环
                for (int step = 0; step < 100; step++)
                {
                    float3 samplePos = saturate(pos);  // 限制在 [0,1] 范围内
                    float sample = tex3D(_VolumeTex, samplePos).r;
                    accum += sample * _StepSize;
                    pos += i.rayDir * _StepSize;
                }
                
                return fixed4(accum, accum, accum, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}