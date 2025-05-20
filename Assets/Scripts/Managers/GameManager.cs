using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance;

    // Score-related
    public int score;
    public int highScore;
    public int combo;

    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI highScoreText;
    public TMPro.TextMeshProUGUI timerText;

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

    // // Combo system
    // public float comboResetTime = 3f;
    // private float comboTimer;

    // Game state
    public enum GameState { StartScreen, Play, Pause, End }
    public GameState currentState = GameState.StartScreen;
    private GameState previousState;
    public bool isPaused => currentState == GameState.Pause;
    public bool isGameActive => currentState == GameState.Play;
    public bool isGameOver => currentState == GameState.End;
    public bool isStartScreen => currentState == GameState.StartScreen;

    // Add a reference to the StartScreen
    private StartScreen startScreen;
    private SpawnManager spawnManager;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize lives
        currentLives = maxLives;
        InitializeHearts();

        // Find the StartScreen component
        startScreen = FindAnyObjectByType<StartScreen>();
        if (startScreen == null)
        {
            Debug.LogWarning("[GameManager] Could not find StartScreen component in scene");
        }

        // Find the SpawnManager
        spawnManager = FindAnyObjectByType<SpawnManager>();
        if (spawnManager == null)
        {
            Debug.LogWarning("[GameManager] Could not find SpawnManager in scene");
        }

        // Start in the start screen state
        SetGameState(GameState.StartScreen);
        Time.timeScale = 0f; // Pause the game until start

        // Show cursor during start screen
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void InitializeHearts()
    {
        // Clear any existing hearts
        foreach (var heart in heartImages)
        {
            if (heart != null)
                Destroy(heart);
        }
        heartImages.Clear();

        // Create new hearts
        for (int i = 0; i < maxLives; i++)
        {
            if (heartPrefab != null && heartsContainer != null)
            {
                GameObject heart = Instantiate(heartPrefab, heartsContainer);
                heartImages.Add(heart);
            }
        }
    }

    private void Update()
    {
        if (isGameActive)
        {
            runTimer += Time.deltaTime;

            // Update timer display
            if (timerText != null)
            {
                float remainingTime = Mathf.Max(0, runDuration - runTimer);
                timerText.text = $"Time: {Mathf.CeilToInt(remainingTime)}s";
            }

            if (runTimer >= runDuration)
            {
                EndGame(success: true); // or false depending on your condition
            }
        }

        // Check for R key press to restart game
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGameKeepHighScore();
        }

        // HandleComboTimer(); // (commented out, which is fine for now)
    }
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
        {
            highScore = score;
            if (highScoreText != null)
                highScoreText.text = "High Score: " + highScore;
        }

        if (scoreText != null)
            scoreText.text = "Score: " + score;

        Debug.Log($"[GameManager] Score: {score}, High Score: {highScore}");
    }

    // Game state methods
    public void SetGameState(GameState state)
    {
        previousState = currentState;
        currentState = state;

        // Handle cursor based on game state
        switch (state)
        {
            case GameState.StartScreen:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case GameState.Play:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            case GameState.Pause:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case GameState.End:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
        }
    }
    public void StartGame()
    {
        score = 0;
        combo = 0;
        currentLives = maxLives;
        runTimer = 0f;
        SetGameState(GameState.Play);
        Time.timeScale = 1f; // Resume game time

        // Reset and spawn new targets
        if (spawnManager != null)
        {
            spawnManager.ResetAndSpawn();
        }

        if (scoreText != null)
            scoreText.text = "Score: 0";
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(runDuration)}s";

        InitializeHearts();  // Reset hearts display
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

        // Show the start screen using the StartScreen component
        if (startScreen != null)
        {
            startScreen.ShowStartScreen();
        }
        else
        {
            Debug.LogWarning("[GameManager] StartScreen component not found, cannot show start screen");
        }
    }

    public void RestartGame()
    {
        // Reset everything including high score
        score = 0;
        highScore = 0;
        combo = 0;
        currentLives = maxLives;
        runTimer = 0f;
        SetGameState(GameState.Play);

        if (scoreText != null)
            scoreText.text = "Score: 0";
        if (highScoreText != null)
            highScoreText.text = "High Score: 0";
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(runDuration)}s";

        InitializeHearts();  // Reset hearts display
    }

    public void RestartGameKeepHighScore()
    {
        // Save the current high score
        int savedHighScore = highScore;

        // Reset game state
        score = 0;
        combo = 0;
        currentLives = maxLives;
        runTimer = 0f;
        SetGameState(GameState.Play);

        // Restore high score
        highScore = savedHighScore;

        // Update UI
        if (scoreText != null)
            scoreText.text = "Score: 0";
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
        if (timerText != null)
            timerText.text = $"Time: {Mathf.CeilToInt(runDuration)}s";

        InitializeHearts();  // Reset hearts display
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    // Lives methods
    public void RemoveLife()
    {
        if (!isGameActive) return;

        currentLives--;
        UpdateLivesDisplay();

        Debug.Log($"[GameManager] Lives remaining: {currentLives}");

        if (currentLives <= 0)
        {
            EndGame(success: false);
        }
    }

    private void UpdateLivesDisplay()
    {
        // Update heart images visibility based on current lives
        for (int i = 0; i < heartImages.Count; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].SetActive(i < currentLives);
            }
        }
    }
}