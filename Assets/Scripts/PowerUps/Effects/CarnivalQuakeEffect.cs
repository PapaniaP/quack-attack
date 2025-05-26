using UnityEngine;

public class CarnivalQuakeEffect : IAcquisitionEffect
{
  public void ApplyEffect()
  {
    GameManager.Instance.StartCoroutine(TriggerQuake());
  }

  private System.Collections.IEnumerator TriggerQuake()
  {
    Debug.Log("🎡 Carnival Quake Activated!");

    // 1. Freeze all ducks and logic
    GameManager.Instance.IsFrozen = true;

    // 2. Trigger screen VFX
    if (VisualEffectsManager.Instance != null)
    {
      VisualEffectsManager.Instance.ScreenFlash();
      VisualEffectsManager.Instance.ScreenTintFadeOut(2f);
      VisualEffectsManager.Instance.CameraShake(1.0f, 0.3f); // optional
    }

    // 3. Wait 10 seconds in real time
    yield return new WaitForSeconds(10f);

    // 4. Unfreeze
    GameManager.Instance.IsFrozen = false;
    Debug.Log("🌀 Carnival Quake Ended");
  }
}