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
        [SerializeField] private float _visualSmoothSpeed = 10f; 

        [Header("Referencias Puntos Líquido")]
        // ARRASTRA AQUÍ LOS OBJETOS VACÍOS QUE CREASTE
        [SerializeField] private Transform _pointBase; 
        [SerializeField] private Transform _pointTapon;
        
        [Header("Configuración Wobble")]
        [SerializeField] private float _maxWobble = 0.03f;
        [SerializeField] private float _wobbleRecovery = 1f;

        [Header("Debug / Testing")]
        [SerializeField] private float _currentFillAmount = 1.0f;
        [SerializeField] private float _currentFlowRate = 0f;
        
        // Variables internas
        private Quaternion _initialVisualRotation;
        private float _simulatedTiltAngle = 0f; 

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
        }

        private void Update()
        {
            if (isSolved) return;
            HandleInputAndLogic();
            UpdateVisuals();
            UpdateLiquidVisuals();
        }

        private void HandleInputAndLogic()
        {
            Vector3 accel = GetCurrentAcceleration();
            bool isShaking = GetCurrentShaking();
            
            // Si la botella está boca abajo (Y > -0.2f), permitimos que caiga
            bool isPouringPosition = accel.y > -0.2f; 

            if (!isPouringPosition) 
            {
                _currentFlowRate = 0;
                return;
            }

            float flowRate = 0f;
            switch (_currentDifficulty)
            {
                case 0: flowRate = _pourSpeed; break;
                case 1: flowRate = isShaking ? _pourSpeed : _pourSpeed * _viscousMultiplier; break;
                case 2:
                    if (_currentFillAmount > 0.5f) flowRate = _pourSpeed;
                    else flowRate = isShaking ? _pourSpeed : 0;
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

            _bottleVisualTransform.localRotation = Quaternion.Slerp(
                _bottleVisualTransform.localRotation, 
                targetRotation, 
                Time.deltaTime * _visualSmoothSpeed
            );
        }

        // --- AQUÍ ESTÁ LA MAGIA CORREGIDA ---
        private void UpdateLiquidVisuals()
        {
            if (_liquidRenderer == null || _pointBase == null || _pointTapon == null) return;

            // 1. CALCULAR NIVEL DEL AGUA (WORLD SPACE)
            // No nos importa cuál es la base y cuál el tapón. Nos importa cuál está más alto en el mundo.
            float y1 = _pointBase.position.y;
            float y2 = _pointTapon.position.y;

            // Buscamos el punto físico más bajo y más alto de la botella en este instante
            float lowestY = Mathf.Min(y1, y2);
            float highestY = Mathf.Max(y1, y2);

            // Interpolamos: Si FillAmount es 1, el agua llega al punto más alto (highestY).
            // Si FillAmount es 0, el agua baja al punto más bajo (lowestY).
            float currentWaterLevelY = Mathf.Lerp(lowestY, highestY, _currentFillAmount);

            // IMPORTANTE: El shader dibuja "por debajo" de este nivel.
            // Pasamos el valor al shader
            _liquidRenderer.material.SetFloat("_FillAmount", currentWaterLevelY);


            // 2. WOBBLE (Sin cambios, solo lógica de agitación)
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

        // --- GIZMOS PARA DEBUGUEAR (Verás líneas en la escena) ---
        private void OnDrawGizmos()
        {
            if (_pointBase == null || _pointTapon == null) return;

            // Dibuja una esfera Verde en el tapón y Roja en la base
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_pointBase.position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(_pointTapon.position, 0.05f);

            // Dibuja una línea azul donde está el nivel del agua calculado
            float y1 = _pointBase.position.y;
            float y2 = _pointTapon.position.y;
            float lowestY = Mathf.Min(y1, y2);
            float highestY = Mathf.Max(y1, y2);
            float currentWaterLevelY = Mathf.Lerp(lowestY, highestY, _currentFillAmount);

            Gizmos.color = Color.cyan;
            Vector3 center = Vector3.Lerp(_pointBase.position, _pointTapon.position, 0.5f);
            center.y = currentWaterLevelY;
            // Dibujamos un plano imaginario del agua
            Gizmos.DrawCube(center, new Vector3(0.5f, 0.01f, 0.5f));
        }

        // --- HELPERS ---
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