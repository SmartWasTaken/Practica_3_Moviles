using UnityEngine;

namespace _Game.Scripts.Core.Menu
{
    public class MainState : IMenuState
    {
        private MenuController _controller;
        private GameObject _panel;
        private RectTransform _panelRect;

        public MainState(MenuController controller, GameObject panel)
        {
            _controller = controller;
            _panel = panel;
            _panelRect = panel.GetComponent<RectTransform>();
        }

        public void Enter()
        {
            _panel.SetActive(true);
            //añadir sonido en el futuro
        }

        public void Exit()
        {
            _panel.SetActive(false);
        }

        public void UpdateState()
        {
            ApplyParallaxEffect();
        }

        private void ApplyParallaxEffect()
        {
            if (_panelRect == null) return;

            Vector2 inputPos = Vector2.zero;
            if (Input.touchCount > 0)
                inputPos = Input.GetTouch(0).position;
            else
                inputPos = Input.mousePosition;

            float x = (inputPos.x / Screen.width) - 0.5f;
            float y = (inputPos.y / Screen.height) - 0.5f;
            Quaternion targetRotation = Quaternion.Euler(-y * _controller.parallaxStrength, x * _controller.parallaxStrength, 0);
            _panelRect.rotation = Quaternion.Lerp(_panelRect.rotation, targetRotation, Time.deltaTime * 5f);
        }


        public void OnPlayPressed()
        {
            _controller.ChangeState(_controller.ModeSelectState);
        }

        public void OnSettingsPressed() => _controller.ChangeState(_controller.SettingsState);
        public void OnCreditsPressed() => _controller.ChangeState(_controller.CreditsState);
        public void OnExitPressed() => _controller.QuitGame();
    }
}