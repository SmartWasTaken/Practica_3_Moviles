using UnityEngine;
using TMPro;

namespace _Game.Scripts.Core.UI
{
    public class RankingRow : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private TextMeshProUGUI _dateText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private GameObject _highScoreStamp;

        public void Setup(string date, int score, bool isFirstPosition)
        {
            _dateText.text = date;
            _scoreText.text = $"{score} pts";

            if (_highScoreStamp != null)
            {
                _highScoreStamp.SetActive(isFirstPosition);
            }
        }
    }
}