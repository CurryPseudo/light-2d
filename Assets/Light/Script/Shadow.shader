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
            // [-180, 180]
            float dirBetweenAngle(float2 v1, float2 v2) {
                return normAngle(dirAngle(v1) - dirAngle(v2));
            }
            float2 _LightPos;
            float _LightVolumeRadius;
            float4 frag(v2f IN) : COLOR
            {
                float2 CE = IN.E - _LightPos;                 
                // CE的法线
                float2 CENorm = normalize(float2(-CE.y, CE.x)) * _LightVolumeRadius;
                float2 dirF = (_LightPos - CENorm) - IN.E;
                float2 dirG = (_LightPos + CENorm) - IN.E;
                float2 dirA = IN.A - IN.E;
                float2 dirB = IN.B - IN.E;
                float full = dirBetweenAngle(dirF, dirG);
                // 若EA在EB顺时针端，为1，否则为0
                float ABiggerThanB = step(0, dirBetweenAngle(dirA, dirB));
                //顺时针端的边
                float2 dirCW = ABiggerThanB * (dirA - dirB) + dirB;
                //偏逆时针端的边
                float2 dirCCW = dirA + dirB - dirCW;
                //若AB跨过EG，为1，否则为0
                float crossG = step(0, dirBetweenAngle(dirG, dirCCW)) * step(0, dirBetweenAngle(dirCW, dirG));
                float sign = crossG * 2 - 1;
                float2 startingEdge = dirF + (dirG - dirF) * crossG;
                float valueCW = saturate(sign * dirBetweenAngle(dirCW, startingEdge) / full);
                float valueCCW = saturate(sign * dirBetweenAngle(dirCCW, startingEdge) / full);
                float occlusion = abs(valueCW - valueCCW);
                return occlusion;
            }
            ENDCG
        }
    }
}
