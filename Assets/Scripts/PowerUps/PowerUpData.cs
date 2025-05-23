using UnityEngine;

// public enum PowerUpCategory
// {
//   Offensive,
//   Survival,
//   Control,
//   Legendary,
//   Passive
// }

public enum PowerUpEffectType
{
  None,
  ComboExplosion,
  SlowMotion,
  ExtraLife,
  MissForgiveness,
  TargetDeathExplosion,
  TargetSpawnSlow,
  Acquisition,
  InstantFreeze,
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