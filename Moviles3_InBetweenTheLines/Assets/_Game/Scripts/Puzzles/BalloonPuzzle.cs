using UnityEngine;
using _Game.Scripts.Core;
using _Game.Scripts.Core.InputSystem;

namespace _Game.Scripts.Puzzles
{
    public class BalloonPuzzle : PuzzleBase
    {
        [Header("Referencias Visuales")]
        [SerializeField] private Transform _balloonTransform;
        [SerializeField] private Transform _targetVisualTransform; // La esfera fantasma que indica la meta
        [SerializeField] private Renderer _balloonRenderer;
        [SerializeField] private GameObject _rhythmIndicator; // Objeto que marca el ritmo (Dificultad 2)
        [SerializeField] private GameObject _noozle;

        [Header("Configuración de Inflado")]
        [SerializeField] private float _sensitivity = 10.0f; // Qué tanto infla un soplido
        [SerializeField] private float _blowThreshold = 0.1f; // Volumen mínimo para detectar soplido
        [SerializeField] private float _maxScale = 2.5f;      // Si pasa de aquí, explota
        [SerializeField] private float _targetScale = 1.5f;   // El tamaño ideal
        [SerializeField] private float _targetTolerance = 0.2f; // Margen de error aceptable

        [Header("Configuración Específica")]
        [SerializeField] private float _leakRate = 0.3f;      // Velocidad de desinflado (Dif 1 y 2)
        [SerializeField] private float _rhythmSpeed = 3.0f;   // Velocidad del pulso (Dif 2)

        // Estado Interno
        private float _currentScale;
        private float _holdTimer = 0f;
        private bool _hasExploded = false;
        
        // Micrófono
        private AudioClip _micClip;
        private string _micDevice;
        private bool _isMicInitialized;

        // Ritmo
        private float _rhythmPhase;
        private bool _isInRhythmWindow;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            InitMicrophone();

            _currentScale = 0.2f;
            _holdTimer = 0f;
            _hasExploded = false;

            if (_targetVisualTransform != null)
                _targetVisualTransform.localScale = Vector3.one * _targetScale;
            if (_rhythmIndicator != null)
                _rhythmIndicator.SetActive(difficulty == 2);
        }

        private void Update()
        {
            if (isSolved || _hasExploded) return;

            float blowIntensity = GetBlowIntensity();
            switch (_currentDifficulty)
            {
                case 0:
                    if (blowIntensity > _blowThreshold)
                    {
                        Inflate(blowIntensity * Time.deltaTime);
                    }
                    break;

                case 1:
                    if (blowIntensity > _blowThreshold)
                    {
                        Inflate(blowIntensity * Time.deltaTime);
                    }
                    else
                    {
                        Deflate(_leakRate * Time.deltaTime);
                    }
                    break;

                case 2:
                    UpdateRhythm();
                    
                    if (blowIntensity > _blowThreshold)
                    {
                        if (_isInRhythmWindow)
                        {
                            Inflate(blowIntensity * Time.deltaTime);
                        }
                        else
                        {
                            Deflate(_leakRate * 2 * Time.deltaTime);
                        }
                    }
                    else
                    {
                        Deflate(_leakRate * Time.deltaTime);
                    }
                    break;
            }

            UpdateBalloonVisuals();
            CheckWinCondition();
        }

        public override void SetUIVisibility(bool isVisible)
        {
            if (_balloonTransform != null) _balloonTransform.gameObject.SetActive(isVisible);
            if (_targetVisualTransform != null) _targetVisualTransform.gameObject.SetActive(isVisible);
            if (_rhythmIndicator != null) _rhythmIndicator.SetActive(isVisible && _currentDifficulty == 2);
            if (_noozle!=null) _noozle.gameObject.SetActive(isVisible);
        }
        
        private void Inflate(float amount)
        {
            _currentScale += amount * _sensitivity;
        }

        private void Deflate(float amount)
        {
            _currentScale -= amount;
            if (_currentScale < 0.2f) _currentScale = 0.2f;
        }

        private void UpdateBalloonVisuals()
        {
            if (_balloonTransform != null)
            {
                _balloonTransform.localScale = Vector3.one * _currentScale;
            }
        }

        private void CheckWinCondition()
        {
            if (_currentScale > _maxScale)
            {
                ExplodeBalloon();
                return;
            }
            
            bool isInRange = Mathf.Abs(_currentScale - _targetScale) <= _targetTolerance;

            if (isInRange)
            {
                _holdTimer += Time.deltaTime;
                if(_balloonRenderer) _balloonRenderer.material.color = Color.green;

                if (_holdTimer >= 0.5f)
                {
                    CompletePuzzle();
                }
            }
            else
            {
                _holdTimer = 0f;
                if(_balloonRenderer)
                {
                    float tension = _currentScale / _maxScale;
                    _balloonRenderer.material.color = Color.Lerp(Color.blue, Color.red, tension);
                }
            }
        }

        private void ExplodeBalloon()
        {
            _hasExploded = true;
            if (_balloonTransform != null) _balloonTransform.gameObject.SetActive(false);
            Debug.Log("¡BOOM! El globo explotó.");
            FailPuzzle();
        }

        private void UpdateRhythm()
        {
            if (_rhythmIndicator == null) return;
            float pulse = Mathf.Sin(Time.time * _rhythmSpeed); 
            _isInRhythmWindow = pulse > 0.0f;
            float visualScale = Mathf.Lerp(0.5f, 1.5f, (pulse + 1f) / 2f);
            _rhythmIndicator.transform.localScale = Vector3.one * visualScale;
            
            Renderer r = _rhythmIndicator.GetComponent<Renderer>();
            if (r) r.material.color = _isInRhythmWindow ? Color.green : Color.gray;
        }

        private void InitMicrophone()
        {
            #if UNITY_EDITOR
                _isMicInitialized = true;
            #else
                if (Microphone.devices.Length > 0)
                {
                    _micDevice = Microphone.devices[0];
                    _micClip = Microphone.Start(_micDevice, true, 10, 44100);
                    _isMicInitialized = true;
                }
                else
                {
                    Debug.LogError("No se detectó micrófono");
                }
            #endif
        }

        private float GetBlowIntensity()
        {
            #if UNITY_EDITOR
            if (Input.GetKey(KeyCode.Space)) return 1.0f;
            return 0f;
            #else
                // En móvil, simplemente preguntamos al InputManager
                if (InputManager.Instance != null)
                {
                    // Clamp 0-1 para normalizar el valor y que no infle de golpe
                    return Mathf.Clamp01(InputManager.Instance.MicLoudness);
                }
                return 0f; 
            #endif
        }
        
        private void OnDestroy()
        {
            #if !UNITY_EDITOR
                Microphone.End(_micDevice);
            #endif
        }
    }
}