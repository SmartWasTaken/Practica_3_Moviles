Shader "Custom/MobileGlass"
{
    Properties
    {
        _MainColor ("Glass Color", Color) = (0.7, 0.9, 1, 0.3) // Azulito transparente
        _RimColor ("Rim Color", Color) = (1, 1, 1, 0.5) // Borde blanco
        _RimPower ("Rim Power", Range(0.5, 8.0)) = 3.0 // Qué tan fino es el borde
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        // Habilitar mezcla para transparencia
        Blend SrcAlpha OneMinusSrcAlpha
        // No escribir en el Z-Buffer para que se vea lo de atrás (el líquido)
        ZWrite Off 

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
                float3 viewDir : TEXCOORD0;
                float3 normal : TEXCOORD1;
            };

            float4 _MainColor;
            float4 _RimColor;
            float _RimPower;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Calculamos vectores necesarios para el efecto de borde (Fresnel)
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normalizar vectores
                float3 normal = normalize(i.normal);
                float3 viewDir = normalize(i.viewDir);

                // Cálculo de Fresnel (Borde brillante)
                // Cuanto más perpendicular sea la vista a la normal, más borde hay
                float rim = 1.0 - saturate(dot(viewDir, normal));
                
                // Hacer el borde más nítido o suave
                float rimIntensity = pow(rim, _RimPower);

                // Color final: Color base + Color del borde
                fixed4 finalColor = _MainColor + (_RimColor * rimIntensity);
                
                // Mantenemos la transparencia base pero el borde lo hacemos más opaco
                finalColor.a = _MainColor.a + (rimIntensity * _RimColor.a);
                
                return finalColor;
            }
            ENDCG
        }
    }
}