// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "2D/Light/ShadowMesh"
 {  
     Properties
     {
     }
     SubShader
     {
         Tags 
         { 
             "LightMode" = "Light" 
             "Queue" = "Geometry" 
         }
 
         Pass
         {
             ZWrite Off
             ColorMask RGBA
             Blend Off
  
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile DUMMY PIXELSNAP_ON
             #include "UnityCG.cginc"
  
 
             struct appdata_t
			 {
				float4 vertex   : POSITION;
                float4 color    : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
                float4 color    : COLOR;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
             v2f vert(appdata_t v)
             {
                 v2f o;
     
                 o.vertex = UnityObjectToClipPos(v.vertex);
				 o.color = v.color;
                 return o;
             }
                                                     
             float4 frag(v2f IN) : COLOR
             {
                 return IN.color;
             }
 
             ENDCG
         }
     }
 }
