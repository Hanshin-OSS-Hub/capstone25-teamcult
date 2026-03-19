Shader "Custom/IceGroundEffect"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0.0, 1.0)) = 1.0
        _Alpha ("Alpha", Range(0.0, 1.0)) = 0.8
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

            sampler2D _NoiseTex;
            float _Progress;
            float _Alpha;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float t = _Time.y;

                // 얼음 노이즈 (기존 셰이더 Ice 로직 그대로)
                float n1 = tex2D(_NoiseTex, uv * 3.0 + t * 0.008).r;
                float n2 = tex2D(_NoiseTex, uv * 1.8 - t * 0.005).r;
                float iceNoise = (n1 + n2) * 0.5;

                // 원형 마스크 (타원형으로 발밑처럼 보이게)
                float2 center = float2(0.5, 0.5);
                float2 diff = (uv - center) * float2(1.0, 1.8); // 세로로 납작하게
                float dist = length(diff);

                // 서리 패턴
                float frost = dist + (iceNoise - 0.5) * 0.4;
                float edge = smoothstep(0.5, 0.3, frost);
                float inner = smoothstep(0.3, 0.1, frost);

                // 얼음 결정 느낌
                float crystal = smoothstep(0.6, 0.8, iceNoise) * edge * 1.5;

                float mask = saturate((edge + crystal) * _Progress);

                // 얼음 색상
                fixed4 coreColor = fixed4(0.8, 0.97f, 1.0, 1.0);  // 밝은 하늘색
                fixed4 edgeColor = fixed4(0.2, 0.6,  1.0, 1.0);   // 진한 파란색
                fixed4 col = lerp(edgeColor, coreColor, inner);
                col.a = mask * _Alpha;

                return col;
            }
        ENDCG
        }
    }
}