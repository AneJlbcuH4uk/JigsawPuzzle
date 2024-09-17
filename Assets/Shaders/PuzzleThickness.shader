Shader "Unlit/PuzzleThickness"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MoveAmount ("Move Amount", Range(-0.1, 0.1)) = 0.0
        //_Color ("Tint Color", Color) = (1,1,1,1)
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
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

             struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _MoveAmount;
            //fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.vertex = v.vertex;
                // Move the UV coordinates down
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // Adjust the UV coordinates
                //o.uv.y += _MoveAmount;
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture

                fixed4 col1 = tex2D(_MainTex, i.uv);
                float4 col2 = tex2D(_MainTex, i.uv + float2(0 ,_MoveAmount)); 

                //col2.xyz = 0;
                col2.xyz = float3(0.03,0.03,0.03);

                // Overlay the second texture on top of the first
      
                half4 col = col2 * (1.0 - col1.a) + col1 * col1.a;

                col.a *= i.color.a;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
