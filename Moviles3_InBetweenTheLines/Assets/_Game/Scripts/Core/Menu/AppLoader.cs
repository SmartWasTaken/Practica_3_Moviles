using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; 
using System.Collections;
using TMPro;
using _Game.Scripts.Core.Game;

public class AppLoader : MonoBehaviour
{
    [Header("Configuración Visual")]
    [SerializeField] private float _minLoadTime = 3.0f;
    [SerializeField] private string _nextSceneName = "MenuScene";

    [Header("Referencias UI")]
    [SerializeField] private Slider _loadingBar; 
    [SerializeField] private TextMeshProUGUI _loadingText; 
    [SerializeField] private GameObject _tapToStartGroup;

    private AsyncOperation _loadingOperation;

    private IEnumerator Start()
    {
        // 1. Estado inicial
        _tapToStartGroup.SetActive(false);
        _loadingBar.gameObject.SetActive(true);
        _loadingText.gameObject.SetActive(true);
        _loadingBar.value = 0;

        _loadingOperation = SceneManager.LoadSceneAsync(_nextSceneName);
        _loadingOperation.allowSceneActivation = false;

        float timer = 0f;

        while (_loadingOperation.progress < 0.9f || timer < _minLoadTime)
        {
            timer += Time.deltaTime;
            float realProgress = Mathf.Clamp01(_loadingOperation.progress / 0.9f);
            
            float timeProgress = Mathf.Clamp01(timer / _minLoadTime);

            float visualProgress = Mathf.Min(realProgress, timeProgress);
            if (_loadingBar) _loadingBar.value = visualProgress;
            if (_loadingText) _loadingText.text = $"LOADING... {(visualProgress * 100):F0}%";

            yield return null;
        }

        if (_loadingBar) _loadingBar.value = 1f;
        if (_loadingText) _loadingText.text = "READY";

        yield return new WaitForSeconds(0.2f);

        _loadingBar.gameObject.SetActive(false);
        _loadingText.gameObject.SetActive(false);
        _tapToStartGroup.SetActive(true); 

        yield return new WaitUntil(() => IsTapDetected());

        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.DoTransition(() => 
            {
                _loadingOperation.allowSceneActivation = true;
            });
        }
        else
        {
            _loadingOperation.allowSceneActivation = true;
        }
    }

    private bool IsTapDetected()
    {
        #if UNITY_EDITOR
        return Input.GetMouseButtonDown(0);
        #else
        return Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        #endif
    }
}