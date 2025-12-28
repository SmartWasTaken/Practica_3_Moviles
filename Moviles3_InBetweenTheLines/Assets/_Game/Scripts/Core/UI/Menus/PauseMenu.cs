using _Game.Scripts.Core.Game;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Game.Scripts.Core.UI.Base;
using _Game.Scripts.Core.Utils;

namespace _Game.Scripts.Core.UI.Menus
{
    public class PauseMenu : MenuView
    {
        [Header("Paneles Internos")]
        [SerializeField] private GameObject _mainButtonsPanel;
        [SerializeField] private GameObject _optionsPanel;

        private bool _isPaused = false;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (_isPaused) ResumeGame();
                else PauseGame();
            }
        }

        public void PauseGame()
        {
            _isPaused = true;
            Time.timeScale = 0f;
            Open();
            
            ShowMainButtons(); 
        }

        public void ResumeGame()
        {
            _isPaused = false;
            Time.timeScale = 1f;
            Close();
        }

        public void OnOptionsButton()
        {
            _mainButtonsPanel.SetActive(false);
            _optionsPanel.SetActive(true);
        }

        public void OnBackFromOptionsButton()
        {
            ShowMainButtons();
        }

        private void ShowMainButtons()
        {
            _optionsPanel.SetActive(false);
            _mainButtonsPanel.SetActive(true);
        }


        public void OnExitButton()
        {
            Time.timeScale = 1f;
            
            SceneNavigation.TargetMenuState = ""; 
            TransitionManager.Instance.LoadScene("MainMenu");
        }
    }
}