Shader "Custom/SimpleLiquid"
{
    Properties
    {
        _Color ("Liquid Color", Color) = (0, 0.5, 1, 1)
        _SurfaceColor ("Top Surface Color", Color) = (0.5, 0.8, 1, 1)
        _FillAmount ("Fill Offset", Float) = 0.0
        _FillNormal ("Fill Normal (World)", Vector) = (0, 1, 0, 0)
        _Center ("Object Center (World)", Vector) = (0,0,0,0)
        _WobbleX ("Wobble Strength", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Cull Off 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float4 _Color;
            float4 _SurfaceColor;
            float _FillAmount;      // Altura relativa
            float4 _FillNormal;     // Dirección de "Arriba" del líquido
            float4 _Center;         // Centro de la botella
            float _WobbleX;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Calcular vector desde el centro de la botella al píxel
                float3 dir = i.worldPos - _Center.xyz;

                // 2. Proyectar sobre la "Normal" del líquido (Gravedad inversa)
                // Esto nos dice qué tan "alto" está el píxel respecto a la gravedad actual
                float height = dot(dir, normalize(_FillNormal.xyz));

                // 3. Aplicar Wobble basado en la posición horizontal relativa
                // Calculamos un vector perpendicular a la normal para el wobble
                float3 tangent = cross(_FillNormal.xyz, float3(0,0,1));
                float wobbleOffset = dot(dir, tangent) * _WobbleX;

                // 4. Corte
                // Si la altura del píxel es mayor que el nivel de llenado + wobble -> Fuera
                if (height > (_FillAmount + wobbleOffset))
                {
                    discard; 
                }

                // 5. Superficie
                float surfaceDist = (_FillAmount + wobbleOffset) - height;
                float surfaceFactor = step(surfaceDist, 0.02); // 2cm de borde

                return lerp(_Color, _SurfaceColor, surfaceFactor);
            }
            ENDCG
        }
    }
}