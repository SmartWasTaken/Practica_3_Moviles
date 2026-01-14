using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace _Game.Scripts.Data
{
    public static class ScoreManager
    {
        public static void SaveScore(string levelID, int score)
        {
            ScoreListWrapper currentData = LoadScoreHistory(levelID);

            ScoreEntry newEntry = new ScoreEntry(score, DateTime.Now);

            currentData.entries.Add(newEntry);

            currentData.entries = currentData.entries.OrderByDescending(x => x.date).ToList();

            string json = JsonUtility.ToJson(currentData);
            PlayerPrefs.SetString($"History_{levelID}", json);
            PlayerPrefs.Save();
        }

        public static ScoreListWrapper LoadScoreHistory(string levelID)
        {
            string key = $"History_{levelID}";
            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                return JsonUtility.FromJson<ScoreListWrapper>(json);
            }
            return new ScoreListWrapper();
        }

        public static int GetBestScore(string levelID)
        {
            var data = LoadScoreHistory(levelID);
            if (data.entries.Count > 0)
                return data.entries[0].score;
            return 0;
        }
    }
}