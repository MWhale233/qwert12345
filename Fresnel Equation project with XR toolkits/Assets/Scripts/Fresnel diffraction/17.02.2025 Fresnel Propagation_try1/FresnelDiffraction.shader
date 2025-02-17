Shader "Custom/FresnelDiffraction"
{
    Properties
    {
        _Lambda ("波长 (λ)", Float) = 0.5
        _Aperture ("孔径半径 (a)", Float) = 1.0
        _ZMin ("最小轴向距离", Float) = 0.1
        _ZMax ("最大轴向距离", Float) = 10.0
        _RMax ("最大径向距离", Float) = 5.0
        _Contrast ("对比度", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Lambda;
            float _Aperture;
            float _ZMin;
            float _ZMax;
            float _RMax;
            float _Contrast;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            // 近似一阶贝塞尔函数
            float besselJ1(float x)
            {
                // 使用多项式近似，适用于x较小的情况
                if (abs(x) < 1e-4) return 0.0;
                float x2 = x*x;
                return x*(0.5 - x2/16.0);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 映射UV到轴向(z)和径向(r)距离
                float z = lerp(_ZMin, _ZMax, i.uv.x);
                float r = lerp(0.0, _RMax, i.uv.y);
                
                // 波数计算
                float k = 2 * 3.1415926 / _Lambda;
                
                // 夫琅禾费衍射（艾里斑近似）
                float alpha = k * _Aperture * r / z;
                float airy = pow(2 * besselJ1(alpha) / alpha, 2);
                
                // 菲涅尔相位调制
                float phase = k * r * r / (2.0 * z);
                float fresnel = pow(cos(phase), 4); // 增强对比度
                
                // 组合光强
                float I = airy * fresnel;
                
                // 距离衰减
                I *= 1.0 / (z * z + 0.1); // 防止除以零
                
                // 调整对比度和输出
                I = saturate(I * _Contrast);
                return fixed4(I, I, I, 1.0);
            }
            ENDCG
        }
    }
}