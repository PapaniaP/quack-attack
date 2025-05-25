using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBar : MonoBehaviour
{
  [Header("UI References")]
  public TextMeshProUGUI levelText; // "Level 3"
  public TextMeshProUGUI progressText; // "1250 / 2000"
  public Image fillImage; // The bar fill (for color changes)

  private Slider xpSlider; // This component itself

  [Header("Visual Settings")]
  public Color normalBarColor = Color.green;
  public Color nearLevelUpColor = Color.yellow;
  public float nearLevelUpThreshold = 0.8f; // 80% = near level up

  [Header("Animation")]
  public float barAnimationSpeed = 2f;
  public bool smoothAnimation = true;

  private float targetProgress = 0f;
  private float currentProgress = 0f;

  void Start()
  {
    // Get the slider component (this script is on the slider)
    xpSlider = GetComponent<Slider>();

    // Setup slider
    if (xpSlider != null)
    {
      xpSlider.minValue = 0f;
      xpSlider.maxValue = 1f;
      xpSlider.value = 0f;
      xpSlider.interactable = false; // Prevent player interaction
    }

    // Initial update
    UpdateDisplay();
  }

  void Update()
  {
    // Smooth animation for XP bar
    if (smoothAnimation && Mathf.Abs(currentProgress - targetProgress) > 0.01f)
    {
      currentProgress = Mathf.Lerp(currentProgress, targetProgress, Time.deltaTime * barAnimationSpeed);

      if (xpSlider != null)
      {
        xpSlider.value = currentProgress;
      }

      UpdateBarColor();
    }

    // Update every frame (could be optimized to only update when score changes)
    UpdateDisplay();
  }

  public void UpdateDisplay()
  {
    if (GameManager.Instance == null) return;

    // Get current progress
    float progress = GameManager.Instance.LevelProgress;
    int currentLevel = GameManager.Instance.CurrentLevel;
    int currentScore = GameManager.Instance.score;
    int nextThreshold = GameManager.Instance.NextLevelThreshold;

    // Update target progress for smooth animation
    targetProgress = Mathf.Clamp01(progress);

    // If not using smooth animation, update immediately
    if (!smoothAnimation)
    {
      currentProgress = targetProgress;
      if (xpSlider != null)
      {
        xpSlider.value = currentProgress;
      }
    }

    // Update level text
    if (levelText != null)
    {
      levelText.text = $"Level {currentLevel}";
    }

    // Update progress text
    if (progressText != null)
    {
      progressText.text = $"{currentScore} / {nextThreshold}";
    }

    UpdateBarColor();
  }

  private void UpdateBarColor()
  {
    if (fillImage == null) return;

    // Change color based on progress
    if (currentProgress >= nearLevelUpThreshold)
    {
      fillImage.color = nearLevelUpColor;
    }
    else
    {
      fillImage.color = normalBarColor;
    }
  }

  // Call this when player levels up for visual feedback
  public void OnLevelUp()
  {
    // Reset progress and add some visual flair
    currentProgress = 0f;
    targetProgress = 0f;

    if (xpSlider != null)
    {
      xpSlider.value = 0f;
    }

    // Could add particle effects, screen flash, etc. here
    Debug.Log("[XPBar] Level up! Resetting progress bar.");
  }
}