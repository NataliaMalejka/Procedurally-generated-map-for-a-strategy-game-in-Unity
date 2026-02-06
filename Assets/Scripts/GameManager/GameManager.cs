using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Represents the global game state
public enum GameState
{
    Playing,
    Paused
}

// Central game manager responsible for game state
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Current game state
    public GameState State { get; private set; } = GameState.Playing;

    // Event fired whenever the game state changes
    public static event System.Action<GameState> OnGameStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Lock game to 60 FPS
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        // Toggle pause when Escape is pressed (only in gameplay scene)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "GameplayScene")
                return;

            TogglePause();
        }
    }

    // Switches between Playing and Paused states
    private void TogglePause()
    {
        if (State == GameState.Playing)
            SetState(GameState.Paused);
        else
            SetState(GameState.Playing);
    }

    // Sets the game state and applies time scaling
    public void SetState(GameState newState)
    {
        State = newState;
        OnGameStateChanged?.Invoke(State);
        Time.timeScale = State == GameState.Paused ? 0f : 1f;
    }

    // Starts a new game by initializing settings and loading scenes
    public void StartGame()
    {
        GameSettings.Instance.SetLayersIndex();

        StartCoroutine(LoadGameWithLoadingScreen());
    }

    // Loads gameplay scene asynchronously with a loading screen
    private IEnumerator LoadGameWithLoadingScreen()
    {
        Time.timeScale = 1f;

        // Load loading screen
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);

        // Load loading screen
        yield return null;
        yield return null;

        // Load loading screen
        AsyncOperation gameplayLoad = SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);

        gameplayLoad.allowSceneActivation = false;

        // Wait until scene is almost loaded
        while (gameplayLoad.progress < 0.9f)
        {
            yield return null; 
        }

        // Activate gameplay scene
        gameplayLoad.allowSceneActivation = true;

        while (!gameplayLoad.isDone)
            yield return null;

        // Remove loading scene
        SceneManager.UnloadSceneAsync("LoadingScene");
    }

    // Quits the application
    public void QuitGame()
    {
        Application.Quit();
    }
}

