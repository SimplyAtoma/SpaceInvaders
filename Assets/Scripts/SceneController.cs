using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton — survives all scene loads.
/// 
/// IMPORTANT SETUP:
///   Create a dedicated "Bootstrap" scene (index 0) containing only
///   this GameObject. It loads MainMenu immediately and is never
///   visited again. This prevents duplicate instances when MainMenu
///   reloads after Credits.
///
///   Build Settings order:
///     0: Bootstrap   ← just SceneController + AudioManager here
///     1: MainMenu
///     2: MainGame
///     3: Credits
///
///   If you don't want a Bootstrap scene, put SceneController in
///   MainMenu but make sure to follow the "first-run guard" below.
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string mainGameScene = "MainGame";
    [SerializeField] private string creditsScene  = "Credits";

    [Header("Timing")]
    [SerializeField] private float creditsDisplayTime = 5f;

    private void Awake()
    {
        // If an instance already exists from a previous load, destroy
        // THIS new duplicate — keep the original alive.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────

    public void GoToMainGame()
    {
        SceneManager.LoadScene(mainGameScene);
    }

    public void GoToCredits()
    {
        StartCoroutine(CreditsRoutine());
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── Internal ──────────────────────────────────────────────────────────

    private IEnumerator CreditsRoutine()
    {
        SceneManager.LoadScene(creditsScene);
        yield return new WaitForSeconds(creditsDisplayTime);
        GoToMainMenu();
    }
}