// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "2D/Light/TexMesh"
{  
    Properties
    {
        _SubLightTex("Light Texture", 2D) = "black" {}
    }
    SubShader
    {
 
        Pass
        {
            ZWrite Off
            ColorMask RGBA
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile DUMMY PIXELSNAP_ON
            #include "UnityCG.cginc"


            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _SubLightTex;
            float4 _SubLightTex_ST;
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _SubLightTex);
                return o;
            }
                                                    
        
        
            float4 frag(v2f IN) : COLOR
            {
                return tex2D(_SubLightTex, IN.uv);
            }

            ENDCG
        }
    }
}
