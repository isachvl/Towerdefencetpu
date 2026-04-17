using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("Panels / Canvases")]
    [SerializeField] private GameObject startMenuCanvas;
    [SerializeField] private GameObject levelSelectCanvas;
    [SerializeField] private GameObject settingsCanvas;
    [SerializeField] private GameObject raiting;

    public void OpenLevelSelect()
    {
        if (startMenuCanvas != null)
            startMenuCanvas.SetActive(false);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        if (levelSelectCanvas != null)
            levelSelectCanvas.SetActive(true);
    }

    public void OpenSettings()
    {
        if (startMenuCanvas != null)
            startMenuCanvas.SetActive(false);

        if (levelSelectCanvas != null)
            levelSelectCanvas.SetActive(false);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(true);
    }
    public void Raiting()
    {
        if (startMenuCanvas != null)
            startMenuCanvas.SetActive(false);

        if (levelSelectCanvas != null)
            levelSelectCanvas.SetActive(false);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        if (raiting != null)
            raiting.SetActive(true);
    }

    public void BackToStartMenu()
    {
        if (levelSelectCanvas != null)
            levelSelectCanvas.SetActive(false);

        if (settingsCanvas != null)
            settingsCanvas.SetActive(false);

        if (startMenuCanvas != null)
            startMenuCanvas.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log("Выход из игры...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}