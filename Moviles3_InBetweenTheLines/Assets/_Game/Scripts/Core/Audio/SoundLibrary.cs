using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Core.Audio
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        public SoundData[] sounds;

        public Dictionary<string, SoundData> GetSoundDictionary()
        {
            Dictionary<string, SoundData> dict = new Dictionary<string, SoundData>();
            foreach (var sound in sounds)
            {
                if (!dict.ContainsKey(sound.id))
                    dict.Add(sound.id, sound);
            }
            return dict;
        }
    }
}