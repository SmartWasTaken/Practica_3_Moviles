using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Data;
using _Game.Scripts.Core.UI;

namespace _Game.Scripts.Core.Menu
{
    public class RankingState : IMenuState
    {
        private MenuController _controller;
        private GameObject _panel;
        private RankingUI _rankingLogic;
        private List<LevelConfig> _levelsData;

        public RankingState(MenuController controller, GameObject panel, List<LevelConfig> levels)
        {
            _controller = controller;
            _panel = panel;
            
            _rankingLogic = panel.GetComponent<RankingUI>();
            _levelsData = levels;
        }

        public void Enter()
        {
            _panel.SetActive(true);
            
            if (_rankingLogic != null)
            {
                _rankingLogic.PopulateRanking(_levelsData);
            }
        }

        public void Exit()
        {
            _panel.SetActive(false);
        }

        public void UpdateState()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnBackPressed();
            }
        }

        public void OnBackPressed()
        {
            _controller.ChangeState(_controller.MainState);
        }
    }
}