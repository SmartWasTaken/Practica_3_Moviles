using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core.Audio;

namespace _Game.Scripts.Core.Menu
{
    public class SettingsPanelController : MonoBehaviour
    {
        public Slider musicSlider;
        public Slider sfxSlider;

        private void Start()
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);
            musicSlider.onValueChanged.AddListener(OnMusicChanged);
            sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        }

        public void OnMusicChanged(float val)
        {
            AudioManager.Instance.SetMusicVolume(val);
        }

        public void OnSFXChanged(float val)
        {
            AudioManager.Instance.SetSFXVolume(val);
            // Opcional: Play un sonido de prueba para que el usuario oiga el volumen
            // if (!dragging) AudioManager.Instance.PlaySFX("ui_hover");
        }
    }
}