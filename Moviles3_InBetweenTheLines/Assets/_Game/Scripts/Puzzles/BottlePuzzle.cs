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
        
        [Header("Configuración Input")]
        [SerializeField] private float _shakeMemoryDuration = 0.5f;

        [Header("Referencias Visuales")]
        [SerializeField] private Transform _bottleVisualTransform; 
        [SerializeField] private Renderer _liquidRenderer;
        [SerializeField] private ParticleSystem _pouringParticleSystem; 
        [SerializeField] private float _visualSmoothSpeed = 10f; 

        [Header("Referencias Puntos Líquido")]
        [SerializeField] private Transform _pointBase; 
        [SerializeField] private Transform _pointTapon;
        
        [Header("Configuración Wobble")]
        [SerializeField] private float _maxWobble = 0.03f;
        [SerializeField] private float _wobbleRecovery = 1f;

        [Header("Debug / Testing")]
        [SerializeField] private float _currentFillAmount = 1.0f;
        [SerializeField] private float _currentFlowRate = 0f;
        
        private Quaternion _initialVisualRotation;
        private float _simulatedTiltAngle = 0f; 
        private float _shakeTimer = 0f;

        private float _wobbleAmountX;
        private float _wobbleAmountZ;
        private float _wobbleAddX;
        private float _wobbleAddZ;
        private float _timeWobble;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            _currentFillAmount = 1.0f;
            
            if (_bottleVisualTransform != null)
            {
                _initialVisualRotation = _bottleVisualTransform.localRotation;
            }
            _simulatedTiltAngle = 0f;
            _wobbleAddX = 0;
            _wobbleAddZ = 0;

            if (_pouringParticleSystem != null && _pouringParticleSystem.isPlaying)
            {
                _pouringParticleSystem.Stop();
            }
        }

        private void Update()
        {
            if (isSolved) return;
            HandleInputAndLogic();
            UpdateVisuals();
            UpdateLiquidVisuals();
            UpdatePouringEffects();
        }

        private void HandleInputAndLogic()
        {
            Vector3 accel = GetCurrentAcceleration();
            bool rawShakeInput = GetCurrentShaking();

            if (rawShakeInput)
            {
                _shakeTimer = _shakeMemoryDuration;
            }
            _shakeTimer -= Time.deltaTime;

            bool isEffectiveShaking = _shakeTimer > 0;
            bool isPouringPosition = accel.y > -0.2f; 

            if (!isPouringPosition) 
            {
                _currentFlowRate = 0;
                return;
            }

            float flowRate = 0f;
            switch (_currentDifficulty)
            {
                case 0: // Fácil
                    flowRate = _pourSpeed; 
                    break;
            
                case 1: // Viscoso
                    if (isEffectiveShaking) 
                    {
                        flowRate = _pourSpeed;
                    }
                    else 
                    {
                        flowRate = _pourSpeed * _viscousMultiplier;
                    }
                    break;
            
                case 2: // Atasco
                    if (_currentFillAmount > 0.5f) 
                    {
                        flowRate = _pourSpeed;
                    }
                    else 
                    {
                        if (isEffectiveShaking) flowRate = _pourSpeed;
                        else flowRate = 0;
                    }
                    break;
            }

            _currentFlowRate = flowRate;
            _currentFillAmount -= flowRate * Time.deltaTime;

            if (_currentFillAmount <= 0)
            {
                _currentFillAmount = 0;
                CompletePuzzle();
            }
        }

        private void UpdateVisuals()
        {
           // ... (Este método NO cambia) ...
            if (_bottleVisualTransform == null) return;

            Quaternion targetRotation;
            #if UNITY_EDITOR
            {
                Quaternion simulatedTilt = Quaternion.Euler(0, _simulatedTiltAngle, 0);
                targetRotation = _initialVisualRotation * simulatedTilt;
            }
            #else
            {
                Vector3 accel = InputManager.Instance.Acceleration;
                float rollAngle = Mathf.Atan2(accel.x, -accel.y) * Mathf.Rad2Deg;
                Quaternion gravityTilt = Quaternion.Euler(0, rollAngle, 0);
                targetRotation = _initialVisualRotation * gravityTilt;
            }
            #endif
            _bottleVisualTransform.localRotation = Quaternion.Slerp(_bottleVisualTransform.localRotation, targetRotation, Time.deltaTime * _visualSmoothSpeed);
        }

        private void UpdateLiquidVisuals()
        {
             // ... (Este método NO cambia, usa la versión de los puntos que te pasé antes) ...
            if (_liquidRenderer == null || _pointBase == null || _pointTapon == null) return;

            float y1 = _pointBase.position.y;
            float y2 = _pointTapon.position.y;
            float lowestY = Mathf.Min(y1, y2);
            float highestY = Mathf.Max(y1, y2);
            float currentWaterLevelY = Mathf.Lerp(lowestY, highestY, _currentFillAmount);

            _liquidRenderer.material.SetFloat("_FillAmount", currentWaterLevelY);

            // WOBBLE
            float deltaTime = Time.deltaTime;
            float wobbleInput = 0f;
            #if UNITY_EDITOR
                if (Input.GetKey(KeyCode.D)) wobbleInput = 1f; 
                else if (Input.GetKey(KeyCode.A)) wobbleInput = -1f;
            #else
                wobbleInput = InputManager.Instance.Acceleration.x * 5f; 
            #endif

            _wobbleAddX = Mathf.Lerp(_wobbleAddX, 0, deltaTime * _wobbleRecovery);
            _wobbleAddZ = Mathf.Lerp(_wobbleAddZ, 0, deltaTime * _wobbleRecovery);
            _timeWobble += deltaTime;
            float pulse = 2 * Mathf.PI * 1.0f; 

            if (Mathf.Abs(wobbleInput) > 0.1f)
            {
                _wobbleAddZ += wobbleInput * 0.01f; 
                _wobbleAddZ = Mathf.Clamp(_wobbleAddZ, -_maxWobble, _maxWobble);
            }

            _wobbleAmountX = _wobbleAddX * Mathf.Sin(pulse * _timeWobble);
            _wobbleAmountZ = _wobbleAddZ * Mathf.Sin(pulse * _timeWobble);

            _liquidRenderer.material.SetFloat("_WobbleX", _wobbleAmountX);
            _liquidRenderer.material.SetFloat("_WobbleZ", _wobbleAmountZ);
        }

        // --- NUEVO MÉTODO: CONTROL DE PARTÍCULAS ---
        private void UpdatePouringEffects()
        {
            if (_pouringParticleSystem == null) return;

            // ¿Se está vertiendo líquido Y queda líquido en la botella?
            bool isPouring = _currentFlowRate > 0 && _currentFillAmount > 0;

            if (isPouring)
            {
                // Si debería estar vertiendo y está apagado, enciéndelo
                if (!_pouringParticleSystem.isPlaying)
                {
                    _pouringParticleSystem.Play();
                }

                // OPCIONAL (Nivel PRO): Modificar la cantidad de partículas según el flujo.
                // Si cae poco (viscoso), salen pocas partículas. Si cae mucho, salen muchas.
                var emissionModule = _pouringParticleSystem.emission;
                // Mapeamos el flujo (ej. 0.1 a 0.5) a una cantidad de partículas (ej. 10 a 50 por segundo)
                emissionModule.rateOverTime = _currentFlowRate * 100f; 
            }
            else
            {
                // Si NO debería estar vertiendo y está encendido, apágalo
                // Usamos Stop con false para que las partículas que ya han salido terminen de caer de forma natural
                if (_pouringParticleSystem.isPlaying)
                {
                    _pouringParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
            }
        }
        // --------------------------------------------

        // ... (Helpers GetCurrentAcceleration y GetCurrentShaking NO cambian) ...
         private Vector3 GetCurrentAcceleration()
        {
            #if UNITY_EDITOR
                float tiltSpeed = 150f * Time.deltaTime;
                if (Input.GetKey(KeyCode.D)) _simulatedTiltAngle += tiltSpeed;
                if (Input.GetKey(KeyCode.A)) _simulatedTiltAngle -= tiltSpeed;
                _simulatedTiltAngle = Mathf.Clamp(_simulatedTiltAngle, -180f, 180f);

                float rad = _simulatedTiltAngle * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(rad), -Mathf.Cos(rad), 0);
            #else
                return InputManager.Instance.Acceleration;
            #endif
        }

        private bool GetCurrentShaking()
        {
            #if UNITY_EDITOR
                return Input.GetKeyDown(KeyCode.Space);
            #else
                return InputManager.Instance.IsShaking(_shakeThreshold);
            #endif
        }
    }
}