using UnityEngine;
using System.Collections;
using _Game.Scripts.Data;
using _Game.Scripts.Puzzles;
using System;
using _Game.Scripts.Core.Game;
using _Game.Scripts.Core.UI;
using UnityEngine.UI;

namespace _Game.Scripts.Core
{
    public class LevelManager : MonoBehaviour
    {
        public event Action<bool> OnLevelFinished;
        
        [Header("Referencias")]
        [SerializeField] private Transform _puzzleSpawnPoint;
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Image _backgroundImage;
        
        [Header("UI Referencias")]
        [SerializeField] private GameUIManager _uiManager;

        private PuzzleBase _currentPuzzle;
        private LevelConfig _currentConfig;
        
        private float _timeRemaining;
        private bool _isPlaying;
        
        private int _currentScore;
        private int _currentLives = 3;
        
        public int CurrentScore => _currentScore;
        public int CurrentLives => _currentLives;

        private void Awake()
        {
            ResetLives();
        }

        public void ResetLives()
        {
            _currentLives = 3;
        }

        public void LoadLevel(LevelConfig config, int targetDifficulty)
    {
        ClearCurrentLevel();
        _currentConfig = config;
    
        string textToShow = "";
        int finalDifficulty = targetDifficulty;
        TutorialType requiredTutorial = TutorialType.None;
        Sprite backgroundToUse = null;
        
        LevelVariation selectedVariation = new LevelVariation();
        bool variationFound = false;
    
        if (config.variations != null && config.variations.Count > 0)
        {
            int index = Mathf.Clamp(targetDifficulty, 0, config.variations.Count - 1);
            selectedVariation = config.variations[index];
            variationFound = true;
        }
    
        if (variationFound)
        {
            _timeRemaining = selectedVariation.timeLimit;
            textToShow = selectedVariation.riddleText;
            finalDifficulty = targetDifficulty; 
            requiredTutorial = selectedVariation.tutorialRequired;
            backgroundToUse = selectedVariation.backgroundSprite;
        }
        else
        {
            _timeRemaining = 10f;
            textToShow = "Configuración no encontrada";
        }
        
        if (_backgroundImage != null)
        {
            if (backgroundToUse != null)
            {
                _backgroundImage.sprite = backgroundToUse;
                _backgroundImage.enabled = true;
            }
        }
        
        if (_uiManager != null)
        {
            _uiManager.SetupLevelUI(textToShow, _timeRemaining);
        }
    
        if (config.puzzlePrefab != null)
        {
            _currentPuzzle = Instantiate(config.puzzlePrefab, _puzzleSpawnPoint);
            _currentPuzzle.Initialize(this, finalDifficulty); 
        }
    
        _isPlaying = false;
        
        TutorialManager.Instance.TryShowTutorial(requiredTutorial, () => 
        {
            _isPlaying = true; 
        });
    }

        private void Update()
        {
            if (!_isPlaying) return;

            _timeRemaining -= Time.deltaTime;

            if (_uiManager != null)
            {
                _uiManager.UpdateTimer(_timeRemaining);
            }

            if (_timeRemaining <= 0)
            {
                if (_uiManager != null) _uiManager.UpdateTimer(0);
                OnLevelFailed();
            }
        }

        public void OnPuzzleSolved()
        {
            if (!_isPlaying) return;
            _isPlaying = false;
            
            _currentScore = Mathf.Max(10, Mathf.CeilToInt(_timeRemaining * 100));
            StartCoroutine(FinishSequence(true));
        }

        public void OnPuzzleFailed()
        {
            OnLevelFailed();
        }

        private void OnLevelFailed()
        {
            if (!_isPlaying) return;
            _isPlaying = false;

            StartCoroutine(FinishSequence(false));
        }

        private IEnumerator FinishSequence(bool win)
        {
            SetPuzzleUI(false);
            if (win)
            {
                if(_uiManager != null) _uiManager.HideHUD();
                yield return new WaitForSeconds(2f);
                OnLevelFinished?.Invoke(true);
            }
            else
            {
                _currentLives--;
                
                yield return new WaitForSeconds(1f);
                OnLevelFinished?.Invoke(false); 
            }
        }
        
        public void SetPuzzleUI(bool visible)
        {
            if (_currentPuzzle != null)
            {
                _currentPuzzle.SetUIVisibility(visible);
            }
        }

        private void ClearCurrentLevel()
        {
            if (_currentPuzzle != null && _currentPuzzle.gameObject != null)
            {
                Destroy(_currentPuzzle.gameObject);
            }
    
            _currentPuzzle = null;
        }
        
        [ContextMenu("Reset All Tutorials")]
        public void ResetTutorialsData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}