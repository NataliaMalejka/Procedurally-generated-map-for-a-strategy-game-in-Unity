using UnityEngine;

// Controls the pause menu UI
public class PausePanel : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= HandleStateChanged;
    }

    // Enables or disables pause UI based on game state
    private void HandleStateChanged(GameState state)
    {
        pauseUI.SetActive(state == GameState.Paused);
    }

    // Resumes gameplay from pause
    public void OnResumeButton()
    {
        GameManager.Instance.SetState(GameState.Playing);
    }

    // Quits the application from pause menu
    public void OnQuitButton()
    {
        GameManager.Instance.QuitGame();
    }
}
