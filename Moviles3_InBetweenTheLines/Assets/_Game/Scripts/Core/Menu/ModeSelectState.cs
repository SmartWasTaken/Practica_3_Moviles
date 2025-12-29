using UnityEngine;
using _Game.Scripts.Core.Game;

namespace _Game.Scripts.Core.Menu
{
    public class ModeSelectState : IMenuState
    {
        private MenuController _controller;
        private GameObject _panel;

        public ModeSelectState(MenuController controller, GameObject panel)
        {
            _controller = controller;
            _panel = panel;
        }

        public void Enter()
        {
            _panel.SetActive(true);
        }

        public void Exit()
        {
            _panel.SetActive(false);
        }

        public void UpdateState()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackActive();
            }
        }

        public void OnStoryModePressed()
        {
            TransitionManager.Instance.LoadScene("GameScene");
        }

        public void OnBackActive()
        {
            _controller.ChangeState(_controller.MainState);
        }
    }
}