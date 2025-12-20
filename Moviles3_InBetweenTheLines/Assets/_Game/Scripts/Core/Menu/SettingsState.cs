using UnityEngine;

namespace _Game.Scripts.Core.Menu
{
    public class SettingsState : IMenuState
    {
        private MenuController _controller;
        private GameObject _panel;

        public SettingsState(MenuController controller, GameObject panel)
        {
            _controller = controller;
            _panel = panel;
        }

        public void Enter()
        {
            _panel.SetActive(true);
            Time.timeScale = 0f; 
        }

        public void Exit()
        {
            _panel.SetActive(false);
            Time.timeScale = 1f;
        }

        public void UpdateState()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackActive();
            }
        }

        public void OnBackActive()
        {
            PlayerPrefs.Save();
            _controller.ChangeState(_controller.MainState);
        }
    }
}