using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to a GameObject in the Credits scene.
/// Displays credits for 5 seconds then returns to Main Menu.
/// Player can press any key to skip.
/// </summary>
public class CreditsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float displayTime = 5f;

    private float elapsed = 0f;
    private bool  done    = false;

    private void Start()
    {
        StartCoroutine(CreditsCountdown());
    }

    private IEnumerator CreditsCountdown()
    {
        while (elapsed < displayTime)
        {
            elapsed += Time.deltaTime;

            // Update countdown text if assigned
            if (countdownText != null)
            {
                float remaining = Mathf.CeilToInt(displayTime - elapsed);
                countdownText.text = $"Returning to menu in {remaining}...";
            }

            // Any key skips credits
            var kb = Keyboard.current;
            var gp = Gamepad.current;
            bool skip = (kb != null && kb.anyKey.wasPressedThisFrame)
                     || (gp != null && gp.buttonSouth.wasPressedThisFrame);
            if (skip) break;

            yield return null;
        }

        if (!done)
        {
            done = true;
            ReturnToMenu();
        }
    }

    private void ReturnToMenu()
    {
        if (SceneController.Instance != null)
            SceneController.Instance.GoToMainMenu();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    // Wire to a "Skip" button's OnClick() if desired
    public void OnSkipButton() => ReturnToMenu();
}