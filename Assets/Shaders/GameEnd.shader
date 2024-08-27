Shader "Unlit/GameEnd"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CurAnimSecond("Current second of animation",float) = 0
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
                //float4 pos : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _CurAnimSecond;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            

            fixed4 frag (v2f i) : SV_Target
            {
                float2 coord = float2(i.uv.x * _MainTex_TexelSize.z * _MainTex_TexelSize.x, i.uv.y * _MainTex_TexelSize.w * _MainTex_TexelSize.x);
                float anim_time = (_CurAnimSecond) * _MainTex_TexelSize.x * _MainTex_TexelSize.w;

                fixed4 color_yellow = fixed4(1.0, 1.0, 0.0, 1.0);
                fixed4 alpha = fixed4(0.0, 0.0, 0.0, 0.0);

                float distance = length(coord - float2(0.5, 0.5 * _MainTex_TexelSize.w * _MainTex_TexelSize.x));
                float inCircle = step(distance, anim_time - 0.01);
                float outCircle = step(distance, anim_time);

                fixed4 col = tex2D(_MainTex, i.uv);
                col = lerp(col, col * color_yellow, 1.0 - inCircle);
                col = lerp(col, alpha, 1.0 - outCircle);

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
