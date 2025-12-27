using UnityEngine;

namespace _Game.Scripts.Data
{
    public static class ScoreManager
    {
        public static void SaveScore(string levelID, int score)
        {
            int currentHighScore = GetHighScore(levelID);
            
            if (score > currentHighScore)
            {
                PlayerPrefs.SetInt($"Highscore_{levelID}", score);
                PlayerPrefs.Save();
                Debug.Log($"¡Nuevo Récord para {levelID}: {score}!");
            }
        }

        public static int GetHighScore(string levelID)
        {
            return PlayerPrefs.GetInt($"Highscore_{levelID}", 0);
        }

        public static bool IsNewHighScore(string levelID, int score)
        {
            return score > GetHighScore(levelID);
        }
    }
}