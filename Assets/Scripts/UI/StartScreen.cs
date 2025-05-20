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

    private void Start()
    {
        // Ensure the start screen is visible
        ShowStartScreen();

        // Set up button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);

        // Update high score display
        UpdateHighScoreDisplay();

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
            Debug.Log("[StartScreen] Start screen panel shown");
        }
        else
        {
            Debug.LogError("[StartScreen] Start screen panel reference is null!");
        }
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("[StartScreen] Attempting to hide start screen panel");
        if (startScreenPanel != null)
        {
            startScreenPanel.SetActive(false);
            Debug.Log("[StartScreen] Start screen panel hidden");
        }
        else
        {
            Debug.LogError("[StartScreen] Start screen panel reference is null!");
        }

        // Let GameManager handle cursor state through SetGameState
        GameManager.Instance.StartGame();
    }

    private void OnQuitButtonClicked()
    {
        GameManager.Instance.ExitGame();
    }

    private void UpdateHighScoreDisplay()
    {
        if (highScoreText != null)
            highScoreText.text = $"High Score: {GameManager.Instance.highScore}";
    }
}