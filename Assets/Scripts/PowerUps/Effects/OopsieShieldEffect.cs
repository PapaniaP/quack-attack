using UnityEngine;
using System;

public class OopsieShieldEffect : IMissForgivenessEffect, ITimedEffect
{
  private int level;
  private int missesLeft;
  private float forgivenessWindow = 1f;
  private float windowTimer = 0f;
  private bool windowActive = false;

  public OopsieShieldEffect(int level)
  {
    this.level = level;
    this.missesLeft = level == 3 ? 1 : level; // 1 or 2 misses
  }

  public bool OnMiss()
  {
    if (level < 3)
    {
      if (missesLeft > 0)
      {
        missesLeft--;
        Debug.Log($"[OopsieShield] Forgave a miss. Remaining: {missesLeft}");
        return true;
      }
    }
    else
    {
      if (!windowActive)
      {
        windowActive = true;
        windowTimer = forgivenessWindow;
        Debug.Log("[OopsieShield] Activated forgiveness window!");
        return true;
      }
      else if (windowTimer > 0f)
      {
        Debug.Log("[OopsieShield] Miss forgiven within window.");
        return true;
      }
    }

    return false; // combo should reset
  }

  public void Update(float deltaTime)
  {
    if (windowActive)
    {
      windowTimer -= deltaTime;
      if (windowTimer <= 0f)
      {
        windowActive = false;
        Debug.Log("[OopsieShield] Forgiveness window expired.");
      }
    }
  }
}