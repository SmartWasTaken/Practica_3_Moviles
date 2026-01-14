using UnityEngine;
using _Game.Scripts.Core.InputSystem;
using _Game.Scripts.Data;
using _Game.Scripts.Core;

namespace _Game.Scripts.Puzzles
{
    public class BottlePuzzle : PuzzleBase
    {
        [Header("Configuración Botella")]
        [SerializeField] private float _pourSpeed = 0.5f;
        [SerializeField] private float _viscousMultiplier = 0.1f;
        [SerializeField] private float _shakeThreshold = 2.0f;
        
        [Header("Referencias Visuales")]
        [SerializeField] private Transform _bottleVisualTransform; 
        [SerializeField] private Renderer _liquidRenderer;
        [SerializeField] private ParticleSystem _pouringParticleSystem; 
        
        [Header("Ajuste Líquido")]
        [Tooltip("Si el líquido sale cortado o no llega al fondo, ajusta esto manualmente. Si es 0, se calcula solo.")]
        [SerializeField] private float _manualBottleHeight = 0f; 
        [SerializeField] private float _fillOffset = 0f; // Para subir/bajar todo el líquido un poco
        [SerializeField] private float _maxWobble = 0.1f;

        // Estado Interno
        private float _bottleHeight; // Calculado automáticamente
        private float _currentFillAmount = 1.0f;
        private float _currentFlowRate = 0f;
        private float _shakeTimer = 0f;
        
        // Wobble
        private float _wobbleVelocity = 0f;
        private float _currentWobble = 0f;
        private float _simulatedTiltX = 0f;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            _currentFillAmount = 1.0f;
            _simulatedTiltX = 0f;
            
            // --- AUTO-CALIBRACIÓN DE ALTURA ---
            if (_manualBottleHeight > 0)
            {
                _bottleHeight = _manualBottleHeight;
            }
            else if (_liquidRenderer != null)
            {
                // Medimos la altura real del mesh del líquido
                _bottleHeight = _liquidRenderer.bounds.size.y;
                // Pequeño margen de seguridad para que cubra bien los bordes
                _bottleHeight *= 0.95f; 
            }
            else
            {
                _bottleHeight = 2.0f; // Valor por defecto si falla todo
            }

            // Aseguramos que las partículas empiecen apagadas
            if (_pouringParticleSystem != null) _pouringParticleSystem.Stop();
        }

        public override void SetUIVisibility(bool isVisible)
        {
            if (_bottleVisualTransform != null) _bottleVisualTransform.gameObject.SetActive(isVisible);
            if (!isVisible && _pouringParticleSystem != null)
                _pouringParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Update()
        {
            if (isSolved) return;
            
            HandlePhysicsAndInput();
            UpdateLiquidShader();
            UpdatePouringEffects();
        }

        private void HandlePhysicsAndInput()
        {
            Vector3 gravityDir = GetGravityDirection();
            
            // LÓGICA DE VERTIDO (POURING)
            // Si gravityDir.y es positivo, el móvil está boca abajo (tapón al suelo).
            // Bajamos el umbral a 0.1 para que empiece a caer antes.
            bool isPouringPosition = gravityDir.y > 0.1f; 

            // Shake
            bool isShaking = IsShaking();
            if (isShaking) _shakeTimer = 0.5f;
            _shakeTimer -= Time.deltaTime;

            if (!isPouringPosition)
            {
                _currentFlowRate = 0;
            }
            else
            {
                float baseSpeed = _pourSpeed;
                switch (_currentDifficulty)
                {
                    case 0: _currentFlowRate = baseSpeed; break;
                    case 1: _currentFlowRate = (_shakeTimer > 0) ? baseSpeed : baseSpeed * _viscousMultiplier; break;
                    case 2: 
                        if (_currentFillAmount > 0.5f) _currentFlowRate = baseSpeed;
                        else _currentFlowRate = (_shakeTimer > 0) ? baseSpeed : 0f;
                        break;
                }
            }

            _currentFillAmount -= _currentFlowRate * Time.deltaTime;
            
            // Evitamos números negativos
            if (_currentFillAmount <= 0)
            {
                _currentFillAmount = 0;
                CompletePuzzle();
            }
        }

        private void UpdateLiquidShader()
        {
            if (_liquidRenderer == null) return;

            Vector3 gravity = GetGravityDirection();
            // Normal invertida a la gravedad
            Vector3 surfaceNormal = -gravity.normalized;
            
            // Mapeo de 0..1 a altura física real
            // Usamos el _bottleHeight calculado automáticamente
            float halfHeight = _bottleHeight * 0.5f;
            float physicalLevel = Mathf.Lerp(-halfHeight, halfHeight, _currentFillAmount);
            
            physicalLevel += _fillOffset; // Ajuste manual fino por si acaso

            // Wobble
            float wobbleTarget = (gravity.x * 5f);
            _wobbleVelocity += (wobbleTarget - _currentWobble) * Time.deltaTime * 10f;
            _wobbleVelocity *= 0.9f;
            _currentWobble += _wobbleVelocity * Time.deltaTime;

            _liquidRenderer.material.SetVector("_FillNormal", surfaceNormal);
            _liquidRenderer.material.SetFloat("_FillAmount", physicalLevel);
            // IMPORTANTE: El shader necesita saber el centro del objeto para calcular la altura
            _liquidRenderer.material.SetVector("_Center", _liquidRenderer.transform.position); 
            _liquidRenderer.material.SetFloat("_WobbleX", Mathf.Clamp(_currentWobble * 0.1f, -0.2f, 0.2f));
        }

        private void UpdatePouringEffects()
        {
            if (_pouringParticleSystem == null) return;

            // Solo sale agua si estamos vertiendo Y queda agua
            if (_currentFlowRate > 0 && _currentFillAmount > 0)
            {
                if (!_pouringParticleSystem.isPlaying) _pouringParticleSystem.Play();
                var em = _pouringParticleSystem.emission;
                em.rateOverTime = _currentFlowRate * 50f;
            }
            else
            {
                if (_pouringParticleSystem.isPlaying) _pouringParticleSystem.Stop();
            }
        }

        private Vector3 GetGravityDirection()
        {
            #if UNITY_EDITOR
            // Control PC: A/D para inclinar lateralmente, W/S para volcar (Pouring)
            if (Input.GetKey(KeyCode.A)) _simulatedTiltX += Time.deltaTime * 2f;
            if (Input.GetKey(KeyCode.D)) _simulatedTiltX -= Time.deltaTime * 2f;
            
            // Simular volcado (W = Poner boca abajo)
            float tiltForward = Input.GetKey(KeyCode.W) ? 1.0f : 0f; 

            // Retorno suave
            if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) 
                _simulatedTiltX = Mathf.Lerp(_simulatedTiltX, 0, Time.deltaTime * 5f);
                
            _simulatedTiltX = Mathf.Clamp(_simulatedTiltX, -1.5f, 1.5f);
            
            // Si pulsamos W, simulamos gravedad positiva en Y (boca abajo)
            if (tiltForward > 0.5f) return new Vector3(0, 1, 0);

            return new Vector3(Mathf.Sin(_simulatedTiltX), -Mathf.Cos(_simulatedTiltX), 0);
            #else
            return Input.acceleration;
            #endif
        }

        private bool IsShaking()
        {
            #if UNITY_EDITOR
            return Input.GetKeyDown(KeyCode.Space);
            #else
            return Input.acceleration.sqrMagnitude > (_shakeThreshold * _shakeThreshold);
            #endif
        }
    }
}