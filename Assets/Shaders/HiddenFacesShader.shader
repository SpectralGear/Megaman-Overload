Shader "Custom/HiddenFacesShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Transparency ("Transparency", Range(0,1)) = 1.0
        _HiddenFaces ("Hidden Faces", Vector) = (0,0,0,0) // x=left, y=right, z=up, w=down
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off // Disable depth writing for transparency

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _Transparency;
            float4 _HiddenFaces; // Stores visibility of each face

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
                col.a = _Transparency;

                // Hide faces based on pre-determined visibility
                if ((_HiddenFaces.x && i.worldNormal.x < -0.5) || // Left face
                    (_HiddenFaces.y && i.worldNormal.x > 0.5) ||  // Right face
                    (_HiddenFaces.z && i.worldNormal.y > 0.5) ||  // Top face
                    (_HiddenFaces.w && i.worldNormal.y < -0.5))   // Bottom face
                {
                    discard; // Don't render this face
                }

                return col;
            }
            ENDCG
        }
    }
}
