using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core.Audio;

public class SettingsPanelUi : MonoBehaviour
{
	public Slider musicSlider;
	public Slider sfxSlider;

	void Start()
	{
		musicSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.75f);
		sfxSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.75f);
		
		musicSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetMusicVolume(val));
		sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSFXVolume(val));
	}
}