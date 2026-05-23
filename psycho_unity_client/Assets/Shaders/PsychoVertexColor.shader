Shader "Psycho/Vertex Color Lit"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.25
        _NoiseScale ("Detail Noise Scale", Range(0.02, 2)) = 0.42
        _NoiseStrength ("Detail Noise Strength", Range(0, 0.35)) = 0.12
        _SlopeDarkening ("Slope Darkening", Range(0, 1)) = 0.24
        _DistanceTint ("Distance Atmosphere Tint", Color) = (0.50, 0.60, 0.66, 1)
        _DistanceStart ("Distance Atmosphere Start", Range(0, 400)) = 95
        _DistanceEnd ("Distance Atmosphere End", Range(20, 800)) = 340
        _DistanceBlend ("Distance Atmosphere Blend", Range(0, 1)) = 0.14
        _TopWarmth ("Top Surface Warmth", Color) = (0.99, 1.00, 0.94, 1)
        _HemisphereContrast ("Hemisphere Contrast", Range(0, 1)) = 0.14
        _RimColor ("NXT Rim Color", Color) = (0.70, 0.84, 1.00, 1)
        _RimStrength ("NXT Rim Strength", Range(0, 0.35)) = 0.08
        _SpecularLift ("NXT Specular Lift", Range(0, 0.35)) = 0.08
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
        fixed4 _DistanceTint;
        half _DistanceStart;
        half _DistanceEnd;
        half _DistanceBlend;
        fixed4 _TopWarmth;
        half _HemisphereContrast;
        fixed4 _RimColor;
        half _RimStrength;
        half _SpecularLift;

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
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
            float3 normal = normalize(input.worldNormal);
            float2 noiseCoord = input.worldPos.xz * _NoiseScale;
            float broadNoise = sin(noiseCoord.x * 1.7 + noiseCoord.y * 1.1) * 0.5 + 0.5;
            float fineNoise = sin(noiseCoord.x * 4.1 - noiseCoord.y * 3.3) * 0.5 + 0.5;
            float detail = ((broadNoise * 0.88 + fineNoise * 0.12) * 2.0 - 1.0) * _NoiseStrength;
            float slope = 1.0 - saturate(normal.y);
            float sunFacing = saturate(dot(normal, normalize(float3(0.36, 0.82, 0.24))));
            float topLight = saturate(normal.y);
            float rim = pow(1.0 - saturate(dot(normalize(input.viewDir), normal)), 2.2) * _RimStrength;
            float viewDistance = length((_WorldSpaceCameraPos.xyz - input.worldPos).xz);
            float distanceFade = saturate((viewDistance - _DistanceStart) / max(1.0, _DistanceEnd - _DistanceStart)) * _DistanceBlend;

            color.rgb *= 1.0 + detail;
            color.rgb *= lerp(0.94, 1.11, sunFacing * _HemisphereContrast + topLight * 0.045);
            color.rgb = lerp(color.rgb, color.rgb * _TopWarmth.rgb, topLight * 0.24);
            color.rgb = lerp(color.rgb, color.rgb * 0.72, slope * _SlopeDarkening);
            color.rgb = lerp(color.rgb, _RimColor.rgb, rim);
            color.rgb = lerp(color.rgb, _DistanceTint.rgb, distanceFade);
            output.Albedo = color.rgb;
            output.Metallic = 0;
            output.Smoothness = saturate(_Smoothness + sunFacing * _SpecularLift);
            output.Occlusion = lerp(1.0, 0.84, slope * _SlopeDarkening);
            output.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
