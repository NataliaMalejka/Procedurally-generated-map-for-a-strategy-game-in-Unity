using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance { get; private set; }

    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioClip nextTurn;

    [SerializeField] private AudioSource audioSource;

    public enum Sounds
    {
        BackgroundMusic,
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
        audioSource.Play();
    }

    public void PlaySounds(Sounds sounds)
    {
        switch (sounds)
        {
            case Sounds.BackgroundMusic:
                audioSource.PlayOneShot(backgroundMusic);
                break;
            case Sounds.NextTurn:
                audioSource.PlayOneShot(nextTurn);
                break;
            default:
                break;
        }
    }
}
