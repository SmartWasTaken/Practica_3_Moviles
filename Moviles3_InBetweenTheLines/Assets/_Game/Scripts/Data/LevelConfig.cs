using _Game.Scripts.Puzzles;
using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Core.Game;

namespace _Game.Scripts.Data
{
    [System.Serializable]
    public struct LevelVariation
    {
        public string variationName;
        [TextArea] public string riddleText;
        public float timeLimit;
        public int difficultyLevel;
        
        public TutorialType tutorialRequired;
        
        [Header("Configuración Visual")]
        public Sprite backgroundSprite; 
    }

    [CreateAssetMenu(fileName = "NewLevel", menuName = "Sensory/LevelConfig")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Identificación")]
        public string levelID;
        public string LvlName; 

        [Header("Lógica")]
        public PuzzleBase puzzlePrefab;

        [Header("Variaciones del Nivel")]
        public List<LevelVariation> variations;
        
        public LevelVariation GetVariation(int index)
        {
            if (variations == null || variations.Count == 0) return default;
            return variations[Mathf.Clamp(index, 0, variations.Count - 1)];
        }
    }
}