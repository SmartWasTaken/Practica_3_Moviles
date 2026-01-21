using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Data;

namespace _Game.Scripts.Core.UI
{
    public class RankingUI : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private RankingRow _rowPrefab;
        [SerializeField] private Transform _contentContainer;

        public void ShowRanking(string levelID)
        {
            foreach (Transform child in _contentContainer)
            {
                Destroy(child.gameObject);
            }
            ScoreListWrapper history = ScoreManager.LoadScoreHistory(levelID);
            
            int realHighScore = ScoreManager.GetBestScore(levelID);

            for (int i = 0; i < history.entries.Count; i++)
            {
                ScoreEntry entry = history.entries[i];
                
                RankingRow newRow = Instantiate(_rowPrefab, _contentContainer);
                
                newRow.transform.localScale = Vector3.one;
                Vector3 pos = newRow.transform.localPosition;
                pos.z = 0;
                newRow.transform.localPosition = pos;
                bool isHighest = (entry.score == realHighScore); 
                
                newRow.Setup(entry.date, entry.score, isHighest);
            }
        }
    }
}