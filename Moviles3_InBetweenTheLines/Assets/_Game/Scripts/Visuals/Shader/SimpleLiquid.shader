Shader "Custom/SimpleLiquid"
{
    Properties
    {
        _Color ("Liquid Color", Color) = (0, 0.5, 1, 1)
        _SurfaceColor ("Top Surface Color", Color) = (0.5, 0.8, 1, 1)
        _FillAmount ("Fill Level (World Y)", Float) = 0.0
        _WobbleX ("Wobble X", Float) = 0.0
        _WobbleZ ("Wobble Z", Float) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Cull Off // Dibujamos ambas caras para que parezca que tiene volumen

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
            float _FillAmount;
            float _WobbleX;
            float _WobbleZ;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 1. Calcular el plano de corte con el Wobble
                // El wobble inclina el plano de corte basado en la posición relativa
                float offset = (i.worldPos.x * _WobbleX) + (i.worldPos.z * _WobbleZ);
                float currentLevel = _FillAmount + offset;

                // 2. Si el píxel está por encima del nivel del líquido, no lo pintamos
                if (i.worldPos.y > currentLevel)
                {
                    discard; // Recorta lo que sobra
                }

                // 3. Detectar si estamos muy cerca del borde para pintar la "superficie"
                // Si la distancia al corte es menor a 1.5cm (aprox), es superficie
                float surfaceDist = currentLevel - i.worldPos.y;
                
                // Mezcla suave entre color de superficie y color de fondo
                float surfaceFactor = step(surfaceDist, 0.015); 
                return lerp(_Color, _SurfaceColor, surfaceFactor);
            }
            ENDCG
        }
    }
}