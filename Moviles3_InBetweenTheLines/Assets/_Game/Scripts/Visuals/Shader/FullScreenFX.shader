Shader "Custom/FullScreenFX"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _VignetteColor ("Color Bordes", Color) = (0,0,0,1)
        _VignettePower ("Intensidad Viñeta", Range(0.0, 3.0)) = 1.2
        _VignetteSmoothness ("Suavidad Viñeta", Range(0.0, 5.0)) = 1.0
        _NoiseAmount ("Cantidad de Ruido", Range(0.0, 0.1)) = 0.03
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent+100" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "PreviewType"="Plane"
        }
        
        // Configuración para que sea transparente y no bloquee
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
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
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _VignetteColor;
            float _VignettePower;
            float _VignetteSmoothness;
            float _NoiseAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            // Función simple de ruido aleatorio
            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453123);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                
                // 1. Calcular Viñeta (distancia desde el centro)
                float2 coord = (uv - 0.5) * 2.0; // Mapear de 0..1 a -1..1
                float rf = sqrt(dot(coord, coord)) * _VignettePower;
                float rf2_1 = rf * rf + 1.0;
                float e = 1.0 / (rf2_1 * rf2_1); // Curva de atenuación suave
                
                // Invertimos para obtener la máscara (1 en centro, 0 en bordes)
                float vignetteMask = clamp(e, 0.0, 1.0);
                
                // 2. Calcular Ruido (Film Grain)
                float noise = (random(uv + _Time.y) - 0.5) * _NoiseAmount;
                
                // 3. Componer
                // El color base es transparente en el centro
                fixed4 finalColor = _VignetteColor;
                
                // Alpha: Donde vignetteMask es 1 (centro), alpha es 0 (transparente).
                // Donde vignetteMask es 0 (bordes), alpha sube.
                finalColor.a = (1.0 - vignetteMask);
                
                // Añadir el ruido (afecta sutilmente a todo)
                finalColor.rgb += noise;
                finalColor.a += _NoiseAmount; // Un pelín de ruido visible siempre ayuda a mezclar

                return finalColor;
            }
            ENDCG
        }
    }
}