using UnityEngine;

/// <summary>
/// Power-ups that care about combo streaks.
/// </summary>
public interface IComboEffect
{
  void OnComboReached(Vector3 position, int comboCount);
}

/// <summary>
/// Power-ups that intercept a missed shot and decide whether to forgive it.
/// </summary>
public interface IMissForgivenessEffect
{
  /// <returns>True if the combo should NOT reset</returns>
  bool OnMiss();
}

/// <summary>
/// Power-ups triggered when a duck is killed.
/// </summary>
public interface ITargetDeathEffect
{
  void OnTargetKilled(Target target, Vector3 position);
}

/// <summary>
/// Power-ups triggered immediately when acquired.
/// </summary>
public interface IAcquisitionEffect
{
  void ApplyEffect();
}

/// <summary>
/// Power-ups that modify or react to ducks when they spawn.
/// </summary>
public interface ITargetSpawnedEffect
{
  void OnTargetSpawned(Target target);
}

/// <summary>
/// Power-ups that need timed behavior (e.g. trigger every X seconds).
/// </summary>
public interface ITimedEffect
{
  void Update(float deltaTime);
}

/// <summary>
/// Power-ups that react when the player hits a target.
/// </summary>
public interface IOnHitEffect
{
  void OnHit(Target target, Vector3 hitPosition);
}
