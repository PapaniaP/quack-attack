using UnityEngine;

public class QuacksplosiveTendenciesEffect : ITargetDeathEffect
{
  private int level;
  private float chance;
  private float radius;

  public QuacksplosiveTendenciesEffect(int level)
  {
    this.level = level;
    chance = level switch
    {
      1 => 0.10f,
      2 => 0.20f,
      3 => 0.30f,
      _ => 0.10f
    };

    radius = level switch
    {
      1 => 2f,
      2 => 3.5f,
      3 => 4.5f,
      _ => 2f
    };
  }

  public void OnTargetKilled(Target target, Vector3 position)
  {
    if (Random.value <= chance)
    {
      Debug.Log($"💣 Quacksplosion at {position} (radius {radius})");

      // Use specific quacksplosion effect
      VisualEffectsManager.Instance?.TriggerQuacksplosion(position, radius);

      // Find and damage nearby ducks
      Collider[] hits = Physics.OverlapSphere(position, radius);
      foreach (var hit in hits)
      {
        Target nearbyDuck = hit.GetComponentInParent<Target>();
        if (nearbyDuck != null && nearbyDuck != target)
        {
          nearbyDuck.Hit(0.5f); // Optional: Reduced points for chain kills
          Debug.Log("🔥 Nearby duck hit by explosion!");
        }
      }
    }
  }
}