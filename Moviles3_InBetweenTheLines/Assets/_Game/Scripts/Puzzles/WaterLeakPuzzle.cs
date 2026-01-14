using UnityEngine;
using _Game.Scripts.Core;
using System.Collections.Generic;

namespace _Game.Scripts.Puzzles
{
    public class WaterLeaksPuzzle : PuzzleBase
    {
        [Header("Referencias")]
        [SerializeField] private Transform[] _leaks;     // Arrastra Leak1, Leak2, Leak3
        [SerializeField] private ParticleSystem[] _jets; // Arrastra los sistemas de partículas de cada Leak en orden
        [SerializeField] private Camera _cam;
        [SerializeField] private Renderer _containerRenderer; // Opcional: Para cambiar color si ganamos

        [Header("Configuración General")]
        [SerializeField] private float _winHoldTime = 1.0f; // Tiempo a aguantar
        [SerializeField] private float _leakRadius = 0.8f;  // Cuánto te puedes alejar del centro de la fuga

        [Header("Variante 2: Movimiento")]
        [SerializeField] private float _moveSpeed = 1.0f;
        [SerializeField] private float _moveRange = 0.5f; // Cuánto se mueven de su origen

        [Header("Variante 3: Intermitencia")]
        [SerializeField] private float _toggleInterval = 1.5f; // Cada cuánto cambian las fugas activas

        // ESTADO
        private float _currentWinTimer = 0f;
        private bool[] _isPlugged; // Estado de cada fuga (Tapada o no)
        private bool[] _isActive;  // Estado de cada fuga (Existe o no - Var 3)
        private Vector3[] _initialPositions; // Para el movimiento relativo
        
        // Input PC (Simulación de dedos pegajosos)
        private bool[] _pcHeldLeaks; 

        // Temporizadores internos
        private float _intermittentTimer = 0f;

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            if (_cam == null) _cam = Camera.main;

            int count = _leaks.Length;
            _isPlugged = new bool[count];
            _isActive = new bool[count];
            _pcHeldLeaks = new bool[count];
            _initialPositions = new Vector3[count];

            for (int i = 0; i < count; i++)
            {
                _initialPositions[i] = _leaks[i].localPosition;
                _pcHeldLeaks[i] = false;
                _isPlugged[i] = false;
            }

            // Configurar según dificultad
            switch (difficulty)
            {
                case 0: // FUGAS SIMPLES (2 Fijas)
                    ActivateLeak(0, true);
                    ActivateLeak(1, true);
                    ActivateLeak(2, false); // La tercera no se usa
                    break;

                case 1: // FUGAS MÓVILES (3 Móviles)
                    ActivateLeak(0, true);
                    ActivateLeak(1, true);
                    ActivateLeak(2, true);
                    break;

                case 2: // FUGAS INTERMITENTES (3 Aleatorias)
                    ActivateLeak(0, true); // Empiezan todas activas o random
                    ActivateLeak(1, false);
                    ActivateLeak(2, true);
                    break;
            }

            _currentWinTimer = 0f;
        }

        private void Update()
        {
            if (isSolved) return;
            
            ResetPlugStatus();
            HandleInput();
            ApplyMechanics();
            UpdateParticles();
            CheckProgress();
        }

        public override void SetUIVisibility(bool isVisible)
        {
            if (_containerRenderer != null) _containerRenderer.gameObject.SetActive(isVisible);
            if (_leaks != null)
            {
                foreach (var leak in _leaks)
                {
                    if (leak != null) leak.gameObject.SetActive(isVisible);
                }
            }
            if (!isVisible && _jets != null)
            {
                foreach (var jet in _jets)
                {
                    if (jet != null) jet.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }
        }
        
        private void ResetPlugStatus()
        {
            for (int i = 0; i < _leaks.Length; i++)
            {
                #if !UNITY_EDITOR
                _isPlugged[i] = false; 
                #else
                _isPlugged[i] = _pcHeldLeaks[i];
                #endif
            }
        }

        private void HandleInput()
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                {
                    Ray ray = _cam.ScreenPointToRay(t.position);
                    CheckRaycast(ray, true);
                }
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                CheckRaycast(ray, false);
            }
        }

        private void CheckRaycast(Ray ray, bool isTouch)
        {
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                for (int i = 0; i < _leaks.Length; i++)
                {
                    if (!_isActive[i]) continue;

                    if (hit.transform == _leaks[i])
                    {
                        if (isTouch)
                        {
                            _isPlugged[i] = true;
                        }
                        else
                        {
                            _pcHeldLeaks[i] = !_pcHeldLeaks[i];
                            _isPlugged[i] = _pcHeldLeaks[i];
                        }
                    }
                }
            }
        }

        private void ApplyMechanics()
        {
            float dt = Time.deltaTime;
            if (_currentDifficulty == 1)
            {
                for (int i = 0; i < _leaks.Length; i++)
                {
                    if (!_isActive[i]) continue;
                    if (!_isPlugged[i]) 
                    {
                        float offsetX = Mathf.Sin(Time.time * _moveSpeed + (i * 10)) * _moveRange;
                        float offsetY = Mathf.Cos(Time.time * _moveSpeed * 0.8f + (i * 10)) * _moveRange;
                        _leaks[i].localPosition = _initialPositions[i] + new Vector3(offsetX, offsetY, 0);
                    }
                }
            }

            else if (_currentDifficulty == 2)
            {
                _intermittentTimer -= dt;
                if (_intermittentTimer <= 0)
                {
                    _intermittentTimer = _toggleInterval;
                    int randomIndex = Random.Range(0, _leaks.Length);
                    bool newState = !_isActive[randomIndex];
                    
                    ActivateLeak(randomIndex, newState);
                    if (!newState) _pcHeldLeaks[randomIndex] = false;
                }
            }
        }

        private void ActivateLeak(int index, bool active)
        {
            _isActive[index] = active;
            _leaks[index].gameObject.SetActive(active);
            if (!active) 
            {
                var em = _jets[index].emission;
                em.enabled = false;
            }
        }

        private void UpdateParticles()
        {
            for (int i = 0; i < _jets.Length; i++)
            {
                if (!_isActive[i]) continue;

                var emission = _jets[i].emission;
                emission.enabled = !_isPlugged[i];
            }
        }

        private void CheckProgress()
        {
            int activeCount = 0;
            int pluggedCount = 0;

            for (int i = 0; i < _leaks.Length; i++)
            {
                if (_isActive[i])
                {
                    activeCount++;
                    if (_isPlugged[i]) pluggedCount++;
                }
            }
            if (activeCount > 0 && pluggedCount == activeCount)
            {
                _currentWinTimer += Time.deltaTime;
                if (_containerRenderer) _containerRenderer.material.color = Color.Lerp(Color.white, Color.green, _currentWinTimer / _winHoldTime);

                if (_currentWinTimer >= _winHoldTime)
                {
                    CompletePuzzle();
                }
            }
            else
            {
                _currentWinTimer -= Time.deltaTime * 2.0f; 
                if (_currentWinTimer < 0) _currentWinTimer = 0;
                
                if (_containerRenderer) _containerRenderer.material.color = Color.white;
            }
        }
    }
}