Shader "Custom/WavingCircle"
{
    Properties {
        _FG_Color ("Foreground Color", Color) = (0, 0, 0, 1)
        _BG_Color ("Background Color", Color) = (1, 0, 0, 1)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag2

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv: TEXCOORD;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv: TEXCOORD;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // float dist = distance(fixed4(0,0,0,0), i.vertex);
                float dist = distance(fixed2(0.5,0.5), i.uv);
                float radius = 0.2;
                // if(radius < dist )
                if(radius < dist && dist < radius + 0.2)
                {
				    return fixed4(110/255.0, 87/255.0, 139/255.0, 1);
                } else {
                    return fixed4(1,1,1,1);
			    }
            }

            fixed4 _FG_Color;
            fixed4 _BG_Color;

            fixed4 frag2 (v2f i) : SV_Target
            {
                float dist = distance(fixed2(0.5,0.5), i.uv);
                float val = abs(sin(dist*6.0-_Time*100));
                if(val > 0.98)
                {
                    return _FG_Color;
                } else {
                    return _BG_Color;
                }
            }

            ENDCG
        }
    }
}
