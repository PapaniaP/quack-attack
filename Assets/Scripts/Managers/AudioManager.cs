using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip backgroundMusic;
    public AudioClip shootSFX;
    public AudioClip missSFX;
    public AudioClip damageOwOuchSFX;  // "Ow!"/"Ouch!" sound when player takes damage

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMissSFX()
    {
        if (missSFX != null)
        {
            sfxSource.PlayOneShot(missSFX);
        }
    }

    public void PlayDamageSFX()
    {
        if (damageOwOuchSFX != null)
        {
            sfxSource.PlayOneShot(damageOwOuchSFX);
        }
    }

    public void PauseMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
        {
            musicSource.Pause();
        }
    }

    public void ResumeMusic()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }
}
