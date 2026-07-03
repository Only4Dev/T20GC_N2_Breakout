using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ballHitSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip pauseClip;
    [SerializeField] private AudioClip ballHitClip;       // paddle + walls
    [SerializeField] private AudioClip metalBrickHitClip; // damaged, not destroyed
    [SerializeField] private AudioClip brickBreakClip;    // destroyed

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    [Header("Brick Break Pitch Variation")]
    [SerializeField] private float minPitch = 0.9f;
    [SerializeField] private float maxPitch = 1.1f;

    private void Start()
    {
        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void PlayPause() => PlaySfx(pauseClip);
    public void PlayBallHit()
    {
        if (ballHitClip == null)
            return;

        ballHitSource.Stop();
        ballHitSource.pitch = Random.Range(minPitch, maxPitch);
        ballHitSource.clip = ballHitClip;
        ballHitSource.volume = sfxVolume;
        ballHitSource.Play();
    }
    public void PlayMetalBrickHit() => PlaySfx(metalBrickHitClip);
    public void PlayBrickBreak()
    {
        sfxSource.pitch = Random.Range(minPitch, maxPitch);
        sfxSource.PlayOneShot(brickBreakClip, sfxVolume);
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();
}