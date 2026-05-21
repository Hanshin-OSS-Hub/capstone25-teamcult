Shader "Custom/MagicCircle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _CoreColor ("Core Color", Color) = (0.5, 0.2, 1.0, 1.0)
        _EdgeColor ("Edge Color", Color) = (1.0, 0.5, 1.0, 1.0)
        _Progress ("Progress", Range(0.0, 1.0)) = 0.0
        _RotateSpeed ("Rotate Speed", Float) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off Lighting Off ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t { float4 vertex : POSITION; float2 texcoord : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float2 texcoord : TEXCOORD0; };

            sampler2D _MainTex, _NoiseTex;
            fixed4 _CoreColor, _EdgeColor;
            float _Progress, _RotateSpeed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord - 0.5;
                float dist = length(uv);
                float angle = atan2(uv.y, uv.x);
                float t = _Time.y;

                // 바깥 링
                // 바깥 링
float outerRing = smoothstep(0.02, 0.0, abs(dist - 0.45 * _Progress));

// 안쪽 링
float innerRing = smoothstep(0.015, 0.0, abs(dist - 0.28 * _Progress));

                // 회전하는 룬 패턴
                float rotAngle = angle + t * _RotateSpeed;
                float2 runeUV = float2(rotAngle / (3.14159 * 2.0) + 0.5, dist * 2.0);
                float rune = tex2D(_NoiseTex, runeUV * float2(6.0, 1.0)).r;
                rune = smoothstep(0.6, 0.9, rune)
                     * smoothstep(0.0, 0.05, dist)
                     * smoothstep(0.48, 0.38, dist)
                     * _Progress;

                // 중앙 글로우
                float glow = smoothstep(0.5, 0.0, dist) * 0.3 * _Progress;

                // 반대 방향 회전 링
                float rotAngle2 = angle - t * _RotateSpeed * 0.7;
                float2 runeUV2 = float2(rotAngle2 / (3.14159 * 2.0) + 0.5, dist * 2.0);
                float rune2 = tex2D(_NoiseTex, runeUV2 * float2(4.0, 1.0)).r;
                rune2 = smoothstep(0.65, 0.95, rune2)
                      * smoothstep(0.0, 0.05, dist)
                      * smoothstep(0.35, 0.25, dist)
                      * _Progress;

                float mask = saturate(outerRing + innerRing + rune + glow + rune2);
                float pattern = saturate(outerRing + rune * 0.8 + rune2 * 0.6 + glow);

                fixed4 col = lerp(_EdgeColor, _CoreColor, pattern);
                fixed4 sp = tex2D(_MainTex, IN.texcoord);
                col.a = mask * _Progress * sp.a;
                return col;
            }
        ENDCG
        }
    }
}