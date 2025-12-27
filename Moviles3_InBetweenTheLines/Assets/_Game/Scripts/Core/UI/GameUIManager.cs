using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Game.Scripts.Core.UI
{
    public class GameUIManager : MonoBehaviour
    {
        [Header("HUD Components")]
        [SerializeField] private Slider _timerSlider;
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private TextMeshProUGUI _riddleText;
        [SerializeField] private GameObject _gamePlayPanel;

        [Header("Rankings")]
        [SerializeField] private TextMeshProUGUI _highScoreText;
        [SerializeField] private GameObject _newRecordEffect;
        
        [Header("Juice Config")]
        [SerializeField] private Color _normalTimeColor = Color.white;
        [SerializeField] private Color _criticalTimeColor = Color.red;

        public void SetupLevelUI(string riddle, float maxTime)
        {
            _gamePlayPanel.SetActive(true);
            _riddleText.text = riddle;
            
            //Slider
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
    }
}