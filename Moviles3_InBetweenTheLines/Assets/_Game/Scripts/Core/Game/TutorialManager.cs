using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Core.Game
{
    public enum TutorialType { None, Gyro, Shake, Tap, Hold }

    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;

        [Header("Paneles de Tutorial (Prefabs o Referencias)")]
        [SerializeField] private GameObject _panelGyro;
        [SerializeField] private GameObject _panelShake;
        [SerializeField] private GameObject _panelTap;
        
        private GameObject _activePanel;
        private System.Action _onTutorialComplete;

        private void Awake() { Instance = this; }

        public void TryShowTutorial(TutorialType type, System.Action onComplete)
        {
            if (type == TutorialType.None || PlayerPrefs.GetInt($"Tut_{type}", 0) == 1)
            {
                onComplete?.Invoke();
                return;
            }

            _onTutorialComplete = onComplete;
            ShowPanel(type);
        }

        private void ShowPanel(TutorialType type)
        {
            Time.timeScale = 0;
            if (type == TutorialType.Gyro) _activePanel = _panelGyro;
            else if (type == TutorialType.Shake) _activePanel = _panelShake;
            else if (type == TutorialType.Tap) _activePanel = _panelTap;

            if (_activePanel != null) _activePanel.SetActive(true);
        }

        public void CloseTutorial()
        {
            if (_activePanel != null)
            {
                TutorialType currentType = GetTypeFromPanel(_activePanel);
                PlayerPrefs.SetInt($"Tut_{currentType}", 1);
                
                _activePanel.SetActive(false);
                _activePanel = null;
            }

            Time.timeScale = 1;
            _onTutorialComplete?.Invoke();
        }

        private TutorialType GetTypeFromPanel(GameObject panel)
        {
            if (panel == _panelGyro) return TutorialType.Gyro;
            if (panel == _panelShake) return TutorialType.Shake;
            return TutorialType.None;
        }
    }
}