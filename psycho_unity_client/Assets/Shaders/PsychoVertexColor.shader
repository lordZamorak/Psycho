Shader "Psycho/Vertex Color Lit"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.25
        _NoiseScale ("Detail Noise Scale", Range(0.02, 2)) = 0.42
        _NoiseStrength ("Detail Noise Strength", Range(0, 0.35)) = 0.12
        _SlopeDarkening ("Slope Darkening", Range(0, 1)) = 0.24
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        fixed4 _Tint;
        half _Smoothness;
        half _NoiseScale;
        half _NoiseStrength;
        half _SlopeDarkening;

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            float3 worldNormal;
        };

        float Hash21(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 color = input.color * _Tint;
            float2 noiseCoord = input.worldPos.xz * _NoiseScale;
            float broadNoise = sin(noiseCoord.x * 1.7 + noiseCoord.y * 1.1) * 0.5 + 0.5;
            float fineNoise = sin(noiseCoord.x * 4.1 - noiseCoord.y * 3.3) * 0.5 + 0.5;
            float detail = ((broadNoise * 0.88 + fineNoise * 0.12) * 2.0 - 1.0) * _NoiseStrength;
            float slope = 1.0 - saturate(input.worldNormal.y);

            color.rgb *= 1.0 + detail;
            color.rgb = lerp(color.rgb, color.rgb * 0.72, slope * _SlopeDarkening);
            output.Albedo = color.rgb;
            output.Metallic = 0;
            output.Smoothness = _Smoothness;
            output.Occlusion = lerp(1.0, 0.84, slope * _SlopeDarkening);
            output.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
