Shader "Custom/IceOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.3, 0.8, 1.0, 1.0)
        _OutlineWidth ("Outline Width", Range(0.0, 0.1)) = 0.03
        _GlowWidth ("Glow Width", Range(0.0, 0.15)) = 0.06
        _GlowIntensity ("Glow Intensity", Range(0.0, 3.0)) = 1.5
        _PulseSpeed ("Pulse Speed", Range(0.0, 5.0)) = 2.0
        _PulseMin ("Pulse Min Alpha", Range(0.0, 1.0)) = 0.5
        _Progress ("Progress", Range(0.0, 1.0)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Cull Off Lighting Off ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            float _GlowWidth;
            float _GlowIntensity;
            float _PulseSpeed;
            float _PulseMin;
            float _Progress;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            // 특정 거리에서 최대 알파 샘플링
            float SampleMaxAlpha(float2 uv, float width)
            {
                float maxA = 0.0;
                float2 offsets[8];
                offsets[0] = float2( width,  0);
                offsets[1] = float2(-width,  0);
                offsets[2] = float2( 0,  width);
                offsets[3] = float2( 0, -width);
                offsets[4] = float2( width,  width) * 0.707;
                offsets[5] = float2(-width,  width) * 0.707;
                offsets[6] = float2( width, -width) * 0.707;
                offsets[7] = float2(-width, -width) * 0.707;
                for (int i = 0; i < 8; i++)
                    maxA = max(maxA, tex2D(_MainTex, uv + offsets[i]).a);
                return maxA;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                fixed4 col = tex2D(_MainTex, uv);

                // 깜빡임
                float pulse = _PulseMin + (1.0 - _PulseMin) * (0.5 + 0.5 * sin(_Time.y * _PulseSpeed));

                // 외곽선 (얇고 선명한 테두리)
                float outerAlpha  = SampleMaxAlpha(uv, _OutlineWidth);
                float outline = outerAlpha * (1.0 - col.a);

                // 글로우 (넓고 흐릿한 빛)
                float glowAlpha1 = SampleMaxAlpha(uv, _GlowWidth * 0.5);
                float glowAlpha2 = SampleMaxAlpha(uv, _GlowWidth);
                float glow = (glowAlpha1 + glowAlpha2) * 0.5 * (1.0 - col.a);

                // 합산
                float totalOutline = saturate(outline * 1.5 + glow * 0.6);
                totalOutline *= pulse * _Progress;

                // 원본 스프라이트
                fixed4 result = col;
                result.rgb = col.rgb * col.a + _OutlineColor.rgb * totalOutline * _GlowIntensity;
                result.a = saturate(col.a + totalOutline);

                return result;
            }
        ENDCG
        }
    }
}
