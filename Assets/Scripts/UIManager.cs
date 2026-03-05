using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Handles all HUD and screen UI.
/// No physics code — identical in 2D and 3D projects.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("Score Advance Table")]
    [SerializeField] private GameObject scoreAdvancePanel;
    [SerializeField] private float      scoreAdvanceDisplayTime = 3f;

    [Header("Game Over")]
    [SerializeField] private GameObject      gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameOverHighScoreText;

    [Header("Win Screen")]
    [SerializeField] private GameObject winPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Intentionally empty — subscriptions happen in Start()
        // so they always run AFTER GameManager.Awake() clears static events
    }

    private void OnDisable()
    {
        GameManager.OnScoreChanged       -= UpdateScoreDisplay;
        GameManager.OnHighScoreChanged   -= UpdateHighScoreDisplay;
        GameManager.OnGameOver           -= ShowGameOver;
        GameManager.OnAllEnemiesDefeated -= ShowWin;
    }

    private void Start()
    {
        // Subscribe here — guaranteed to run after all Awake() calls including GameManager
        GameManager.OnScoreChanged       += UpdateScoreDisplay;
        GameManager.OnHighScoreChanged   += UpdateHighScoreDisplay;
        GameManager.OnGameOver           += ShowGameOver;
        GameManager.OnAllEnemiesDefeated += ShowWin;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (winPanel)      winPanel.SetActive(false);

        if (scoreAdvancePanel)
        {
            scoreAdvancePanel.SetActive(true);
            StartCoroutine(HideScoreAdvanceTable());
        }

        if (GameManager.Instance != null)
        {
            UpdateScoreDisplay(GameManager.Instance.GetScore());
            UpdateHighScoreDisplay(GameManager.Instance.GetHighScore());
        }
    }

    private IEnumerator HideScoreAdvanceTable()
    {
        float elapsed = 0f;
        while (elapsed < scoreAdvanceDisplayTime)
        {
            // Input System: check keyboard any key or mouse left button
            bool anyKey = (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
                       || (Mouse.current    != null && Mouse.current.leftButton.wasPressedThisFrame);
            if (anyKey) break;
            elapsed += Time.deltaTime;
            yield return null;
        }
        scoreAdvancePanel.SetActive(false);
    }

    // D4 = leading zeros, e.g. 0040
    private string FormatScore(int score) => score.ToString("D4");

    private void UpdateScoreDisplay(int score)
    {
        Debug.Log($"[UIManager] UpdateScoreDisplay({score}), scoreText={(scoreText != null ? scoreText.name : "NULL")}");
        if (scoreText) scoreText.text = FormatScore(score);
    }

    private void UpdateHighScoreDisplay(int score)
    {
        if (highScoreText) highScoreText.text = FormatScore(score);
    }

    private void ShowGameOver()
    {
        if (!gameOverPanel) return;
        gameOverPanel.SetActive(true);
        if (gameOverScoreText && GameManager.Instance)
            gameOverScoreText.text = "SCORE: " + FormatScore(GameManager.Instance.GetScore());
        if (gameOverHighScoreText && GameManager.Instance)
            gameOverHighScoreText.text = "HI-SCORE: " + FormatScore(GameManager.Instance.GetHighScore());
    }

    private void ShowWin()
    {
        if (winPanel) winPanel.SetActive(true);
    }

    public void OnRestartButton()
    {
        GameManager.Instance?.RestartGame();
    }
}