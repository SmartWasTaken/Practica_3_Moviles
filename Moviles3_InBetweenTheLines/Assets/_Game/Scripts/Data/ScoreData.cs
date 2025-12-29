using System;
using System.Collections.Generic;

namespace _Game.Scripts.Data
{
    [Serializable]
    public class ScoreEntry
    {
        public int score;
        public string date;
        
        // Constructor para crearlo rápido
        public ScoreEntry(int score, DateTime dateObj)
        {
            this.score = score;
            this.date = dateObj.ToString("dd/MM/yyyy - HH:mm");
        }
    }

    [Serializable]
    public class ScoreListWrapper
    {
        public List<ScoreEntry> entries = new List<ScoreEntry>();
    }
}