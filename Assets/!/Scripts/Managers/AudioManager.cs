using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixers")]
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup bgmMixerGroup;

    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource;


    [Header("Audio")]
    [SerializeField] private AudioClip[] sfxClips;

    public AudioMixer MasterMixer => masterMixer;
    public AudioMixerGroup SfxMixerGroup => sfxMixerGroup;
    public AudioMixerGroup BgmMixerGroup => bgmMixerGroup;
    public AudioSource BgmSource => bgmSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureBgmSource();
    }

    private void EnsureBgmSource()
    {
        if (bgmSource == null)
        {
            bgmSource = GetComponent<AudioSource>();
            if (bgmSource == null)
            {
                bgmSource = gameObject.AddComponent<AudioSource>();
            }
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        if (bgmMixerGroup != null)
        {
            bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        }
    }

    /// <summary>
    /// Plays a background music clip through the dedicated BGM AudioSource.
    /// </summary>
    public void PlayBGM(AudioClip clip, bool loop = true, float volume = 1f)
    {
        if (bgmSource == null)
        {
            EnsureBgmSource();
        }

        if (clip == null)
        {
            Debug.LogWarning("PlayBGM called with null clip.");
            return;
        }

        if (bgmMixerGroup != null)
        {
            bgmSource.outputAudioMixerGroup = bgmMixerGroup;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = Mathf.Clamp01(volume);
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    public void PauseBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Pause();
        }
    }

    public void ResumeBGM()
    {
        if (bgmSource != null && bgmSource.clip != null && !bgmSource.isPlaying)
        {
            bgmSource.UnPause();
        }
    }

    /// <summary>
    /// Plays a one-shot SFX routed through the SFX mixer group.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("PlaySFX called with null clip.");
            return;
        }

        GameObject temp = new GameObject("SFX_OneShot");
        temp.transform.SetParent(transform);
        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Clamp(pitch, 0.01f, 4f);
        source.playOnAwake = false;
        if (sfxMixerGroup != null)
        {
            source.outputAudioMixerGroup = sfxMixerGroup;
        }

        source.Play();
        Destroy(temp, clip.length / source.pitch + 0.1f);
    }

    /// <summary>
    /// Plays a one-shot SFX at a world position routed through the SFX mixer group.
    /// </summary>
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("PlaySFXAtPoint called with null clip.");
            return;
        }

        GameObject temp = new GameObject("SFX_3D_OneShot");
        temp.transform.position = position;
        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Clamp(pitch, 0.01f, 4f);
        source.spatialBlend = 1f;
        source.playOnAwake = false;
        if (sfxMixerGroup != null)
        {
            source.outputAudioMixerGroup = sfxMixerGroup;
        }
        source.Play();
        Destroy(temp, clip.length / source.pitch + 0.1f);
    }

    public void SetBGMVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("BGMVolume", LinearToDecibel(volume));
        }
        else if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("SFXVolume", LinearToDecibel(volume));
        }
    }

    public void SetMasterVolume(float volume)
    {
        if (masterMixer != null)
        {
            masterMixer.SetFloat("MasterVolume", LinearToDecibel(volume));
        }
        else
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }
    }

    private static float LinearToDecibel(float linear)
    {
        linear = Mathf.Clamp01(linear);
        return linear <= 0.0001f ? -80f : 20f * Mathf.Log10(linear);
    }
}