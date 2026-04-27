Shader "Custom/LightningChain"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _CoreColor  ("Core Color", Color) = (1, 1, 1, 1)
        _EdgeColor  ("Edge Color", Color) = (0.0, 0.8, 1, 1)
        _Alpha      ("Alpha", Range(0,1)) = 1.0
        _Speed      ("Speed", Float) = 15.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off Lighting Off ZWrite Off ZTest Always
        Blend One One

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f      { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

            sampler2D _MainTex;
            fixed4 _CoreColor, _EdgeColor;
            float  _Alpha, _Speed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color    = IN.color;
                return OUT;
            }

            float Hash11(float p) { p = frac(p * 0.1031); p *= p + 33.33; p *= p + p; return frac(p); }

            float hash2(float2 p)
            {
                p = frac(p * float2(234.34, 435.345));
                p += dot(p, p + 34.23);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash2(i), hash2(i + float2(1,0)), u.x),
                    lerp(hash2(i + float2(0,1)), hash2(i + float2(1,1)), u.x),
                    u.y);
            }

            // FireVignette SingleBolt 그대로 + 노이즈 추가
            float SingleBolt(float2 uv, float t, float bx)
            {
                float seed = floor(t * 0.5);
                bx += sin(uv.y * 15.0 + seed * 3.1) * 0.04;
                bx += sin(uv.y * 40.0 + seed * 7.3) * 0.02;
                float stepS = floor(uv.y * 20.0);
                bx += (frac(sin(stepS + seed * 127.1) * 43758.5) - 0.5) * 0.05;
                bx += (noise(float2(uv.y * 6.0, t * 2.0 + seed)) - 0.5) * 0.07;

                float d    = abs(uv.x - bx);
                float core = smoothstep(0.003, 0.0, d);
                float glow = smoothstep(0.025, 0.0, d) * 0.8;
                float aura = smoothstep(0.07,  0.0, d) * 0.15;
                return saturate(core + glow + aura);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float  t  = _Time.y * (_Speed / 15.0);

                // FireVignette 번개와 동일한 flash 로직
                float flashCycle = frac(t * 0.9 + 0.1);
                float flashOn    = step(0.85, flashCycle)
                                 + step(0.60, flashCycle) * step(flashCycle, 0.63)
                                 + 0.5;
                flashOn = saturate(flashOn);

                float frame     = floor(t * 12.0);
                float crossBolt = 0.0;

                // 볼트 3개
                for (int bi = 0; bi < 3; bi++)
                {
                    float bSeed = float(bi) * 91.3 + frame * 37.1;
                    float bx    = 0.2 + frac(sin(bSeed * 2.3) * 43758.5) * 0.6;

                    float bolt  = SingleBolt(uv, t, bx);

                    // 가지치기
                    float branchSeed  = bSeed + floor(uv.y * 20.0) * 7.3;
                    float branchProb  = step(0.72, frac(sin(branchSeed) * 43758.5));
                    float branchX     = bx + (frac(sin(branchSeed * 2.1) * 43758.5) - 0.5) * 0.12;
                    float branch      = SingleBolt(uv, t, branchX) * branchProb * 0.5
                                      * smoothstep(1.0, 0.4, uv.y);

                    float blink = 0.6 + 0.4 * sin(t * _Speed * 0.05 + float(bi) * 2.3);
                    crossBolt += (bolt + branch) * blink * flashOn;
                }

                crossBolt = saturate(crossBolt);

                // 번개 없는 픽셀 완전 제거
                if (crossBolt < 0.01) discard;

                // 위아래 페이드
                float fade = smoothstep(0.0, 0.06, uv.y) * smoothstep(1.0, 0.94, uv.y);
                crossBolt *= fade;

                // FireVignette 와 동일한 색상 합성
                fixed4 col = lerp(_EdgeColor, _CoreColor, saturate(crossBolt * 1.8));
                col.a = crossBolt * _Alpha * IN.color.a;

                return col;
            }
        ENDCG
        }
    }
}
