Shader "Custom/SpriteTintOverlay"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags {"Queue"="Overlay" "RenderType"="Overlay"}
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // 使用顶点颜色（SpriteRenderer.color 会自动传入）
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR; // 传递颜色
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color; // 传递颜色
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texCol = tex2D(_MainTex, i.uv);

                // 使用 SpriteRenderer.color 的值 (传递过来的颜色)
                if (i.color.r == 1.0 && i.color.g == 1.0 && i.color.b == 1.0 && i.color.a == 1.0)
                {
                    return texCol; // 显示原始贴图
                }
                else
                {
                    // 通过线性插值而不是乘法来混合颜色
                    fixed overlayAlpha = i.color.a; // 使用传递的 alpha 值
                    // 这里的 lerp 是一个平滑过渡，避免颜色变暗
                    fixed4 result = lerp(texCol, i.color, overlayAlpha); 
                    return result;
                }
            }
            ENDCG
        }
    }
}