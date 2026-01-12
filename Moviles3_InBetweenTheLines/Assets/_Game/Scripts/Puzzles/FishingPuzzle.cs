using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core;

namespace _Game.Scripts.Puzzles
{
    public class FishingPuzzle : PuzzleBase
    {
        [Header("UI References")]
        [SerializeField] private Slider _waterDepthSlider;
        [SerializeField] private Image _fishIcon;
        [SerializeField] private Image _tensionIcon;
        [SerializeField] private Image _rodLine;

        [Header("Configuración General")]
        [SerializeField] private float _gravity = 0.2f;
        [SerializeField] private float _shakeSensitivity = 2.0f;

        private float _fishHeight = 0f;
        
        private bool _isLineTense = false;
        private float _tensionTimer = 0f;
        private float _nextTensionSwitch = 0f;

        private float _heavyShakeCooldown = 0f;
        private int _heavyShakesCount = 0;
        private int _requiredHeavyShakes = 5;
        private Canvas _parentCanvas;

        private Vector3 _lastAcceleration;
        
        private void Awake()
        {
            _parentCanvas = GetComponentInChildren<Canvas>();
        }

        public override void SetUIVisibility(bool isVisible)
        {
            if (_parentCanvas != null)
            {
                _parentCanvas.enabled = isVisible;
            }
            else
            {
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(isVisible);
                }
            }
        }

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            _fishHeight = 0f;
            _heavyShakesCount = 0;
            _lastAcceleration = Input.acceleration;
            
            if (_waterDepthSlider != null) 
            {
                _waterDepthSlider.value = 0f;
                _waterDepthSlider.maxValue = 1f;
            }

            SetupVisuals();
        }

        private void SetupVisuals()
        {
            if (_tensionIcon != null) _tensionIcon.gameObject.SetActive(false);
            
            switch (_currentDifficulty)
            {
                case 0:
                    _gravity = 0.3f;
                    break;
                case 1:
                    _gravity = 0.25f;
                    _nextTensionSwitch = Random.Range(2.0f, 4.0f);
                    break;
                case 2:
                    _gravity = 0.1f;
                    if (_waterDepthSlider != null) _waterDepthSlider.maxValue = _requiredHeavyShakes;
                    break;
            }
        }

        private void Update()
        {
            if (isSolved) return;

            float shakeIntensity = GetShakeIntensity();

            switch (_currentDifficulty)
            {
                case 0:
                    HandleSpeedFishing(shakeIntensity);
                    break;

                case 1:
                    HandleEnduranceFishing(shakeIntensity);
                    break;

                case 2:
                    HandleHeavyFishing(shakeIntensity);
                    break;
            }

            UpdateUI();
        }

        private float GetShakeIntensity()
        {
            float intensity = 0f;

            #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Space)) intensity = 5.0f; 
            #else
            // Acelerómetro real
            Vector3 acceleration = Input.acceleration;
            // Usamos la diferencia entre frames para detectar cambios bruscos (sacudidas)
            // en lugar de la inclinación estática.
            float delta = (acceleration - _lastAcceleration).sqrMagnitude;
            _lastAcceleration = acceleration;

            if (delta > _shakeSensitivity) intensity = delta;
            #endif

            return intensity;
        }

        private void HandleSpeedFishing(float intensity)
        {
            if (intensity > 0)
            {
                _fishHeight += 0.05f;
            }

            ApplyGravity();
            CheckWinCondition();
        }

        private void HandleEnduranceFishing(float intensity)
        {
            _tensionTimer += Time.deltaTime;
            if (_tensionTimer >= _nextTensionSwitch)
            {
                ToggleTension();
            }

            if (intensity > 0)
            {
                if (_isLineTense)
                {
                    _fishHeight -= 0.2f; 
                    #if UNITY_ANDROID || UNITY_IOS
                    Handheld.Vibrate();
                    #endif
                }
                else
                {
                    _fishHeight += 0.08f;
                }
            }

            ApplyGravity();
            CheckWinCondition();
        }
        
        private void HandleHeavyFishing(float intensity)
        {
            if (_heavyShakeCooldown > 0) _heavyShakeCooldown -= Time.deltaTime;

            float threshold = 4.0f; 
            
            if (intensity > threshold && _heavyShakeCooldown <= 0)
            {
                _fishHeight += 1.0f;
                _heavyShakeCooldown = 0.5f;
                
                if (_fishIcon != null) 
                {
                    _fishIcon.rectTransform.localScale = Vector3.one * 1.5f;
                }
            }

            if (_fishIcon != null) 
            {
                _fishIcon.rectTransform.localScale = Vector3.Lerp(_fishIcon.rectTransform.localScale, Vector3.one, Time.deltaTime * 5f);
            }
            if (_fishHeight >= _requiredHeavyShakes)
            {
                CompletePuzzle();
            }
        }

        private void ApplyGravity()
        {
            _fishHeight -= _gravity * Time.deltaTime;
            _fishHeight = Mathf.Clamp(_fishHeight, 0f, (_currentDifficulty == 2) ? _requiredHeavyShakes : 1.0f);
        }

        private void CheckWinCondition()
        {
            if (_fishHeight >= 0.99f)
            {
                CompletePuzzle();
            }
        }

        private void ToggleTension()
        {
            _tensionTimer = 0f;
            _isLineTense = !_isLineTense;
            
            _nextTensionSwitch = _isLineTense ? Random.Range(1.0f, 2.0f) : Random.Range(2.0f, 4.0f);

            if (_tensionIcon != null)
            {
                _tensionIcon.gameObject.SetActive(_isLineTense);
                _tensionIcon.color = _isLineTense ? Color.red : Color.clear;
            }
        }

        private void UpdateUI()
        {
            if (_waterDepthSlider != null)
            {
                _waterDepthSlider.value = _fishHeight;
            }
        }
    }
}