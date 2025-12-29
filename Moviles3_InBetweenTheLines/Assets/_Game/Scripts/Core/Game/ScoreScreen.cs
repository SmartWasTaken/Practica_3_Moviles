using UnityEngine;
using TMPro;
using DG.Tweening; 
using UnityEngine.UI;
using System;

namespace _Game.Scripts.Core.UI
{
    public class ScoreScreen : MonoBehaviour
    {
        [Header("Referencias UI")]
        [SerializeField] private CanvasGroup _canvasGroup; 
        [SerializeField] private Image[] _hearts; 
        [SerializeField] private TextMeshProUGUI _levelScoreText; 
        [SerializeField] private TextMeshProUGUI _totalScoreText; 
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private GameObject _newRecordStamp; 

        [Header("Configuración")]
        [SerializeField] private float _heartBeatDuration = 0.5f;
        [SerializeField] private float _scoreCountDuration = 1.0f;
        [SerializeField] private float _finalDelay = 2.0f; 

        [Header("Colores")]
        [SerializeField] private Color _aliveColor = Color.white;
        [SerializeField] private Color _deadColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color _winTitleColor = Color.yellow;
        [SerializeField] private Color _loseTitleColor = Color.red;

        private Sequence _sequence;

        public void AnimateSequence(int livesAfterGame, int scoreEarned, int previousTotalScore, bool isWin, bool isRecord, Action onComplete)
        {
            gameObject.SetActive(true);
            _canvasGroup.alpha = 0; 
            _canvasGroup.blocksRaycasts = true;

            _titleText.text = isWin ? "YOU WON!" : "YOU FAILED!";
            _titleText.color = isWin ? _winTitleColor : _loseTitleColor;

            _levelScoreText.text = $"+{scoreEarned}";
            _totalScoreText.text = previousTotalScore.ToString();
            _newRecordStamp.SetActive(false);

            int livesBeforeImpact = isWin ? livesAfterGame : livesAfterGame + 1;

            for (int i = 0; i < _hearts.Length; i++)
            {
                _hearts[i].gameObject.SetActive(true);
                _hearts[i].color = (i < livesBeforeImpact) ? _aliveColor : _deadColor;
                _hearts[i].transform.localScale = Vector3.one; 
            }

            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence.SetUpdate(true); 

            _sequence.Append(_canvasGroup.DOFade(1, 0.3f));

            for (int i = 0; i < livesBeforeImpact; i++)
            {
                if (i < _hearts.Length) 
                    _sequence.Join(_hearts[i].transform.DOPunchScale(Vector3.one * 0.3f, _heartBeatDuration, 5, 1));
            }

            if (!isWin)
            {
                int heartToDieIndex = livesAfterGame;
                if (heartToDieIndex < _hearts.Length && heartToDieIndex >= 0)
                {
                    Image heartImg = _hearts[heartToDieIndex];
                    _sequence.Append(heartImg.transform.DOScale(0.8f, 0.4f)); 
                    _sequence.Join(heartImg.DOColor(_deadColor, 0.4f)); 
                }
            }

            // Conteo de Puntos
            if (scoreEarned > 0)
            {
                _sequence.AppendInterval(0.2f);
                _sequence.Append(DOVirtual.Float(scoreEarned, 0, _scoreCountDuration, v => _levelScoreText.text = $"+{Mathf.RoundToInt(v)}"));
                _sequence.Join(DOVirtual.Float(previousTotalScore, previousTotalScore + scoreEarned, _scoreCountDuration, v => _totalScoreText.text = Mathf.RoundToInt(v).ToString()));
            }

            _sequence.AppendInterval(_finalDelay);
            _sequence.OnComplete(() => {
                _canvasGroup.DOFade(0, 0.2f).OnComplete(() => {
                     gameObject.SetActive(false);
                     onComplete?.Invoke();
                });
            });
        }
    }
}