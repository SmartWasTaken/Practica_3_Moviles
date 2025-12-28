using UnityEngine;
using System.Collections;
using _Game.Scripts.Data;
using _Game.Scripts.Puzzles;
using System;
using _Game.Scripts.Core.UI;
using _Game.Scripts.Core.Game;

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
        
        // Estado del juego
        private float _timeRemaining;
        private bool _isPlaying;
        private int _currentLives = 3;

        private void Awake()
        {
            ResetLives();
        }

        public void ResetLives()
        {
            _currentLives = 3;
            // Aquí deberías actualizar los corazones en la UI si fuera necesario
            // if(_uiManager != null) _uiManager.UpdateLives(_currentLives);
        }

        public void LoadLevel(LevelConfig config)
        {
            ClearCurrentLevel();
            _currentConfig = config;

            string textToShow = "";
            int difficulty = 0;
            TutorialType requiredTutorial = TutorialType.None; 

            if (config.variations != null && config.variations.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, config.variations.Count);
                LevelVariation variation = config.GetVariation(randomIndex);
                
                _timeRemaining = variation.timeLimit;
                textToShow = variation.riddleText;
                difficulty = variation.difficultyLevel;
                requiredTutorial = variation.tutorialRequired;
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

            _isPlaying = false;
            
            TutorialManager.Instance.TryShowTutorial(requiredTutorial, () => 
            {
                Debug.Log("Tutorial cerrado o no necesario. Empieza el juego.");
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

            StartCoroutine(FinishSequence(false));
        }

        private IEnumerator FinishSequence(bool win)
        {
            if (win)
            {
                if(_uiManager != null) _uiManager.HideHUD();
                
                
                yield return new WaitForSeconds(2f);
                OnLevelFinished?.Invoke(true);
            }
            else
            {
                // --- LÓGICA DE VIDAS ---
                _currentLives--;
                Debug.Log($"Vida perdida. Restantes: {_currentLives}");
                

                if (_currentLives <= 0)
                {
                    Debug.Log("GAME OVER REAL - Sin vidas");
                    if(_uiManager != null) _uiManager.ShowGameOver();
                    
                }
                else
                {
                    Debug.Log("Reintentando nivel...");
                    yield return new WaitForSeconds(1f);
                    OnLevelFinished?.Invoke(false); 
                }
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
        
        [ContextMenu("Reset All Tutorials")]
        public void ResetTutorialsData()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.LogWarning("PlayerPrefs borrados. Los tutoriales volverán a salir.");
        }
    }
}