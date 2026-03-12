using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a GameObject in the MainMenu scene.
/// Handles Start / Quit buttons and Enter key shortcut.
/// Falls back to direct scene load if SceneController isn't present.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string mainGameScene    = "MainGame";
    [SerializeField] private bool   allowEnterToStart = true;

    private void Update()
    {
        if (!allowEnterToStart) return;
        var kb = Keyboard.current;
        if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
            OnStartButton();
    }

    // Wire to Start Button OnClick()
    public void OnStartButton()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.GoToMainGame();
        else
            SceneManager.LoadScene(mainGameScene);  // fallback
    }

    // Wire to Quit Button OnClick()
    public void OnQuitButton()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.QuitGame();
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}