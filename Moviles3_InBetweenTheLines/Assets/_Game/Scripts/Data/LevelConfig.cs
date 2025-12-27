using _Game.Scripts.Puzzles;
using UnityEngine;

namespace _Game.Scripts.Data
{
    [CreateAssetMenu(fileName = "NewLevel", menuName = "Sensory/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identificación")]
        public string levelID;
        
        [Header("Configuración Visual")]
        [TextArea] public string riddleText; 
        public Color backgroundColor; 

        [Header("Lógica")]
        public PuzzleBase puzzlePrefab;

        [Header("Tiempo")] public float timeLimit;
    }
}