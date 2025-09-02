using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip musicBackground;
    public AudioClip sfxLose;
    public AudioClip sfxKillEnemy;
    public AudioClip sfxThrow;

    [Header("Optional Audio Mixer")]
    public AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Phát nhạc nền khi bắt đầu
        if (musicBackground != null)
            PlayMusic(musicBackground);
    }

    #region Music
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
        audioMixer?.SetFloat("MusicVolume", Mathf.Log10(volume + 0.0001f) * 20);
    }

    public void SetMusicMute(bool mute)
    {
        musicSource.mute = mute;
    }
    #endregion

    #region SFX
    public void PlayLose()
    {
        PlaySFX(sfxLose);
    }

    public void PlayKillEnemy()
    {
        PlaySFX(sfxKillEnemy);
    }

    public void PlayThrow()
    {
        PlaySFX(sfxThrow);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
        audioMixer?.SetFloat("SFXVolume", Mathf.Log10(volume + 0.0001f) * 20);
    }

    public void SetSFXMute(bool mute)
    {
        sfxSource.mute = mute;
    }
    #endregion

   
}
