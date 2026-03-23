Shader "Custom/FireVignette"
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

        // Fire
        _FireCenterY ("Fire Center Y", Float) = 1.05
        _FireSpread  ("Fire Spread", Range(0.0, 0.5)) = 0.5

        // Lightning
        _LightningFlash  ("Lightning Flash",  Range(0.0, 1.0)) = 0.0
        _LightningStrike ("Lightning Strike", Range(0.0, 1.0)) = 0.0
        _BoomFlash ("Boom Flash", Range(0.0, 1.0)) = 0.0
        _EdgeCurrent ("Edge Current", Range(0.0, 2.0)) = 1.0

        // Holy
        _HolyBreath ("Holy Breath", Range(0.0, 1.0)) = 1.0
        _HolyFlash  ("Holy Flash",  Range(0.0, 1.0)) = 0.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        // 캐릭터 위치 (UV 공간 0~1)
        _PlayerPos ("Player Position", Vector) = (0.5, 0.5, 0, 0)
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
            float2 _PlayerPos;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            float Hash11(float p) { p = frac(p * 0.1031); p *= p + 33.33; p *= p + p; return frac(p); }

            float LightningFlame(float2 uv, float t)
            {
                float totalMask = 0.0;
                {
                    float sn1 = tex2D(_NoiseTex, float2(uv.x * 2.5, uv.y * 1.5 + t * 6.0)).r;
                    float sn2 = tex2D(_NoiseTex, float2(uv.x * 1.5, uv.y * 2.5 - t * 5.0)).r;
                    totalMask += smoothstep(0.04, 0.01, abs(sn1 - sn2)) * step(0.35, sn1) * smoothstep(0.15, 0.0, uv.x) * 3.0;
                }
                {
                    float fromRight = 1.0 - uv.x;
                    float sn1 = tex2D(_NoiseTex, float2(fromRight * 2.5, uv.y * 1.5 - t * 5.5)).r;
                    float sn2 = tex2D(_NoiseTex, float2(fromRight * 1.5, uv.y * 2.5 + t * 6.5)).r;
                    totalMask += smoothstep(0.04, 0.01, abs(sn1 - sn2)) * step(0.35, sn1) * smoothstep(0.15, 0.0, fromRight) * 3.0;
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
                return saturate(smoothstep(0.002, 0.0, d) + smoothstep(0.02, 0.0, d) * 0.8) * reveal;
            }

            float GodRay(float2 uv, float centerX, float width, float t)
            {
                float wander = sin(t * 0.15 + centerX * 6.28) * 0.015;
                float rayMask = smoothstep(width, 0.0, abs(uv.x - centerX - wander));
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

                if (_EffectType < 0.5) // Fire
                {
                    float2 fs = t * float2(_ScrollSpeed.x, -_ScrollSpeed.y);
                    float fireNoise = (tex2D(_NoiseTex, uv * float2(2.5, 1.5) + fs).r + tex2D(_NoiseTex, uv * float2(1.5, 2.5) + fs * 1.3).r) * 0.5;
                    float fireShape = (sqrt(pow(uv.x - 0.5, 2) / 0.4 + pow(uv.y - _FireCenterY, 2) / 0.6)) - 1.0 + (fireNoise - 0.5) * _DistortPower;
                    mask = smoothstep(0.0, _Softness, fireShape) * smoothstep(_FireSpread + (fireNoise - 0.5) * 0.05, _FireSpread + (fireNoise - 0.5) * 0.05 - 0.06, abs(uv.x - 0.5));
                    pattern = smoothstep(0.0, 0.5, fireShape);
                }
                else if (_EffectType < 1.5) // Ice
                {
                    float n1 = tex2D(_NoiseTex, uv * 2.5 + float2(t * 0.012, t * 0.008)).r;
                    float n2 = tex2D(_NoiseTex, uv * 5.0 - float2(t * 0.009, t * 0.015)).r;
                    float n3 = tex2D(_NoiseTex, uv * 9.0 + float2(t * 0.006, -t * 0.010)).r;

                    float frostNoise = n1 * 0.5 + n2 * 0.35 + n3 * 0.15;

                    float fromLeft   = uv.x - _PlayerPos.x + 0.5;
                    float fromRight  = _PlayerPos.x + 0.5 - uv.x;
                    float fromBottom = uv.y - _PlayerPos.y + 0.5;
                    float fromTop    = (_PlayerPos.y + 0.5 - uv.y) * 1.5;
                    float edgeDist   = min(min(fromLeft, fromRight), min(fromBottom, fromTop));

                    float breathe = sin(t * 0.4) * 0.018 + cos(t * 0.27) * 0.010;
                    float grow    = sin(t * 0.2 + uv.x * 3.14) * 0.012
                                  + cos(t * 0.17 + uv.y * 3.14) * 0.010;

                    float frostDepth = _Radius * 0.26;

                    float progress = 1.0 - pow(1.0 - _Progress, 2.5);

                    // 노이즈 기반 불규칙 번짐 — 위치마다 다른 속도로 자라남
                    float localSpeed = frostNoise * 0.4 + n3 * 0.3; // 0~0.7 불규칙 오프셋
                    float localProgress = saturate((progress - (1.0 - edgeDist) * 0.0 ) * 1.0);
                    // 각 픽셀이 가장자리에서 고유한 타이밍에 나타남
                    float appear = saturate((progress + localSpeed * progress) * 2.0 - edgeDist * 3.5);

                    // 크랙 번지는 앞부분 번쩍임
                    float crackEdge = abs(appear - 0.5);
                    float crackFront = smoothstep(0.25, 0.0, crackEdge)
                                     * (1.0 - progress) * 1.5;

                    float frostBoundary = edgeDist
                                        - frostNoise * frostDepth * 2.35
                                        + frostDepth * 0.6
                                        - breathe
                                        - grow;

                    float frostMask = (1.0 - smoothstep(-0.015, 0.05, frostBoundary)) * appear;

                    float sparkle = sin(t * 1.5 + n1 * 6.28) * 0.5 + 0.5;
                    float crystalDetail = smoothstep(0.6, 1.0, n2 * n3 * 4.0)
                                        * smoothstep(0.08, 0.0, abs(frostBoundary))
                                        * (0.5 + sparkle * 0.2);

                    float shimmer = smoothstep(0.02, 0.0, abs(frostBoundary + 0.01))
                                  * (0.5 + 0.5 * sin(t * 2.0 + uv.x * 8.0 + uv.y * 5.0))
                                  * 0.2;

                    float innerFrost = smoothstep(0.0, -0.07, frostBoundary) * 0.55 * appear;

                    mask    = saturate(frostMask + crystalDetail * 0.4 * appear + shimmer * appear + crackFront * 0.5);
                    pattern = saturate(frostMask * 0.7 + crystalDetail + innerFrost + shimmer * 0.5 + crackFront);
                }
                else if (_EffectType < 2.5) // Poison
                {
                    float st = t * 0.08;
                    float pn1 = tex2D(_NoiseTex, uv * 3.0 + float2(st * 0.4, st * 0.3)).r;
                    float pn2 = tex2D(_NoiseTex, uv * 1.8 - float2(st * 0.3, st * 0.2)).r;
                    float poisonNoise = (pn1 + pn2) * 0.5;

                    float dist   = max(abs(uv.x - 0.5), abs(uv.y - 0.5)) * 2.0;
                    float sludge = dist + (poisonNoise - 0.5) * _DistortPower * 1.2;
                    float slimeMask = smoothstep(0.78, 0.78 + _Softness, sludge) * 1.1;

                    float2 crystalUV    = uv * float2(6.0, 5.0) + float2(st * 0.1, st * 0.05);
                    float poisonCrystal = smoothstep(0.82, 1.0, tex2D(_NoiseTex, crystalUV).r)
                                       * smoothstep(0.12, 0.0, min(min(uv.x, 1.0-uv.x), min(uv.y, 1.0-uv.y)))
                                       * 0.8;

                    float2 bubbleUV = float2(uv.x * 7.0, frac(uv.y * 6.0 - t * 0.35 + poisonNoise * 0.4));
                    float bubble    = smoothstep(0.92, 1.0, tex2D(_NoiseTex, bubbleUV).r)
                                    * smoothstep(0.4, 0.0, uv.y)
                                    * (0.5 + 0.5 * sin(t * 2.5 + uv.x * 10.0)) * 0.7;

                    float2 dripUV = float2(uv.x * 5.0, frac(uv.y * 2.5 + t * 0.25 + pn1 * 0.3));
                    float drip    = smoothstep(0.87, 1.0, tex2D(_NoiseTex, dripUV).r)
                                  * smoothstep(0.12, 0.0, min(uv.x, 1.0 - uv.x))
                                  * 0.6;

                    mask    = saturate((slimeMask + poisonCrystal + bubble * 0.5 + drip) * _Progress);
                    pattern = smoothstep(_Radius - 0.04, _Radius + _Softness + 0.04, sludge)
                            + poisonCrystal * 0.5 + bubble * 0.3;
                }
                else if (_EffectType < 3.5) // Lightning
                {
                    float glitch = step(0.4, Hash11(t * 15.0));

                    float flashCycle = frac(t * 0.9 + 0.1);
                    float flashOn    = step(0.85, flashCycle)
                                    + step(0.60, flashCycle) * step(flashCycle, 0.63)
                                    + _LightningFlash;
                    flashOn = saturate(flashOn);

                    float frame = floor(t * 12.0);
                    float crossBolt = 0.0;

                    for (int bi = 0; bi < 6; bi++)
                    {
                        float bSeed = float(bi) * 91.3 + frame * 37.1;
                        float edgeSel = frac(sin(bSeed * 1.1) * 43758.5);
                        float2 startPos;
                        float2 inDir;

                        if (edgeSel < 0.25) {
                            startPos = float2(frac(sin(bSeed * 2.3) * 43758.5), 0.0);
                            inDir    = float2((frac(sin(bSeed*3.1)*43758.5)-0.5)*0.4, 1.0);
                        } else if (edgeSel < 0.5) {
                            startPos = float2(frac(sin(bSeed * 2.3) * 43758.5), 1.0);
                            inDir    = float2((frac(sin(bSeed*3.1)*43758.5)-0.5)*0.4, -1.0);
                        } else if (edgeSel < 0.75) {
                            startPos = float2(0.0, frac(sin(bSeed * 2.3) * 43758.5));
                            inDir    = float2(1.0, (frac(sin(bSeed*3.1)*43758.5)-0.5)*0.4);
                        } else {
                            startPos = float2(1.0, frac(sin(bSeed * 2.3) * 43758.5));
                            inDir    = float2(-1.0, (frac(sin(bSeed*3.1)*43758.5)-0.5)*0.4);
                        }
                        inDir = normalize(inDir);

                        float2 dFromStart = uv - startPos;
                        float  proj = dot(dFromStart, inDir);
                        float  perp = dFromStart.x * inDir.y - dFromStart.y * inDir.x;

                        float seg    = floor(proj * 20.0);
                        float zigzag = (frac(sin(seg + bSeed * 3.1) * 43758.5) - 0.5) * 0.018;
                        float perpDist = abs(perp - zigzag);

                        float bLen   = 0.09 + frac(sin(bSeed * 1.7) * 43758.5) * 0.02;
                        float reveal = step(0.0, proj) * smoothstep(bLen, bLen - 0.05, proj);

                        float core = smoothstep(0.003, 0.0, perpDist) * reveal;
                        float glow = smoothstep(0.02,  0.0, perpDist) * reveal * 0.5;
                        float aura = smoothstep(0.05,  0.0, perpDist) * reveal * 0.12;

                        float branchSeed  = bSeed + seg * 7.3;
                        float branchProb  = step(0.75, frac(sin(branchSeed) * 43758.5));
                        float branchAngle = atan2(inDir.y, inDir.x) + (frac(sin(branchSeed*2.1)*43758.5)-0.5)*1.0;
                        float2 bDir2  = float2(cos(branchAngle), sin(branchAngle));
                        float  bProj2 = dot(dFromStart - inDir * seg/20.0, bDir2);
                        float  bPerp2 = abs((dFromStart - inDir*seg/20.0).x*bDir2.y - (dFromStart-inDir*seg/20.0).y*bDir2.x);
                        float  branch = smoothstep(0.004, 0.0, bPerp2)
                                      * step(0.0, bProj2) * smoothstep(0.08, 0.0, bProj2)
                                      * branchProb * 0.5 * reveal;

                        float blink = 0.6 + 0.4 * sin(t * 15.0 + float(bi) * 2.3);
                        crossBolt += (core + glow + aura + branch) * blink * flashOn;
                    }

                    float edge = LightningFlame(uv, t) * _EdgeCurrent * 0.4;
                    float boom = _BoomFlash;
                    float heightMask = smoothstep(0.05, 0.2, abs(uv.y - 0.5));
                    mask    = saturate(crossBolt * 1.3 + edge * heightMask * 0.6 + boom * 0.8);
                    pattern = saturate(crossBolt * 1.8 + edge * heightMask + boom);
                }
                else if (_EffectType < 4.5) // Holy
                {
                    float holyPulse = _HolyBreath * (0.5 + 0.2 * sin(t * 0.6));
                    float vignette = smoothstep(0.40, 0.70, distance(uv, float2(0.5, 0.5))) * 0.2 * holyPulse;
                    float rays = (GodRay(uv, 0.08, 0.05, t) + GodRay(uv, 0.92, 0.05, t + 1.5)) * holyPulse * 1.5;
                    float particles = smoothstep(0.85, 0.98, tex2D(_NoiseTex, float2(uv.x * 4.0, uv.y * 2.0 - t * 0.1)).r) * 0.15;
                    mask = saturate(vignette + rays + particles) * _Progress;
                    pattern = saturate(rays + vignette * 0.3 + particles);
                }
                else // Grass
                {
                    float ex = min(uv.x, 1.0 - uv.x);
                    float ey = uv.y;
                    float cornerMask = smoothstep(0.32, 0.0, ex) * smoothstep(0.35, 0.0, ey);
                    float vn1 = tex2D(_NoiseTex, uv * 4.0 + t * 0.04).r;
                    float vn2 = tex2D(_NoiseTex, uv * 2.5 - t * 0.03).r;
                    float vineNoise = (vn1 + vn2) * 0.5;
                    float vine     = cornerMask * (0.7 + vineNoise * 0.5);
                    float vineEdge = smoothstep(0.30, 0.10, ex) * smoothstep(0.0, 0.12, ex)
                                   * smoothstep(0.30, 0.05, ey) * smoothstep(0.0, 0.08, ey)
                                   * vineNoise * 0.9;

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

                    float spores = smoothstep(0.91, 0.98, tex2D(_NoiseTex,
                        float2(uv.x * 7.0, uv.y * 5.0 - t * 0.20)).r) * 0.55;
                    mask    = saturate((vine * 0.5 + vineEdge * 0.6 + firefly + spores * 0.35) * _Progress);
                    pattern = saturate(vineEdge * 1.0 + firefly * 0.6 + vine * 0.4 + spores * 0.3);
                }

                float introMask = (_EffectType >= 1.5 && _EffectType < 2.5) ? 1.0 : smoothstep(0.0, 1.0, _Progress);

                fixed4 lightningEdge = fixed4(0.0, 0.6, 1.0, 1.0);
                fixed4 lightningCore = fixed4(0.7, 0.95, 1.0, 1.0);
                fixed4 fc = (_EffectType >= 2.5 && _EffectType < 3.5)
                           ? lerp(lightningEdge, lightningCore, pattern)
                           : lerp(_EdgeColor, _CoreColor, pattern);

                fixed4 sp = tex2D(_MainTex, uv);
                fc.a *= mask * introMask * sp.a * IN.color.a;
                return fc;
            }
        ENDCG
        }
    }
}
