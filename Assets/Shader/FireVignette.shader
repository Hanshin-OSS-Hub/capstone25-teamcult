Shader "Custom/UniversalFireVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _NoiseTex ("Seamless Noise Texture", 2D) = "white" {}
        
        _CoreColor ("Core Color", Color) = (1, 0.9, 0.5, 1)
        _EdgeColor ("Edge Color", Color) = (1, 0.1, 0.0, 1)
        
        _Radius ("Vignette Radius", Float) = 0.5
        _Softness ("Softness", Range(0.0, 1.0)) = 0.3
        
        _ScrollSpeed ("Scroll Speed (X, Y)", Vector) = (0.1, 1.2, 0, 0)
        _DistortPower ("Distortion Power", Range(0.0, 0.5)) = 0.1
        
        _Progress ("Fill Progress", Range(0.0, 1.0)) = 1.0
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
        
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }

        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]

        Pass
        {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };
            
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            fixed4 _CoreColor;
            fixed4 _EdgeColor;
            float _Radius;
            float _Softness;
            float2 _ScrollSpeed;
            float _DistortPower;
            float _Progress; 

            v2f vert(appdata_t IN) {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                float2 uv = IN.texcoord;
                
                // ★ [수정됨] 중심점을 화면 상단 바깥(1.1)으로 이동
                // 효과를 바닥 끝까지 밀어냅니다.
                float2 center = float2(0.5, 1.0);
                
                // 1. Texture
                fixed4 spriteColor = tex2D(_MainTex, uv);
                float shapeAlpha = spriteColor.a;

                // 2. Noise Calculation
                float2 scroll1 = _Time.y * _ScrollSpeed;
                float noise1 = tex2D(_NoiseTex, (uv + scroll1) * 1.5).r;
                float2 scroll2 = _Time.y * float2(-_ScrollSpeed.x * 0.5, _ScrollSpeed.y * 0.8);
                float noise2 = tex2D(_NoiseTex, (uv + scroll2) * 2.5).r;
                float finalNoise = noise1 * noise2;

                // 3. Vignette Calculation
                float dist = distance(uv, center);
                float distortedDist = dist + (finalNoise * _DistortPower * dist * 1.5);
                
                float vignetteMask = 1.0;
                vignetteMask = smoothstep(_Radius, _Radius + _Softness, distortedDist);
                
                // 4. Clockwise Mask
                float2 dir = uv - float2(0.5, 0.5); 
                float angle = atan2(dir.x, dir.y); 
                if (angle < 0) angle += 6.2831853; 
                float angle01 = angle / 6.2831853; 
                float clockMask = smoothstep(angle01, angle01 + 0.02, _Progress);
                
                // 5. Final Color
                float firePattern = pow(finalNoise, 1.5); 
                fixed4 fireColor = lerp(_EdgeColor, _CoreColor, firePattern);
                
                fireColor.a *= vignetteMask * clockMask * shapeAlpha * IN.color.a;
                
                return fireColor;
            }
        ENDCG
        }
    }
}