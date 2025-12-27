using UnityEngine;
using System;
using _Game.Scripts.Core.Game; // 1. NECESARIO PARA VER EL LEVELMANAGER

namespace _Game.Scripts.Puzzles
{
    public abstract class PuzzleBase : MonoBehaviour
    {
        public event Action OnLevelCompleted;

        protected LevelManager _levelManager; 
        protected bool isSolved = false;

        protected void CompletePuzzle()
        {
            if (isSolved) return;
            
            isSolved = true;
            Debug.Log($"[PuzzleBase] Nivel '{gameObject.name}' completado.");
            
            OnLevelCompleted?.Invoke();

            if (_levelManager != null) 
            {
                _levelManager.OnPuzzleSolved();
            }
        }

        public virtual void Initialize(LevelManager manager) 
        {
            _levelManager = manager;
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