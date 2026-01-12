using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core;
using System.Collections;

namespace _Game.Scripts.Puzzles
{
    public class OverloadPuzzle : PuzzleBase
    {
        [Header("UI References")]
        [SerializeField] private Slider _mainSlider;
        [SerializeField] private Image _fillImage;
        [SerializeField] private Image _statusIcon;
        
        [Header("Elementos Var 1 (Mantener)")]
        [SerializeField] private RectTransform _needleObject;
        [SerializeField] private RectTransform _targetZone;
        [SerializeField] private RectTransform _sliderArea;

        [Header("Configuración General")]
        [SerializeField] private float _decaySpeed = 0.3f;
        [SerializeField] private float _tapForce = 0.1f;

        private float _progressValue = 0f;
        private float _needleValue = 0f; 
        
        private bool _isGreenLight = false;
        private float _trafficTimer = 0f;
        private float _nextTrafficSwitch = 0f;

        private Canvas _parentCanvas;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            _progressValue = 0f;
            _needleValue = 0f;
            _mainSlider.value = 0f;
            _parentCanvas = GetComponentInChildren<Canvas>();

            SetupVisuals();
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

        private void SetupVisuals()
        {
            if (_targetZone != null) _targetZone.gameObject.SetActive(false);
            if (_needleObject != null) _needleObject.gameObject.SetActive(false);
            if (_statusIcon != null) _statusIcon.gameObject.SetActive(false);
            if (_fillImage != null) _fillImage.color = Color.yellow;

            switch (_currentDifficulty)
            {
                case 0:
                    break;

                case 1:
                    if (_targetZone != null) _targetZone.gameObject.SetActive(true);
                    if (_needleObject != null) _needleObject.gameObject.SetActive(true);
                    break;

                case 2:
                    if (_statusIcon != null) 
                    {
                        _statusIcon.gameObject.SetActive(true);
                        _statusIcon.color = Color.red;
                    }
                    _isGreenLight = false;
                    _nextTrafficSwitch = Random.Range(1.0f, 2.0f);
                    break;
            }
        }

        private void Update()
        {
            if (isSolved) return;

            bool isTapping = Input.GetMouseButtonDown(0); 

            switch (_currentDifficulty)
            {
                case 0: // CARRERA
                    HandleRaceLogic(isTapping);
                    break;

                case 1: // MANTENER (Agujas)
                    HandleMaintainLogic(isTapping);
                    break;

                case 2: // SEMÁFORO
                    HandleTrafficLogic(isTapping);
                    break;
            }

            if (_mainSlider != null)
            {
                _mainSlider.value = _progressValue;
            }
        }

        private void HandleRaceLogic(bool tap)
        {
            if (tap) _progressValue += _tapForce;
            _progressValue -= _decaySpeed * Time.deltaTime;
            _progressValue = Mathf.Clamp01(_progressValue);

            if (_progressValue >= 0.99f) CompletePuzzle();
        }

        private void HandleMaintainLogic(bool tap)
        {
            if (tap) _needleValue += _tapForce * 2.0f;
            _needleValue -= (_decaySpeed * 2.0f) * Time.deltaTime;
            _needleValue = Mathf.Clamp01(_needleValue);

            UpdateNeedlePosition(_needleValue);
            float minZone = 0.4f;
            float maxZone = 0.6f;

            if (_needleValue > minZone && _needleValue < maxZone)
            {
                _progressValue += Time.deltaTime * 0.4f;
                if (_fillImage != null) _fillImage.color = Color.green;
            }
            else
            {
                _progressValue -= Time.deltaTime * 0.05f; 
                
                if (_fillImage != null) _fillImage.color = (_needleValue > maxZone) ? Color.red : Color.gray;
            }

            _progressValue = Mathf.Clamp01(_progressValue);

            if (_progressValue >= 0.99f) CompletePuzzle();
        }

        private void HandleTrafficLogic(bool tap)
        {
            _trafficTimer += Time.deltaTime;
            if (_trafficTimer >= _nextTrafficSwitch) ToggleTrafficLight();

            if (tap)
            {
                if (_isGreenLight) _progressValue += 0.20f;
                else _progressValue -= 0.25f;
            }

            _progressValue = Mathf.Clamp01(_progressValue);
            if (_progressValue >= 0.99f) CompletePuzzle();
        }

        private void ToggleTrafficLight()
        {
            _trafficTimer = 0f;
            _isGreenLight = !_isGreenLight;
            _nextTrafficSwitch = _isGreenLight ? Random.Range(0.5f, 1.0f) : Random.Range(1.0f, 2.5f);
            if (_statusIcon != null) _statusIcon.color = _isGreenLight ? Color.green : Color.red;
        }

        private void UpdateNeedlePosition(float value)
        {
            if (_needleObject == null || _sliderArea == null) return;
            float width = _sliderArea.rect.width;
            float xPos = Mathf.Lerp(-width/2, width/2, value); 
            _needleObject.anchoredPosition = new Vector2(xPos, _needleObject.anchoredPosition.y);
        }
    }
}