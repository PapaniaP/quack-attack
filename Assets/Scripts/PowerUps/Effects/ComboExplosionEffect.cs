using UnityEngine;

public class ComboExplosionEffect : IComboEffect
{
  private int level;
  private bool isArmed;

  public ComboExplosionEffect(int level)
  {
    this.level = level;
    this.isArmed = false;
  }

  public void OnComboReached(Vector3 position, int comboCount)
  {
    int threshold = level == 1 ? 10 : level == 2 ? 8 : 5;

    if (!isArmed && comboCount >= threshold)
    {
      isArmed = true;
      Debug.Log("[ComboExplosionEffect] Armed for AoE.");
    }
    else if (isArmed)
    {
      VisualEffectsManager.Instance?.TriggerAOE(position);
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

      isArmed = false;
    }
  }
}