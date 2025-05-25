using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PowerUpMenuUI : MonoBehaviour
{
  [Header("UI References")]
  public TextMeshProUGUI titleText;
  public Transform cardContainer;
  public GameObject cardPrefab;
  public Button confirmButton;
  public Button skipButton;
  public CanvasGroup canvasGroup; // Add this for fade in/out

  [Header("Testing")]
  public KeyCode testKey = KeyCode.P; // Press P to test

  private PowerUpCardUI selectedCard = null;
  private List<PowerUpCardUI> spawnedCards = new();

  void Start()
  {
    // Start with menu hidden
    HideMenu();

    // Check button assignments and set up OnClick events
    SetupButtons();
  }

  private void SetupButtons()
  {
    if (confirmButton != null)
    {
      // Clear any existing listeners and add our method
      confirmButton.onClick.RemoveAllListeners();
      confirmButton.onClick.AddListener(OnConfirmClicked);
      Debug.Log("[PowerUpMenuUI] Confirm button configured");
    }
    else
    {
      Debug.LogError("[PowerUpMenuUI] Confirm button is not assigned in the inspector!");
    }

    if (skipButton != null)
    {
      // Clear any existing listeners and add our method
      skipButton.onClick.RemoveAllListeners();
      skipButton.onClick.AddListener(OnSkipClicked);
      Debug.Log("[PowerUpMenuUI] Skip button configured");
    }
    else
    {
      Debug.LogError("[PowerUpMenuUI] Skip button is not assigned in the inspector!");
    }
  }

  void Update()
  {
    // Test functionality - remove this in final build
    if (Input.GetKeyDown(testKey))
    {
      TestPowerUpSelection();
    }
  }

  [ContextMenu("Test Power-Up Selection")]
  public void TestPowerUpSelection()
  {
    // Use GameManager's trigger method for consistent behavior
    GameManager.Instance.TriggerPowerUpSelection();
  }

  public void Show(List<PowerUpData> powerUpOptions)
  {
    ShowMenu();
    selectedCard = null;
    confirmButton.interactable = false;

    // Clear previous cards
    foreach (Transform child in cardContainer)
    {
      Destroy(child.gameObject);
    }
    spawnedCards.Clear(); // Clear the list too

    // Spawn cards
    foreach (var data in powerUpOptions)
    {
      GameObject obj = Instantiate(cardPrefab, cardContainer);
      PowerUpCardUI card = obj.GetComponent<PowerUpCardUI>();
      card.Setup(data, () => OnCardSelected(card));
      spawnedCards.Add(card);
    }

    Debug.Log($"[PowerUpMenuUI] Showing {powerUpOptions.Count} power-up options");
  }

  private void OnCardSelected(PowerUpCardUI card)
  {
    foreach (var c in spawnedCards)
    {
      c.SetSelected(c == card);
    }

    selectedCard = card;
    confirmButton.interactable = true;

    Debug.Log($"[PowerUpMenuUI] Selected: {card.powerUpData.powerUpName}");
  }

  public void OnConfirmClicked()
  {
    Debug.Log("[PowerUpMenuUI] Confirm button clicked!"); // Debug log

    if (selectedCard != null)
    {
      Debug.Log($"[PowerUpMenuUI] Applying power-up: {selectedCard.powerUpData.powerUpName}");
      PowerUpManager.Instance.ApplyPowerUp(selectedCard.powerUpData);
      Close();
    }
    else
    {
      Debug.LogWarning("[PowerUpMenuUI] Confirm clicked but no card is selected!");
    }
  }

  public void OnSkipClicked()
  {
    Debug.Log("[PowerUpMenuUI] Skip button clicked!"); // Debug log
    PowerUpManager.Instance.SkipPowerUp();
    Close();
  }

  private void Close()
  {
    Debug.Log("[PowerUpMenuUI] Closing menu..."); // Debug log
    HideMenu();
    // Notify GameManager that selection is complete
    GameManager.Instance.OnPowerUpSelectionComplete();
    Debug.Log("[PowerUpMenuUI] Menu closed");
  }

  private void ShowMenu()
  {
    // GameManager already handles cursor and game state via PowerUpSelection state
    // We just need to show the UI
    if (canvasGroup != null)
    {
      canvasGroup.alpha = 1f;
      canvasGroup.interactable = true;
      canvasGroup.blocksRaycasts = true;
    }
    else
    {
      // Fallback if no CanvasGroup
      gameObject.SetActive(true);
    }
  }

  private void HideMenu()
  {
    // Just hide the UI - GameManager handles the rest
    if (canvasGroup != null)
    {
      canvasGroup.alpha = 0f;
      canvasGroup.interactable = false;
      canvasGroup.blocksRaycasts = false;
    }
    else
    {
      // Fallback if no CanvasGroup
      gameObject.SetActive(false);
    }
  }
}