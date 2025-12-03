Shader "Custom/HeatDistortion_Final"
{
    Properties
    {
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DistortionStrength ("Distortion Strength", Range(0, 0.3)) = 0.0
        _Speed ("Speed", Range(0, 5)) = 1.0
        _VignetteSize ("Vignette Size", Range(0, 1)) = 0.4
        _Aberration ("Chromatic Aberration", Range(0, 0.5)) = 0.1 
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "HeatDistortionPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            sampler2D _NoiseTex;
            float _DistortionStrength;
            float _Speed;
            float _VignetteSize;
            float _Aberration; 

            half4 Frag (Varyings input) : SV_Target
            {
                float2 uv = input.texcoord;
                float2 center = float2(0.5, 0.5);
                float dist = distance(uv, center);
                float mask = smoothstep(_VignetteSize, 1.0, dist);

                float2 noiseUV = uv + _Time.y * _Speed;
                half4 noise = tex2D(_NoiseTex, noiseUV);
                float2 baseDistortion = (noise.rg - 0.5) * _DistortionStrength * mask;
                
                float2 distR = baseDistortion; 
                float2 distG = baseDistortion * (1.0 + _Aberration); 
                float2 distB = baseDistortion * (1.0 + _Aberration * 2.0); 

                half r = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + distR).r;
                half g = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + distG).g;
                half b = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, uv + distB).b;

                half4 finalColor = half4(r, g, b, 1.0);
                float heatTint = length(baseDistortion) * 10.0; 
                finalColor.rgb += float3(0.2, 0.05, 0.0) * heatTint; 

                return finalColor;
            }
            ENDHLSL
        }
    }
}