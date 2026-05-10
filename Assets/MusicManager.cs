using UnityEngine;

/// <summary>
/// Place this on a GameObject in your very first scene (e.g. the main menu or Tutorial).
/// It survives scene loads and plays the background music on loop forever.
/// Only one instance ever exists — duplicates destroy themselves on load.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Background Music")]
    public AudioClip backgroundMusic;   // Assign your music clip in Inspector
    [Range(0f, 1f)]
    public float volume = 0.4f;         // Adjust to taste

    private AudioSource audioSource;

    void Awake()
    {
        // Singleton — only one MusicManager survives across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);   // Persists across all scene loads

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip        = backgroundMusic;
        audioSource.loop        = true;
        audioSource.playOnAwake = false;
        audioSource.volume      = volume;

        if (backgroundMusic != null)
            audioSource.Play();
        else
            Debug.LogWarning("MusicManager: no background music clip assigned.");
    }

    // ── Optional public controls ──────────────────────────────────────────

    public void SetVolume(float v)
    {
        volume = Mathf.Clamp01(v);
        if (audioSource != null)
            audioSource.volume = volume;
    }

    public void Pause()
    {
        if (audioSource != null) audioSource.Pause();
    }

    public void Resume()
    {
        if (audioSource != null) audioSource.UnPause();
    }

    public void Stop()
    {
        if (audioSource != null) audioSource.Stop();
    }
}