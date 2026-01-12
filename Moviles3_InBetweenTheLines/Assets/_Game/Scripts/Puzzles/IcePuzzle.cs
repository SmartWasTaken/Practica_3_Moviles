using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core;
using TMPro;

namespace _Game.Scripts.Puzzles
{
    public class IcePuzzle : PuzzleBase
    {
        [Header("UI References")]
        [SerializeField] private Image _iceImage;
        [SerializeField] private Image _crackOverlay;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        [Header("Configuración")]
        [SerializeField] private float _meltSpeed = 0.5f;
        [SerializeField] private float _shakeThreshold = 2.0f;

        private int _currentPhase = 0;
        private float _layerHealth = 1.0f;
        private Canvas _parentCanvas;

        private Vector3 _lastAcceleration;

        private void Awake()
        {
            _parentCanvas = GetComponentInChildren<Canvas>();
            _lastAcceleration = Input.acceleration;
        }

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            _currentPhase = 0;
            _layerHealth = 1.0f;
            
            SetupVisuals();
        }

        public override void SetUIVisibility(bool isVisible)
        {
            if (_parentCanvas != null) _parentCanvas.enabled = isVisible;
            else foreach (Transform child in transform) child.gameObject.SetActive(isVisible);
        }

        private void SetupVisuals()
        {
            // Reset visual
            if (_iceImage != null)
            {
                _iceImage.color = new Color(0.8f, 0.9f, 1f, 1f);
                _iceImage.gameObject.SetActive(true);
            }
            if (_crackOverlay != null) _crackOverlay.gameObject.SetActive(false);
            
            UpdateFeedbackText();
        }

        private void Update()
        {
            if (isSolved) return;

            int touchCount = GetTouchCount();
            float rubIntensity = GetRubIntensity();
            bool isShaking = DetectShake();

            switch (_currentDifficulty)
            {
                case 0:
                    HandleBasicMultitouch(touchCount, rubIntensity);
                    break;

                case 1:
                    HandleBreakAndMelt(isShaking, rubIntensity);
                    break;

                case 2:
                    HandleLayers(isShaking, touchCount, rubIntensity);
                    break;
            }
        }

        private void HandleBasicMultitouch(int fingers, float rub)
        {
            if (fingers >= 2 && rub > 0)
            {
                MeltIce(rub * _meltSpeed);
            }
        }

        private void HandleBreakAndMelt(bool shaken, float rub)
        {
            if (_currentPhase == 0)
            {
                if (shaken)
                {
                    DamageLayer(0.2f);
                    if (_crackOverlay != null) 
                    {
                        _crackOverlay.gameObject.SetActive(true);
                        var c = _crackOverlay.color; c.a = 1f - _layerHealth; _crackOverlay.color = c;
                    }

                    if (_layerHealth <= 0)
                    {
                        NextPhase();
                    }
                }
            }
            else
            {
                if (rub > 0) MeltIce(rub * _meltSpeed);
            }
        }

        private void HandleLayers(bool shaken, int fingers, float rub)
        {
            switch (_currentPhase)
            {
                case 0:
                    if (shaken)
                    {
                        DamageLayer(0.25f);
                        if (_crackOverlay != null) 
                        {
                            _crackOverlay.gameObject.SetActive(true);
                            var c = _crackOverlay.color; c.a = (1f - _layerHealth); _crackOverlay.color = c;
                        }
                        if (_layerHealth <= 0) NextPhase();
                    }
                    break;

                case 1:
                    if (fingers >= 3)
                    {
                        DamageLayer(Time.deltaTime * 0.8f);
                        if (_layerHealth <= 0) NextPhase();
                    }
                    break;

                case 2:
                    if (fingers >= 1 && rub > 0)
                    {
                         MeltIce(rub * _meltSpeed * 1.5f);
                    }
                    break;
            }
        }

        private void MeltIce(float amount)
        {
            _layerHealth -= amount * Time.deltaTime;
            
            if (_iceImage != null)
            {
                Color c = _iceImage.color;
                c.a = _layerHealth;
                _iceImage.color = c;
            }

            if (_layerHealth <= 0.05f)
            {
                CompletePuzzle();
            }
        }

        private void DamageLayer(float damage)
        {
            _layerHealth -= damage;
        }

        private void NextPhase()
        {
            _currentPhase++;
            _layerHealth = 1.0f;
            UpdateFeedbackText();
            if (_currentDifficulty == 1 && _currentPhase == 1)
            {
                if (_crackOverlay != null) _crackOverlay.gameObject.SetActive(false);
            }
        }

        private void UpdateFeedbackText()
        {
            if (_feedbackText == null) return;
            
            string text = "";
            if (_currentDifficulty == 0) text = "¡Usa 2 dedos!";
            else if (_currentDifficulty == 1) text = (_currentPhase == 0) ? "¡Agita!" : "¡Frota!";
            else if (_currentDifficulty == 2)
            {
                if (_currentPhase == 0) text = "¡Agita duro!";
                else if (_currentPhase == 1) text = "¡Pon 3 dedos!";
                else text = "¡Limpia!";
            }
            _feedbackText.text = text;
        }
        
        private int GetTouchCount()
        {
            #if UNITY_EDITOR
                if (Input.GetMouseButton(2)) return 3;
                if (Input.GetMouseButton(1)) return 2;
                if (Input.GetMouseButton(0)) return 1;
                return 0;
            #else
                return Input.touchCount;
            #endif
        }

        private float GetRubIntensity()
        {
            #if UNITY_EDITOR
                if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                {
                    // Velocidad del ratón
                    return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")).magnitude * 5f;
                }
                return 0f;
            #else
                float totalDelta = 0f;
                if (Input.touchCount > 0)
                {
                    foreach(Touch t in Input.touches)
                    {
                        if (t.phase == TouchPhase.Moved) totalDelta += t.deltaPosition.magnitude;
                    }
                    // Normalizar un poco para que no sea instantáneo
                    return totalDelta / Input.touchCount; 
                }
                return 0f;
            #endif
        }

        private bool DetectShake()
        {
            #if UNITY_EDITOR
                // Tecla Espacio
                return Input.GetKeyDown(KeyCode.Space);
            #else
                Vector3 acceleration = Input.acceleration;
                float delta = (acceleration - _lastAcceleration).sqrMagnitude;
                _lastAcceleration = acceleration;
                return delta >= (_shakeThreshold * _shakeThreshold);
            #endif
        }
    }
}