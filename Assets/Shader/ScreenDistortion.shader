Shader "Custom/ScreenDistortion"
{
    Properties
    {
        // 에디터에서 강도를 조절하기 위한 프로퍼티들
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0.5
        _ChromaAmount ("Chromatic Aberration Amount", Range(0, 1)) = 0.1
        [HideInInspector] _EffectIntensity ("Effect Intensity", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "DistortionPass"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // URP 화면 텍스처
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            // 프로퍼티 변수 선언
            float _EffectIntensity;
            float _GlitchIntensity;
            float _ChromaAmount;

            // 단순한 랜덤 노이즈 함수
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            half4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y;

                // --- 핵심 수정 사항: 애니메이션 강도 적용 ---
                // C#에서 계산된 f(t) 값인 _EffectIntensity를 베이스 강도에 곱합니다.
                float currentGlitch = _GlitchIntensity * _EffectIntensity;
                float currentChroma = _ChromaAmount * _EffectIntensity;

                // 1. 가로 줄무늬 왜곡 (currentGlitch 사용)
                float jitter = rand(float2(time * 0.1, floor(uv.y * 100.0) * 0.1)) - 0.5;
                jitter *= currentGlitch; // 여기서 반영됨
                jitter *= step(0.9, rand(float2(time, 0.0))); 

                float2 distortedUV = uv + float2(jitter, 0.0);

                // 2. 색추차 (currentChroma 사용)
                float2 distToCenter = distortedUV - 0.5;
                float distFactor = length(distToCenter);
                float chromaOffset = currentChroma * distFactor; // 여기서 반영됨

                float3 col;
                col.r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, distortedUV + float2(chromaOffset, 0.0)).r;
                col.g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, distortedUV).g;
                col.b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, distortedUV - float2(chromaOffset, 0.0)).b;

                return half4(col, 1.0);
            }
            ENDHLSL
        }
    }
}