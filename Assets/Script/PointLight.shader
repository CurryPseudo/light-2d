Shader "2D/Light/PointLight"
{  
    Properties
    {
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
            #include "UnityCG.cginc"
  
 
            struct appdata_t
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
                float2 world : TEXCOORD0;
                float2 screen : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
            float4 _LightPos;
            float4 _LightColor;
            float _LightMaxDis;
            sampler2D _ShadowMap;
            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex);
                o.screen = ComputeScreenPos(o.vertex);
                return o;
            }
                                                    
            float4 frag(v2f IN) : COLOR
            {
                float2 dir = IN.world - _LightPos;
                float norm = length(dir) / _LightMaxDis;
                norm = saturate(norm);
                float4 c = float4(_LightColor.xyz * smoothstep(0, 1, (1 - norm)), 1);
                c.xyz *= tex2D(_ShadowMap, IN.screen.xy).r;
                return c;
            }
 
            ENDCG
        }
    }
}
