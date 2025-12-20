using UnityEngine;

namespace _Game.Scripts.Core.Audio
{
    [CreateAssetMenu(fileName = "NewSound", menuName = "Audio/Sound Data")]
    public class SoundData : ScriptableObject
    {
        public string id;
        public AudioClip clip;

        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;

        [Header("Randomness (Juice)")]
        public bool randomPitch = false;
        [Range(0f, 0.5f)] public float pitchVariance = 0.1f;
    }
}