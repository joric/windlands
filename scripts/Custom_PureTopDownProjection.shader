Shader "Custom/PureTopDownProjection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TilingX ("Tiling X", Float) = 0.1
        _TilingZ ("Tiling Z", Float) = 0.1
        _OffsetX ("Offset X", Float) = 0.0
        _OffsetZ ("Offset Z", Float) = 0.0
        _Color ("Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Explicitly use the surface shader with Standard lighting model
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        float _TilingX;
        float _TilingZ;
        float _OffsetX;
        float _OffsetZ;
        fixed4 _Color;

        struct Input
        {
            // We ONLY want world position, explicitly ignoring UV
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Calculate planar projection UV coordinates from world position
            // Using only X and Z for top-down projection (ignoring Y height)
            float2 projectedUV = float2(
                IN.worldPos.x * _TilingX + _OffsetX,
                IN.worldPos.z * _TilingZ + _OffsetZ
            );
            
            // Sample texture with our custom projected UVs
            fixed4 c = tex2D(_MainTex, projectedUV) * _Color;
            
            o.Albedo = c.rgb;
            o.Alpha = c.a;
            
            // Set some reasonable defaults for the Standard shader outputs
            o.Metallic = 0;
            o.Smoothness = 0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}