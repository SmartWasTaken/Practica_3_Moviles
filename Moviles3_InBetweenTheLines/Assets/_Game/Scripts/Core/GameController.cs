using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using _Game.Scripts.Core.Game;
using _Game.Scripts.Data;
using _Game.Scripts.Core.UI;

namespace _Game.Scripts.Core
{
    public class GameController : MonoBehaviour
    {
        [Header("Gestores")]
        [SerializeField] private LevelManager _levelManager;
        [SerializeField] private ScoreScreen _scoreScreen;
        
        [Header("Referencias UI")]
        [SerializeField] private GameObject _gameOverPanel;

        [Header("Banco de Niveles")]
        [Tooltip("Arrastra aquí TODOS tus niveles.")]
        [SerializeField] private List<LevelConfig> _allLevels; 

        [Header("Ajustes")]
        [SerializeField] private int _pityScore = 50;
        
        [Header("--- MODO DEBUG ---")]
        [Tooltip("Si pones un nivel aquí, se ignora la lógica de fases.")]
        [SerializeField] private LevelConfig _debugLevel; 
        [SerializeField] private int _debugDifficulty = 0;

        private LevelConfig _currentLevelConfig;
        private int _totalAccumulatedScore = 0;
        private int _levelsCompletedCount = 0;
        private bool _isGameActive = false;

        private int _currentPhaseDifficulty = 0; // 0, 1, 2. Si es 3+ -> Modo Batiburrillo
        private List<LevelConfig> _remainingLevelsInPhase = new List<LevelConfig>();

        private void Awake()
        {
            if (_gameOverPanel == null)
            {
                var panelScript = FindFirstObjectByType<GameOverPanel>(FindObjectsInactive.Include);
                if (panelScript != null) _gameOverPanel = panelScript.gameObject;
            }
        }

        private void Start()
        {
            Application.targetFrameRate = 60;

            if (_allLevels.Count == 0)
            {
                Debug.LogError("GAME CONTROLLER: ¡No has asignado niveles en _allLevels!");
                return;
            }

            _levelManager.OnLevelFinished -= HandleLevelFinished;
            _levelManager.OnLevelFinished += HandleLevelFinished;

            _totalAccumulatedScore = 0;
            _levelsCompletedCount = 0;
            
            // Inicializar Fase 0
            _currentPhaseDifficulty = 0;
            RefillLevelPool();

            StartCoroutine(StartGameRoutine());
        }

        private void OnDestroy()
        {
            if (_levelManager != null) _levelManager.OnLevelFinished -= HandleLevelFinished;
        }

        private IEnumerator StartGameRoutine()
        {
            yield return null;
            LoadNextLevelBasedOnProgression();
        }

        private void LoadNextLevelBasedOnProgression()
        {
            if (_debugLevel != null)
            {
                Debug.LogWarning($"[DEBUG MODE] Forzando: {_debugLevel.name} Dif: {_debugDifficulty}");
                PlayLevel(_debugLevel, _debugDifficulty);
                return;
            }
            
            if (_currentPhaseDifficulty > 2)
            {
                LevelConfig randomLevel = _allLevels[Random.Range(0, _allLevels.Count)];
                // Elegimos CUALQUIER dificultad al azar (0, 1 o 2)
                int randomDiff = Random.Range(0, 3);
                
                Debug.Log($"[ENDLESS] Nivel: {randomLevel.name} | Dif: {randomDiff}");
                PlayLevel(randomLevel, randomDiff);
                return;
            }

            if (_remainingLevelsInPhase.Count == 0)
            {
                _currentPhaseDifficulty++;
                if (_currentPhaseDifficulty > 2)
                {
                    LoadNextLevelBasedOnProgression(); 
                    return;
                }
                RefillLevelPool();
            }
            
            int randomIndex = Random.Range(0, _remainingLevelsInPhase.Count);
            LevelConfig configToLoad = _remainingLevelsInPhase[randomIndex];
            _remainingLevelsInPhase.RemoveAt(randomIndex);

            Debug.Log($"[PHASE {_currentPhaseDifficulty}] Nivel: {configToLoad.name} | Restantes: {_remainingLevelsInPhase.Count}");
            PlayLevel(configToLoad, _currentPhaseDifficulty);
        }

        private void PlayLevel(LevelConfig config, int difficulty)
        {
            _currentLevelConfig = config;
            _isGameActive = true;
            _levelManager.LoadLevel(config, difficulty);
        }

        private void RefillLevelPool()
        {
            _remainingLevelsInPhase.Clear();
            _remainingLevelsInPhase.AddRange(_allLevels);
            Debug.Log($"--- NUEVA FASE: DIFICULTAD {_currentPhaseDifficulty} --- Bolsa recargada.");
        }

        private void HandleLevelFinished(bool playerWon)
        {
            if (!_isGameActive) return;
            _isGameActive = false;
            
            if (_levelManager != null)
            {
                _levelManager.SetPuzzleUI(false);
            }

            int levelScore = _levelManager.CurrentScore;
            int currentLives = _levelManager.CurrentLives;
            int previousTotal = _totalAccumulatedScore;
            int pointsToAnim = playerWon ? levelScore : _pityScore;
            
            _totalAccumulatedScore += pointsToAnim;

            _scoreScreen.AnimateSequence(
                livesAfterGame: currentLives,
                scoreEarned: pointsToAnim,
                previousTotalScore: previousTotal,
                isWin: playerWon,
                isRecord: false, 
                onComplete: () => {
                    
                    if(_scoreScreen != null) _scoreScreen.gameObject.SetActive(false);
                    
                    if (currentLives > 0)
                    {
                        if(playerWon)
                        {
                            _levelsCompletedCount++; 
                        }
                        LoadNextLevelBasedOnProgression();
                    }
                    else
                    {
                        ScoreManager.SaveScore("Global_Ranking", _totalAccumulatedScore);
                        _gameOverPanel.SetActive(true);
                    }
                }
            );
        }
    }
}