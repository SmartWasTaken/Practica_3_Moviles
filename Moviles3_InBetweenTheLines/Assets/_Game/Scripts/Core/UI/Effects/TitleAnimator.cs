using UnityEngine;
using System.Collections;

namespace _Game.Scripts.Core.UI.Effects
{
    public class TitleAnimator : MonoBehaviour
    {
        private static bool _hasIntroPlayed = false;

        [Header("Configuración")]
        [SerializeField] private float _delay = 0.5f;
        [SerializeField] private float _duration = 1.0f;
        
        [SerializeField] private AnimationCurve _curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
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
                StartCoroutine(PlayIntro());
                _hasIntroPlayed = true;
            }
            else
            {
                transform.localScale = _finalScale;
            }
        }

        private IEnumerator PlayIntro()
        {
            yield return new WaitForSeconds(_delay);

            float timer = 0;
            while(timer < _duration)
            {
                timer += Time.deltaTime;
                float progress = timer / _duration;
                
                float value = _curve.Evaluate(progress);

                transform.localScale = _finalScale * value;
                yield return null;
            }
            transform.localScale = _finalScale;
        }
    }
}