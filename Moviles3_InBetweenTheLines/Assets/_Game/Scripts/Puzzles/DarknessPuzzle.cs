using UnityEngine;
using _Game.Scripts.Core;
using UnityEngine.UI;

namespace _Game.Scripts.Puzzles
{
    public class DarknessPuzzle : PuzzleBase
    {
        [Header("Referencias Visuales")]
        [SerializeField] private Image _darknessPanel; // El panel negro que tapa la pantalla

        [Header("Configuración Var 2 (Agitar)")]
        [SerializeField] private float _shakeThreshold = 2.0f; // Sensibilidad del agitado
        [SerializeField] private float _shakeEnergyNeeded = 10.0f; // Cuánto hay que agitar para ganar

        [Header("Configuración Var 3 (Frotar)")]
        [SerializeField] private float _rubSensitivity = 0.5f; // Cuánto cuenta cada movimiento de dedo
        [SerializeField] private float _rubEnergyNeeded = 500.0f; // Distancia total a frotar en pantalla

        private float _currentEnergy = 0f;
        
        private float _editorBrightness = 0.5f;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            _currentEnergy = 0f;
            if (_darknessPanel != null)
            {
                _darknessPanel.color = Color.black;
                _darknessPanel.gameObject.SetActive(true);
            }
        }

        public override void SetUIVisibility(bool isVisible)
        {
            if (_darknessPanel != null)
            {
                _darknessPanel.gameObject.SetActive(isVisible);
            }
        }

        private void Update()
        {
            if (isSolved) return;

            switch (_currentDifficulty)
            {
                case 0:
                    CheckBrightness();
                    break;
                case 1:
                    CheckShake();
                    break;
                case 2:
                    CheckRubbing();
                    break;
            }
        }
        
        private void CheckBrightness()
        {
            float brightness = GetCurrentBrightness();
            UpdateDarknessAlpha(1f - brightness); 
            if (brightness > 0.7f) 
            {
                WinLevel();
            }
        }

        private void CheckShake()
        {
            Vector3 acceleration = Input.acceleration;
            
            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space)) acceleration = new Vector3(3, 3, 3);
            #endif
            
            if (acceleration.sqrMagnitude >= _shakeThreshold * _shakeThreshold)
            {
                _currentEnergy += Time.deltaTime * 5f;
            }
            else
            {
                _currentEnergy -= Time.deltaTime * 0.5f;
            }

            _currentEnergy = Mathf.Clamp(_currentEnergy, 0, _shakeEnergyNeeded);
            UpdateDarknessAlpha(1f - (_currentEnergy / _shakeEnergyNeeded));

            if (_currentEnergy >= _shakeEnergyNeeded)
            {
                WinLevel();
            }
        }

        private void CheckRubbing()
        {
            float rubDelta = 0f;

            #if UNITY_EDITOR
                if (Input.GetMouseButton(0))
                {
                    rubDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).magnitude * 10f;
                }
            #else
                if (Input.touchCount > 0)
                {
                    foreach (Touch t in Input.touches)
                    {
                        if (t.phase == TouchPhase.Moved)
                        {
                            rubDelta += t.deltaPosition.magnitude;
                        }
                    }
                }
            #endif

            if (rubDelta > 0)
            {
                _currentEnergy += rubDelta * _rubSensitivity;
            }
            
            UpdateDarknessAlpha(1f - (_currentEnergy / _rubEnergyNeeded));

            if (_currentEnergy >= _rubEnergyNeeded)
            {
                WinLevel();
            }
        }

        private void WinLevel()
        {
            SetUIVisibility(false);
            CompletePuzzle();
        }

        private void UpdateDarknessAlpha(float alpha)
        {
            if (_darknessPanel != null)
            {
                Color c = _darknessPanel.color;
                c.a = Mathf.Clamp01(alpha);
                _darknessPanel.color = c;
            }
        }

        private float GetCurrentBrightness()
        {
            #if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.UpArrow)) _editorBrightness += 0.1f;
                if (Input.GetKeyDown(KeyCode.DownArrow)) _editorBrightness -= 0.1f;
                return Mathf.Clamp01(_editorBrightness);
            #else
                if (Screen.brightness < 0) return 0f;
                return Mathf.Clamp01(Screen.brightness);
            #endif
        }
    }
}