using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton GameManager — no physics code, works identically in 2D and 3D.
/// Handles score, high score persistence (PlayerPrefs), and C# events.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private int currentScore = 0;
    private int highScore    = 0;
    private const string HIGH_SCORE_KEY = "HighScore";

    // C# Events used across gameplay systems
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnHighScoreChanged;
    public static event Action      OnPlayerDied;
    public static event Action      OnGameOver;
    public static event Action      OnAllEnemiesDefeated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Clear static events — in the Unity editor, static events persist
        // between Play sessions and accumulate duplicate listeners
        OnScoreChanged       = null;
        OnHighScoreChanged   = null;
        OnPlayerDied         = null;
        OnGameOver           = null;
        OnAllEnemiesDefeated = null;

        LoadHighScore();
    }

    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        OnHighScoreChanged?.Invoke(highScore);
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        Debug.Log($"[GameManager] AddScore({amount}) → currentScore={currentScore}, listeners={OnScoreChanged?.GetInvocationList().Length ?? 0}");
        OnScoreChanged?.Invoke(currentScore);

        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
            PlayerPrefs.Save();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    public int GetScore()     => currentScore;
    public int GetHighScore() => highScore;

    public void NotifyPlayerDied()
    {
        OnPlayerDied?.Invoke();
        OnGameOver?.Invoke();
    }

    public void NotifyAllEnemiesDefeated() => OnAllEnemiesDefeated?.Invoke();

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void RestartGame()
    {
        ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}