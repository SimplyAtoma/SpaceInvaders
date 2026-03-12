using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to a GameObject in the MainMenu scene.
/// Handles the Start and Quit buttons.
/// The idle enemy animations on the main menu are handled automatically
/// by the Animator components on the enemy display GameObjects.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Optional — keyboard shortcut to start")]
    [SerializeField] private bool allowEnterToStart = true;

    private void Update()
    {
        if (!allowEnterToStart) return;
        var kb = Keyboard.current;
        if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame))
            OnStartButton();
    }

    // Wire to Start Button's OnClick()
    public void OnStartButton()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.GoToMainGame();
        else
            // Fallback if SceneController not present
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }

    // Wire to Quit Button's OnClick()
    public void OnQuitButton()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.QuitGame();
    }
}