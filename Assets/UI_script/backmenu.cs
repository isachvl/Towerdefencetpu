using UnityEngine;

public class PauseEscInput : MonoBehaviour
{
    [SerializeField] private PauseCanvasController pauseController;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseController != null)
            {
                pauseController.TogglePause();
            }
        }
    }
}