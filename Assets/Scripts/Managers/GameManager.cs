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

    // Leveling system
    private int level = 1;
    private int nextLevelThreshold; // Will be initialized from baseLevelThreshold

    [Header("Leveling System")]
    [SerializeField] private int baseLevelThreshold = 1000; // Starting threshold
    [SerializeField] private float levelScalingFactor = 2.2f; // How much harder each level gets

    [Header("UI References")]
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI highScoreText;
    public TMPro.TextMeshProUGUI timerText;
    public GameObject hudCanvas;  // Reference to the HUD Canvas

    // Lives system
    public int maxLives = 5;
    private int currentLives;
    public int CurrentLives => currentLives;

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip shootSFX;


    [Header("Lives UI")]
    public GameObject heartPrefab;  // Prefab for a single heart image
    public Transform heartsContainer;  // Parent transform to hold all hearts
    private List<GameObject> heartImages = new List<GameObject>();  // List to track heart GameObjects

    // Game timer
    public float runDuration = 60f;  // total time of a run in seconds
    private float runTimer;
    public float RunProgress => Mathf.Clamp01(runTimer / runDuration);

    // Game state
    public enum GameState { StartScreen, Play, Pause, PowerUpSelection, End }
    public GameState currentState = GameState.StartScreen;
    private GameState previousState;
    public bool isPaused => currentState == GameState.Pause;
    public bool isGameActive => currentState == GameState.Play;
    public bool isGameOver => currentState == GameState.End;
    public bool isStartScreen => currentState == GameState.StartScreen;
    public bool isPowerUpSelection => currentState == GameState.PowerUpSelection;
    public bool IsFrozen { get; set; } = false;

    // Add a reference to the StartScreen
    private StartScreen startScreen;
    private SpawnManager spawnManager;

    [Header("Power-Up System")]
    public PowerUpMenuUI powerUpMenuUI; // Reference to power-up menu

    // Public properties for UI
    public int CurrentLevel => level;
    public int NextLevelThreshold => nextLevelThreshold;
    public float LevelProgress => (float)score / nextLevelThreshold;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Initialize leveling system
        nextLevelThreshold = baseLevelThreshold;

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
        // Check for pause key press
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isGameActive)
            {
                PauseGame();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
        }

        if (isGameActive && !IsFrozen)
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
                EndGame(success: true);
            }
        }

        // Check for R key press to restart game
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGameKeepHighScore();
        }
    }

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

            // Update high score on start screen if it exists
            if (startScreen != null)
            {
                startScreen.UpdateScoreDisplays();
            }
        }

        if (scoreText != null)
            scoreText.text = "Score: " + score;

        Debug.Log($"[GameManager] Score: {score}, High Score: {highScore}");

        CheckLevelUp();  // 🚨 NEW LINE
    }

    // Game state methods
    public void SetGameState(GameState state)
    {
        previousState = currentState;
        currentState = state;

        // Handle cursor and UI based on game state
        switch (state)
        {
            case GameState.StartScreen:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (hudCanvas != null) hudCanvas.SetActive(false);  // Hide HUD
                if (startScreen != null)
                {
                    startScreen.ShowStartScreen();
                    startScreen.UpdateStatusMessage(state);
                }
                break;
            case GameState.Play:
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                if (hudCanvas != null) hudCanvas.SetActive(true);   // Show HUD
                if (startScreen != null) startScreen.HideStartScreen();
                break;
            case GameState.Pause:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (hudCanvas != null) hudCanvas.SetActive(true);   // Keep HUD visible
                if (startScreen != null)
                {
                    startScreen.ShowStartScreen();
                    startScreen.UpdateStatusMessage(state);
                }
                break;
            case GameState.PowerUpSelection:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (hudCanvas != null) hudCanvas.SetActive(true);   // Keep HUD visible
                // Don't show start screen - let PowerUpMenuUI handle its own UI
                if (startScreen != null) startScreen.HideStartScreen();
                break;
            case GameState.End:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                if (hudCanvas != null) hudCanvas.SetActive(false);  // Hide HUD
                if (startScreen != null)
                {
                    startScreen.ShowStartScreen();
                    startScreen.UpdateStatusMessage(state, runTimer >= runDuration);
                }
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
        if (!isGameActive) return;  // Only pause if game is active

        SetGameState(GameState.Pause);
        Time.timeScale = 0f;

        // Update start screen for pause state
        if (startScreen != null)
        {
            startScreen.ShowStartScreen();
            startScreen.UpdateStatusMessage(GameState.Pause);
            startScreen.SetPauseMode(true);
        }
    }

    public void ResumeGame()
    {
        if (!isPaused) return;  // Only resume if game is paused

        SetGameState(GameState.Play);
        Time.timeScale = 1f;

        if (startScreen != null)
        {
            startScreen.HideStartScreen();
            startScreen.SetPauseMode(false);
        }
    }

    public void EndGame(bool success)
    {
        SetGameState(GameState.End);
        Debug.Log(success ? "Game Over – Success!" : "Game Over – Fail!");

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
    }

    public void RestartGame()
    {
        // Reset everything including high score
        score = 0;
        highScore = 0;
        combo = 0;
        level = 1;
        nextLevelThreshold = baseLevelThreshold;
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
        level = 1;
        nextLevelThreshold = baseLevelThreshold;
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

    private void CheckLevelUp()
    {
        if (score >= nextLevelThreshold)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        Debug.Log($"[GameManager] Level up! Now level {level}");

        // Scale up the threshold for next level
        nextLevelThreshold = Mathf.RoundToInt(nextLevelThreshold * levelScalingFactor);

        // Trigger power-up selection
        TriggerPowerUpSelection();
    }

    public void TriggerPowerUpSelection()
    {
        if (powerUpMenuUI != null && PowerUpManager.Instance != null)
        {
            var options = PowerUpManager.Instance.GetRandomPowerUpOptions(2);
            if (options.Count >= 2)
            {
                SetGameState(GameState.PowerUpSelection);
                powerUpMenuUI.Show(options);
                Debug.Log("[GameManager] Power-up selection triggered");
            }
            else
            {
                Debug.LogWarning("[GameManager] Not enough power-ups available for selection");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] PowerUpMenuUI or PowerUpManager not found");
        }
    }

    public void OnPowerUpSelectionComplete()
    {
        // Called by PowerUpMenuUI when selection is complete
        SetGameState(GameState.Play);
        Debug.Log("[GameManager] Power-up selection complete, resuming game");
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

    public void AddLife(int amount)
    {
        if (!isGameActive) return;

        currentLives += amount;

        // Increase maxLives if current lives exceeds it
        if (currentLives > maxLives)
        {
            maxLives = currentLives;
            InitializeHearts(); // Recreate hearts to match new max
        }

        UpdateLivesDisplay();

        Debug.Log($"[GameManager] Gained {amount} life. Total lives: {currentLives}, Max lives: {maxLives}");
    }
    public void PlayShootSFX()
    {
        if (sfxSource != null && shootSFX != null)
            sfxSource.PlayOneShot(shootSFX);
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
