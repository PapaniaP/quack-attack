using UnityEngine;

public class DuckDilationEffect : ITargetSpawnedEffect
{
  private int level;
  private float slowFactor = 0.6f; // 60% of original speed
  private float chance;

  public DuckDilationEffect(int level)
  {
    this.level = level;

    // Define slow chance based on level
    chance = level switch
    {
      1 => 0.25f,
      2 => 0.50f,
      3 => 0.750f,
      _ => 0.25f
    };
  }

  public void OnTargetSpawned(Target target)
  {
    if (Random.value <= chance)
    {
      target.ApplySlow(slowFactor);

      if (level == 3)
      {
        target.EnableGlitterTrail(); // Optional visual effect
      }

      Debug.Log($"🌀 Duck Dilation: Slowed down {target.name}");
    }
  }
}