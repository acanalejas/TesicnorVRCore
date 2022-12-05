Shader "Unlit/HighlightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OutlineThickness("Thickness", float) = 1
        _OutlineColor("Color", Color) = (0,0,0,1)
        _OutlineColor2("Color 2", Color) = (0,0,0,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Cull Front
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _OutlineColor;
            float _OutlineThickness;

            v2f vert (appdata v)
            {
                v2f o;
                float3 normal = normalize(v.normal);
                float3 outlineOffset = normal * _OutlineThickness;
                float3 position = v.vertex + outlineOffset;
                o.position = UnityObjectToClipPos(position);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
                ENDCG
        }
        Pass
        {
                CGPROGRAM
                #pragma vertex vert
            #pragma fragment frag
                // make fog work
                #pragma multi_compile_fog
            struct appdata
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
            };

            struct v2f {
                float4 position : POSITION;
            };

            float _OutlineThickness;
            fixed4 _OutlineColor2;

            v2f vert(appdata v) {
                v2f o;
                float3 normal = normalize(v.normal);
                float3 offset = (_OutlineThickness * 1.2f) * normal;
                float3 position = v.vertex + offset;
                o.position = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target{
                return _OutlineColor2;
            }
                ENDCG
        }
    }
}
