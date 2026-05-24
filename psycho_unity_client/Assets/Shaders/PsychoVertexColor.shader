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
        _GroundBlendStrength ("Ground Blend Strength", Range(0, 1)) = 0
        _GrassTint ("Grass Blend Tint", Color) = (0.32, 0.52, 0.25, 1)
        _PathTint ("Path Blend Tint", Color) = (0.43, 0.35, 0.24, 1)
        _RockTint ("Rock Blend Tint", Color) = (0.46, 0.46, 0.41, 1)
        _BlendNoiseScale ("Ground Blend Noise Scale", Range(0.02, 2)) = 0.26
        _BlendNoiseStrength ("Ground Blend Noise Strength", Range(0, 1)) = 0.35
        _HighlandTextureStrength ("Highland Texture Strength", Range(0, 1)) = 0.55
        _StoneStrataStrength ("Stone Strata Strength", Range(0, 1)) = 0.45
        _SnowDustStrength ("Snow Dust Strength", Range(0, 1)) = 0.18
        _RimColor ("NXT Rim Color", Color) = (0.70, 0.84, 1.00, 1)
        _RimStrength ("NXT Rim Strength", Range(0, 0.35)) = 0.08
        _SpecularLift ("NXT Specular Lift", Range(0, 0.35)) = 0.08
        _GrassAlbedo ("Grass Splat Albedo", 2D) = "white" {}
        _PathAlbedo ("Path Splat Albedo", 2D) = "white" {}
        _RockAlbedo ("Rock Splat Albedo", 2D) = "white" {}
        _GrassNormalMap ("Grass Splat Normal", 2D) = "bump" {}
        _PathNormalMap ("Path Splat Normal", 2D) = "bump" {}
        _RockNormalMap ("Rock Splat Normal", 2D) = "bump" {}
        _TerrainTexScale ("Terrain Splat Texture Scale", Range(0.01, 1)) = 0.075
        _TerrainAlbedoStrength ("Terrain Splat Albedo Strength", Range(0, 1)) = 0.70
        _TerrainNormalStrength ("Terrain Splat Normal Strength", Range(0, 1)) = 0.42
        _SplatContrast ("Terrain Splat Mask Contrast", Range(0.5, 3)) = 1.35
        _MacroVariationScale ("Terrain Macro Variation Scale", Range(0.005, 0.12)) = 0.035
        _MacroVariationStrength ("Terrain Macro Variation Strength", Range(0, 0.45)) = 0.16
        _SlopeProjectionStrength ("Slope Texture Projection Strength", Range(0, 1)) = 0.72
        _PathPebbleStrength ("Path Pebble Strength", Range(0, 1)) = 0.42
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
        half _GroundBlendStrength;
        fixed4 _GrassTint;
        fixed4 _PathTint;
        fixed4 _RockTint;
        half _BlendNoiseScale;
        half _BlendNoiseStrength;
        half _HighlandTextureStrength;
        half _StoneStrataStrength;
        half _SnowDustStrength;
        fixed4 _RimColor;
        half _RimStrength;
        half _SpecularLift;
        sampler2D _GrassAlbedo;
        sampler2D _PathAlbedo;
        sampler2D _RockAlbedo;
        sampler2D _GrassNormalMap;
        sampler2D _PathNormalMap;
        sampler2D _RockNormalMap;
        half _TerrainTexScale;
        half _TerrainAlbedoStrength;
        half _TerrainNormalStrength;
        half _SplatContrast;
        half _MacroVariationScale;
        half _MacroVariationStrength;
        half _SlopeProjectionStrength;
        half _PathPebbleStrength;

        struct Input
        {
            float4 color : COLOR;
            float3 worldPos;
            float3 worldNormal;
            float3 viewDir;
            INTERNAL_DATA
        };

        float Hash21(float2 p)
        {
            p = frac(p * float2(123.34, 456.21));
            p += dot(p, p + 45.32);
            return frac(p.x * p.y);
        }

        float ValueNoise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
            float a = Hash21(i);
            float b = Hash21(i + float2(1.0, 0.0));
            float c = Hash21(i + float2(0.0, 1.0));
            float d = Hash21(i + float2(1.0, 1.0));
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }

        float Fbm(float2 p)
        {
            float value = 0.0;
            float amplitude = 0.5;
            for (int i = 0; i < 4; i++)
            {
                value += ValueNoise(p) * amplitude;
                p = p * 2.07 + float2(17.13, -11.71);
                amplitude *= 0.5;
            }

            return saturate(value);
        }

        float3 SafeNormalize(float3 value, float3 fallback)
        {
            float lengthSquared = dot(value, value);
            float invLength = rsqrt(max(lengthSquared, 0.000001));
            return lerp(fallback, value * invLength, step(0.000001, lengthSquared));
        }

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 color = input.color * _Tint;
            float3 normal = SafeNormalize(input.worldNormal, float3(0.0, 1.0, 0.0));
            float2 noiseCoord = input.worldPos.xz * _NoiseScale;
            float broadNoise = sin(noiseCoord.x * 1.7 + noiseCoord.y * 1.1) * 0.5 + 0.5;
            float fineNoise = sin(noiseCoord.x * 4.1 - noiseCoord.y * 3.3) * 0.5 + 0.5;
            float macroFbm = Fbm(input.worldPos.xz * (_BlendNoiseScale * 0.72));
            float microFbm = Fbm(input.worldPos.xz * (_NoiseScale * 5.6));
            float detail = ((broadNoise * 0.42 + fineNoise * 0.14 + microFbm * 0.44) * 2.0 - 1.0) * _NoiseStrength;
            float slope = 1.0 - saturate(normal.y);
            float sunFacing = saturate(dot(normal, SafeNormalize(float3(0.36, 0.82, 0.24), float3(0.0, 1.0, 0.0))));
            float topLight = saturate(normal.y);
            float blendNoise = (sin(input.worldPos.x * _BlendNoiseScale + input.worldPos.z * (_BlendNoiseScale * 0.61)) * 0.5 + 0.5) * 0.65
                + (sin(input.worldPos.x * (_BlendNoiseScale * 3.7) - input.worldPos.z * (_BlendNoiseScale * 2.9)) * 0.5 + 0.5) * 0.20
                + macroFbm * 0.15;
            float flatMask = saturate(normal.y * 1.35 - 0.18);
            float greenDominance = saturate((color.g - max(color.r, color.b)) * 2.15 + 0.36 + (blendNoise - 0.5) * (_BlendNoiseStrength * 0.70));
            float pathWarmth = saturate((color.r - color.b) * 1.25 + (0.58 - color.g) * 0.58 + (0.5 - blendNoise) * (_BlendNoiseStrength * 0.78) + flatMask * 0.22);
            float rockMask = saturate(slope * 2.20 + (1.0 - flatMask) * 0.26 + (1.0 - greenDominance) * 0.06 + (blendNoise - 0.64) * (_BlendNoiseStrength * 0.48) - 0.34);
            float pebble = saturate((microFbm - 0.48) * 2.2);
            float striation = abs(sin(input.worldPos.y * 0.92 + input.worldPos.x * 0.18 + input.worldPos.z * 0.11));
            float snowDust = saturate((input.worldPos.y - 26.0) / 28.0 + slope * 0.28 + (macroFbm - 0.62) * 0.35) * _SnowDustStrength;
            float3 groundBlend = color.rgb;
            float3 grassDetail = _GrassTint.rgb * lerp(0.66, 1.22, saturate(macroFbm * 0.52 + microFbm * 0.48));
            float3 pathDetail = _PathTint.rgb * lerp(0.70, 1.18, saturate(pebble * 0.68 + fineNoise * 0.32));
            float3 rockDetail = _RockTint.rgb * lerp(0.62, 1.20, saturate(striation * _StoneStrataStrength + microFbm * 0.45));
            rockDetail = lerp(rockDetail, float3(0.68, 0.72, 0.70), snowDust);

            float grassMask = pow(saturate(greenDominance * flatMask), _SplatContrast);
            float pathMask = pow(saturate(pathWarmth * flatMask * (1.0 - greenDominance * 0.36)), _SplatContrast);
            float rockSplatMask = pow(saturate(rockMask), _SplatContrast);
            float totalSplatMask = max(0.001, grassMask + pathMask + rockSplatMask);
            grassMask /= totalSplatMask;
            pathMask /= totalSplatMask;
            rockSplatMask /= totalSplatMask;

            float2 terrainUv = input.worldPos.xz * _TerrainTexScale;
            float macroVariation = Fbm(input.worldPos.xz * _MacroVariationScale);
            float macroShade = lerp(1.0 - _MacroVariationStrength, 1.0 + _MacroVariationStrength, macroVariation);
            float3 grassTex = tex2D(_GrassAlbedo, terrainUv * 1.05 + float2(0.031, -0.017)).rgb;
            float3 pathTex = tex2D(_PathAlbedo, terrainUv * 0.88 + float2(-0.047, 0.019)).rgb;
            float3 rockTex = tex2D(_RockAlbedo, terrainUv * 0.64 + float2(0.071, 0.043)).rgb;
            float3 projectionWeights = pow(abs(normal), 4.0);
            projectionWeights /= max(0.001, projectionWeights.x + projectionWeights.y + projectionWeights.z);
            float3 rockProjected =
                tex2D(_RockAlbedo, input.worldPos.zy * (_TerrainTexScale * 0.70) + float2(0.013, 0.061)).rgb * projectionWeights.x
                + tex2D(_RockAlbedo, input.worldPos.xz * (_TerrainTexScale * 0.64) + float2(0.071, 0.043)).rgb * projectionWeights.y
                + tex2D(_RockAlbedo, input.worldPos.xy * (_TerrainTexScale * 0.70) + float2(-0.029, 0.037)).rgb * projectionWeights.z;
            rockTex = lerp(rockTex, rockProjected, saturate(slope * _SlopeProjectionStrength));
            grassDetail *= lerp(float3(1.0, 1.0, 1.0), grassTex * 1.38, _TerrainAlbedoStrength);
            pathDetail *= lerp(float3(1.0, 1.0, 1.0), pathTex * 1.42, _TerrainAlbedoStrength);
            rockDetail *= lerp(float3(1.0, 1.0, 1.0), rockTex * 1.34, _TerrainAlbedoStrength);
            grassDetail *= macroShade;
            pathDetail *= lerp(1.0, macroShade, 0.58) * lerp(1.0, lerp(0.76, 1.32, pebble), _PathPebbleStrength);
            rockDetail *= lerp(1.0, macroShade, 0.78);

            float3 grassNormal = UnpackNormal(tex2D(_GrassNormalMap, terrainUv * 1.05 + float2(0.031, -0.017)));
            float3 pathNormal = UnpackNormal(tex2D(_PathNormalMap, terrainUv * 0.88 + float2(-0.047, 0.019)));
            float3 rockNormal = UnpackNormal(tex2D(_RockNormalMap, terrainUv * 0.64 + float2(0.071, 0.043)));
            float3 splatNormal = SafeNormalize(grassNormal * grassMask + pathNormal * pathMask + rockNormal * rockSplatMask, float3(0.0, 0.0, 1.0));

            groundBlend = lerp(groundBlend, grassDetail, greenDominance * flatMask);
            groundBlend = lerp(groundBlend, pathDetail, pathWarmth * flatMask * (1.0 - greenDominance * 0.42));
            groundBlend = lerp(groundBlend, rockDetail, rockMask * lerp(0.30, 1.0, slope));
            float rim = pow(1.0 - saturate(dot(SafeNormalize(input.viewDir, normal), normal)), 2.2) * _RimStrength;
            float viewDistance = length((_WorldSpaceCameraPos.xyz - input.worldPos).xz);
            float distanceFade = saturate((viewDistance - _DistanceStart) / max(1.0, _DistanceEnd - _DistanceStart)) * _DistanceBlend;

            color.rgb = lerp(color.rgb, groundBlend, _GroundBlendStrength);
            color.rgb *= 1.0 + detail;
            color.rgb = lerp(color.rgb, color.rgb * lerp(0.74, 1.18, microFbm), _HighlandTextureStrength * 0.42);
            color.rgb = lerp(color.rgb, color.rgb * lerp(0.88, 1.10, pebble), _HighlandTextureStrength * pathWarmth * flatMask);
            color.rgb *= lerp(0.94, 1.11, sunFacing * _HemisphereContrast + topLight * 0.045);
            color.rgb = lerp(color.rgb, color.rgb * _TopWarmth.rgb, topLight * 0.24);
            color.rgb = lerp(color.rgb, color.rgb * 0.72, slope * _SlopeDarkening);
            color.rgb = lerp(color.rgb, _RimColor.rgb, rim);
            color.rgb = lerp(color.rgb, _DistanceTint.rgb, distanceFade);
            output.Albedo = color.rgb;
            output.Metallic = 0;
            output.Smoothness = saturate(_Smoothness + sunFacing * _SpecularLift);
            output.Occlusion = lerp(1.0, 0.84, slope * _SlopeDarkening);
            output.Normal = SafeNormalize(lerp(float3(0.0, 0.0, 1.0), splatNormal, _TerrainNormalStrength * _GroundBlendStrength), float3(0.0, 0.0, 1.0));
            output.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
