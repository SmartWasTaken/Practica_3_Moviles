using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Data;
using _Game.Scripts.Core.Utils;
using UnityEngine.SceneManagement;

namespace _Game.Scripts.Core.Menu
{
    public class MenuController : MonoBehaviour
    {
        [Header("UI Containers")]
        public GameObject mainPanel;
        public GameObject modeSelectorPanel;
        public GameObject settingsPanel;
        public GameObject creditsPanel;
        public GameObject rankingPanel;

        [Header("Visual Juice")]
        public float parallaxStrength = 15f;
        public GameObject mainParallaxObject; 
        public Camera uiCamera;

        [Header("Data")]
        public List<LevelConfig> allLevels;

        private IMenuState _currentState;

        public MainState MainState { get; private set; }
        public ModeSelectState ModeSelectState { get; private set; }
        public SettingsState SettingsState { get; private set; }
        public CreditsState CreditsState { get; private set; }
        public RankingState RankingState { get; private set; }

        private void Awake()
        {
            MainState = new MainState(this, mainPanel, mainParallaxObject);
            ModeSelectState = new ModeSelectState(this, modeSelectorPanel);
            SettingsState = new SettingsState(this, settingsPanel);
            CreditsState = new CreditsState(this, creditsPanel);
            RankingState = new RankingState(this, rankingPanel, allLevels);
        }

        private void Start()
        {
            mainPanel.SetActive(false);
            modeSelectorPanel.SetActive(false);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            rankingPanel.SetActive(false);

            if (SceneNavigation.TargetMenuState == "Ranking")
            {
                ChangeState(RankingState);
            }
            else
            {
                ChangeState(MainState);
            }
            SceneNavigation.TargetMenuState = "";
        }

        private void Update()
        {
            if (_currentState != null)
                _currentState.UpdateState();
        }

        public void ChangeState(IMenuState newState)
        {
            if (_currentState != null)
                _currentState.Exit();

            _currentState = newState;
            _currentState.Enter();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}