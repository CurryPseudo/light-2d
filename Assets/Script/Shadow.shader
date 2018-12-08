
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
             BlendOp RevSub
  
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #include "UnityCG.cginc"
  
 
             struct appdata_t
			 {
				float4 vertex   : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
             v2f vert(appdata_t v)
             {
                 v2f o;
     
                 o.vertex = UnityObjectToClipPos(v.vertex);
                 return o;
             }
                                                     
             float4 frag(v2f IN) : COLOR
             {
                 return float4(0,0,0,0);
             }
 
             ENDCG
         }
     }
 }
