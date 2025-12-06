using UnityEngine;
using System;

namespace _Game.Scripts.Puzzles
{
    public abstract class PuzzleBase : MonoBehaviour
    {
        public event Action OnLevelCompleted;

        protected bool isSolved = false;

        protected void CompletePuzzle()
        {
            if (isSolved) return;
            
            isSolved = true;
            Debug.Log($"[PuzzleBase] Nivel '{gameObject.name}' completado.");
            
            OnLevelCompleted?.Invoke();
        }

        public virtual void Initialize() 
        {
            isSolved = false;
        }
    }
}