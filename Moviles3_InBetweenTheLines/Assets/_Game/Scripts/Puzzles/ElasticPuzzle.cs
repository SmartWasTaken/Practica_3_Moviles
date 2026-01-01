using UnityEngine;
using _Game.Scripts.Core;
using System.Collections.Generic;

namespace _Game.Scripts.Puzzles
{
    public class ElasticPuzzle : PuzzleBase
    {
        [Header("Referencias")]
        [SerializeField] private LineRenderer _elasticLine;
        [SerializeField] private Transform[] _anchors; 
        [SerializeField] private Camera _cam;

        [Header("Configuración Tensión")]
        [Tooltip("Cuántos metros EXTRA hay que estirar la goma respecto a su reposo.")]
        [SerializeField] private float _stretchNeeded = 2.0f; // <--- CAMBIO DE NOMBRE PARA QUE SE ENTIENDA MEJOR
        [SerializeField] private float _maxStretchLimit = 6.0f; // Cuánto extra aguanta antes de romper
        [SerializeField] private float _recoilSpeed = 2.0f;    
        [SerializeField] private float _targetHoldTime = 1.0f;

        [Header("Feedback Visual")]
        [SerializeField] private Color _colorLoose = Color.white;
        [SerializeField] private Color _colorTension = Color.green;
        [SerializeField] private Color _colorBreaking = Color.red;

        // ESTADO
        private float _holdTimer = 0f;
        private bool _isBroken = false;
        private float _initialPerimeter = 0f; // <--- NUEVO: Para saber cuánto medía al empezar
        
        // Input y Lógica
        private Vector3[] _initialPositions;
        private Dictionary<int, Transform> _activeTouches = new Dictionary<int, Transform>();
        private Transform _pcSelectedAnchor = null; 

        public override void Initialize(LevelManager manager, int difficulty)
        {
            base.Initialize(manager, difficulty);
            
            if (_cam == null) _cam = Camera.main;

            _initialPositions = new Vector3[_anchors.Length];
            for (int i = 0; i < _anchors.Length; i++)
            {
                _initialPositions[i] = _anchors[i].position;
            }

            // Configuración por dificultad
            switch (difficulty)
            {
                case 0: // 2 PUNTOS
                    if (_anchors.Length > 2) _anchors[2].gameObject.SetActive(false);
                    _elasticLine.positionCount = 2;
                    _elasticLine.loop = false;
                    break;

                case 1: // 3 PUNTOS
                case 2: // DINÁMICO
                    if (_anchors.Length > 2) _anchors[2].gameObject.SetActive(true);
                    _elasticLine.positionCount = 3;
                    _elasticLine.loop = true;
                    break;
            }

            // --- CORRECCIÓN CLAVE ---
            // Calculamos cuánto mide la goma en reposo (al empezar el nivel)
            // Para ganar, habrá que superar ESTO + _stretchNeeded
            _initialPerimeter = CalculateCurrentPerimeter();

            _isBroken = false;
            _holdTimer = 0f;
        }

        private void Update()
        {
            if (isSolved || _isBroken) return;

            HandleInput();
            ApplyMechanics();
            UpdateVisuals();
            CheckWinCondition();
        }

        private void HandleInput()
        {
            // MÓVIL
            if (Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                {
                    Vector3 touchWorldPos = GetWorldPos(t.position);
                    if (t.phase == TouchPhase.Began)
                    {
                        Transform bestAnchor = FindClosestAnchor(touchWorldPos);
                        if (bestAnchor != null && !_activeTouches.ContainsKey(t.fingerId))
                            _activeTouches.Add(t.fingerId, bestAnchor);
                    }
                    else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                    {
                        if (_activeTouches.ContainsKey(t.fingerId))
                            _activeTouches[t.fingerId].position = new Vector3(touchWorldPos.x, touchWorldPos.y, 0);
                    }
                    else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    {
                        if (_activeTouches.ContainsKey(t.fingerId)) _activeTouches.Remove(t.fingerId);
                    }
                }
            }
            // PC
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 mousePos = GetWorldPos(Input.mousePosition);
                    Transform clickedAnchor = FindClosestAnchor(mousePos);
                    
                    if (clickedAnchor != null)
                    {
                        if (_pcSelectedAnchor == clickedAnchor) _pcSelectedAnchor = null;
                        else _pcSelectedAnchor = clickedAnchor;
                    }
                }

                if (_pcSelectedAnchor != null)
                {
                    Vector3 mousePos = GetWorldPos(Input.mousePosition);
                    _pcSelectedAnchor.position = new Vector3(mousePos.x, mousePos.y, 0);
                }
            }
        }

        private void ApplyMechanics()
        {
            for (int i = 0; i < _anchors.Length; i++)
            {
                if (!_anchors[i].gameObject.activeSelf) continue;

                bool isBeingTouched = IsAnchorTouched(_anchors[i]);
                if (!isBeingTouched)
                {
                    _anchors[i].position = Vector3.MoveTowards(
                        _anchors[i].position, 
                        _initialPositions[i], 
                        _recoilSpeed * Time.deltaTime
                    );
                }
            }
        }

        private void UpdateVisuals()
        {
            if (_elasticLine == null) return;

            int activeCount = 0;
            for (int i = 0; i < _anchors.Length; i++)
            {
                if (_anchors[i].gameObject.activeSelf)
                {
                    _elasticLine.SetPosition(i, _anchors[i].position);
                    activeCount++;
                }
            }
            _elasticLine.positionCount = activeCount;

            // VISUALIZAR TENSIÓN
            float current = CalculateCurrentPerimeter();
            float target = _initialPerimeter + _stretchNeeded;
            float breakPoint = _initialPerimeter + _maxStretchLimit;
            
            if (current > breakPoint)
            {
                _elasticLine.startColor = _colorBreaking;
                _elasticLine.endColor = _colorBreaking;
            }
            else if (current >= target)
            {
                _elasticLine.startColor = _colorTension; // Verde
                _elasticLine.endColor = _colorTension;
            }
            else
            {
                _elasticLine.startColor = _colorLoose; // Blanco
                _elasticLine.endColor = _colorLoose;
            }
        }

        private void CheckWinCondition()
        {
            float currentPerimeter = CalculateCurrentPerimeter();
            
            // 1. ROTURA (Si estiramos demasiado respecto al inicio)
            if (currentPerimeter > (_initialPerimeter + _maxStretchLimit))
            {
                BreakElastic();
                return;
            }

            // 2. VICTORIA (Si superamos el umbral relativo)
            // La condición es: Tamaño Actual > (Tamaño Inicial + Lo que pidas en inspector)
            if (currentPerimeter >= (_initialPerimeter + _stretchNeeded))
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= _targetHoldTime)
                {
                    CompletePuzzle();
                }
            }
            else
            {
                _holdTimer = 0f;
            }
        }

        private void BreakElastic()
        {
            _isBroken = true;
            _elasticLine.enabled = false;
            // Aquí tu lógica de fallo
        }

        // --- HELPERS ---

        private float CalculateCurrentPerimeter()
        {
            float dist = 0f;
            // Solo calculamos distancia entre los activos
            if (_currentDifficulty == 0) // Línea (A -> B)
            {
                dist = Vector3.Distance(_anchors[0].position, _anchors[1].position);
            }
            else // Triángulo (A->B->C->A)
            {
                dist += Vector3.Distance(_anchors[0].position, _anchors[1].position);
                dist += Vector3.Distance(_anchors[1].position, _anchors[2].position);
                dist += Vector3.Distance(_anchors[2].position, _anchors[0].position);
            }
            return dist;
        }

        private Transform FindClosestAnchor(Vector3 pos)
        {
            Transform best = null;
            float minDst = 0.8f; // Radio de toque un poco más generoso

            foreach (var anchor in _anchors)
            {
                if (!anchor.gameObject.activeSelf) continue;
                float d = Vector3.Distance(pos, anchor.position);
                if (d < minDst)
                {
                    minDst = d;
                    best = anchor;
                }
            }
            return best;
        }

        private bool IsAnchorTouched(Transform anchor)
        {
            if (_pcSelectedAnchor == anchor) return true;
            return _activeTouches.ContainsValue(anchor);
        }

        private Vector3 GetWorldPos(Vector2 screenPos)
        {
            Vector3 p = _cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, -_cam.transform.position.z));
            p.z = 0;
            return p;
        }
    }
}