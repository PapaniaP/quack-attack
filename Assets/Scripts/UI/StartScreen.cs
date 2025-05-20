using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StartScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject startScreenPanel;
    public Button startButton;
    public Button quitButton;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI currentScoreText;  // For showing current score
    public TextMeshProUGUI statusMessageText;  // For showing game state messages

    private void Start()
    {
        // Ensure the start screen is visible
        ShowStartScreen();

        // Set up button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);

        // Update score displays
        UpdateScoreDisplays();

        // Set initial status message
        UpdateStatusMessage("Let's get it started!");

        // Ensure cursor is visible during start screen
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        // Optional: Also allow starting with spacebar
        if (Input.GetKeyDown(KeyCode.Space) && GameManager.Instance.isStartScreen)
        {
            OnStartButtonClicked();
        }
    }

    public void ShowStartScreen()
    {
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(true);
            if (statusMessageText != null)
            {
                statusMessageText.gameObject.SetActive(true);
            }
            UpdateScoreDisplays();  // Update both scores when showing start screen
            Debug.Log("[StartScreen] Start screen panel shown");
        }
        else
        {
            Debug.LogError("[StartScreen] Start screen panel reference is null!");
        }
    }

    public void HideStartScreen()
    {
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(false);
            Debug.Log("[StartScreen] Start screen panel hidden");
        }
        else
        {
            Debug.LogError("[StartScreen] Start screen panel reference is null!");
        }
    }

    public void UpdateScoreDisplays()
    {
        UpdateHighScoreDisplay();
        UpdateCurrentScoreDisplay();
    }

    private void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {GameManager.Instance.highScore}";
        }
    }

    public void UpdateCurrentScoreDisplay()
    {
        if (currentScoreText != null)
        {
            currentScoreText.text = $"Score: {GameManager.Instance.score}";
        }
    }

    public void UpdateStatusMessage(string message)
    {
        if (statusMessageText != null)
        {
            statusMessageText.text = message;
        }
    }

    public void UpdateStatusMessage(GameManager.GameState state, bool success = false)
    {
        if (statusMessageText == null) return;

        switch (state)
        {
            case GameManager.GameState.StartScreen:
                statusMessageText.text = "Let's get it started!";
                break;
            case GameManager.GameState.Pause:
                statusMessageText.text = "Your game is paused";
                break;
            case GameManager.GameState.End:
                if (success)
                {
                    statusMessageText.text = "Congratulations!";
                }
                else
                {
                    statusMessageText.text = "Game Over!";
                }
                break;
        }
    }

    private void OnStartButtonClicked()
    {
        // Let GameManager handle the visibility through SetGameState
        GameManager.Instance.StartGame();
    }

    private void OnQuitButtonClicked()
    {
        GameManager.Instance.ExitGame();
    }
}