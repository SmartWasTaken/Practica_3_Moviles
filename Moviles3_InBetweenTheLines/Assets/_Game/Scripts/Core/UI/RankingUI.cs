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
            
            for (int i = 0; i < history.entries.Count; i++)
            {
                ScoreEntry entry = history.entries[i];
                
                RankingRow newRow = Instantiate(_rowPrefab, _contentContainer);
                
                newRow.transform.localScale = Vector3.one;
                Vector3 pos = newRow.transform.localPosition;
                pos.z = 0;
                newRow.transform.localPosition = pos;

                bool isHighest = (i == 0); 
                newRow.Setup(entry.date, entry.score, isHighest);
            }
        }
    }
}