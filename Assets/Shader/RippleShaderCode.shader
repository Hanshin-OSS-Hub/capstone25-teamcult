Shader "Custom/URP_SoundWave_Final"
{
    Properties
    {
        _WaveCenter ("Wave Center", Vector) = (0.5, 0.5, 0, 0)
        _DistortionStrength ("Strength", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; };

            // 핵심: _BlitTexture를 사용하여 Render Feature의 현재 화면을 가져옴
            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float4 _WaveCenter;
            float _DistortionStrength;

            Varyings vert (Attributes v) {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target {
                float2 uv = i.uv;
                
                // 화면 비율 보정 (원형 유지)
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 dir = uv - _WaveCenter.xy;
                dir.x *= aspect;
                
                float d = length(dir);
                
                // 파동 계산 (사인파 왜곡)
                float wave = sin(d * 40.0 - (_Time.y * 15.0)) * _DistortionStrength;
                float2 offset = normalize(dir) * wave;
                offset.x /= aspect; // 비율 보정 해제

                // 결과 샘플링: 색 반전 로직 없이 원본 색상을 그대로 반환
                return SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, uv + offset);
            }
            ENDHLSL
        }
    }
}