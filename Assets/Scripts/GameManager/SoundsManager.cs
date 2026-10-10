using UnityEngine;

// Handles playing global game sounds
public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance { get; private set; }

    // Audio clip played when a new turn starts
    [SerializeField] private AudioClip nextTurn;

    [SerializeField] private AudioSource audioSource;

    public enum Sounds
    {
        NextTurn
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Start background audio
        audioSource.Play();
    }

    // Plays a selected sound effect
    public void PlaySounds(Sounds sounds)
    {
        switch (sounds)
        {
            case Sounds.NextTurn:
                audioSource.PlayOneShot(nextTurn);
                break;
            default:
                break;
        }
    }
}
