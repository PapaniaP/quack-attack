using UnityEngine;

public class TestPowerUpTrigger : MonoBehaviour
{
  [Header("References")]
  public PowerUpMenuUI powerUpMenu;

  [Header("Testing")]
  public KeyCode testKey = KeyCode.P;

  void Update()
  {
    if (Input.GetKeyDown(testKey))
    {
      Debug.Log("[TestPowerUpTrigger] P key pressed - triggering power-up menu");

      if (powerUpMenu != null)
      {
        powerUpMenu.TestPowerUpSelection();
      }
      else
      {
        Debug.LogError("[TestPowerUpTrigger] PowerUpMenuUI reference is null! Assign it in the inspector.");
      }
    }
  }
}