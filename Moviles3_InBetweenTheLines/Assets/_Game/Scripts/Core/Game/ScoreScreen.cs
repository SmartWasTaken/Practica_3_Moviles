using _Game.Scripts.Core.UI.Effects;
using TMPro;
using UnityEngine;

public class ScoreScreen : MonoBehaviour
{
    [SerializeField] private GameObject[] _hearts;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _newRecordStamp;
    [SerializeField] private UIAnimator _stampAnimator;

    public void Show(int lives, int score, bool isNewRecord)
    {
        gameObject.SetActive(true);

        for (int i = 0; i < _hearts.Length; i++)
        {
            _hearts[i].SetActive(i < lives); 
        }

        _scoreText.text = score.ToString();

        if (isNewRecord)
        {
            _newRecordStamp.SetActive(true);
        }
        else
        {
            _newRecordStamp.SetActive(false);
        }
    }
}