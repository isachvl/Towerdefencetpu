using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseCanvasController : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject gameplayCanvas;
    [SerializeField] private GameObject pauseCanvas;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "Startmenu";

    private bool isPaused = false;

    private void Start()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (gameplayCanvas != null)
            gameplayCanvas.SetActive(true);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (gameplayCanvas != null)
            gameplayCanvas.SetActive(false);

        if (pauseCanvas != null)
            pauseCanvas.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pauseCanvas != null)
            pauseCanvas.SetActive(false);

        if (gameplayCanvas != null)
            gameplayCanvas.SetActive(true);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public bool IsPaused()
    {
        return isPaused;
    }
}