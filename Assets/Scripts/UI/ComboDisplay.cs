using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ComboDisplay : MonoBehaviour
{
  [Header("UI References")]
  public TextMeshProUGUI comboText; // Main combo text
  public TextMeshProUGUI multiplierText; // "x3" or "3x COMBO!"
  public Image comboBackground; // Background for visual effects
  public GameObject comboContainer; // Container to hide/show entire display

  [Header("Visual Effects")]
  public Color[] comboColors; // Different colors for combo levels
  public float pulseDuration = 0.3f;
  public float pulseScale = 1.2f;
  public bool showBackground = true;

  [Header("Combo Thresholds")]
  public int[] comboThresholds = { 5, 10, 15, 25, 50 }; // When to change colors/effects

  private int currentCombo = 0;
  private Vector3 originalScale;
  private Coroutine pulseCoroutine;

  void Start()
  {
    originalScale = transform.localScale;

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
    }

    UpdateDisplay();
  }

  void Update()
  {
    // Get current combo from GameManager
    if (GameManager.Instance != null)
    {
      int newCombo = GameManager.Instance.combo;

      // Only update if combo changed
      if (newCombo != currentCombo)
      {
        int previousCombo = currentCombo;
        currentCombo = newCombo;
        UpdateDisplay();

        // Trigger pulse effect when combo increases
        if (newCombo > previousCombo && newCombo > 0)
        {
          TriggerPulseEffect();
        }
      }
    }
  }

  private void UpdateDisplay()
  {
    // Show/hide combo display
    bool shouldShow = currentCombo > 0;
    if (comboContainer != null)
    {
      comboContainer.SetActive(shouldShow);
    }

    if (!shouldShow) return;

    // Update combo text
    if (comboText != null)
    {
      comboText.text = currentCombo.ToString();
    }

    // Update multiplier text
    if (multiplierText != null)
    {
      if (currentCombo >= 10)
      {
        multiplierText.text = $"{currentCombo}x COMBO!";
      }
      else
      {
        multiplierText.text = $"x{currentCombo}";
      }
    }

    // Update colors based on combo level
    Color comboColor = GetComboColor(currentCombo);

    if (comboText != null)
    {
      comboText.color = comboColor;
    }

    if (multiplierText != null)
    {
      multiplierText.color = comboColor;
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

  private void TriggerPulseEffect()
  {
    // Stop any existing pulse
    if (pulseCoroutine != null)
    {
      StopCoroutine(pulseCoroutine);
    }

    pulseCoroutine = StartCoroutine(PulseAnimation());
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

  // Call this when combo resets for special effects
  public void OnComboReset()
  {
    // Could add special "combo lost" effects here
    if (pulseCoroutine != null)
    {
      StopCoroutine(pulseCoroutine);
      pulseCoroutine = null;
    }

    transform.localScale = originalScale;
  }
}