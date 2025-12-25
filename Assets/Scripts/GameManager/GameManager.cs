using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private bool gamePaused = false;

    private MapSize mapSize = MapSize.Medium;

    private string seedString;
    
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
        SceneManager.LoadScene("GameplayScene");
    }

    public void SetMapSize(int size)
    {
        mapSize = (MapSize)size;
    }

    public MapSize GetMapSize()
    {
        return mapSize;
    }

    public void SaveSeed(TMP_InputField seedInputField)
    {
        seedString = seedInputField.text;
    }

    public string GetSeedString()
    {
        return seedString;
    }

    public void Quit()
    {
        Application.Quit();
    }
}

