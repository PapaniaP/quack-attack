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
    Debug.Log("[PowerUpHUD] RefreshDisplay called");

    if (PowerUpManager.Instance == null)
    {
      Debug.LogError("[PowerUpHUD] PowerUpManager.Instance is null!");
      return;
    }

    Debug.Log($"[PowerUpHUD] Found {PowerUpManager.Instance.acquiredPowerUps.Count} acquired power-ups");

    if (powerUpContainer == null)
    {
      Debug.LogError("[PowerUpHUD] powerUpContainer is null! Assign it in the inspector.");
      return;
    }

    if (powerUpIconPrefab == null)
    {
      Debug.LogError("[PowerUpHUD] powerUpIconPrefab is null! Assign it in the inspector.");
      return;
    }

    // Clear old icons
    ClearOldIcons();

    // Add current power-ups
    var acquiredPowerUps = PowerUpManager.Instance.acquiredPowerUps;
    foreach (var powerUp in acquiredPowerUps.Take(maxDisplayedPowerUps))
    {
      Debug.Log($"[PowerUpHUD] Processing power-up: {powerUp.powerUpName}, Icon: {(powerUp.icon != null ? powerUp.icon.name : "NULL")}");
      CreateOrUpdateIcon(powerUp);
    }

    // Layout Group will automatically arrange icons vertically
    Debug.Log($"[PowerUpHUD] RefreshDisplay complete. Active icons: {activeIcons.Count}");
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
    Debug.Log($"[PowerUpHUD] CreateOrUpdateIcon called for {powerUp.powerUpName}");

    if (powerUpIconPrefab == null || powerUpContainer == null)
    {
      Debug.LogError($"[PowerUpHUD] Missing references! Prefab: {powerUpIconPrefab != null}, Container: {powerUpContainer != null}");
      return;
    }

    PowerUpHUDIcon icon;

    // Create new icon or update existing one
    if (activeIcons.ContainsKey(powerUp.effectType))
    {
      Debug.Log($"[PowerUpHUD] Updating existing icon for {powerUp.effectType}");
      icon = activeIcons[powerUp.effectType];
      icon.UpdateDisplay(powerUp);
    }
    else
    {
      Debug.Log($"[PowerUpHUD] Creating new icon GameObject for {powerUp.effectType}");
      // Create new icon
      GameObject iconObj = Instantiate(powerUpIconPrefab, powerUpContainer);
      Debug.Log($"[PowerUpHUD] Icon GameObject created: {iconObj.name}");

      icon = iconObj.GetComponent<PowerUpHUDIcon>();

      if (icon == null)
      {
        Debug.Log("[PowerUpHUD] PowerUpHUDIcon component not found, adding it");
        icon = iconObj.AddComponent<PowerUpHUDIcon>();
      }

      Debug.Log($"[PowerUpHUD] Setting up icon with size {iconSize}");
      icon.Setup(powerUp, iconSize);
      activeIcons[powerUp.effectType] = icon;
      Debug.Log($"[PowerUpHUD] Icon successfully added to activeIcons dictionary");
    }
  }

  // Call this when power-ups are acquired/upgraded
  public void OnPowerUpChanged()
  {
    Debug.Log("[PowerUpHUD] OnPowerUpChanged called");
    RefreshDisplay();
  }
}

// Component for individual power-up icons in the HUD
public class PowerUpHUDIcon : MonoBehaviour
{
  [Header("Icon Components")]
  public Image iconImage;

  private PowerUpData powerUpData;

  public void Setup(PowerUpData data, Vector2 size)
  {
    Debug.Log($"[PowerUpHUDIcon] Setup called for {data.powerUpName}, size: {size}");
    powerUpData = data;

    // Get or create components
    SetupComponents();

    // Note: Don't set RectTransform properties here as Layout Groups will control them
    // The LayoutElement component (added in CreateOrUpdateIcon) will handle sizing

    Debug.Log($"[PowerUpHUDIcon] Components setup complete, letting Layout Group handle positioning");

    // Update display
    UpdateDisplay(data);
    Debug.Log($"[PowerUpHUDIcon] Setup complete for {data.powerUpName}");
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
  }

  public void UpdateDisplay(PowerUpData data)
  {
    Debug.Log($"[PowerUpHUDIcon] UpdateDisplay called for {data.powerUpName}");
    powerUpData = data;

    // Update icon
    if (iconImage != null && data.icon != null)
    {
      iconImage.sprite = data.icon;
      iconImage.color = Color.white; // Keep icon at normal white color
      Debug.Log($"[PowerUpHUDIcon] Icon sprite set to {data.icon.name}");
    }
    else
    {
      Debug.LogWarning($"[PowerUpHUDIcon] Missing icon image or sprite! IconImage: {iconImage != null}, Sprite: {data.icon != null}");
    }

    Debug.Log($"[PowerUpHUDIcon] UpdateDisplay complete for {data.powerUpName}");
  }
}