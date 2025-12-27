using UnityEngine;

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

    private void HandleStateChanged(GameState state)
    {
        pauseUI.SetActive(state == GameState.Paused);
    }

    public void OnResumeButton()
    {
        GameManager.Instance.SetState(GameState.Playing);
    }

    public void OnQuitButton()
    {
        GameManager.Instance.QuitGame();
    }
}
