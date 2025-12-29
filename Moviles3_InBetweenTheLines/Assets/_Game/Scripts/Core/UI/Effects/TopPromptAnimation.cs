using UnityEngine;
using DG.Tweening;

namespace _Game.Scripts.Core.UI.Effects
{
    public class TapPromptAnimation : MonoBehaviour
    {
        [Header("Configuración de Animación")]
        [Tooltip("Tiempo que tarda en hacerse grande/pequeño")]
        [SerializeField] private float _duration = 0.8f;

        [Tooltip("Cuánto crece (1.1 = 10% más grande)")]
        [SerializeField] private float _scaleMultiplier = 1.1f;

        [Tooltip("Si quieres que también se desvanezca un poco")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _minAlpha = 0.5f;

        private void Start()
        {
            AnimateScale();
            AnimateAlpha();
        }

        private void AnimateScale()
        {
            transform.localScale = Vector3.one;

            transform.DOScale(Vector3.one * _scaleMultiplier, _duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo) //infinito + ida y vuelta
                .SetLink(gameObject);
        }

        private void AnimateAlpha()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.DOFade(_minAlpha, _duration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetLink(gameObject);
            }
        }

        private void OnDestroy()
        {
            transform.DOKill(); 
        }
    }
}