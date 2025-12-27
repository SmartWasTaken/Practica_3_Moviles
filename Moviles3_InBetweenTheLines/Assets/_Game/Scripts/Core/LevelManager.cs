using UnityEngine;
using System.Collections;
using _Game.Scripts.Data;
using _Game.Scripts.Puzzles;
using System;
using _Game.Scripts.Core.UI;

namespace _Game.Scripts.Core.Game
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
            _timeRemaining = config.timeLimit; 
            
            _mainCamera.backgroundColor = config.backgroundColor;
            
            if (_uiManager != null)
            {
                _uiManager.SetupLevelUI(config.riddleText, _timeRemaining);
            }

            if (config.puzzlePrefab != null)
            {
                _currentPuzzle = Instantiate(config.puzzlePrefab, _puzzleSpawnPoint);
                _currentPuzzle.Initialize(this); 
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

        // Métodos llamados por el PuzzleBase
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
            if(_uiManager != null) _uiManager.HideHUD();
            // Aquí vamos a lanzar partículas, sonidos, esperamos 2 segundos y pasamos al siguiente nivel
            yield return new WaitForSeconds(2f);
            
            // Lógica para volver al menú o cargar siguiente
            OnLevelFinished?.Invoke(win);
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