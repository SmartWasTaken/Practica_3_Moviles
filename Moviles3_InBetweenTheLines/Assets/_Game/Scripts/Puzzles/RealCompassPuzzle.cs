using UnityEngine;
using _Game.Scripts.Core;
using UnityEngine.UI;

namespace _Game.Scripts.Puzzles
{
    public class RealCompassPuzzle : PuzzleBase
    {
        [Header("Referencias UI")]
        [SerializeField] private RectTransform _rotatingDial;   // El disco con letras N,S,E,O
        [SerializeField] private Image _staticNeedleImage;      // La flecha fija del centro

        [Header("Configuración")]
        [SerializeField] private float _compassSmoothTime = 0.2f;
        [SerializeField] private float _tolerance = 20f; // Margen de error (grados)
        [SerializeField] private float _holdTimeForStaticLevels = 1.0f; // Tiempo para niveles 0 y 1

        // ESTADO INTERNO
        private float _currentHeading;
        private float _compassVelocity;
        
        // Checkpoints
        private bool _hasVisitedEast = false; // ¿Ya hemos pasado por el checkpoint?
        private float _holdTimer = 0f;        // Para niveles 0 y 1
        private bool _isWinning = false;      // Para bloquear lógica al ganar

        // Debug PC
        private float _simulatedHeading = 0f;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            Input.compass.enabled = true;
            Input.location.Start();

            // Reseteamos estados
            _hasVisitedEast = false;
            _holdTimer = 0f;
            _isWinning = false;
            
            // Si quieres probar directamente el estado amarillo en el editor, cambia esto a true temporalmente
            // _hasVisitedEast = true; 
        }

        private void Update()
        {
            if (isSolved || _isWinning) return;

            // 1. INPUT
            float rawHeading = GetHeading();
            _currentHeading = Mathf.SmoothDampAngle(_currentHeading, rawHeading, ref _compassVelocity, _compassSmoothTime);

            // 2. LÓGICA
            CheckLogic();

            // 3. VISUALES
            UpdateUI();
        }

        private void CheckLogic()
        {
            // Definimos el objetivo según la dificultad y el estado actual
            float targetAngle = 0f;
            
            switch (_currentDifficulty)
            {
                case 0: targetAngle = 90f; break;  // Solo Este
                case 1: targetAngle = 270f; break; // Solo Oeste
                case 2: 
                    // Si NO hemos visitado el Este, el objetivo es 90.
                    // Si YA lo visitamos, el objetivo cambia a 270.
                    targetAngle = _hasVisitedEast ? 270f : 90f; 
                    break;
            }

            // Comprobamos si estamos mirando al objetivo
            float angleDiff = Mathf.DeltaAngle(_currentHeading, targetAngle);
            bool isAligned = Mathf.Abs(angleDiff) < _tolerance;

            if (isAligned)
            {
                if (_currentDifficulty == 2)
                {
                    // LÓGICA NIVEL 2 (CHECKPOINT)
                    if (!_hasVisitedEast)
                    {
                        // FASE 1: Acabamos de encontrar el ESTE
                        _hasVisitedEast = true;
                        // Opcional: Vibrar aquí para dar feedback de "Checkpoint"
                        // Handheld.Vibrate(); 
                    }
                    else
                    {
                        // FASE 2: Ya teníamos el Este, y ahora estamos en el OESTE
                        // ¡VICTORIA!
                        _isWinning = true;
                        if(_staticNeedleImage) _staticNeedleImage.color = Color.green; // Feedback instantáneo
                        CompletePuzzle();
                    }
                }
                else
                {
                    // LÓGICA NIVEL 0 y 1 (AGUANTAR)
                    _holdTimer += Time.deltaTime;
                    if (_holdTimer >= _holdTimeForStaticLevels)
                    {
                        _isWinning = true;
                        if(_staticNeedleImage) _staticNeedleImage.color = Color.green;
                        CompletePuzzle();
                    }
                }
            }
            else
            {
                // Si nos desalineamos en niveles 0 y 1, reseteamos timer
                if (_currentDifficulty != 2) _holdTimer = 0f;
            }
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
                    // ESTADO NIVEL 2
                    if (_hasVisitedEast)
                    {
                        // Checkpoint conseguido (buscando Oeste) -> AMARILLO
                        _staticNeedleImage.color = Color.yellow;
                    }
                    else
                    {
                        // Empezando (buscando Este) -> BLANCO
                        // Opcional: Si el jugador se alinea con el Este (antes de que cambie),
                        // podrías ponerlo verde momentáneamente, pero Blanco está bien para "Neutro".
                        _staticNeedleImage.color = Color.white;
                    }
                }
                else
                {
                    // ESTADO NIVEL 0 y 1
                    // Calculamos alineación solo para pintar la flecha
                    float target = _currentDifficulty == 0 ? 90f : 270f;
                    bool aligned = Mathf.Abs(Mathf.DeltaAngle(_currentHeading, target)) < _tolerance;
                    
                    _staticNeedleImage.color = aligned ? Color.green : Color.white;
                }
            }
        }

        private float GetHeading()
        {
            #if UNITY_EDITOR
                float speed = 150f * Time.deltaTime; // Un poco más rápido para testear
                if (Input.GetKey(KeyCode.D)) _simulatedHeading += speed;
                if (Input.GetKey(KeyCode.A)) _simulatedHeading -= speed;
                
                if(_simulatedHeading >= 360) _simulatedHeading -= 360;
                if(_simulatedHeading < 0) _simulatedHeading += 360;
                return _simulatedHeading;
            #else
                return Input.compass.magneticHeading;
            #endif
        }
        
        public override void SetUIVisibility(bool isVisible)
        {
            if (_rotatingDial != null && _rotatingDial.parent != null)
                _rotatingDial.parent.gameObject.SetActive(isVisible);
        }
    }
}