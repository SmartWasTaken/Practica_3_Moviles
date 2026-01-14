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
        [SerializeField] private float _decaySpeed = 0.45f; // Velocidad a la que cae la aguja
        [SerializeField] private float _tapForce = 0.15f;   // Cuánto salta la aguja con cada clic (15%)

        private float _progressValue = 0f;
        private float _needleValue = 0.5f; // La aguja física
        
        private bool _isGreenLight = false;
        private float _trafficTimer = 0f;
        private float _nextTrafficSwitch = 0f;

        private Canvas _parentCanvas;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            _progressValue = 0f;
            _mainSlider.value = 0f;
            _parentCanvas = GetComponentInChildren<Canvas>();

            // Configuración inicial
            if (difficulty == 1) 
            {
                _needleValue = 0.5f; // Empezamos en el medio para que el jugador reaccione
            }
            else
            {
                _needleValue = 0f;
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
            // Apagar todo primero
            if (_targetZone != null) _targetZone.gameObject.SetActive(false);
            if (_needleObject != null) _needleObject.gameObject.SetActive(false);
            if (_statusIcon != null) _statusIcon.gameObject.SetActive(false);
            if (_fillImage != null) _fillImage.color = Color.yellow;

            // Encender lo necesario según dificultad
            switch (_currentDifficulty)
            {
                case 0: break; // Carrera no necesita extras
                case 1:
                    if (_targetZone != null) _targetZone.gameObject.SetActive(true);
                    if (_needleObject != null) _needleObject.gameObject.SetActive(true);
                    break;
                case 2:
                    if (_statusIcon != null) { _statusIcon.gameObject.SetActive(true); _statusIcon.color = Color.red; }
                    _isGreenLight = false;
                    _nextTrafficSwitch = Random.Range(1.0f, 2.0f);
                    break;
            }
        }

        private void Update()
        {
            if (isSolved) return;

            // DETECCIÓN DE INPUT: SIEMPRE TAP (CLIC)
            bool isTapping = Input.GetMouseButtonDown(0);

            switch (_currentDifficulty)
            {
                case 0: // CARRERA SIMPLE
                    HandleRaceLogic(isTapping);
                    break;

                case 1: // MANTENER EN ZONA (Flappy Bird style)
                    HandleMaintainLogic(isTapping);
                    break;

                case 2: // SEMÁFORO
                    HandleTrafficLogic(isTapping);
                    break;
            }

            if (_mainSlider != null) _mainSlider.value = _progressValue;
        }

        // --- LÓGICA CASO 0: CARRERA ---
        private void HandleRaceLogic(bool tap)
        {
            if (tap) _progressValue += 0.1f;
            _progressValue -= 0.3f * Time.deltaTime;
            _progressValue = Mathf.Clamp01(_progressValue);
            if (_progressValue >= 0.99f) CompletePuzzle();
        }

        // --- LÓGICA CASO 1: MANTENER EN ZONA ---
        private void HandleMaintainLogic(bool tap)
        {
            // 1. FÍSICA DE LA AGUJA (CONTROL)
            if (tap)
            {
                Debug.Log("Estoy dentro");
                _needleValue += _tapForce; // Salto seco hacia arriba
            }
            
            // Gravedad constante
            _needleValue -= _decaySpeed * Time.deltaTime;
            _needleValue = Mathf.Clamp01(_needleValue);

            // Actualizar posición visual de la aguja (necesario para ver dónde estás)
            UpdateNeedlePosition(_needleValue);

            // 2. LÓGICA DE PROGRESO (ZONA VERDE)
            float minZone = 0.3f;
            float maxZone = 0.7f;

            if (_needleValue > minZone && _needleValue < maxZone)
            {
                // DENTRO: La barra de progreso se llena
                _progressValue += Time.deltaTime * 0.5f; 
                if (_fillImage != null) _fillImage.color = Color.green;
            }
            else
            {
                // FUERA: La barra de progreso baja
                _progressValue -= Time.deltaTime * 0.25f; 
                
                // Feedback visual: Rojo si te pasas, Gris si te caes
                if (_fillImage != null) 
                    _fillImage.color = (_needleValue >= maxZone) ? Color.red : Color.gray;
            }

            _progressValue = Mathf.Clamp01(_progressValue);

            if (_progressValue >= 0.99f) CompletePuzzle();
        }

        // --- LÓGICA CASO 2: SEMÁFORO ---
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
            
            // Mapeamos 0..1 a la posición X dentro del área
            float xPos = Mathf.Lerp(-width/2f, width/2f, value); 
            _needleObject.anchoredPosition = new Vector2(xPos, _needleObject.anchoredPosition.y);
        }
    }
}