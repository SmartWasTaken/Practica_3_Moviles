using UnityEngine;
using TMPro;

namespace _Game.Scripts.Core.UI
{
    public class TextShimmer : MonoBehaviour
    {
        [Header("Colores")]
        [SerializeField] private Color _baseColor = new Color(1f, 1f, 1f, 1f); // Color normal (ej. Blanco o Gris)
        [SerializeField] private Color _shineColor = new Color(0f, 1f, 1f, 1f); // Color del brillo (ej. Cyan o Dorado)

        [Header("Ajustes del Brillo")]
        [SerializeField] private float _speed = 2.0f;       // Qué tan rápido pasa la luz
        [SerializeField] private float _shineWidth = 1.5f;  // Qué tan ancha es la franja de luz
        [Range(0f, 1f)]
        [SerializeField] private float _intensity = 1.0f;   // Fuerza de la mezcla

        private TMP_Text _textMesh;

        private void Awake()
        {
            _textMesh = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (_textMesh == null) return;

            _textMesh.ForceMeshUpdate();

            var textInfo = _textMesh.textInfo;
            int characterCount = textInfo.characterCount;

            if (characterCount == 0) return;

            // Calculamos la posición del brillo basada en el tiempo
            // Esto crea un valor que va de 0 al número total de letras y se repite
            float shinePos = Mathf.Repeat(Time.time * _speed, characterCount + _shineWidth * 2) - _shineWidth;

            for (int i = 0; i < characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Color32[] newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                // Calculamos qué tan cerca está esta letra del centro del brillo
                float dist = Mathf.Abs(i - shinePos);
                
                // Creamos una curva suave: 1 si está en el centro, 0 si está lejos
                float shineFactor = 1f - Mathf.Clamp01(dist / _shineWidth);
                
                // Suavizamos la curva (opcional, para que sea más elegante)
                shineFactor = Mathf.SmoothStep(0, 1, shineFactor) * _intensity;

                // Mezclamos el color Base con el color Brillo
                Color32 finalColor = Color32.Lerp(_baseColor, _shineColor, shineFactor);

                // Aplicamos el color a los 4 vértices de la letra
                newVertexColors[vertexIndex + 0] = finalColor;
                newVertexColors[vertexIndex + 1] = finalColor;
                newVertexColors[vertexIndex + 2] = finalColor;
                newVertexColors[vertexIndex + 3] = finalColor;
            }

            // Enviamos los cambios a la pantalla
            _textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}