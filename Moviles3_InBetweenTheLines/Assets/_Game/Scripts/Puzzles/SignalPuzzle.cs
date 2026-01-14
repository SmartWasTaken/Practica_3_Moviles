using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core;

namespace _Game.Scripts.Puzzles
{
    public class SignalPuzzle : PuzzleBase
    {
        [Header("UI References")]
        [SerializeField] private RectTransform _targetSignal;
        [SerializeField] private Slider _signalStrengthSlider;
        [SerializeField] private Image _crosshair;
        [SerializeField] private Image _noiseOverlay;

        [Header("Configuración")]
        [SerializeField] private float _sensitivity = 2.0f;

        [SerializeField] private float speed = 1.0f;

        private float _currentHeading = 0f;
        private float _targetHeading = 0f;
        
        private float _connectionProgress = 0f;
        private Canvas _parentCanvas;
        private float _satelliteTime = 0f;
        private bool _isLockedOn = false;

        private void Awake()
        {
            _parentCanvas = GetComponentInChildren<Canvas>();
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
            }
        }

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            _currentHeading = 0f;
            _connectionProgress = 0f;
            _isLockedOn = false;
            _targetHeading = Random.Range(90f, 270f);

            if (_signalStrengthSlider != null)
            {
                _signalStrengthSlider.value = 0f;
            }

            SetupVisuals();
        }

        public override void SetUIVisibility(bool isVisible)
        {
            if (_parentCanvas != null) _parentCanvas.enabled = isVisible;
            else foreach (Transform child in transform) child.gameObject.SetActive(isVisible);
        }

        private void SetupVisuals()
        {
            if (_crosshair != null) _crosshair.color = Color.white;
            if (_noiseOverlay != null) 
            {
                _noiseOverlay.gameObject.SetActive(true);
                var c = _noiseOverlay.color; c.a = 0.8f; _noiseOverlay.color = c;
            }

            switch (_currentDifficulty)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    if (_crosshair != null) _crosshair.color = Color.cyan;
                    break;
            }
        }

        private void Update()
        {
            if (isSolved) return;
            HandleRotationInput();
            UpdateTargetLogic();
            UpdateVisuals();
            CheckWinCondition();
        }

        private void HandleRotationInput()
        {
            float rotationDelta = 0f;

            #if UNITY_EDITOR
                if (Input.GetMouseButton(0))
                {
                    rotationDelta = Input.GetAxis("Mouse X") * _sensitivity * 2f;
                }
            #else
                // Giroscopio en Móvil
                rotationDelta = -Input.gyro.rotationRateUnbiased.y * _sensitivity;
            #endif

            _currentHeading += rotationDelta;
            
            if (_currentHeading > 360) _currentHeading -= 360;
            if (_currentHeading < 0) _currentHeading += 360;
        }

        private void UpdateTargetLogic()
        {
            if (_currentDifficulty == 1)
            {
                _satelliteTime += Time.deltaTime;
                float center = 180f; 
                float amplitude = 60f;
                
                _targetHeading = center + Mathf.Sin(_satelliteTime * speed) * amplitude;
            }
        }

        private void UpdateVisuals()
        {
            if (_targetSignal == null) return;

            float angleDiff = Mathf.DeltaAngle(_currentHeading, _targetHeading);

            float fov = 90f;
            float screenWidth = 800f;
            
            float xPos = (angleDiff / (fov / 2)) * (screenWidth / 2);
            
            _targetSignal.anchoredPosition = new Vector2(xPos, 0);
            bool isVisible = Mathf.Abs(angleDiff) < (fov / 2);
            _targetSignal.gameObject.SetActive(isVisible);
            
            if (_noiseOverlay != null)
            {
                float signalQuality = 1f - Mathf.Clamp01(Mathf.Abs(angleDiff) / 30f);
                Color c = _noiseOverlay.color;
                c.a = 0.8f - (signalQuality * 0.8f);
                _noiseOverlay.color = c;
            }
        }

        private void CheckWinCondition()
        {
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(_currentHeading, _targetHeading));
            float threshold = 10f;
            
            if (_currentDifficulty == 2) threshold = 3f;

            bool isAligned = angleDiff < threshold;

            if (isAligned)
            {
                if (_currentDifficulty == 2)
                {
                    HandleStabilizeLogic();
                }
                else
                {
                    _connectionProgress += Time.deltaTime * 0.8f;
                }
                
                if (_crosshair != null) _crosshair.color = Color.green;
            }
            else
            {
                _connectionProgress -= Time.deltaTime * 0.5f;
                
                if (_currentDifficulty == 2)
                {
                     _connectionProgress = 0f;
                     _crosshair.color = Color.cyan;
                }
                else
                {
                    _crosshair.color = Color.white;
                }
            }

            _connectionProgress = Mathf.Clamp01(_connectionProgress);
            if (_signalStrengthSlider != null) _signalStrengthSlider.value = _connectionProgress;

            if (_connectionProgress >= 0.99f)
            {
                CompletePuzzle();
            }
        }

        private void HandleStabilizeLogic()
        {
            float shake = 0f;
            
            #if UNITY_EDITOR
                if (Input.GetMouseButton(0) && Input.GetAxis("Mouse X") != 0) shake = 1f;
            #else
                shake = Input.gyro.rotationRateUnbiased.sqrMagnitude;
            #endif
            
            if (shake > 0.5f) 
            {
                _connectionProgress -= Time.deltaTime * 2f; 
                if (_crosshair != null) _crosshair.color = Color.red;
            }
            else
            {
                _connectionProgress += Time.deltaTime * 1.5f;
            }
        }
    }
}