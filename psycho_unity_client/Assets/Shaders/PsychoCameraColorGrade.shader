Shader "Hidden/Psycho/Camera Color Grade"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Exposure ("Exposure", Float) = 1
        _Contrast ("Contrast", Float) = 1
        _Saturation ("Saturation", Float) = 1
        _Warmth ("Warmth", Float) = 0
        _Vignette ("Vignette", Float) = 0
        _Sharpen ("Sharpen", Float) = 0
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _TexelSize;
            float _Exposure;
            float _Contrast;
            float _Saturation;
            float _Warmth;
            float _Vignette;
            float _Sharpen;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 uv = input.uv;
                float3 color = tex2D(_MainTex, uv).rgb;

                if (_Sharpen > 0.001)
                {
                    float3 blur =
                        tex2D(_MainTex, uv + float2(_TexelSize.x, 0)).rgb +
                        tex2D(_MainTex, uv - float2(_TexelSize.x, 0)).rgb +
                        tex2D(_MainTex, uv + float2(0, _TexelSize.y)).rgb +
                        tex2D(_MainTex, uv - float2(0, _TexelSize.y)).rgb;
                    blur *= 0.25;
                    color = lerp(color, color + (color - blur), saturate(_Sharpen));
                }

                color *= _Exposure;
                float luminance = dot(color, float3(0.2126, 0.7152, 0.0722));
                color = lerp(float3(luminance, luminance, luminance), color, _Saturation);
                color = (color - 0.5) * _Contrast + 0.5;
                color += float3(_Warmth * 0.055, _Warmth * 0.022, -_Warmth * 0.026);

                float2 centered = uv * 2.0 - 1.0;
                centered.x *= _TexelSize.z / max(1.0, _TexelSize.w);
                float vignetteMask = smoothstep(1.48, 0.18, dot(centered, centered));
                color *= lerp(1.0 - _Vignette, 1.0, vignetteMask);

                return fixed4(saturate(color), 1);
            }
            ENDCG
        }
    }
    FallBack Off
}
