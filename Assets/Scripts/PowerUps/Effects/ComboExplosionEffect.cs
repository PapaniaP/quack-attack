using UnityEngine;

public class ComboExplosionEffect : IComboEffect
{
  private int level;
  private bool isArmed;
  private int lastComboCount;
  private int explosionInterval;

  public ComboExplosionEffect(int level)
  {
    this.level = level;
    this.isArmed = false;
    this.lastComboCount = 0;

    // Set how often explosions can trigger when combo is high
    this.explosionInterval = level == 3 ? 3 : level == 2 ? 4 : 5;
  }

  public void OnComboReached(Vector3 position, int comboCount)
  {
    int threshold = level == 1 ? 10 : level == 2 ? 8 : 5;

    // Arm the effect when reaching threshold
    if (!isArmed && comboCount >= threshold)
    {
      isArmed = true;
      lastComboCount = comboCount;
      Debug.Log("[ComboExplosionEffect] Armed for AoE.");
    }
    // If armed and combo increased by interval amount, trigger explosion
    else if (isArmed && comboCount >= lastComboCount + explosionInterval)
    {
      TriggerExplosion(position);
      lastComboCount = comboCount; // Update last trigger point
    }
    // If combo dropped below threshold, disarm and reset
    else if (isArmed && comboCount < threshold)
    {
      isArmed = false;
      lastComboCount = 0;
      Debug.Log("[ComboExplosionEffect] Disarmed - combo dropped below threshold.");
    }
  }

  private void TriggerExplosion(Vector3 position)
  {
    VisualEffectsManager.Instance?.TriggerComboExplosion(position, level == 3 ? 8f : 5f);
    Debug.Log("[ComboExplosionEffect] 💥 Triggering AoE explosion!");

    // 🧨 Damage logic
    float radius = level == 3 ? 8f : 5f;
    Collider[] hits = Physics.OverlapSphere(position, radius);

    foreach (var hit in hits)
    {
      if (hit.CompareTag("Target"))
      {
        Target target = hit.GetComponentInParent<Target>();
        if (target != null)
        {
          target.Hit(1f); // Apply normal score
          Debug.Log($"[ComboExplosionEffect] AoE hit: {hit.name}");
        }
      }
    }
  }
}