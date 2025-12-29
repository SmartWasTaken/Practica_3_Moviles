using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening; // Importante
using _Game.Scripts.Core.Game; 

namespace _Game.Scripts.Core.UI.Effects
{
    public class UIAnimator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Configuración Entrada")]
        [SerializeField] private bool _animateOnEnable = true;
        [SerializeField] private float _startDelay = 0f;
        [SerializeField] private float _enterDuration = 0.5f;
        [SerializeField] private Ease _enterEase = Ease.OutBack;

        [Header("Interacción (Click)")]
        [SerializeField] private float _clickScale = 0.9f;
        [SerializeField] private float _clickDuration = 0.1f; 
        
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
                StartCoroutine(WaitAndAnimateRoutine());
            }
            else
            {
                transform.localScale = _originalScale;
            }
        }

        private void OnDisable()
        {
            transform.DOKill(); 
        }

        private IEnumerator WaitAndAnimateRoutine()
        {
            if (TransitionManager.Instance != null && TransitionManager.Instance.IsTransiting)
            {
                yield return new WaitUntil(() => !TransitionManager.Instance.IsTransiting);
            }

            if (_startDelay > 0) yield return new WaitForSecondsRealtime(_startDelay);

            transform.DOScale(_originalScale, _enterDuration)
                .SetEase(_enterEase)
                .SetUpdate(true); 
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.DOKill(); 
            
            transform.DOScale(_originalScale * _clickScale, _clickDuration)
                .SetEase(Ease.OutQuad)
                .SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            transform.DOKill();

            transform.DOScale(_originalScale, _clickDuration)
                .SetEase(Ease.OutBack) 
                .SetUpdate(true);
        }
    }
}