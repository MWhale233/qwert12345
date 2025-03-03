Shader "Custom/SectionPlane"
{
    Properties
    {
        _VolumeTex ("Volume Texture", 3D) = "white" {}
        _PlaneEquation ("Plane Equation", Vector) = (0,1,0,0)
        _LineWidth ("Line Width", Range(0.001, 0.1)) = 0.01
        _LineColor ("Line Color", Color) = (1,0,0,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent+100" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off  // 新增：关闭面片剔除

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
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                float3 localPos : TEXCOORD1;
            };

            sampler3D _VolumeTex;
            float4 _PlaneEquation;
            float4 _LineColor;
            float _LineWidth;
            float4x4 _VolumeWorldToLocal;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.localPos = mul(_VolumeWorldToLocal, float4(o.worldPos, 1)).xyz; // 修正点：补充右括号
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 采样体积数据
                float3 uv = i.localPos + 0.5f;
                float4 data = tex3D(_VolumeTex, uv);

                // // 修正采样坐标计算（适配forward方向）
                // float3 uv = i.localPos + 0.5f;
                
                // // 添加forward方向补偿（关键修改）
                // uv.z = 1 - uv.z; // 反转Z轴方向
                
                // float4 data = tex3D(_VolumeTex, uv);

                // 计算到剪切平面的距离
                float dist = abs(dot(_PlaneEquation.xyz, i.worldPos) + _PlaneEquation.w);

                // 绘制轮廓线 (修正点：重命名line变量)
                float lineFactor = smoothstep(_LineWidth, _LineWidth*0.5, dist);
                float4 color = data;
                
                // 修正点：添加类型转换确保参数匹配
                color.rgb = lerp(
                    color.rgb, 
                    _LineColor.rgb, 
                    (float)(lineFactor * _LineColor.a) // 显式转换为float
                );

                return color;
            }
            ENDCG
        }
    }
}