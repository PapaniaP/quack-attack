using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class PowerUpHUD : MonoBehaviour
{
  [Header("HUD Settings")]
  public Transform powerUpContainer; // Parent for power-up icons
  public GameObject powerUpIconPrefab; // Prefab for individual power-up icons
  public int maxDisplayedPowerUps = 6; // Maximum icons to show

  [Header("Icon Settings")]
  public Vector2 iconSize = new Vector2(50f, 50f);
  public float iconSpacing = 10f;

  private Dictionary<PowerUpEffectType, PowerUpHUDIcon> activeIcons = new();

  void Start()
  {
    // Start with empty HUD
    RefreshDisplay();
  }

  public void RefreshDisplay()
  {
    if (PowerUpManager.Instance == null) return;

    // Clear old icons
    ClearOldIcons();

    // Add current power-ups
    var acquiredPowerUps = PowerUpManager.Instance.acquiredPowerUps;
    foreach (var powerUp in acquiredPowerUps.Take(maxDisplayedPowerUps))
    {
      CreateOrUpdateIcon(powerUp);
    }

    // Arrange icons
    ArrangeIcons();
  }

  private void ClearOldIcons()
  {
    // Remove icons for power-ups we no longer have
    var currentEffectTypes = PowerUpManager.Instance.acquiredPowerUps
        .Select(p => p.effectType).ToHashSet();

    var iconsToRemove = activeIcons.Keys
        .Where(type => !currentEffectTypes.Contains(type)).ToList();

    foreach (var type in iconsToRemove)
    {
      if (activeIcons[type] != null)
      {
        Destroy(activeIcons[type].gameObject);
      }
      activeIcons.Remove(type);
    }
  }

  private void CreateOrUpdateIcon(PowerUpData powerUp)
  {
    if (powerUpIconPrefab == null || powerUpContainer == null) return;

    PowerUpHUDIcon icon;

    // Create new icon or update existing one
    if (activeIcons.ContainsKey(powerUp.effectType))
    {
      icon = activeIcons[powerUp.effectType];
      icon.UpdateDisplay(powerUp);
    }
    else
    {
      // Create new icon
      GameObject iconObj = Instantiate(powerUpIconPrefab, powerUpContainer);
      icon = iconObj.GetComponent<PowerUpHUDIcon>();

      if (icon == null)
      {
        icon = iconObj.AddComponent<PowerUpHUDIcon>();
      }

      icon.Setup(powerUp, iconSize);
      activeIcons[powerUp.effectType] = icon;
    }
  }

  private void ArrangeIcons()
  {
    int index = 0;
    foreach (var icon in activeIcons.Values)
    {
      if (icon != null)
      {
        Vector3 position = new Vector3(
            index * (iconSize.x + iconSpacing),
            0,
            0
        );
        icon.transform.localPosition = position;
        index++;
      }
    }
  }

  // Call this when power-ups are acquired/upgraded
  public void OnPowerUpChanged()
  {
    RefreshDisplay();
  }
}

// Component for individual power-up icons in the HUD
public class PowerUpHUDIcon : MonoBehaviour
{
  [Header("Icon Components")]
  public Image iconImage;
  public TextMeshProUGUI levelText;
  public Image backgroundImage;

  [Header("Level Colors")]
  public Color level1Color = Color.white;
  public Color level2Color = Color.yellow;
  public Color level3Color = Color.magenta;

  private PowerUpData powerUpData;

  public void Setup(PowerUpData data, Vector2 size)
  {
    powerUpData = data;

    // Get or create components
    SetupComponents();

    // Set size
    RectTransform rectTransform = GetComponent<RectTransform>();
    if (rectTransform != null)
    {
      rectTransform.sizeDelta = size;
    }

    // Update display
    UpdateDisplay(data);
  }

  private void SetupComponents()
  {
    // Get or create Image component for icon
    if (iconImage == null)
    {
      iconImage = GetComponent<Image>();
      if (iconImage == null)
      {
        iconImage = gameObject.AddComponent<Image>();
      }
    }

    // Get or create background image
    if (backgroundImage == null)
    {
      GameObject bgObj = new GameObject("Background");
      bgObj.transform.SetParent(transform, false);
      backgroundImage = bgObj.AddComponent<Image>();
      backgroundImage.color = new Color(0, 0, 0, 0.3f); // Semi-transparent background

      // Set background to fill parent
      RectTransform bgRect = backgroundImage.GetComponent<RectTransform>();
      bgRect.anchorMin = Vector2.zero;
      bgRect.anchorMax = Vector2.one;
      bgRect.offsetMin = Vector2.zero;
      bgRect.offsetMax = Vector2.zero;

      // Move icon to front
      iconImage.transform.SetAsLastSibling();
    }

    // Get or create level text
    if (levelText == null)
    {
      GameObject textObj = new GameObject("LevelText");
      textObj.transform.SetParent(transform, false);
      levelText = textObj.AddComponent<TextMeshProUGUI>();

      // Position at bottom-right corner
      RectTransform textRect = levelText.GetComponent<RectTransform>();
      textRect.anchorMin = new Vector2(0.7f, 0f);
      textRect.anchorMax = new Vector2(1f, 0.3f);
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;

      // Style the text
      levelText.text = "";
      levelText.fontSize = 12f;
      levelText.fontStyle = FontStyles.Bold;
      levelText.alignment = TextAlignmentOptions.Center;
      levelText.color = Color.white;
    }
  }

  public void UpdateDisplay(PowerUpData data)
  {
    powerUpData = data;

    // Update icon
    if (iconImage != null && data.icon != null)
    {
      iconImage.sprite = data.icon;
      iconImage.color = GetLevelColor(data.level);
    }

    // Update level text
    if (levelText != null)
    {
      if (data.level > 1)
      {
        levelText.text = data.level == 3 ? "MAX" : data.level.ToString();
        levelText.color = GetLevelColor(data.level);
        levelText.gameObject.SetActive(true);
      }
      else
      {
        levelText.gameObject.SetActive(false);
      }
    }

    // Update background color
    if (backgroundImage != null)
    {
      Color bgColor = GetLevelColor(data.level);
      bgColor.a = 0.3f; // Keep it semi-transparent
      backgroundImage.color = bgColor;
    }
  }

  private Color GetLevelColor(int level)
  {
    return level switch
    {
      1 => level1Color,
      2 => level2Color,
      3 => level3Color,
      _ => level1Color
    };
  }
}