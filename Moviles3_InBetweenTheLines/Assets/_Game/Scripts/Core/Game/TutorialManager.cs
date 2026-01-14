using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Core.Game
{
    public enum TutorialType 
    { 
        None, Tap, Hold, Shake, Tilt, Rotate, Multitouch, Rub, MakeNoise, Brightness 
    }

    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance;

        [Header("Referencias Generales")]
        [SerializeField] private Canvas _myCanvas;

        [Header("Paneles de Tutorial")]
        [SerializeField] private GameObject _panelTap;
        [SerializeField] private GameObject _panelHold;
        [SerializeField] private GameObject _panelShake;
        [SerializeField] private GameObject _panelTilt;
        [SerializeField] private GameObject _panelRotate;
        [SerializeField] private GameObject _panelMultitouch;
        [SerializeField] private GameObject _panelRub;
        [SerializeField] private GameObject _panelMakeNoise;
        [SerializeField] private GameObject _panelBrightness;
        
        private GameObject _activePanel;
        private System.Action _onTutorialComplete;

        private void Awake() 
        { 
            Instance = this; 
            if (_myCanvas == null) _myCanvas = GetComponentInParent<Canvas>();
            
            if (_myCanvas != null)
            {
                _myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _myCanvas.sortingOrder = 32000;
            }

            HideAllPanels();
        }

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
            HideAllPanels();
            if (_myCanvas != null) _myCanvas.gameObject.SetActive(true);

            switch (type)
            {
                case TutorialType.Tap: _activePanel = _panelTap; break;
                case TutorialType.Hold: _activePanel = _panelHold; break;
                case TutorialType.Shake: _activePanel = _panelShake; break;
                case TutorialType.Tilt: _activePanel = _panelTilt; break;
                case TutorialType.Rotate: _activePanel = _panelRotate; break;
                case TutorialType.Multitouch: _activePanel = _panelMultitouch; break;
                case TutorialType.Rub: _activePanel = _panelRub; break;
                case TutorialType.MakeNoise: _activePanel = _panelMakeNoise; break;
                case TutorialType.Brightness: _activePanel = _panelBrightness; break;
            }

            if (_activePanel != null) 
            {
                _activePanel.SetActive(true);
                _activePanel.transform.SetAsLastSibling(); 
            }
            else
            {
                CloseTutorial(); 
            }
        }

        public void CloseTutorial()
        {
            if (_activePanel != null)
            {
                TutorialType currentType = GetTypeFromPanel(_activePanel);
                if (currentType != TutorialType.None)
                {
                    PlayerPrefs.SetInt($"Tut_{currentType}", 1);
                    PlayerPrefs.Save();
                }
                
                _activePanel.SetActive(false);
                _activePanel = null;
            }

            Time.timeScale = 1; 
            _onTutorialComplete?.Invoke();
        }
        private void HideAllPanels()
        {
            if (_panelTap) _panelTap.SetActive(false);
            if (_panelHold) _panelHold.SetActive(false);
            if (_panelShake) _panelShake.SetActive(false);
            if (_panelTilt) _panelTilt.SetActive(false);
            if (_panelRotate) _panelRotate.SetActive(false);
            if (_panelMultitouch) _panelMultitouch.SetActive(false);
            if (_panelRub) _panelRub.SetActive(false);
            if (_panelMakeNoise) _panelMakeNoise.SetActive(false);
            if (_panelBrightness) _panelBrightness.SetActive(false);
        }

        private TutorialType GetTypeFromPanel(GameObject panel)
        {
            if (panel == _panelTap) return TutorialType.Tap;
            if (panel == _panelHold) return TutorialType.Hold;
            if (panel == _panelShake) return TutorialType.Shake;
            if (panel == _panelTilt) return TutorialType.Tilt;
            if (panel == _panelRotate) return TutorialType.Rotate;
            if (panel == _panelMultitouch) return TutorialType.Multitouch;
            if (panel == _panelRub) return TutorialType.Rub;
            if (panel == _panelMakeNoise) return TutorialType.MakeNoise;
            if (panel == _panelBrightness) return TutorialType.Brightness;
            return TutorialType.None;
        }

        [ContextMenu("Reset Tutorials")]
        public void ResetTutorialPrefs()
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("Tutoriales reseteados");
        }
    }
}