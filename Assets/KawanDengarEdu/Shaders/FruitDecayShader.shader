Shader "KawanDengarEdu/FruitDecayShader"
{
    Properties
    {
        _Color ("Fresh Color", Color) = (0.94, 0.27, 0.27, 1.0)
        _DecayColor ("Rotten Decay Color", Color) = (0.32, 0.18, 0.08, 1.0)
        _DecayProgress ("Decay Progress (0 to 1)", Range(0.0, 1.0)) = 0.0
        _MainTex ("Albedo Texture (Optional)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 worldNormal;
        };

        fixed4 _Color;
        fixed4 _DecayColor;
        half _DecayProgress;
        half _Glossiness;
        half _Metallic;

        // Simple pseudo-random procedural noise for spotty decay patches
        float HashNoise(float3 p)
        {
            return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * 43758.5453);
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Sample base texture if assigned
            fixed4 texColor = tex2D(_MainTex, IN.uv_MainTex);

            // Generate decay spot factor
            float spot = HashNoise(floor(IN.worldPos * 8.0));
            float threshold = saturate(_DecayProgress * 1.5 - (spot * 0.4));

            // Smoothly lerp fresh color to rotten brown
            fixed4 blendedColor = lerp(_Color, _DecayColor, threshold) * texColor;

            o.Albedo = blendedColor.rgb;
            o.Metallic = _Metallic;
            // Less glossy when rotten
            o.Smoothness = _Glossiness * (1.0 - (_DecayProgress * 0.7));
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
