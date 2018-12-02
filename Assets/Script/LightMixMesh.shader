﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "2D/Light Mix Mesh"
 {  
     Properties
     {
		 _MainTex("Texture",2D) = "white" {}
     }
     SubShader
     {
         Tags 
         { 
             "RenderType" = "Opaque" 
             "Queue" = "Transparent+1" 
         }
 
         Pass
         {
             ZWrite Off
             //Blend Off
             Blend DstColor Zero, Zero One
             ColorMask RGBA
             //Blend SrcColor One
             Cull Off
  
             CGPROGRAM
             #pragma vertex vert
             #pragma fragment frag
             #pragma multi_compile DUMMY PIXELSNAP_ON
             #include "UnityCG.cginc"
  
             sampler2D _MainTex;
             float4 _Color;
 
             struct appdata_t
			 {
				float4 vertex   : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
             v2f vert(appdata_t v)
             {
                 v2f o;
     
                 o.vertex = UnityObjectToClipPos(v.vertex);
				 o.texcoord = v.texcoord;
                 return o;
             }
                                                     
             float4 frag(v2f IN) : COLOR
             {
                 float2 uv = float2(IN.texcoord.x, 1 - IN.texcoord.y);
				 float4 c = tex2D(_MainTex, uv);
                 c.a = 1;
                 return c;
             }
 
             ENDCG
         }
     }
 }
