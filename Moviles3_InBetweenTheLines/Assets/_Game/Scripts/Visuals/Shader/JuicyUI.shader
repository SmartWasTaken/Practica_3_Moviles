Shader "Custom/JuicyUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color1 ("Color Arriba/Izquierda", Color) = (1,1,1,1)
        _Color2 ("Color Abajo/Derecha", Color) = (1,1,1,1)
        
        _ShineColor ("Color del Brillo", Color) = (1,1,1,0.5)
        _ShineSpeed ("Velocidad Brillo", Range(0, 10)) = 2.0
        _ShineWidth ("Ancho Brillo", Range(0.05, 1)) = 0.1
        _ShineInterval ("Intervalo (Segundos)", Range(0, 10)) = 3.0
        
        // Propiedades requeridas para el sistema de UI de Unity (Stencil y Masking)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _WriteMask ("Stencil Write Mask", Float) = 255
        _ReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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
            "RenderPipeline" = "UniversalPipeline" 
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_ReadMask]
            WriteMask [_WriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _ShineColor;
            float _ShineSpeed;
            float _ShineWidth;
            float _ShineInterval;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _ClipRect;
            fixed4 _TextureSampleAdd;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 1. Gradiente Vertical
                fixed4 gradient = lerp(_Color2, _Color1, IN.texcoord.y);
                
                // 2. Efecto de Brillo (Shine)
                float time = _Time.y;
                float pos = fmod(time * _ShineSpeed, _ShineInterval + 2.0) - 1.0; 
                float shineLine = step(pos, IN.texcoord.x + IN.texcoord.y * 0.2) * step(IN.texcoord.x + IN.texcoord.y * 0.2, pos + _ShineWidth);
                
                fixed4 finalColor = gradient + (shineLine * _ShineColor);
                
                // Aplicar color del componente Image y textura base
                finalColor *= IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                
                // Recorte para máscaras de UI (RectMask2D) - Crucial para ScrollViews
                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}