Shader "Custom/AnimatedVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius ("Vignette Radius", Range(0.0, 1.0)) = 0.4
        _Softness ("Vignette Softness", Range(0.0, 1.0)) = 0.4
        // ★ 새로 추가된 일렁임 조절 옵션
        _DistortStrength ("Distort Strength", Range(0.0, 0.1)) = 0.02
        _Speed ("Animation Speed", Range(0.0, 10.0)) = 3.0
    }

    SubShader
    {
        Tags
        { 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };
            
            fixed4 _Color;
            float _Radius;
            float _Softness;
            // 새 변수들
            float _DistortStrength;
            float _Speed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 uv = IN.texcoord;

                // ★ 아지랑이 효과를 만드는 핵심 수학 공식 (Sin/Cos 파동 이용)
                float2 dir = uv - center;
                float angle = atan2(dir.y, dir.x); // 중심 기준 각도 계산
                
                // 시간(_Time)에 따라 물결치는 파동 생성
                float wave = sin(angle * 8.0 + _Time.y * _Speed) * cos(angle * 4.0 - _Time.y * _Speed * 0.7);
                float distortion = wave * _DistortStrength;

                // 거리 계산에 왜곡(distortion)을 더해서 원을 찌그러뜨림
                float dist = distance(uv, center) + distortion;
                
                // 비네트 적용
                float vignette = smoothstep(_Radius, _Radius + _Softness, dist);
                
                fixed4 c = IN.color;
                c.a *= vignette;
                return c;
            }
        ENDCG
        }
    }
}