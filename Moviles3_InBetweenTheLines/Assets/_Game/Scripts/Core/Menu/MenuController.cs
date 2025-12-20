using UnityEngine;
using System.Collections.Generic;

namespace _Game.Scripts.Core.Menu
{
    public class MenuController : MonoBehaviour
    {
        [Header("UI Containers")]
        public GameObject mainPanel;
        public GameObject modeSelectorPanel;
        public GameObject settingsPanel;
        public GameObject creditsPanel;

        [Header("Visual Juice")]
        public float parallaxStrength = 15f;
        public Camera uiCamera;

        private IMenuState _currentState;

        public MainState MainState { get; private set; }
        public ModeSelectState ModeSelectState { get; private set; }
        public SettingsState SettingsState { get; private set; }
        public CreditsState CreditsState { get; private set; }

        private void Awake()
        {
            MainState = new MainState(this, mainPanel);
            ModeSelectState = new ModeSelectState(this, modeSelectorPanel);
            SettingsState = new SettingsState(this, settingsPanel);
            CreditsState = new CreditsState(this, creditsPanel);
        }

        private void Start()
        {
            mainPanel.SetActive(false);
            modeSelectorPanel.SetActive(false);
            settingsPanel.SetActive(false);
            creditsPanel.SetActive(false);

            ChangeState(MainState);
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
            Debug.Log("Cerrando juego...");
            Application.Quit();
        }
    }
}