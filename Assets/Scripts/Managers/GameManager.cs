using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager Instance;

    // Score-related
    public int score;
    public int highScore;
    public int combo;



// Game timer
    public float runDuration = 60f;  // total time of a run in seconds
    private float runTimer;
    public float RunProgress => Mathf.Clamp01(runTimer / runDuration);

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

    private void Update()

{
    if (isGameActive)
    {
        runTimer += Time.deltaTime;

        if (runTimer >= runDuration)
        {
            EndGame(success: true); // or false depending on your condition
        }
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
            highScore = score;

        Debug.Log($"[GameManager] Score: {score}, High Score: {highScore}");
    }

    // Game state methods
    public void SetGameState(GameState state)
    {
        previousState = currentState;
        currentState = state;
    }

    public void StartGame()
    {
        score = 0;
        combo = 0;
        runTimer = 0f;
        SetGameState(GameState.Play);
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