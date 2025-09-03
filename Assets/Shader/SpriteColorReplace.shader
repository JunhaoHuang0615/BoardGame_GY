Shader "Custom/SpriteReplaceColor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _ReplaceFrom ("Replace From Color", Color) = (0,0,1,1) // 默认蓝色
        _ReplaceTo ("Replace To Color", Color) = (1,0,0,1) // 默认红色
        _Tolerance ("Color Tolerance", Range(0,1)) = 0.2
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
                float4 color : COLOR; // SpriteRenderer 的 color，保留但可不用
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _ReplaceFrom;
            fixed4 _ReplaceTo;
            float _Tolerance;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.texcoord);

                // 判断与目标颜色的距离
                float dist = distance(texCol.rgb, _ReplaceFrom.rgb);

                if (dist < _Tolerance)
                {
                    // 替换为新颜色，保留透明度
                    return fixed4(_ReplaceTo.rgb, texCol.a * _ReplaceTo.a);
                }
                else
                {
                    // 保持原始像素
                    return texCol;
                }
            }
            ENDCG
        }
    }
}
