using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Default Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float volume = 0.4f;

    [Header("Per-Scene Music")]
    [Tooltip("Add an entry for each scene that needs its own music clip")]
    public SceneMusic[] sceneMusics;

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip music;
    }

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource             = gameObject.AddComponent<AudioSource>();
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.volume      = volume;

        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    void PlayMusicForScene(string sceneName)
    {
        AudioClip clipToPlay = backgroundMusic; // default

        if (sceneMusics != null)
        {
            foreach (SceneMusic sm in sceneMusics)
            {
                if (sm.sceneName == sceneName && sm.music != null)
                {
                    clipToPlay = sm.music;
                    break;
                }
            }
        }

        if (clipToPlay == null) return;

        // Only swap if the clip actually changed
        if (audioSource.clip == clipToPlay) return;

        audioSource.Stop();
        audioSource.clip = clipToPlay;
        audioSource.Play();
    }

    // ── Optional public controls ──────────────────────────────────────────

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (audioSource != null) audioSource.volume = volume;
    }

    public void Pause()  { if (audioSource != null) audioSource.Pause(); }
    public void Resume() { if (audioSource != null && !audioSource.isPlaying) audioSource.Play(); }
    public void Stop()   { if (audioSource != null) audioSource.Stop(); }
}