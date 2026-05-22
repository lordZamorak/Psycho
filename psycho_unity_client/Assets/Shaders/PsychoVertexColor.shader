Shader "Psycho/Vertex Color Lit"
{
    Properties
    {
        _Tint ("Tint", Color) = (1, 1, 1, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.25
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

        struct Input
        {
            float4 color : COLOR;
        };

        void surf(Input input, inout SurfaceOutputStandard output)
        {
            fixed4 color = input.color * _Tint;
            output.Albedo = color.rgb;
            output.Metallic = 0;
            output.Smoothness = _Smoothness;
            output.Alpha = color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
