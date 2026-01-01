using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing,
    Paused
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Playing;

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

       Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "GameplayScene")
                return;

            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (State == GameState.Playing)
            SetState(GameState.Paused);
        else
            SetState(GameState.Playing);
    }

    public void SetState(GameState newState)
    {
        State = newState;
        OnGameStateChanged?.Invoke(State);
        Time.timeScale = State == GameState.Paused ? 0f : 1f;
    }

    public void StartGame()
    {
        GameSettings.Instance.SetLayersIndex();

        StartCoroutine(LoadGameWithLoadingScreen());
    }

    private IEnumerator LoadGameWithLoadingScreen()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);

        yield return null;
        yield return null;

        AsyncOperation gameplayLoad = SceneManager.LoadSceneAsync("GameplayScene", LoadSceneMode.Additive);

        gameplayLoad.allowSceneActivation = false;

        while (gameplayLoad.progress < 0.9f)
        {
            yield return null; 
        }

        gameplayLoad.allowSceneActivation = true;

        while (!gameplayLoad.isDone)
            yield return null;

        SceneManager.UnloadSceneAsync("LoadingScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

