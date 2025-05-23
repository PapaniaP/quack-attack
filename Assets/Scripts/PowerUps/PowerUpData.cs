using UnityEngine;

// public enum PowerUpType
// {
//   Passive,
//   Combo,
//   Unique
// }

public enum PowerUpEffectType
{
  None,
  ComboExplosion,
  SlowMotion,
  ExtraLife,
  MissForgiveness,
  TargetDeathExplosion
}

[CreateAssetMenu(menuName = "PowerUps/New PowerUp")]
public class PowerUpData : ScriptableObject
{
  public string powerUpName;
  [TextArea] public string description;
  public Sprite icon;

  // public PowerUpType type;
  public bool upgradeable;
  public int level;

  public PowerUpEffectType effectType; // ← Used in PowerUpManager to create effect logic at runtime
}