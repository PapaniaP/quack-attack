using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class PowerUpCardUI : MonoBehaviour
{
  [Header("UI References")]
  public TextMeshProUGUI nameText;
  public Image iconImage;
  public TextMeshProUGUI descriptionText;
  public UnityEngine.UI.Outline cardOutline; // Outline component for selection feedback
  public Image cardBackground; // Add background image for color effects

  [Header("Upgrade Display")]
  public GameObject upgradeIndicator; // "UPGRADE" badge/text
  public TextMeshProUGUI levelText; // Shows "Level 2", "Level 3", "MAX"
  public Image[] levelStars; // Optional: star indicators for level (max 3)
  public Color upgradeCardColor = new Color(1f, 0.9f, 0.7f, 1f); // Light golden tint for upgrades
  public Color maxLevelCardColor = new Color(0.9f, 0.7f, 1f, 1f); // Light purple tint for max level
  public Color repeatableCardColor = new Color(0.7f, 1f, 0.9f, 1f); // Light green tint for repeatable power-ups

  [Header("Level Text Colors")]
  public Color level1TextColor = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray for normal
  public Color level2TextColor = new Color(1f, 0.85f, 0.3f, 1f); // Bright gold for level 2
  public Color level3TextColor = new Color(1f, 0.4f, 1f, 1f); // Bright magenta for max level
  public Color repeatableTextColor = new Color(0.3f, 1f, 0.6f, 1f); // Bright green for repeatable

  [Header("Selection Effects")]
  public float selectedScale = 1.1f;
  public float animationDuration = 0.3f;
  public Color selectedColor = new Color(0.7f, 0.9f, 1f, 1f); // More noticeable blue tint for selection
  public Color normalColor = Color.white; // White = no tint, shows image normally

  [HideInInspector] public PowerUpData powerUpData;

  private Action onClickCallback;
  private bool isSelected = false;
  private Vector3 originalScale;
  private Coroutine currentAnimation;
  private bool isUpgrade = false;
  private bool isRepeatable = false;

  private void Awake()
  {
    originalScale = transform.localScale;

    // If no background is assigned, try to find one
    if (cardBackground == null)
      cardBackground = GetComponent<Image>();
  }

  public void Setup(PowerUpData data, Action onClick)
  {
    powerUpData = data;
    onClickCallback = onClick;

    // Check if this is a repeatable power-up
    isRepeatable = IsRepeatablePowerUp(data.effectType);

    // Check if this is an upgrade (only for non-repeatable power-ups)
    int currentLevel = PowerUpManager.Instance.GetPowerUpLevel(data.effectType);
    isUpgrade = !isRepeatable && currentLevel > 0 && data.level > currentLevel;

    // Setup basic info
    nameText.text = data.powerUpName;
    iconImage.sprite = data.icon;
    descriptionText.text = data.description;

    // Setup text colors for better readability
    SetupTextColors();

    // Setup upgrade display
    SetupUpgradeDisplay();

    SetSelected(false); // default: not selected
  }

  private void SetupTextColors()
  {
    // Set main text colors based on card type for better contrast
    Color textColor;

    if (isRepeatable)
    {
      textColor = new Color(0.9f, 1f, 0.95f, 1f); // Light green tint for repeatable
    }
    else if (powerUpData.level == 3)
    {
      textColor = Color.white; // White for max level
    }
    else if (isUpgrade)
    {
      textColor = new Color(1f, 0.95f, 0.8f, 1f); // Light golden for upgrades
    }
    else
    {
      textColor = new Color(0.95f, 0.95f, 0.95f, 1f); // Light gray for normal
    }

    if (nameText != null)
    {
      nameText.color = textColor;
    }

    if (descriptionText != null)
    {
      descriptionText.color = textColor;
    }
  }

  private void SetupUpgradeDisplay()
  {
    // Show/hide upgrade indicator
    if (upgradeIndicator != null)
    {
      upgradeIndicator.SetActive(isUpgrade);
    }

    // Setup level display
    if (levelText != null)
    {
      if (isRepeatable)
      {
        // For repeatable power-ups, show "REPEATABLE" instead of level
        levelText.gameObject.SetActive(true);
        levelText.text = "REPEATABLE";
        levelText.color = repeatableTextColor;
      }
      else if (powerUpData.level > 1)
      {
        levelText.gameObject.SetActive(true);
        levelText.text = powerUpData.level == 3 ? "MAX" : $"Level {powerUpData.level}";

        // Color coding for level text
        if (powerUpData.level == 3)
        {
          levelText.color = level3TextColor;
        }
        else if (powerUpData.level == 2)
        {
          levelText.color = level2TextColor;
        }
      }
      else
      {
        levelText.gameObject.SetActive(false);
      }
    }

    // Setup star indicators
    if (levelStars != null && levelStars.Length >= 3)
    {
      for (int i = 0; i < levelStars.Length; i++)
      {
        if (levelStars[i] != null)
        {
          if (isRepeatable)
          {
            // For repeatable power-ups, show a different pattern (maybe just one star with special color)
            levelStars[i].gameObject.SetActive(i == 0); // Only show first star
            if (i == 0)
            {
              levelStars[i].color = repeatableTextColor; // Green color for repeatable
            }
          }
          else
          {
            levelStars[i].gameObject.SetActive(i < powerUpData.level);

            // Different colors for different levels with better contrast
            if (i < powerUpData.level)
            {
              levelStars[i].color = powerUpData.level == 3 ?
                new Color(1f, 0.2f, 1f, 1f) : // Bright magenta for max level
                new Color(1f, 0.9f, 0.1f, 1f); // Bright yellow for upgrades
            }
          }
        }
      }
    }

    // Set card background color based on upgrade status
    if (cardBackground != null)
    {
      if (isRepeatable)
      {
        cardBackground.color = repeatableCardColor; // Green for repeatable
      }
      else if (powerUpData.level == 3)
      {
        cardBackground.color = maxLevelCardColor; // Purple for max level
      }
      else if (isUpgrade)
      {
        cardBackground.color = upgradeCardColor; // Golden for upgrades
      }
      else
      {
        cardBackground.color = normalColor; // Normal for new power-ups
      }
    }

    // Keep original name - no symbols
    if (nameText != null)
    {
      nameText.text = powerUpData.powerUpName;
    }
  }

  public void OnCardClicked()
  {
    onClickCallback?.Invoke();
  }

  public void SetSelected(bool selected)
  {
    if (isSelected == selected) return;

    isSelected = selected;

    // Stop any current animation
    if (currentAnimation != null)
    {
      StopCoroutine(currentAnimation);
    }

    // Start selection animation
    currentAnimation = StartCoroutine(AnimateSelection(selected));

    // Update highlight border
    SetHighlight(selected);
  }

  public void SetHighlight(bool active)
  {
    if (cardOutline != null)
    {
      cardOutline.enabled = active;

      // Configure outline when active
      if (active)
      {
        cardOutline.effectColor = new Color(1f, 0.537f, 0f, 1f); // Orange outline (FF8900)
        cardOutline.effectDistance = new Vector2(3f, 3f); // Make outline visible
        Debug.Log($"[PowerUpCardUI] Card outline activated for {powerUpData?.powerUpName}");
      }
    }
    else
    {
      Debug.LogWarning("[PowerUpCardUI] Card outline component is not assigned in inspector!");
    }
  }

  private IEnumerator AnimateSelection(bool selected)
  {
    Vector3 targetScale = selected ? originalScale * selectedScale : originalScale;
    Color targetColor = selected ? selectedColor : GetBaseCardColor();

    Vector3 startScale = transform.localScale;
    Color startColor = cardBackground != null ? cardBackground.color : normalColor;

    float elapsed = 0f;

    while (elapsed < animationDuration)
    {
      elapsed += Time.deltaTime;
      float progress = elapsed / animationDuration;

      // Use smooth curve for better animation feel
      float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

      // Animate scale
      transform.localScale = Vector3.Lerp(startScale, targetScale, smoothProgress);

      // Animate color if background exists
      if (cardBackground != null)
      {
        cardBackground.color = Color.Lerp(startColor, targetColor, smoothProgress);
      }

      yield return null;
    }

    // Ensure final values are set
    transform.localScale = targetScale;
    if (cardBackground != null)
    {
      cardBackground.color = targetColor;
    }

    // Add a subtle bounce effect when selected
    if (selected)
    {
      yield return StartCoroutine(BounceEffect());
    }

    currentAnimation = null;
  }

  private Color GetBaseCardColor()
  {
    // Return the appropriate base color based on upgrade status
    if (isRepeatable)
    {
      return repeatableCardColor;
    }
    else if (powerUpData.level == 3)
    {
      return maxLevelCardColor;
    }
    else if (isUpgrade)
    {
      return upgradeCardColor;
    }
    else
    {
      return normalColor;
    }
  }

  private IEnumerator BounceEffect()
  {
    Vector3 targetScale = originalScale * selectedScale;
    Vector3 bounceScale = targetScale * 1.05f; // Slightly bigger bounce

    float bounceTime = 0.1f;
    float elapsed = 0f;

    // Bounce up
    while (elapsed < bounceTime)
    {
      elapsed += Time.deltaTime;
      float progress = elapsed / bounceTime;
      transform.localScale = Vector3.Lerp(targetScale, bounceScale, progress);
      yield return null;
    }

    elapsed = 0f;

    // Bounce back down
    while (elapsed < bounceTime)
    {
      elapsed += Time.deltaTime;
      float progress = elapsed / bounceTime;
      transform.localScale = Vector3.Lerp(bounceScale, targetScale, progress);
      yield return null;
    }

    transform.localScale = targetScale;
  }

  // Reset card to normal state (useful when returning to pool or resetting)
  public void ResetCard()
  {
    if (currentAnimation != null)
    {
      StopCoroutine(currentAnimation);
      currentAnimation = null;
    }

    isSelected = false;
    transform.localScale = originalScale;
    if (cardBackground != null)
    {
      cardBackground.color = GetBaseCardColor();
    }
    SetHighlight(false);
  }

  private void OnDestroy()
  {
    if (currentAnimation != null)
    {
      StopCoroutine(currentAnimation);
    }
  }

  // Helper method to identify repeatable power-ups
  private bool IsRepeatablePowerUp(PowerUpEffectType effectType)
  {
    return effectType == PowerUpEffectType.Acquisition ||
           effectType == PowerUpEffectType.InstantFreeze;
  }
}