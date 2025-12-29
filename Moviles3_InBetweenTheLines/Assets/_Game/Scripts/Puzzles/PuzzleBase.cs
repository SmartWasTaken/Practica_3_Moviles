using UnityEngine;
using System;
using _Game.Scripts.Core;

namespace _Game.Scripts.Puzzles
{
    public abstract class PuzzleBase : MonoBehaviour
    {
        public event Action OnLevelCompleted;

        protected LevelManager _levelManager; 
        protected bool isSolved = false;
        
        protected int _currentDifficulty; 

        protected void CompletePuzzle()
        {
            if (isSolved) return;
            
            isSolved = true;
            
            OnLevelCompleted?.Invoke();

            if (_levelManager != null) 
            {
                _levelManager.OnPuzzleSolved();
            }
        }

        public virtual void Initialize(LevelManager manager, int difficulty) 
        {
            _levelManager = manager;
            _currentDifficulty = difficulty;
            isSolved = false;
        }

        protected void FailPuzzle()
        {
            if (_levelManager != null)
            {
                _levelManager.OnPuzzleFailed();
            }
        }
    }
}