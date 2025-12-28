using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using _Game.Scripts.Core.Game; // Necesario para ver al TransitionManager

namespace _Game.Scripts.Core.UI.Effects
{
    public class UIAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Configuración Entrada")]
        [SerializeField] private bool _animateOnEnable = true;
        [SerializeField] private float _startDelay = 0f;
        [SerializeField] private float _enterDuration = 0.5f;
        [SerializeField] private AnimationCurve _popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Interacción")]
        [SerializeField] private float _clickScale = 0.9f;

        private Vector3 _originalScale;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        private void OnEnable()
        {
            if (_animateOnEnable)
            {
                transform.localScale = Vector3.zero;
                StopAllCoroutines();
                StartCoroutine(WaitAndAnimate());
            }
        }

        private IEnumerator WaitAndAnimate()
        {
            if (TransitionManager.Instance != null && TransitionManager.Instance.IsTransiting)
            {
                yield return new WaitUntil(() => !TransitionManager.Instance.IsTransiting);
            }

            if (_startDelay > 0) yield return new WaitForSecondsRealtime(_startDelay);

            yield return StartCoroutine(AnimatePop());
        }

        private IEnumerator AnimatePop()
        {
            float timer = 0f;
            while (timer < _enterDuration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = timer / _enterDuration;
                float value = _popCurve.Evaluate(progress);
                
                transform.localScale = _originalScale * value;
                yield return null;
            }
            transform.localScale = _originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StopAllCoroutines();
            transform.localScale = _originalScale * _clickScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = _originalScale;
        }
    }
}