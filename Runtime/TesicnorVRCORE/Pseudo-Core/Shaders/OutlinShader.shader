// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/OutlinShader"
{
    Properties
    {
        _Width ("Width", Float) = 1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True"}
        Cull Front

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VertexInput{
                float4 vertex : POSITION;
                float4 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
            };

            uniform float _Width;
            uniform float4 _Color;

            VertexOutput vert(VertexInput v) {
                VertexOutput o;
                float4 objPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));

                float dist = distance(_WorldSpaceCameraPos, objPos.xyz) / _ScreenParams.g;
                float expand = dist * 0.25 * _Width;
                float4 pos = float4(v.vertex.xyz + v.normal * expand, 1);

                o.pos = UnityObjectToClipPos(pos);
                return o;
            }

            float4 frag(VertexOutput i) : Color
            {
                return fixed4(_Color.rgb,0);
            }
            ENDCG
        }
    }
}
