using UnityEngine;
using System.Collections.Generic;
using _Game.Scripts.Data;
using TMPro;

namespace _Game.Scripts.Core.UI
{
    public class RankingUI : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameObject _rowPrefab;
        [SerializeField] private Transform _contentContainer;

        public void PopulateRanking(List<LevelConfig> allLevels)
        {
            foreach (Transform child in _contentContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var level in allLevels)
            {
                CreateScoreRow(level);
            }
        }

        private void CreateScoreRow(LevelConfig levelData)
        {
            GameObject newRow = Instantiate(_rowPrefab, _contentContainer);
            newRow.transform.localScale = Vector3.one;
            Vector3 pos = newRow.transform.localPosition;
            pos.z = 0;
            newRow.transform.localPosition = pos;
            TextMeshProUGUI[] texts = newRow.GetComponentsInChildren<TextMeshProUGUI>();
            
            if (texts.Length >= 2)
            {
                texts[0].text = levelData.LvlName;
                
                int score = ScoreManager.GetHighScore(levelData.levelID);
                texts[1].text = score > 0 ? $"{score} pts" : "0 pts";
            }
        }
    }
}