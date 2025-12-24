using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool gamePaused = false;

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

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1f;
    }

    public void StartGame()
    {

    }
}

