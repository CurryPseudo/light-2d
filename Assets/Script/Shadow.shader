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
            Blend One One

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"


            struct appdata_t
            {
				float4 vertex   : POSITION;
                float2 aPos     : TEXCOORD0;
                float2 bPos     : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
                float2 A     : TEXCOORD0;
                float2 B     : TEXCOORD1;
                float2 E    : TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};
  
            v2f vert(appdata_t v)
            {
                v2f o;
    
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.A = v.aPos;
                o.B = v.bPos;
                o.E = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            float angleBetween(float2 v1, float2 v2) {
                float M_PI = 3.14159265358;
                float angle = atan2(v1.y, v1.x) - atan2(v2.y, v2.x);
                angle = angle + step(M_PI, angle) * -2 * M_PI;
                angle = angle + step(angle, -M_PI) * 2 * M_PI;
                return angle;
            }
            float2 _LightPos;
            float _LightVolumeRadius;
            float4 frag(v2f IN) : COLOR
            {
                float2 CE = IN.E - _LightPos;                 
                float2 CENorm = normalize(float2(-CE.y, CE.x)) * _LightVolumeRadius;
                float2 dirASide = (_LightPos - CENorm) - IN.E;
                float2 dirBSide = (_LightPos + CENorm) - IN.E;
                float2 dirA = IN.A - IN.E;
                float2 dirB = IN.B - IN.E;
                float fullAngle = angleBetween(dirA, dirB);
                float angleA = clamp(angleBetween(dirA, IN.A) / fullAngle, 0, 1);
                float angleB = clamp(angleBetween(dirA, IN.B) / fullAngle, 0, 1);
                float occlusion = angleB - angleA;                
                return 1;
            }

            ENDCG
        }
    }
}
