using UnityEngine;

namespace _Game.Scripts.Core.Menu
{
    public class CreditsState : IMenuState
    {
        private MenuController _controller;
        private GameObject _panel;

        public CreditsState(MenuController controller, GameObject panel)
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
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
            {
                OnBackActive();
            }
        }

        public void OnBackActive()
        {
            _controller.ChangeState(_controller.MainState);
        }
    }
}