Shader "2D/Light/Shadow"
{  
    Properties
    {
    }
    SubShader
    {

        Pass
        {
            ZWrite Off
            ColorMask R
            BlendOp Sub

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float2 uv       : TEXCOORD;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
            v2f vert(appdata_t v)
            {
                v2f o;
    
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
                                                    
            float4 frag(v2f IN) : COLOR
            {
                
                return float4(smoothstep(0, 1, IN.uv.x / IN.uv.y),1,1,1);
            }

            ENDCG
        }
    }
}
