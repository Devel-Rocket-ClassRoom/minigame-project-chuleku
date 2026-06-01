Shader "UI/SpotlightHole"
{
    // 어두운 풀스크린 UI 패널에 "원형 구멍"을 뚫어, 그 안쪽으로 게임 월드가
    // 환하게 비쳐 보이게 하는 셰이더. _Center(화면 픽셀)에 _Radius 크기로 구멍을 낸다.
    // _Softness 로 구멍 가장자리를 부드럽게 한다.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,0.7) // 패널 어둡기(알파)
        _Center ("Center 1 (screen px)", Vector) = (0.5, 0.5, 0, 0)
        _Radius ("Radius 1 (px)", Float) = 120
        _Softness ("Edge Softness 1 (px)", Float) = 40
        _Center2 ("Center 2 (screen px)", Vector) = (-9999, -9999, 0, 0)
        _Radius2 ("Radius 2 (px)", Float) = 0
        _Softness2 ("Edge Softness 2 (px)", Float) = 40

        // uGUI Mask 호환용 스텐실 (Mask 자식으로 안 쓰면 기본값 그대로 둬도 됨)
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
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

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
                float4 vertex    : SV_POSITION;
                fixed4 color     : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            fixed4 _Color;
            float4 _Center;
            float  _Radius;
            float  _Softness;
            float4 _Center2;
            float  _Radius2;
            float  _Softness2;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex    = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.texcoord  = v.texcoord;
                o.color     = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 이 픽셀의 실제 화면 좌표(픽셀)
                float2 px = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;

                // 구멍 1: 안쪽 0, 바깥 1
                float d1 = distance(px, _Center.xy);
                float hole1 = smoothstep(_Radius - _Softness, _Radius, d1);

                // 구멍 2: 반지름 0이면 사실상 비활성(항상 1)
                float d2 = distance(px, _Center2.xy);
                float hole2 = smoothstep(_Radius2 - _Softness2, _Radius2, d2);

                // 두 구멍의 합집합 = 더 밝은(알파 작은) 쪽 채택
                float hole = min(hole1, hole2);

                fixed4 col = _Color * i.color;
                col.a *= hole;
                return col;
            }
            ENDCG
        }
    }
}
