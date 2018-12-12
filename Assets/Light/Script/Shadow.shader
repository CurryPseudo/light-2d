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
            // [-180, 180]
            float dirAngle(float2 v) {
                float angle = atan2(v.y, v.x);
                angle = degrees(angle);
                return angle;
            }
            // [-360, 360] norm to [-180, 180]
            float normAngle(float angle) {
                angle = angle - step(180, angle) * 360;
                angle = angle + step(angle, -180) * 360;
                return angle;
            }
            float2 _LightPos;
            float _LightVolumeRadius;
            float4 frag(v2f IN) : COLOR
            {
                float2 CE = IN.E - _LightPos;                 
                float2 CENorm = normalize(float2(-CE.y, CE.x)) * _LightVolumeRadius;
                float angleF = dirAngle((_LightPos - CENorm) - IN.E);
                float2 angleG = dirAngle((_LightPos + CENorm) - IN.E);
                float2 angleA = dirAngle(IN.A - IN.E);
                float2 angleB = dirAngle(IN.B - IN.E);
                float full = normAngle(angleF - angleG);
                float ABiggerThanB = step(0, normAngle(angleA - angleB));
                float angleCW = ABiggerThanB * (angleA - angleB) + angleB;
                float angleCCW = ABiggerThanB * (angleB - angleA) + angleA;
                //float crossG = step(0, normAngle(angleG - angleCCW)) * step(0, normAngle(angleCW - angleG));
                float crossG = 1;
                float sign = crossG * 2 - 1;
                float side = angleF + (angleG - angleF) * crossG;
                float valueCW = saturate(normAngle(sign * (angleCW - side)) / full);
                float valueCCW = saturate(normAngle(sign * (angleCCW - side)) / full);
                float occlusion = abs(valueCW - valueCCW);
                return occlusion;
            }

            ENDCG
        }
    }
}
