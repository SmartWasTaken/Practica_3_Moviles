using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _Game.Scripts.Core.Game
{
    public class GameOverPanel : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;

        private void Awake()
        {
            if (_restartButton != null) _restartButton.onClick.AddListener(RestartGame);
            if (_menuButton != null) _menuButton.onClick.AddListener(BackToMenu);

            if (_canvasGroup != null) 
            {
                _canvasGroup.alpha = 0;
                _canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            
            transform.SetAsLastSibling(); 

            if (_canvasGroup != null)
            {
                _canvasGroup.DOKill();
                
                _canvasGroup.alpha = 0; 
                _canvasGroup.blocksRaycasts = true;
                
                _canvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
            }
        }

        private void RestartGame()
        {
            Time.timeScale = 1; 
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void BackToMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }
    }
}