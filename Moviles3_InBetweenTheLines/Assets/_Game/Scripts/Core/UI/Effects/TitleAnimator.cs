using UnityEngine;
using DG.Tweening; // Importante

namespace _Game.Scripts.Core.UI.Effects
{
    public class TitleAnimator : MonoBehaviour
    {
        private static bool _hasIntroPlayed = false;

        [Header("Configuración")]
        [SerializeField] private float _delay = 0.5f;
        [SerializeField] private float _duration = 1.0f;
        [SerializeField] private Ease _easeType = Ease.OutBack;
        [SerializeField] private AnimationCurve _customCurve;
        [SerializeField] private bool _useCustomCurve = false;
        
        private Vector3 _finalScale;

        private void Awake()
        {
            _finalScale = transform.localScale;
        }

        private void Start()
        {
            if (!_hasIntroPlayed)
            {
                transform.localScale = Vector3.zero;

                var tween = transform.DOScale(_finalScale, _duration)
                    .SetDelay(_delay)
                    .SetUpdate(true);

                if (_useCustomCurve) tween.SetEase(_customCurve);
                else tween.SetEase(_easeType);

                _hasIntroPlayed = true;
            }
            else
            {
                transform.localScale = _finalScale;
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}