using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ComboDisplay : MonoBehaviour
{
  [Header("UI References")]
  public TextMeshProUGUI multiplierText; // Shows "x3" or "5x COMBO!"
  public TextMeshProUGUI pointsEarnedText; // Shows "+600pts" with juice effects
  public Image comboBackground; // Background for visual effects
  public GameObject comboContainer; // Container to hide/show entire display

  [Header("Visual Effects")]
  public Color[] comboColors; // Different colors for combo levels
  public Color[] pointColors; // Different colors for point levels (for pointsEarnedText only)
  public float pulseDuration = 0.3f;
  public float pulseScale = 1.2f;
  public bool showBackground = true;

  [Header("Points Animation")]
  public float pointsAnimationDuration = 1.0f;
  public float pointsScaleEffect = 1.5f;
  public float pointsFadeDelay = 0.3f; // How long to wait before fading out
  public float pointsFadeDuration = 0.3f; // How long the fade takes
  public AnimationCurve pointsAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

  [Header("Combo Thresholds")]
  public int[] comboThresholds = { 5, 10, 15, 25, 50 }; // When to change colors/effects

  [Header("Point Thresholds")]
  public int[] pointThresholds = { 1500, 5000 }; // When to change pointsEarnedText colors

  private int currentCombo = 0;
  private int currentPointsEarned = 0;
  private Vector3 originalScale;
  private Vector3 pointsOriginalScale;
  private Color pointsOriginalColor;
  private Coroutine pulseCoroutine;
  private Coroutine pointsAnimationCoroutine;

  void Start()
  {
    originalScale = transform.localScale;

    if (pointsEarnedText != null)
    {
      pointsOriginalScale = pointsEarnedText.transform.localScale;
      pointsOriginalColor = pointsEarnedText.color;
    }

    // Initialize default colors if not set
    if (comboColors == null || comboColors.Length == 0)
    {
      comboColors = new Color[]
      {
                Color.white,                           // 0-4 combo
                Color.yellow,                          // 5-9 combo
                new Color(1f, 0.5f, 0f, 1f),         // 10-14 combo (orange)
                Color.red,                             // 15-24 combo
                Color.magenta,                         // 25-49 combo
                Color.cyan                             // 50+ combo
      };
      Debug.Log("[ComboDisplay] Using default combo colors");
    }
    else
    {
      Debug.Log($"[ComboDisplay] Using Inspector combo colors, count: {comboColors.Length}");
    }

    // Initialize default point colors if not set
    if (pointColors == null || pointColors.Length == 0)
    {
      pointColors = new Color[]
      {
                Color.yellow,                           // 0-1499 points
                new Color(1f, 0.5f, 0f, 1f),          // 1500-4999 points (orange)
                Color.red,                             // 5000+ points
      };
      Debug.Log("[ComboDisplay] Using default point colors");
    }
    else
    {
      Debug.Log($"[ComboDisplay] Using Inspector point colors, count: {pointColors.Length}");
    }

    UpdateDisplay();
  }

  void Update()
  {
    // Get current combo and points from GameManager
    if (GameManager.Instance != null)
    {
      int newCombo = GameManager.Instance.combo;
      int newPointsEarned = GameManager.Instance.lastPointsEarned;

      // Only update if combo or points changed
      if (newCombo != currentCombo || newPointsEarned != currentPointsEarned)
      {
        int previousCombo = currentCombo;
        int previousPointsEarned = currentPointsEarned;
        currentCombo = newCombo;
        currentPointsEarned = newPointsEarned;
        UpdateDisplay();

        // Trigger pulse effect when combo increases
        if (newCombo > previousCombo && newCombo > 0)
        {
          TriggerPulseEffect();
        }

        // Trigger points animation when points change (including negative points)
        if (newPointsEarned != previousPointsEarned && newPointsEarned != 0)
        {
          TriggerPointsAnimation();
        }
      }
    }
  }

  private void UpdateDisplay()
  {
    // Show/hide combo display - show if combo > 0 OR if showing negative points
    bool shouldShow = currentCombo > 0 || currentPointsEarned < 0;
    if (comboContainer != null)
    {
      comboContainer.SetActive(shouldShow);
    }

    if (!shouldShow) return;

    // Update multiplier text (stays white, shows combo or miss penalty)
    if (multiplierText != null)
    {
      if (currentPointsEarned < 0)
      {
        multiplierText.text = "MISS!";
        multiplierText.color = Color.red;
      }
      else if (currentCombo >= 10)
      {
        multiplierText.text = $"{currentCombo}x COMBO!";
        multiplierText.color = Color.white;
      }
      else
      {
        multiplierText.text = $"x{currentCombo}";
        multiplierText.color = Color.white;
      }
    }

    // Update colors based on combo level (for background)
    Color comboColor = GetComboColor(currentCombo);

    // Update colors based on point level (for pointsEarnedText)
    Color pointColor = GetPointColor(currentPointsEarned);

    // Update points text (colored based on point value)
    if (pointsEarnedText != null)
    {
      // Handle negative points (miss penalty)
      if (currentPointsEarned < 0)
      {
        pointsEarnedText.text = $"{currentPointsEarned}pts"; // Already has negative sign
      }
      else
      {
        pointsEarnedText.text = $"+{currentPointsEarned}pts";
      }

      // Only update color if not currently animating (to avoid interfering with fade)
      if (pointsAnimationCoroutine == null)
      {
        // Use red color for negative points, otherwise use point-based color
        Color displayColor = currentPointsEarned < 0 ? Color.red : pointColor;
        pointsOriginalColor = displayColor;
        pointsEarnedText.color = displayColor;
      }
    }

    if (comboBackground != null && showBackground)
    {
      Color bgColor = comboColor;
      bgColor.a = 0.3f; // Semi-transparent background
      comboBackground.color = bgColor;
    }
  }

  private Color GetComboColor(int combo)
  {
    // Determine color based on combo thresholds
    for (int i = comboThresholds.Length - 1; i >= 0; i--)
    {
      if (combo >= comboThresholds[i])
      {
        return comboColors[Mathf.Min(i + 1, comboColors.Length - 1)];
      }
    }

    // Default color for low combos
    return comboColors[0];
  }

  private Color GetPointColor(int points)
  {
    // Determine color based on point thresholds
    for (int i = pointThresholds.Length - 1; i >= 0; i--)
    {
      if (points >= pointThresholds[i])
      {
        return pointColors[Mathf.Min(i + 1, pointColors.Length - 1)];
      }
    }

    // Default color for low points
    return pointColors[0];
  }

  private void TriggerPulseEffect()
  {
    // Stop any existing pulse
    if (pulseCoroutine != null)
    {
      StopCoroutine(pulseCoroutine);
    }

    pulseCoroutine = StartCoroutine(PulseAnimation());
  }

  private void TriggerPointsAnimation()
  {
    // Stop any existing points animation
    if (pointsAnimationCoroutine != null)
    {
      StopCoroutine(pointsAnimationCoroutine);
    }

    pointsAnimationCoroutine = StartCoroutine(PointsAnimation());
  }

  private IEnumerator PulseAnimation()
  {
    Vector3 targetScale = originalScale * pulseScale;
    float elapsed = 0f;
    float halfDuration = pulseDuration * 0.5f;

    // Scale up
    while (elapsed < halfDuration)
    {
      elapsed += Time.deltaTime;
      float progress = elapsed / halfDuration;
      transform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
      yield return null;
    }

    elapsed = 0f;

    // Scale back down
    while (elapsed < halfDuration)
    {
      elapsed += Time.deltaTime;
      float progress = elapsed / halfDuration;
      transform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
      yield return null;
    }

    transform.localScale = originalScale;
    pulseCoroutine = null;
  }

  private IEnumerator PointsAnimation()
  {
    if (pointsEarnedText == null) yield break;

    // Make sure points text is visible and at starting position
    Color currentColor = pointsOriginalColor;
    pointsEarnedText.color = currentColor;

    Vector3 targetScale = pointsOriginalScale * pointsScaleEffect;
    float elapsed = 0f;

    // Phase 1: Scale animation
    while (elapsed < pointsAnimationDuration)
    {
      elapsed += Time.deltaTime;
      float progress = elapsed / pointsAnimationDuration;

      // Scale animation
      float scaleValue = pointsAnimationCurve.Evaluate(progress);
      float scaleMultiplier = 1f + (pointsScaleEffect - 1f) * (1f - Mathf.Abs(scaleValue * 2f - 1f));
      pointsEarnedText.transform.localScale = pointsOriginalScale * scaleMultiplier;

      yield return null;
    }

    // Ensure final scale is reset
    pointsEarnedText.transform.localScale = pointsOriginalScale;

    // Phase 2: Wait before fading (while staying at original position)
    yield return new WaitForSeconds(pointsFadeDelay);

    // Phase 3: Fade out
    elapsed = 0f;
    while (elapsed < pointsFadeDuration)
    {
      elapsed += Time.deltaTime;
      float progress = elapsed / pointsFadeDuration;

      // Fade alpha from 1 to 0
      Color fadeColor = pointsOriginalColor;
      fadeColor.a = Mathf.Lerp(1f, 0f, progress);
      pointsEarnedText.color = fadeColor;

      yield return null;
    }

    // Reset to original state (invisible)
    Color finalColor = pointsOriginalColor;
    finalColor.a = 0f;
    pointsEarnedText.color = finalColor;
    pointsEarnedText.transform.localScale = pointsOriginalScale;

    // Clear negative points from GameManager after animation
    if (currentPointsEarned < 0 && GameManager.Instance != null)
    {
      GameManager.Instance.lastPointsEarned = 0;
    }

    pointsAnimationCoroutine = null;
  }

  // Call this when combo resets for special effects
  public void OnComboReset()
  {
    // Stop all animations when combo resets
    if (pulseCoroutine != null)
    {
      StopCoroutine(pulseCoroutine);
      pulseCoroutine = null;
    }

    if (pointsAnimationCoroutine != null)
    {
      StopCoroutine(pointsAnimationCoroutine);
      pointsAnimationCoroutine = null;
    }
  }
}