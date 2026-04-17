using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject startMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Scene Names")]
    [SerializeField] private string levelSceneName = "123";

    public void OpenLevelSelect()
    {
        if (startMenuPanel != null)
            startMenuPanel.SetActive(false);

        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);

        if (startMenuPanel != null)
            startMenuPanel.SetActive(true);
    }

    public void LoadLevel123()
    {
        if (!string.IsNullOrWhiteSpace(levelSceneName))
        {
            SceneManager.LoadScene(levelSceneName);
        }
        else
        {
            Debug.LogError("Имя сцены не задано в LevelSelectMenu.");
        }
    }
}