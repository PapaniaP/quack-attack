using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;

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
    public TextMeshProUGUI startButtonText;  // Reference to the start button's text component

    [Header("DPI Settings")]
    public Slider dpiSlider;
    public TextMeshProUGUI dpiValueText;
    private float minDPI = 0.5f;
    private float maxDPI = 2.0f;

    private bool isPauseMode = false;

    private void Start()
    {
        // Ensure the start screen is visible
        ShowStartScreen();

        // Set up button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);

        // Set up DPI slider
        if (dpiSlider != null)
        {
            dpiSlider.minValue = minDPI;
            dpiSlider.maxValue = maxDPI;
            dpiSlider.value = QualitySettings.resolutionScalingFixedDPIFactor;
            dpiSlider.onValueChanged.AddListener(OnDPISliderChanged);
            UpdateDPIText();
        }

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
                statusMessageText.text = "Quack them all!";
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
        if (isPauseMode)
        {
            GameManager.Instance.ResumeGame();
        }
        else
        {
            GameManager.Instance.StartGame();
        }
    }

    private void OnQuitButtonClicked()
    {
        GameManager.Instance.ExitGame();
    }

    private void OnDPISliderChanged(float value)
    {
        QualitySettings.resolutionScalingFixedDPIFactor = value;
        UpdateDPIText();
    }

    private void UpdateDPIText()
    {
        if (dpiValueText != null)
        {
            dpiValueText.text = $"DPI Scale: {QualitySettings.resolutionScalingFixedDPIFactor:F2}x";
        }
    }

    public void SetPauseMode(bool pause)
    {
        isPauseMode = pause;
        UpdateUIForPauseState();
    }

    private void UpdateUIForPauseState()
    {
        if (titleText != null)
        {
            titleText.text = isPauseMode ? "Game Paused" : "Quack Attack";
        }

        if (startButtonText != null)
        {
            startButtonText.text = isPauseMode ? "Resume Game" : "Start Game";
        }
    }
}