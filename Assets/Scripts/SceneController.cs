using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages all scene transitions.
/// Attach to a persistent GameObject (DontDestroyOnLoad).
/// 
/// Build Settings scene order:
///   0 = MainMenu
///   1 = MainGame
///   2 = Credits
/// </summary>
public class SceneController : MonoBehaviour
{
    public static SceneController Instance { get; private set; }

    [Header("Scene Names (must match Build Settings exactly)")]
    [SerializeField] private string mainMenuScene  = "MainMenu";
    [SerializeField] private string mainGameScene  = "MainGame";
    [SerializeField] private string creditsScene   = "Credits";

    [Header("Timing")]
    [SerializeField] private float creditsDisplayTime = 5f;

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

    // Called by Main Menu Start button
    public void GoToMainGame()
    {
        SceneManager.LoadScene(mainGameScene);
    }

    // Called when player dies OR all enemies defeated
    public void GoToCredits()
    {
        StartCoroutine(CreditsRoutine());
    }

    private IEnumerator CreditsRoutine()
    {
        SceneManager.LoadScene(creditsScene);
        yield return new WaitForSeconds(creditsDisplayTime);
        GoToMainMenu();
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
}