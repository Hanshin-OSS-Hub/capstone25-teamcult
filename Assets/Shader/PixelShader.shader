Shader "Custom/PixelShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PixelSize ("Pixel Density", Range(1, 512)) = 100
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // 텍스처와 샘플러 선언
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            // SRP Batcher 호환을 위한 CBUFFER 선언
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _PixelSize;
            CBUFFER_END

            Varyings vert (Attributes IN) {
                Varyings OUT;
                // Unity 6/URP 표준 변환 함수
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target {
                // UV를 픽셀 크기에 맞춰서 자르는 로직
                // _PixelSize가 클수록 더 정밀해집니다 (작은 도트)
                float2 pixelatedUV = floor(IN.uv * _PixelSize) / _PixelSize;
                
                // URP 방식의 샘플링
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, pixelatedUV);
            }
            ENDHLSL
        }
    }
}