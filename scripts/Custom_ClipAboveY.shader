Shader "Custom/ClipAboveY"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _MySpecColor ("Specular Color", Color) = (1,1,1,1)
        _Shininess ("Shininess", Range(0.03, 1)) = 0.078125
        _CutoffY ("Max Y Height", Float) = 1000
        _Falloff ("Falloff Distance", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200
        
        // Enable depth writing with alpha cutout for proper z-buffering
        ZWrite On
        Cull Back
        
        CGPROGRAM
        // Use the surf directive with a custom lighting model
        #pragma surface surf BlinnPhong alpha:fade
        #pragma target 3.0
        
        sampler2D _MainTex;
        fixed4 _Color;
        fixed4 _MySpecColor;
        half _Shininess;
        float _CutoffY;
        float _Falloff;
        
        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };
        
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Calculate alpha based on height
            float heightAlpha = 1.0;
            
            if (IN.worldPos.y > _CutoffY)
            {
                // Completely transparent above cutoff
                heightAlpha = 0.0;
            }
            else if (IN.worldPos.y > _CutoffY - _Falloff)
            {
                // Gradual transparency in falloff zone
                heightAlpha = (_CutoffY - IN.worldPos.y) / _Falloff;
            }
            
            // Sample the texture
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            
            // Apply material properties
            o.Albedo = c.rgb * _Color.rgb;
            o.Specular = _Shininess;
            o.Gloss = 1.0;
            o.Alpha = c.a * _Color.a * heightAlpha; // Combine texture alpha with height alpha
        }
        
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
