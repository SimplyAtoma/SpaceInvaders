using UnityEngine;

/// <summary>
/// Singleton AudioManager — plays all game sounds.
/// Attach to a persistent GameObject with AudioSource components.
/// 
/// Required AudioSource setup:
///   - musicSource:  AudioSource with Loop=true, Play On Awake=false
///   - sfxSource:    AudioSource with Loop=false, Play On Awake=false
///
/// Assign AudioClips in the Inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] [Range(0,1)] private float musicVolume = 0.4f;

    [Header("SFX — Player")]
    [SerializeField] private AudioClip playerShootSFX;
    [SerializeField] private AudioClip playerExplodeSFX;

    [Header("SFX — Enemy")]
    [SerializeField] private AudioClip enemyShootSFX;
    [SerializeField] private AudioClip enemyExplodeSFX;

    [Header("SFX Volume")]
    [SerializeField] [Range(0,1)] private float sfxVolume = 0.8f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayMusic();
    }

    private void OnEnable()
    {
        GameManager.OnPlayerDied += PlayPlayerExplode;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerDied -= PlayPlayerExplode;
    }

    // ── Music ──────────────────────────────────────────────────────────────

    public void PlayMusic()
    {
        if (musicSource == null || backgroundMusic == null) return;
        musicSource.clip   = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop   = true;
        musicSource.Play();
    }

    public void StopMusic() => musicSource?.Stop();

    // ── SFX ───────────────────────────────────────────────────────────────

    public void PlayPlayerShoot()   => PlaySFX(playerShootSFX);
    public void PlayPlayerExplode() => PlaySFX(playerExplodeSFX);
    public void PlayEnemyShoot()    => PlaySFX(enemyShootSFX);
    public void PlayEnemyExplode()  => PlaySFX(enemyExplodeSFX);

    private void PlaySFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
}