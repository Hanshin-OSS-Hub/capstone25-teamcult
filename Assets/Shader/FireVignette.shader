Shader "Custom/UniversalFireVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Seamless Noise Texture", 2D) = "white" {}

        _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)

        _Radius ("Radius / Height", Float) = 0.5
        _Softness ("Softness", Range(0.0, 1.0)) = 0.3

        _ScrollSpeed ("Scroll Speed (X, Y)", Vector) = (0.1, 1.2, 0, 0)
        _DistortPower ("Distortion Power", Range(0.0, 2.0)) = 0.1
        _Progress ("Fill Progress", Range(0.0, 1.0)) = 1.0
        _EffectType ("Effect Type (0~5)", Float) = 0

        // 🔥 Fire
        _FireCenterY ("Fire Center Y", Float) = 1.05
        _FireSpread  ("Fire Spread", Range(0.0, 0.5)) = 0.5

        // ⚡ Lightning
        _LightningFlash  ("Lightning Flash",  Range(0.0, 1.0)) = 0.0
        _LightningStrike ("Lightning Strike", Range(0.0, 1.0)) = 0.0
        _BoomFlash ("Boom Flash", Range(0.0, 1.0)) = 0.0
        _EdgeCurrent ("Edge Current", Range(0.0, 2.0)) = 1.0

        // ✨ Holy
        _HolyBreath ("Holy Breath", Range(0.0, 1.0)) = 1.0
        _HolyFlash  ("Holy Flash",  Range(0.0, 1.0)) = 0.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float4 color : COLOR; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; fixed4 color : COLOR; float2 texcoord : TEXCOORD0; };

            sampler2D _MainTex, _NoiseTex;
            fixed4 _CoreColor, _EdgeColor;
            float _Radius, _Softness, _DistortPower, _Progress, _EffectType;
            float2 _ScrollSpeed;

            float _FireCenterY, _FireSpread;
            float _LightningFlash, _LightningStrike, _BoomFlash, _EdgeCurrent;
            float _HolyBreath, _HolyFlash;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            float Hash11(float p) { p = frac(p * 0.1031); p *= p + 33.33; p *= p + p; return frac(p); }

            // ======== ⚡ Lightning helpers ========
            float LightningFlame(float2 uv, float t)
            {
                float totalMask = 0.0;
                {
                    float sn1 = tex2D(_NoiseTex, float2(uv.x * 2.5, uv.y * 1.5 + t * 6.0)).r;
                    float sn2 = tex2D(_NoiseTex, float2(uv.x * 1.5, uv.y * 2.5 - t * 5.0)).r;
                    float elec = abs(sn1 - sn2);
                    float widthMask = smoothstep(0.15, 0.0, uv.x);
                    float spark = smoothstep(0.04, 0.01, elec) * step(0.35, sn1);
                    totalMask += spark * widthMask * 3.0;
                }
                {
                    float fromRight = 1.0 - uv.x;
                    float sn1 = tex2D(_NoiseTex, float2(fromRight * 2.5, uv.y * 1.5 - t * 5.5)).r;
                    float sn2 = tex2D(_NoiseTex, float2(fromRight * 1.5, uv.y * 2.5 + t * 6.5)).r;
                    float elec = abs(sn1 - sn2);
                    float widthMask = smoothstep(0.15, 0.0, fromRight);
                    float spark = smoothstep(0.04, 0.01, elec) * step(0.35, sn1);
                    totalMask += spark * widthMask * 3.0;
                }
                return saturate(totalMask);
            }

            float SingleBolt(float2 uv, float t, float strike, float bx)
            {
                float seed = floor(t * 0.5);
                bx += sin(uv.y * 15.0 + seed * 3.1) * 0.04;
                bx += sin(uv.y * 40.0 + seed * 7.3) * 0.02;
                float stepS = floor(uv.y * 20.0);
                bx += (frac(sin(stepS + seed * 127.1) * 43758.5) - 0.5) * 0.05;
                float reveal = smoothstep(1.0 - strike - 0.10, 1.0 - strike + 0.10, uv.y);
                float d = abs(uv.x - bx);
                float core = smoothstep(0.002, 0.0, d);
                float glow = smoothstep(0.02, 0.0, d) * 0.8;
                return saturate(core + glow) * reveal;
            }

            // ======== ✨ Holy: 빛기둥 ========
            float GodRay(float2 uv, float centerX, float width, float t)
            {
                float wander = sin(t * 0.15 + centerX * 6.28) * 0.015;
                float dx = abs(uv.x - centerX - wander);
                float rayMask = smoothstep(width, 0.0, dx);
                float fadeY = smoothstep(0.0, 0.25, uv.y) * smoothstep(1.0, 0.35, uv.y);
                return rayMask * fadeY * 0.2;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float t = _Time.y;
                float2 scroll = t * _ScrollSpeed;
                float finalNoise = tex2D(_NoiseTex, (uv + scroll) * 1.5).r * tex2D(_NoiseTex, (uv - scroll * 0.5) * 2.0).r;

                float mask = 0.0;
                float pattern = pow(finalNoise, 1.5);

                if (_EffectType < 0.5) // 🔥 Fire
                {
                    float2 fs = t * float2(_ScrollSpeed.x, -_ScrollSpeed.y);
                    float fireNoise = (tex2D(_NoiseTex, uv * float2(2.5, 1.5) + fs).r + tex2D(_NoiseTex, uv * float2(1.5, 2.5) + fs * 1.3).r) * 0.5;
                    float fireShape = (sqrt(pow(uv.x - 0.5, 2) / 0.4 + pow(uv.y - _FireCenterY, 2) / 0.6)) - 1.0 + (fireNoise - 0.5) * _DistortPower;
                    mask = smoothstep(0.0, _Softness, fireShape) * smoothstep(_FireSpread + (fireNoise - 0.5) * 0.05, _FireSpread + (fireNoise - 0.5) * 0.05 - 0.06, abs(uv.x - 0.5));
                    pattern = smoothstep(0.0, 0.5, fireShape);
                }
                else if (_EffectType < 1.5) // ❄ Ice (얇은 외각 서리)
                {
                    float n1 = tex2D(_NoiseTex, uv * 3.0 + t * 0.008).r;
                    float n2 = tex2D(_NoiseTex, uv * 1.8 - t * 0.005).r;
                    float iceNoise = (n1 + n2) * 0.5;
                    float dist = max(abs(uv.x - 0.5), abs(uv.y - 0.5)) * 2.0;
                    float frost = dist + (iceNoise - 0.5) * _DistortPower;
                    mask    = smoothstep(_Radius, _Radius + _Softness, frost) * 1.3;
                    pattern = smoothstep(_Radius - 0.04, _Radius + _Softness + 0.04, frost);
                }
                else if (_EffectType < 2.5) // 🧪 Poison
                {
                    float st = t * 0.15;
                    float fogNoise = (tex2D(_NoiseTex, float2(uv.x * 2.0 + st * 0.5, uv.y * 1.5 + st * 0.2)).r + tex2D(_NoiseTex, float2(uv.x * 1.5 - st * 0.3, uv.y * 2.0 - st * 0.1)).r) * 0.5;
                    float cornerDist = min(distance(uv, float2(-0.1, -0.1)), distance(uv, float2(1.1, -0.1)));
                    float fogMask = 1.0 - smoothstep(_Radius, _Radius + _Softness, cornerDist - (fogNoise * _DistortPower * 1.5));
                    mask = saturate(fogMask * _Progress) * 0.4;
                    pattern = fogMask + fogNoise * 0.2;
                }
                else if (_EffectType < 3.5) // ⚡ Lightning
                {
                    float glitch = step(0.4, Hash11(t * 15.0));
                    float edge = LightningFlame(uv, t) * _EdgeCurrent * (0.8 + _LightningFlash * 2.0 * glitch);
                    float bolt = (SingleBolt(uv, t, _LightningStrike, 0.12) + SingleBolt(uv, t, _LightningStrike, 0.88)) * saturate(_LightningStrike * 5.0);
                    float doorHeightMask = smoothstep(0.1, 0.25, abs(uv.y - 0.5));
                    mask = saturate((edge * doorHeightMask + bolt * 1.5 + _BoomFlash));
                    pattern = saturate((edge * 1.5 * doorHeightMask + bolt * 2.0 + _BoomFlash));
                }
                else if (_EffectType < 4.5) // ✨ Holy (부드럽게 조정)
                {
                    float holyPulse = _HolyBreath * (0.5 + 0.2 * sin(t * 0.6));
                    float vignette = smoothstep(0.40, 0.70, distance(uv, float2(0.5, 0.5))) * 0.2 * holyPulse;
                    float rays = (GodRay(uv, 0.08, 0.05, t) + GodRay(uv, 0.92, 0.05, t + 1.5)) * holyPulse * 1.5;
                    float particles = smoothstep(0.85, 0.98, tex2D(_NoiseTex, float2(uv.x * 4.0, uv.y * 2.0 - t * 0.1)).r) * 0.15;
                    mask = saturate(vignette + rays + particles) * _Progress;
                    pattern = saturate(rays * 1.0 + vignette * 0.3 + particles);
                }
                else // 🌿 Grass (덩굴 침식 하단 + 반딧불이)
                {
                    // ── 덩굴: 하단 양쪽 모서리에서만 ──
                    float ex = min(uv.x, 1.0 - uv.x); // 좌우 가장자리 거리
                    float ey = uv.y;                   // 아래에서의 거리 (0=하단, 1=상단)
                    float cornerMask = smoothstep(0.32, 0.0, ex) * smoothstep(0.35, 0.0, ey);

                    float vn1 = tex2D(_NoiseTex, uv * 4.0 + t * 0.04).r;
                    float vn2 = tex2D(_NoiseTex, uv * 2.5 - t * 0.03).r;
                    float vineNoise = (vn1 + vn2) * 0.5;

                    float vine     = cornerMask * (0.7 + vineNoise * 0.5);
                    float vineEdge = smoothstep(0.30, 0.10, ex) * smoothstep(0.0, 0.12, ex)
                                   * smoothstep(0.30, 0.05, ey) * smoothstep(0.0, 0.08, ey)
                                   * vineNoise * 0.9;

                    // ── 반딧불이 6개 (화면 전체) ──
                    float firefly = 0.0;
                    for (int gi = 0; gi < 6; gi++) {
                        float seed = float(gi) * 127.1;
                        float2 pos = frac(float2(frac(sin(seed) * 43758.5), frac(cos(seed * 1.3) * 31415.9))
                                   + float2(sin(t * 0.35 + seed) * 0.10, -frac(t * 0.06 + seed * 0.17)));
                        float d = distance(uv, pos);
                        float blink = 0.5 + 0.5 * sin(t * 2.2 + seed * 4.0);
                        firefly += smoothstep(0.018, 0.0, d) * blink * 2.0;
                        firefly += smoothstep(0.05,  0.0, d) * blink * 0.4;
                    }

                    // ── 포자 ──
                    float spores = smoothstep(0.91, 0.98, tex2D(_NoiseTex,
                        float2(uv.x * 7.0, uv.y * 5.0 - t * 0.20)).r) * 0.55;

                    mask    = saturate((vine * 0.5 + vineEdge * 0.6 + firefly + spores * 0.35) * _Progress);
                    pattern = saturate(vineEdge * 1.0 + firefly * 0.6 + vine * 0.4 + spores * 0.3);
                }

                float introMask = (_EffectType >= 1.5 && _EffectType < 2.5) ? 1.0 : smoothstep(0.0, 1.0, _Progress);
                fixed4 fc = lerp(_EdgeColor, _CoreColor, pattern);
                fixed4 sp = tex2D(_MainTex, uv);
                fc.a *= mask * introMask * sp.a * IN.color.a;
                return fc;
            }
        ENDCG
        }
    }
}
