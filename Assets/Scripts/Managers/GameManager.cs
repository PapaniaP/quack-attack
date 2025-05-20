using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance;

    // Score-related
    public int score;
    public int highScore;
    public int combo;

    [Header("UI References")]
    public TMPro.TextMeshProUGUI scoreText;
<<<<<<< Updated upstream
=======
    public TMPro.TextMeshProUGUI highScoreText;
    public TMPro.TextMeshProUGUI timerText;
    public GameObject hudCanvas;  // Reference to the HUD Canvas

    // Lives system
    public int maxLives = 5;
    private int currentLives;
    public int CurrentLives => currentLives;

    [Header("Lives UI")]
    public GameObject heartPrefab;  // Prefab for a single heart image
    public Transform heartsContainer;  // Parent transform to hold all hearts
    private List<GameObject> heartImages = new List<GameObject>();  // List to track heart GameObjects

    // Game timer
    public float runDuration = 60f;  // total time of a run in seconds
    private float runTimer;
    public float RunProgress => Mathf.Clamp01(runTimer / runDuration);
>>>>>>> Stashed changes

    // // Combo system
    // public float comboResetTime = 3f;
    // private float comboTimer;

    // Game state
    public enum GameState { Play, Pause, End }
    public GameState currentState = GameState.Play;
    private GameState previousState;
    public bool isPaused => currentState == GameState.Pause;
    public bool isGameActive => currentState == GameState.Play;
    public bool isGameOver => currentState == GameState.End;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // private void Update()
    // {
    //     HandleComboTimer();
    // }

    // private void HandleComboTimer()
    // {
    //     if (combo > 0)
    //     {
    //         comboTimer -= Time.deltaTime;
    //         if (comboTimer <= 0f)
    //         {
    //             ResetCombo();
    //         }
    //     }
    // }

    // Combo methods
    public void AddCombo()
    {
        combo++;
        Debug.Log($"[GameManager] Combo is now {combo}");
    }

    public void ResetCombo()
    {
        combo = 0;
        // comboTimer = 0f;
        Debug.Log("[GameManager] Combo reset");
    }

    // Score methods
    public void AddPoints(int value)
    {
        score += value;
        if (score > highScore)
            highScore = score;
<<<<<<< Updated upstream
=======
            if (highScoreText != null)
                highScoreText.text = "High Score: " + highScore;

            // Update high score on start screen if it exists
            if (startScreen != null)
            {
                startScreen.UpdateHighScoreDisplay();
            }
        }
>>>>>>> Stashed changes

        if (scoreText != null)
            scoreText.text = "Score: " + score;

        Debug.Log($"[GameManager] Score: {score}, High Score: {highScore}");
    }

    // Game state methods
    public void SetGameState(GameState state)
    {
        previousState = currentState;
        currentState = state;
<<<<<<< Updated upstream
=======

        // Handle cursor and UI based on game state
        switch (state)
        {
            case GameState.StartScreen:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (hudCanvas != null) hudCanvas.SetActive(false);
                if (startScreen != null) startScreen.UpdateStatusMessage(state);
                break;
            case GameState.Play:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                if (hudCanvas != null) hudCanvas.SetActive(true);
                break;
            case GameState.Pause:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (hudCanvas != null) hudCanvas.SetActive(true);
                if (startScreen != null) startScreen.UpdateStatusMessage(state);
                break;
            case GameState.End:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (hudCanvas != null) hudCanvas.SetActive(false);
                if (startScreen != null) startScreen.UpdateStatusMessage(state, runTimer >= runDuration);
                break;
        }
>>>>>>> Stashed changes
    }

    public void StartGame()
    {
        score = 0;
        combo = 0;
        SetGameState(GameState.Play);

        if (scoreText != null)
            scoreText.text = "Score: 0";
    }

    public void PauseGame()
    {
        SetGameState(GameState.Pause);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        SetGameState(GameState.Play);
        Time.timeScale = 1f;
    }

    public void EndGame(bool success)
    {
        SetGameState(GameState.End);
        Debug.Log(success ? "Game Over – Success!" : "Game Over – Fail!");
<<<<<<< Updated upstream
=======

        // Show the start screen using the StartScreen component
        if (startScreen != null)
        {
            startScreen.ShowStartScreen();
            startScreen.UpdateScoreDisplays();  // Update both scores
            startScreen.UpdateStatusMessage(GameState.End, success);
        }
        else
        {
            Debug.LogWarning("[GameManager] StartScreen component not found, cannot show start screen");
        }
>>>>>>> Stashed changes
    }

    public void RestartGame()
    {
        StartGame();
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}