using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    public List<PowerUpData> acquiredPowerUps = new();
    public List<IComboEffect> activeEffects = new();
    public List<IMissForgivenessEffect> missForgivenessEffects = new();
    public List<ITimedEffect> timedEffects = new();
    public List<ITargetDeathEffect> deathEffects = new();

    [SerializeField] private PowerUpData testPowerUp; // assign via Inspector

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (testPowerUp != null)
        {
            AddPowerUp(testPowerUp);
        }
    }

    void Update()
    {
        float delta = Time.deltaTime;
        foreach (var effect in timedEffects)
            effect.Update(delta);
    }

    public void AddPowerUp(PowerUpData powerUp)
    {
        acquiredPowerUps.Add(powerUp);

        switch (powerUp.effectType)
        {
            case PowerUpEffectType.ComboExplosion:
                var comboEffect = new ComboExplosionEffect(powerUp.level);
                activeEffects.Add(comboEffect);
                break;

            case PowerUpEffectType.MissForgiveness:
                var shield = new OopsieShieldEffect(powerUp.level);
                missForgivenessEffects.Add(shield);
                timedEffects.Add(shield); // Level 3 uses Update()
                break;

            case PowerUpEffectType.TargetDeathExplosion:
                var boom = new QuacksplosiveTendenciesEffect(powerUp.level);
                deathEffects.Add(boom);
                break;

            default:
                Debug.LogWarning($"[PowerUpManager] No effect logic mapped for: {powerUp.effectType}");
                break;
        }

        Debug.Log($"[PowerUpManager] Acquired power-up: {powerUp.powerUpName}");
    }

    public void TriggerComboEffects(Vector3 position, int combo)
    {
        foreach (var effect in activeEffects)
        {
            effect.OnComboReached(position, combo);
        }
    }
}