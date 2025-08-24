Shader "Unlit/LoadImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CurFillPercent("Current percent of filling", Range(0.0,1.0)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenPos : TEXCOORD1; 
                UNITY_FOG_COORDS(1)
                
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color = (0.1,0.1,0.1,1);
            float4 _MainTex_TexelSize;
            float _CurFillPercent;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
			{
			    // Sample the texture
			    fixed4 col = tex2D(_MainTex, i.uv);

			    // Convert to grayscale
			    float grey = dot(col.rgb, float3(0.299, 0.567, 0.114));
			    fixed4 grayCol = fixed4(grey, grey, grey, col.a);

			    // Calculate screen-space coordinates (normalized) for bottom-left corner start
			    float2 screenCoord = i.screenPos.xy / i.screenPos.w;

			    // Calculate distance based on screen-space coordinates
			    float distance = (screenCoord.x + screenCoord.y) * 0.5;

			    // Calculate a smooth step transition
			    float blend = smoothstep(0.0, 1, 0.5 - (distance - (_CurFillPercent)) / _CurFillPercent);

			    // Interpolate between dark and gray based on the smooth transition
			    col = lerp(fixed4(0.1, 0.1, 0.1, 1), grayCol, blend);

			    // Apply fog
			    UNITY_APPLY_FOG(i.fogCoord, col);

			    return col;
			}

            ENDCG
        }
    }
}
