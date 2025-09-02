using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;    

public class AudioSettingUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
   
    [SerializeField] private AudioMixer MyMixer;

    [SerializeField] private Slider SfxSlider;
    [SerializeField] private AudioMixer SfxMixer;
     
    private void Start()
    {
        if( PlayerPrefs.HasKey("MusicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            LoadSfxVolume();
        }
        else
        {
            SetSfxVolume();
        }
    }
    public void SetMusicVolume()
    {
        float volume = musicSlider.value;
        MyMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSfxVolume()
    {
        float volume = musicSlider.value;
        SfxMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
    public void LoadVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SetMusicVolume();
    }

    public void LoadSfxVolume()
    {
        musicSlider.value = PlayerPrefs.GetFloat("SFXVolume");
        SetSfxVolume();
    }

}
