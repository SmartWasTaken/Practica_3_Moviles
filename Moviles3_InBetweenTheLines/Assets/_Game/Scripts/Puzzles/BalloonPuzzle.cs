using UnityEngine;
using _Game.Scripts.Core;

namespace _Game.Scripts.Puzzles
{
    public class BalloonPuzzle : PuzzleBase
    {
        [Header("Referencias Visuales")]
        [SerializeField] private Transform _balloonTransform;
        [SerializeField] private Transform _targetVisualTransform; // La esfera fantasma que indica la meta
        [SerializeField] private Renderer _balloonRenderer;
        [SerializeField] private GameObject _rhythmIndicator; // Objeto que marca el ritmo (Dificultad 2)

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
        }
        
        private void Inflate(float amount)
        {
            _currentScale += amount * _sensitivity;
        }

        private void Deflate(float amount)
        {
            _currentScale -= amount;
            if (_currentScale < 0.2f) _currentScale = 0.2f; // No puede ser más pequeño que el inicio
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
            // 1. CONDICIÓN DE DERROTA: EXPLOSIÓN
            if (_currentScale > _maxScale)
            {
                ExplodeBalloon();
                return;
            }

            // 2. CONDICIÓN DE VICTORIA: TAMAÑO CORRECTO
            // Verificamos si estamos dentro del rango (Target +/- Tolerancia)
            bool isInRange = Mathf.Abs(_currentScale - _targetScale) <= _targetTolerance;

            if (isInRange)
            {
                _holdTimer += Time.deltaTime;
                
                // Feedback visual: Poner verde si está en rango
                if(_balloonRenderer) _balloonRenderer.material.color = Color.green;

                if (_holdTimer >= 0.5f)
                {
                    CompletePuzzle();
                }
            }
            else
            {
                _holdTimer = 0f;
                // Feedback visual: Color normal (rojo/azul) o de tensión si está muy grande
                if(_balloonRenderer)
                {
                    // Interpolamos de Azul (pequeño) a Rojo (a punto de explotar)
                    float tension = _currentScale / _maxScale;
                    _balloonRenderer.material.color = Color.Lerp(Color.blue, Color.red, tension);
                }
            }
        }

        private void ExplodeBalloon()
        {
            _hasExploded = true;
            if (_balloonTransform != null) _balloonTransform.gameObject.SetActive(false); // Desaparece
            
            // Aquí llamarías a tu sistema de Game Over o reinicio
            Debug.Log("¡BOOM! El globo explotó.");
            
            // En tu juego real, aquí lanzarías el evento de fallo o reiniciarías el nivel
            // _levelManager.FailLevel(); // Si tienes algo así
        }

        // --- LÓGICA DE RITMO (DIFICULTAD 2) ---
        private void UpdateRhythm()
        {
            if (_rhythmIndicator == null) return;

            // Oscilación senoidal para marcar el ritmo
            // Usamos Time.time * velocidad
            float pulse = Mathf.Sin(Time.time * _rhythmSpeed); 
            
            // Definimos que la "ventana buena" es cuando el pulso está alto (> 0.5)
            _isInRhythmWindow = pulse > 0.0f;

            // Feedback visual del ritmo (escalar una bolita o cambiar color)
            float visualScale = Mathf.Lerp(0.5f, 1.5f, (pulse + 1f) / 2f);
            _rhythmIndicator.transform.localScale = Vector3.one * visualScale;
            
            Renderer r = _rhythmIndicator.GetComponent<Renderer>();
            if (r) r.material.color = _isInRhythmWindow ? Color.green : Color.gray;
        }

        // --- LÓGICA DE MICRÓFONO ---

        private void InitMicrophone()
        {
            #if UNITY_EDITOR
                _isMicInitialized = true; // En editor usaremos teclas
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
                // SIMULACIÓN EN PC: Mantener ESPACIO es soplar
                if (Input.GetKey(KeyCode.Space)) return 1.0f;
                return 0f;
            #else
                if (!_isMicInitialized || _micClip == null) return 0f;

                // Analizamos los últimos datos de audio para sacar el volumen promedio (RMS)
                int sampleSize = 128;
                float[] samples = new float[sampleSize];
                int startPosition = Microphone.GetPosition(_micDevice) - (sampleSize + 1);
                
                if (startPosition < 0) return 0f;

                _micClip.GetData(samples, startPosition);

                float sum = 0;
                for (int i = 0; i < sampleSize; i++)
                {
                    sum += samples[i] * samples[i]; // Cuadrado de la amplitud
                }
                
                float rmsValue = Mathf.Sqrt(sum / sampleSize); // Raíz cuadrada del promedio
                
                // Amplificamos el valor porque el RMS suele ser muy bajo (0.01 - 0.1)
                return Mathf.Clamp01(rmsValue * 10f); 
            #endif
        }
        
        // Limpieza del micro al salir
        private void OnDestroy()
        {
            #if !UNITY_EDITOR
                Microphone.End(_micDevice);
            #endif
        }
    }
}