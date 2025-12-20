using UnityEngine;

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
            // AQUÍ: Podríamos resetear la posición del scroll/carrusel al centro
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
            Debug.Log("Iniciando Modo Historia...");
            // SceneManager.LoadScene("GameLevel");
        }

        public void OnBackActive()
        {
            _controller.ChangeState(_controller.MainState);
        }
    }
}