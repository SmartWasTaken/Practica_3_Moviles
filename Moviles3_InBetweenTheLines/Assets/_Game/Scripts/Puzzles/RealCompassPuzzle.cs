using UnityEngine;
using _Game.Scripts.Core;
using UnityEngine.UI;

namespace _Game.Scripts.Puzzles
{
    public class CheckpointCompassPuzzle : PuzzleBase
    {
        [Header("Referencias UI")]
        [SerializeField] private RectTransform _rotatingDial;   // El disco con letras N,S,E,O
        [SerializeField] private Image _staticNeedleImage;      // La flecha fija del centro

        [Header("Configuración")]
        [SerializeField] private float _compassSmoothTime = 0.1f; // El gyro es rápido, bajamos el suavizado
        [SerializeField] private float _tolerance = 20f; 
        [SerializeField] private float _holdTimeForStaticLevels = 1.0f;

        // ESTADO
        private float _currentHeading; // 0 a 360
        private float _compassVelocity;
        
        // Variables Giroscopio
        private float _initialHeadingOffset = 0f; // Para calibrar el "0" al inicio

        // Checkpoints
        private bool _hasVisitedEast = false;
        private float _holdTimer = 0f;
        private bool _isWinning = false;

        // Debug PC
        private float _simulatedHeading = 0f;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            // 1. ACTIVAR GIROSCOPIO (En vez de Brújula)
            Input.gyro.enabled = true;

            // 2. CALIBRAR EL "NORTE"
            // Guardamos hacia dónde mira el móvil AHORA MISMO para que eso sea el "0"
            // Damos un pequeño delay o tomamos el valor inicial
            _initialHeadingOffset = GetRawGyroY(); 

            // Reseteamos estados
            _hasVisitedEast = false;
            _holdTimer = 0f;
            _isWinning = false;
            _simulatedHeading = 0f; // Reset simulación PC
        }

        private void Update()
        {
            if (isSolved || _isWinning) return;

            // 1. INPUT (GIROSCOPIO RELATIVO)
            float rawHeading = GetRelativeHeading();
            
            // Suavizamos un poco, aunque el gyro ya es suave
            _currentHeading = Mathf.SmoothDampAngle(_currentHeading, rawHeading, ref _compassVelocity, _compassSmoothTime);

            // 2. LÓGICA (Igual que antes)
            CheckLogic();

            // 3. VISUALES
            UpdateUI();
        }

        private void CheckLogic()
        {
            float targetAngle = 0f;
            
            switch (_currentDifficulty)
            {
                case 0: targetAngle = 90f; break;  // Busca Derecha
                case 1: targetAngle = 270f; break; // Busca Izquierda
                case 2: targetAngle = _hasVisitedEast ? 270f : 90f; break; // Checkpoint
            }

            float angleDiff = Mathf.DeltaAngle(_currentHeading, targetAngle);
            bool isAligned = Mathf.Abs(angleDiff) < _tolerance;

            if (isAligned)
            {
                if (_currentDifficulty == 2)
                {
                    if (!_hasVisitedEast)
                    {
                        _hasVisitedEast = true; // Checkpoint conseguido
                        // Feedback: Vibración pequeña si quieres
                    }
                    else
                    {
                        WinLevel(); // Final conseguido
                    }
                }
                else
                {
                    _holdTimer += Time.deltaTime;
                    if (_holdTimer >= _holdTimeForStaticLevels) WinLevel();
                }
            }
            else
            {
                if (_currentDifficulty != 2) _holdTimer = 0f;
            }
        }

        private void WinLevel()
        {
            _isWinning = true;
            if(_staticNeedleImage) _staticNeedleImage.color = Color.green;
            CompletePuzzle();
        }

        private void UpdateUI()
        {
            // 1. GIRAR EL DISCO
            if (_rotatingDial != null)
            {
                _rotatingDial.localRotation = Quaternion.Euler(0, 0, _currentHeading);
            }

            // 2. COLOREAR LA FLECHA
            if (_staticNeedleImage != null && !_isWinning)
            {
                if (_currentDifficulty == 2)
                {
                    if (_hasVisitedEast) _staticNeedleImage.color = Color.yellow; // Buscando OESTE
                    else _staticNeedleImage.color = Color.white;  // Buscando ESTE
                }
                else
                {
                    // Lógica Dificultad 0 y 1
                    float target = _currentDifficulty == 0 ? 90f : 270f;
                    bool aligned = Mathf.Abs(Mathf.DeltaAngle(_currentHeading, target)) < _tolerance;
                    _staticNeedleImage.color = aligned ? Color.green : Color.white;
                }
            }
        }

        // --- MAGIA DEL GIROSCOPIO ---

        // Obtiene el ángulo Y actual del giroscopio (0-360)
        private float GetRawGyroY()
        {
            #if UNITY_EDITOR
                return _simulatedHeading;
            #else
                // Convertimos la actitud del gyro (que es complicada) a una rotación de Unity
                Quaternion q = Input.gyro.attitude;
                // Reorientar para que coincida con el sistema de coordenadas de Unity
                Quaternion rot = new Quaternion(q.x, q.y, -q.z, -q.w);
                return rot.eulerAngles.y; // Devolvemos la rotación en el eje vertical
            #endif
        }

        // Calcula el ángulo relativo al inicio del nivel
        private float GetRelativeHeading()
        {
            float currentRaw = GetRawGyroY();
            
            #if UNITY_EDITOR
                // En el editor simulamos directamente el heading relativo
                float speed = 150f * Time.deltaTime;
                if (Input.GetKey(KeyCode.D)) _simulatedHeading += speed; // Derecha
                if (Input.GetKey(KeyCode.A)) _simulatedHeading -= speed; // Izquierda
                return _simulatedHeading; 
            #else
                // En móvil: Restamos el offset inicial para que al empezar sea 0
                // DeltaAngle maneja el salto de 360 a 0 automáticamente
                float relative = Mathf.DeltaAngle(_initialHeadingOffset, currentRaw);
                
                // Convertimos de -180/180 a 0/360 si prefieres, o lo dejamos así.
                // Mathf.DeltaAngle devuelve -180 a 180.
                // Si el gyro gira a la derecha, el ángulo aumenta.
                
                // Importante: El sentido de giro del Gyro a veces es inverso al de la brújula visual.
                // Si ves que al girar a la derecha la brújula gira al revés, pon un menos: return -relative;
                return relative;
            #endif
        }
        
        public override void SetUIVisibility(bool isVisible)
        {
            if (_rotatingDial != null && _rotatingDial.parent != null)
                _rotatingDial.parent.gameObject.SetActive(isVisible);
        }
    }
}