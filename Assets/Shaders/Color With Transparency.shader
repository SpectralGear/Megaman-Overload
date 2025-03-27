Shader "Custom/Color With Transparency"
{
    Properties
    {
        _Color ("Base Color", Color) = (1, 1, 1, 1) // Base color
        _Transparency ("Transparency", Range(0, 1)) = 1.0 // Transparency control
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" }

        Pass
        {
            Name "TransparentPass"
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha // Standard transparency blending
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Declare the properties
            float4 _Color; // Base color
            float _Transparency; // Transparency level

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Vertex shader
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); // Transform vertex position
                o.uv = v.uv;
                return o;
            }

            // Fragment shader
            half4 frag(v2f i) : SV_Target
            {
                half4 color = _Color; // Fetch the _Color property from the shader
                color.a *= _Transparency; // Modify the alpha based on the transparency property

                return color; // Return the final color with modified alpha
            }
            ENDCG
        }
    }

    Fallback "Diffuse"
}