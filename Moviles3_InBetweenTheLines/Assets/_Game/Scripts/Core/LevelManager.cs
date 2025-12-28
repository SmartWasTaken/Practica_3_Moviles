using UnityEngine;
using System.Collections;
using _Game.Scripts.Data;
using _Game.Scripts.Puzzles;
using System;
using _Game.Scripts.Core.UI;

namespace _Game.Scripts.Core
{
    public class LevelManager : MonoBehaviour
    {
        public event Action<bool> OnLevelFinished;
        
        [Header("Referencias")]
        [SerializeField] private Transform _puzzleSpawnPoint;
        [SerializeField] private Camera _mainCamera;
        
        [Header("UI Referencias")]
        [SerializeField] private GameUIManager _uiManager;

        private PuzzleBase _currentPuzzle;
        private LevelConfig _currentConfig;
        private float _timeRemaining;
        private bool _isPlaying;

        public void LoadLevel(LevelConfig config)
        {
            ClearCurrentLevel();

            _currentConfig = config;

            string textToShow = "";
            int difficulty = 0;

            if (config.variations != null && config.variations.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, config.variations.Count);
                LevelVariation variation = config.GetVariation(randomIndex);
                
                _timeRemaining = variation.timeLimit;
                textToShow = variation.riddleText;
                difficulty = variation.difficultyLevel;
            }
            else
            {
                Debug.LogWarning("El nivel no tiene variaciones configuradas. Usando valores por defecto.");
                _timeRemaining = 10f; 
                textToShow = "Configura las variaciones";
            }
            
            _mainCamera.backgroundColor = config.backgroundColor;
            
            if (_uiManager != null)
            {
                _uiManager.SetupLevelUI(textToShow, _timeRemaining);
            }

            if (config.puzzlePrefab != null)
            {
                _currentPuzzle = Instantiate(config.puzzlePrefab, _puzzleSpawnPoint);
                _currentPuzzle.Initialize(this, difficulty); 
            }

            _isPlaying = true;
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
            
            Debug.Log("NIVEL COMPLETADO");
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

            Debug.Log("GAME OVER");
            StartCoroutine(FinishSequence(false));
        }

        private IEnumerator FinishSequence(bool win)
        {
            if (win)
            {
                if(_uiManager != null) _uiManager.HideHUD();
                // Lanzar partículas, sonidos...
                yield return new WaitForSeconds(2f);
                OnLevelFinished?.Invoke(true);
            }
            else
            {
                if(_uiManager != null) _uiManager.ShowGameOver();
                
            }
        }

        private void ClearCurrentLevel()
        {
            if (_currentPuzzle != null)
            {
                Destroy(_currentPuzzle.gameObject);
                _currentPuzzle = null;
            }
        }
    }
}