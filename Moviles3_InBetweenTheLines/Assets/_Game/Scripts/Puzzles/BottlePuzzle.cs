using UnityEngine;
using _Game.Scripts.Core.InputSystem;
using _Game.Scripts.Data;
using _Game.Scripts.Core;

namespace _Game.Scripts.Puzzles
{
    public class BottlePuzzle : PuzzleBase
    {
        [Header("Configuración Botella")]
        //[SerializeField] private Transform _bottleVisuals;
        [SerializeField] private float _pourSpeed = 0.5f;
        [SerializeField] private float _viscousMultiplier = 0.1f;
        [SerializeField] private float _shakeThreshold = 2.0f;

        // Variables de estado
        private float _currentFillAmount = 1.0f;
        //private bool _isViscousPhase = false;
        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            _currentFillAmount = 1.0f;
            //_isViscousPhase = false;
        }

        private void Update()
        {
            if (isSolved) return;

            HandleInput();
            //UpdateVisuals();
        }

        private void HandleInput()
        {
            Vector3 accel = InputManager.Instance.Acceleration;
            bool isShaking = InputManager.Instance.IsShaking(_shakeThreshold);

            bool isTipping = accel.y > 0.3f || accel.y < -0.8f;
            bool isPouringPosition = accel.y > -0.2f; 

            if (!isPouringPosition) return;
            float flowRate = 0f;

            switch (_currentDifficulty)
            {
                case 0:
                    flowRate = _pourSpeed;
                    break;

                case 1:
                    if (isShaking)
                    {
                        flowRate = _pourSpeed;
                    }
                    else
                    {
                        flowRate = _pourSpeed * _viscousMultiplier;
                    }
                    break;

                case 2:
                    if (_currentFillAmount > 0.5f)
                    {
                        flowRate = _pourSpeed;
                    }
                    else
                    {
                        if (isShaking)
                            flowRate = _pourSpeed;
                        else
                            flowRate = 0;
                    }
                    break;
            }

            _currentFillAmount -= flowRate * Time.deltaTime;

            if (_currentFillAmount <= 0)
            {
                _currentFillAmount = 0;
                CompletePuzzle();
            }
        }

        //private void UpdateVisuals()
        //{
        //    if (_bottleVisuals != null)
        //    {
        //        Quaternion targetRot = InputManager.Instance.GyroRotation;
        //        _bottleVisuals.rotation = Quaternion.Slerp(_bottleVisuals.rotation, targetRot, Time.deltaTime * 10f);
        //    }

        //    // renderer.material.SetFloat("_FillAmount", _currentFillAmount);
        //    
        //    if (_bottleVisuals != null)
        //    {
        //        
        //    }
        //}
    }
}