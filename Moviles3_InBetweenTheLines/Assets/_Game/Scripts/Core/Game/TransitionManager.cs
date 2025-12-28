using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

namespace _Game.Scripts.Core.Game
{
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance;

        [Header("Referencias")]
        [SerializeField] private CanvasGroup _fadeGroup; 
        [SerializeField] private float _duration = 0.5f;

        public bool IsTransiting { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                
                _fadeGroup.alpha = 0;
                _fadeGroup.blocksRaycasts = false;
            }
            else Destroy(gameObject);
        }

        public void DoTransition(Action midAction)
        {
            if (IsTransiting) return;
            StartCoroutine(RunPanelTransition(midAction));
        }

        public void LoadScene(string sceneName)
        {
            if (IsTransiting) return;
            StartCoroutine(RunSceneTransition(sceneName));
        }

        private IEnumerator RunPanelTransition(Action action)
        {
            IsTransiting = true;
            yield return StartCoroutine(Fade(0, 1));

            action?.Invoke();
            yield return new WaitForSecondsRealtime(0.1f);

            yield return StartCoroutine(Fade(1, 0));
            IsTransiting = false;
        }

        private IEnumerator RunSceneTransition(string sceneName)
        {
            IsTransiting = true;
            yield return StartCoroutine(Fade(0, 1));

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (op.progress < 0.9f)
            {
                yield return null;
            }

            op.allowSceneActivation = true;
            while (!op.isDone)
            {
                yield return null;
            }
            
            yield return new WaitForSecondsRealtime(0.2f);
            yield return StartCoroutine(Fade(1, 0));
            
            IsTransiting = false;
        }

        private IEnumerator Fade(float startAlpha, float endAlpha)
        {
            _fadeGroup.blocksRaycasts = true;
            float timer = 0;
            while (timer < _duration)
            {
                timer += Time.unscaledDeltaTime;
                _fadeGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, timer / _duration);
                yield return null;
            }
            _fadeGroup.alpha = endAlpha;
            
            if (endAlpha == 0) _fadeGroup.blocksRaycasts = false;
        }
    }
}