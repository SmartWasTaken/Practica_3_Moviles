using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using System.Collections.Generic;
using _Game.Scripts.Data;
using _Game.Scripts.Puzzles;


namespace _Game.Scripts.Core
{
    public class GameController : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private TextMeshProUGUI riddleTextLabel;
        [SerializeField] private Image backgroundPanel;
        [SerializeField] private Transform puzzleSpawnPoint; 

        [Header("Configuración Niveles")]
        [SerializeField] private List<LevelConfig> allLevels; 

        private int currentLevelIndex = 0;
        private PuzzleBase currentPuzzleInstance;

        void Start()
        {
            if(allLevels == null || allLevels.Count == 0)
            {
                Debug.LogWarning("¡No has asignado niveles en el GameController!");
                return;
            }

            LoadLevel(currentLevelIndex);
        }

        void LoadLevel(int index)
        {
            if (index >= allLevels.Count)
            {
                Debug.Log("¡Juego Completado!");
                if(riddleTextLabel != null) riddleTextLabel.text = "FIN DEL JUEGO";
                return;
            }

            LevelConfig data = allLevels[index];

            if(riddleTextLabel != null) riddleTextLabel.text = data.riddleText;
            if(backgroundPanel != null) backgroundPanel.color = data.backgroundColor;

            if (currentPuzzleInstance != null)
            {
                Destroy(currentPuzzleInstance.gameObject);
            }

            if (data.puzzlePrefab != null)
            {
                currentPuzzleInstance = Instantiate(data.puzzlePrefab, puzzleSpawnPoint);
                currentPuzzleInstance.OnLevelCompleted += HandleLevelCompleted;
                currentPuzzleInstance.Initialize();
            }
            else
            {
                Debug.LogError($"El nivel {index} no tiene un Prefab asignado en su configuración.");
            }
        }

        private void HandleLevelCompleted()
        {
            if (currentPuzzleInstance != null)
            {
                currentPuzzleInstance.OnLevelCompleted -= HandleLevelCompleted;
                Destroy(currentPuzzleInstance.gameObject);
            }

            currentLevelIndex++;
            LoadLevel(currentLevelIndex);
        }
    }
}