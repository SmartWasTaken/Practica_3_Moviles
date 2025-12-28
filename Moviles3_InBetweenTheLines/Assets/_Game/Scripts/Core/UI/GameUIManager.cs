using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using _Game.Scripts.Core.Utils;
using _Game.Scripts.Core.Game;

namespace _Game.Scripts.Core.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("HUD Components")]
        [SerializeField] private Slider _timerSlider;
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private TextMeshProUGUI _riddleText;
        [SerializeField] private GameObject _gamePlayPanel;

        [Header("Game Over / Feedback")]
        [SerializeField] private GameObject _gameOverPanel;

        [Header("Rankings")]
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private GameObject _newRecordEffect;
        
        [Header("Juice Config")]
        [SerializeField] private Color _normalTimeColor = Color.white;
        [SerializeField] private Color _criticalTimeColor = Color.red;
        
        private string _menuName = "MainMenu";

        public void SetupLevelUI(string riddle, float maxTime)
        {
            _gamePlayPanel.SetActive(true);
            _gameOverPanel.SetActive(false);
            
            _riddleText.text = riddle;
            
            _timerSlider.maxValue = maxTime;
            _timerSlider.value = maxTime;
            _timerFillImage.color = _normalTimeColor;
        }

        public void UpdateTimer(float currentTime)
        {
            _timerSlider.value = currentTime;

            if (currentTime <= _timerSlider.maxValue * 0.2f)
            {
                _timerFillImage.color = _criticalTimeColor;
            }
        }

        public void HideHUD()
        {
            _gamePlayPanel.SetActive(false);
        }

        public void ShowGameOver()
        {
            HideHUD();
            _gameOverPanel.SetActive(true);
        }


        public void OnExitButton()
        {
            SceneNavigation.TargetMenuState = "";
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadScene(_menuName);
            }
            else
            {
                SceneManager.LoadScene(_menuName);
            }
        }

        public void OnRankingButton()
        {
            SceneNavigation.TargetMenuState = "Ranking";
            
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadScene(_menuName);
            }
            else
            {
                SceneManager.LoadScene(_menuName);
            }
        }
    }
}