using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Data;
using _Game.Scripts.Core.Game;

namespace _Game.Scripts.Core
{
    public class GameController : MonoBehaviour
    {
        [Header("El Ejecutor")]
        [SerializeField] private LevelManager _levelManager;

        [Header("La Lista de Reproducción")]
        [SerializeField] private List<LevelConfig> _allLevels; 

        private int _currentLevelIndex = 0;

        private void Start()
        {
            if(_allLevels == null || _allLevels.Count == 0)
            {
                Debug.LogWarning("GameController: No hay niveles en la lista.");
                return;
            }

            // Nos suscribimos al evento del LevelManager
            // "Cuando termines un nivel, avísame a la función OnLevelFinished"
            _levelManager.OnLevelFinished += HandleLevelFinished;

            // Cargamos el primero
            LoadCurrentLevel();
        }

        private void OnDestroy()
        {
            if (_levelManager != null)
            {
                _levelManager.OnLevelFinished -= HandleLevelFinished;
            }
        }

        private void LoadCurrentLevel()
        {
            if (_currentLevelIndex >= _allLevels.Count)
            {
                Debug.Log("¡JUEGO COMPLETADO! No quedan más niveles.");
                // Aquí podríamos cargar la escena del Menú Principal o el RANKING
                // SceneManager.LoadScene("MenuScene");
                return;
            }

            // Obtenemos los datos del nivel actual
            LevelConfig config = _allLevels[_currentLevelIndex];

            // LE ORDENAMOS AL LEVEL MANAGER QUE LO CARGUE
            // El GameController ya no toca prefabs ni textos, delega everything.
            _levelManager.LoadLevel(config);
        }

        // Esta función se llama automáticamente cuando LevelManager termina su trabajo
        private void HandleLevelFinished(bool playerWon)
        {
            if (playerWon)
            {
                // Si ganó, sumamos índice y cargamos el siguiente
                _currentLevelIndex++;
                LoadCurrentLevel();
            }
            else
            {
                // Si perdió, recargamos el mismo nivel (Retry)
                Debug.Log("Reintentando nivel...");
                LoadCurrentLevel();
            }
        }
    }
}