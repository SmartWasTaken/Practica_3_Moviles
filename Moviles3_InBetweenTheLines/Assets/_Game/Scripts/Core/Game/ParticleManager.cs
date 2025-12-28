using UnityEngine;

namespace _Game.Scripts.Core.Game
{
    public class ParticleManager : MonoBehaviour
    {
        public static ParticleManager Instance;
        [SerializeField] private ParticleSystem _confettiWin;
        [SerializeField] private ParticleSystem _tapEffect;

        private void Awake() { Instance = this; }

        public void PlayWinConfetti()
        {
            _confettiWin.Play();
        }

        public void PlayTapEffect(Vector3 screenPos)
        {
            _tapEffect.transform.position = Camera.main.ScreenToWorldPoint(screenPos);
            _tapEffect.Play();
        }
    }
}