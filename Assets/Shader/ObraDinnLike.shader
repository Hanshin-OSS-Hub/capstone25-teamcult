Shader "Example/ObraDinnLike"
{
    Properties
    {
        // _MainTex는 C# 스크립트가 화면 텍스처를 넣어줄 통로입니다.
        // 또한 _MainTex_TexelSize 변수를 사용하기 위해 선언이 필요합니다.
        _MainTex ("Texture", 2D) = "white" {}
        
        // 인스펙터에서 조절할 값들
        _DitherScale ("Dither Scale", Range(1, 10)) = 4.0 // 디더링 패턴 크기
        _Threshold ("Threshold", Range(0, 1)) = 0.5 // 흑백 전환 임계값
        _LineIntensity ("Line Intensity", Range(0, 2)) = 1.0 // 윤곽선 강도
    }
    SubShader
    {
        // 후처리는 컬링(Cull), 깊이 쓰기(ZWrite), 깊이 테스트(ZTest)를 모두 끕니다.
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" // URP에서도 호환성을 위해 기본 함수들을 제공합니다.

            // --- 정점 쉐이더 (Vertex Shader) ---
            // 화면 크기의 사각형을 그리기 위한 기본 코드입니다.
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // 정점 위치를 클립 공간으로 변환
                o.uv = v.uv; // UV 좌표 전달
                return o;
            }

            // --- 픽셀 쉐이더 (Fragment Shader) ---

            // Properties에서 선언한 변수들을 CGPROGRAM 안에서 다시 선언해야 합니다.
            sampler2D _MainTex;
            float _DitherScale;
            float _Threshold;
            float _LineIntensity;

            // [수정된 부분 1]
            // _TexelSize (float2) 대신 _MainTex_TexelSize (float4)를 사용합니다.
            // 유니티가 이 변수에 (1/width, 1/height, width, height) 값을 자동으로 채워줍니다.
            float4 _MainTex_TexelSize; 

            // 4x4 Bayer Dithering 패턴
            const static float DitherMatrix4x4[16] =
            {
                0,  8,  2, 10,
                12, 4, 14, 6,
                3, 11,  1, 9,
                15, 7, 13, 5
            };
            const static float DitherDivisor = 16.0f; // 0~1 범위로 정규화하기 위한 값

            // 실제 픽셀 색상을 계산하는 함수
            fixed4 frag (v2f i) : SV_Target
            {
                // 1. 원본 화면 색상 가져오기
                fixed4 originalColor = tex2D(_MainTex, i.uv);

                // 2. 그레이스케일 변환 (밝기 값 'luma' 추출)
                float luma = dot(originalColor.rgb, float3(0.299, 0.587, 0.114));

                // 3. 디더링 계산
                float2 screenPos = i.uv * _ScreenParams.xy; // 픽셀의 화면 좌표
                float2 ditherUV = fmod(screenPos / _DitherScale, 4); // 4x4 매트릭스 인덱스 계산
                int ditherIndex = ditherUV.y * 4 + ditherUV.x;
                float ditherValue = DitherMatrix4x4[ditherIndex] / DitherDivisor - 0.5; // -0.5 ~ 0.5 범위의 보정 값
                
                float ditheredLuma = luma + ditherValue; // 밝기 값에 디더링 보정

                // 4. 흑백 양자화 (두 가지 색으로 결정)
                fixed4 finalColor = (ditheredLuma > _Threshold) ? fixed4(1, 1, 1, 1) : fixed4(0, 0, 0, 1);

                // 5. 윤곽선 검출
                // [수정된 부분 2]
                // _MainTex_TexelSize.x (1/width) 와 _MainTex_TexelSize.y (1/height)를 사용합니다.
                float2 offset_x = float2(_MainTex_TexelSize.x, 0.0);
                float2 offset_y = float2(0.0, _MainTex_TexelSize.y);

                // 상하좌우 픽셀의 밝기 값을 가져옵니다.
                float luma_left   = dot(tex2D(_MainTex, i.uv - offset_x).rgb, float3(0.299, 0.587, 0.114));
                float luma_right  = dot(tex2D(_MainTex, i.uv + offset_x).rgb, float3(0.299, 0.587, 0.114));
                float luma_up     = dot(tex2D(_MainTex, i.uv - offset_y).rgb, float3(0.299, 0.587, 0.114));
                float luma_down   = dot(tex2D(_MainTex, i.uv + offset_y).rgb, float3(0.299, 0.587, 0.114));
                
                // 밝기 차이를 이용해 엣지(윤곽선) 강도를 계산합니다.
                float edgeX = (luma_right - luma_left);
                float edgeY = (luma_up - luma_down);
                float edgeStrength = sqrt(edgeX * edgeX + edgeY * edgeY) * _LineIntensity;

                // 엣지가 강한 부분은 검정색(0,0,0)으로 덮어씌웁니다.
                finalColor.rgb = lerp(finalColor.rgb, fixed3(0,0,0), saturate(edgeStrength - 0.1)); 

                return finalColor;
            }
            ENDCG
        }
    }
}