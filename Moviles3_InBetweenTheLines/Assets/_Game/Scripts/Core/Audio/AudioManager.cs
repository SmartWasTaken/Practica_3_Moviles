using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;

namespace _Game.Scripts.Core.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [Header("Config")]
        public SoundLibrary soundLibrary;
        public AudioMixer audioMixer;
        public AudioSource musicSource;
        public AudioSource sfxSource;

        private Dictionary<string, SoundData> _soundLookup;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            _soundLookup = soundLibrary.GetSoundDictionary();
            LoadVolumeSettings();
        }

        public void PlaySFX(string soundID)
        {
            if (_soundLookup.TryGetValue(soundID, out SoundData data))
            {
                float finalPitch = data.pitch;
                if (data.randomPitch)
                    finalPitch += Random.Range(-data.pitchVariance, data.pitchVariance);

                sfxSource.pitch = finalPitch;
                sfxSource.PlayOneShot(data.clip, data.volume);
            }
            else
            {
                Debug.LogWarning($"Sonido no encontrado: {soundID}");
            }
        }

        public void PlayMusic(string soundID)
        {
            if (_soundLookup.TryGetValue(soundID, out SoundData data))
            {
                musicSource.clip = data.clip;
                musicSource.volume = data.volume;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void SetMusicVolume(float value)
        {
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            audioMixer.SetFloat("MusicVol", db);

            PlayerPrefs.SetFloat("MusicVol", value);
            PlayerPrefs.Save();
        }

        public void SetSFXVolume(float value)
        {
            float db = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
            audioMixer.SetFloat("SFXVol", db);

            PlayerPrefs.SetFloat("SFXVol", value);
            PlayerPrefs.Save();
        }

        private void LoadVolumeSettings()
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.75f);

            SetMusicVolume(musicVol);
            SetSFXVolume(sfxVol);
        }
    }
}