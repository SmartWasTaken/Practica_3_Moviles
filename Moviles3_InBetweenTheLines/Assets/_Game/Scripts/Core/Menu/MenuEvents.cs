using UnityEngine;

namespace _Game.Scripts.Core.Menu
{
    public class MenuEvents : MonoBehaviour
    {
        private MenuController _controller;

        private void Awake()
        {
            _controller = GetComponent<MenuController>();
        }
        
        public void Main_Play() => _controller.MainState.OnPlayPressed();
        public void Main_Settings() => _controller.MainState.OnSettingsPressed();
        public void Main_Credits() => _controller.MainState.OnCreditsPressed();
        public void Main_Ranking() => _controller.MainState.OnRankingPressed();
        public void Main_Exit() => _controller.MainState.OnExitPressed();

        public void Selector_Back() => _controller.ModeSelectState.OnBackActive();
        public void Selector_Story() => _controller.ModeSelectState.OnStoryModePressed();

        public void Settings_Back() => _controller.ChangeState(_controller.MainState);
        public void Credits_Back() => _controller.ChangeState(_controller.MainState);
        public void Ranking_Back() => _controller.ChangeState(_controller.MainState);
    }
}