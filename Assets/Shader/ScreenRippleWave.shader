Shader "Custom/ScreenRippleWave"
{
    Properties
    {
        [HideInInspector] _BlitTexture("Base (RGB)", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // 최대 30개까지 동시 표현 가능하도록 설정
            float4 _WaveCenters[30];
            float _WaveRadii[30];
            float _WaveStrengths[30];
            int _ActiveWaveCount; // 현재 활성화된 파동 개수
            float _WaveThickness;

            half4 frag (Varyings i) : SV_Target {
                float2 uv = i.texcoord;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 finalOffset = float2(0, 0);

                // 활성화된 개수만큼만 루프를 돕니다.
                for(int j = 0; j < _ActiveWaveCount; j++) {
                    float2 centeredUV = uv - _WaveCenters[j].xy;
                    centeredUV.x *= aspect;
                    float d = length(centeredUV);

                    // 계단식 마스크
                    float mask = step(_WaveRadii[j] - _WaveThickness, d) * step(d, _WaveRadii[j]);
                    
                    float2 distortDir = normalize(centeredUV + 0.0001);
                    float2 offset = distortDir * mask * _WaveStrengths[j];
                    offset.x /= aspect;
                    
                    finalOffset += offset;
                }

                return SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv + finalOffset);
            }
            ENDHLSL
        }
    }
}