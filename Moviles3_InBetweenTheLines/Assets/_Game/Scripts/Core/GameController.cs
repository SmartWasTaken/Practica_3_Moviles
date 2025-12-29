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
        [SerializeField] private GameObject _gameOverPanel;

        [Header("Banco de Niveles")]
        [Tooltip("Arrastra aquí TODOS tus niveles. El código elegirá la dificultad interna.")]
        [SerializeField] private List<LevelConfig> _allLevels; 

        [Header("Ajustes")]
        [SerializeField] private int _pityScore = 50;

        private int _totalAccumulatedScore = 0;
        private int _levelsCompletedCount = 0;
        private bool _isGameActive = false;

        private void Start()
        {
            Application.targetFrameRate = 60;

            if (_allLevels.Count == 0)
            {
                return;
            }

            _levelManager.OnLevelFinished -= HandleLevelFinished;
            _levelManager.OnLevelFinished += HandleLevelFinished;

            _totalAccumulatedScore = 0;
            _levelsCompletedCount = 0;

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
            int targetDifficulty = 0;

            if (_levelsCompletedCount < 4)
            {
                targetDifficulty = 0; 
            }
            else if (_levelsCompletedCount < 9)
            {
                targetDifficulty = 1; 
            }
            else
            {
                targetDifficulty = 2; 
            }

            int randomIndex = Random.Range(0, _allLevels.Count);
            LevelConfig configToLoad = _allLevels[randomIndex];

            _isGameActive = true;
            
            _levelManager.LoadLevel(configToLoad, targetDifficulty);
        }

        private void HandleLevelFinished(bool playerWon)
        {
            if (!_isGameActive) return;
            _isGameActive = false;

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
                        Debug.Log("Game Over");
                        _gameOverPanel.SetActive(true);
                    }
                }
            );
        }
    }
}