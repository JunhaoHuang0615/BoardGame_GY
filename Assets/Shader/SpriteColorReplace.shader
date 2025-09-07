Shader "Custom/SpriteReplaceColor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ReplaceFrom ("Replace From Color", Color) = (0,0,1,1)
        _ReplaceTo ("Replace To Color", Color) = (1,0,0,1)
        _Tolerance ("Color Tolerance", Range(0,1)) = 0.2
        _Color ("Tint Color", Color) = (1,1,1,1) // 新增
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR; // SpriteRenderer 的 color
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ReplaceFrom;
            fixed4 _ReplaceTo;
            float _Tolerance;
            fixed4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                o.color = v.color * _Color; // 同时带上 SpriteRenderer.color 和材质 _Color
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.texcoord);
                float dist = distance(texCol.rgb, _ReplaceFrom.rgb);

                fixed4 result;
                if (dist < _Tolerance)
                {
                    // 替换颜色
                    result = fixed4(_ReplaceTo.rgb, texCol.a);
                }
                else
                {
                    // 原始颜色
                    result = texCol;
                }

                // 应用 SpriteRenderer.color 和材质 _Color
                return result * i.color;
            }
            ENDCG
        }
    }
}