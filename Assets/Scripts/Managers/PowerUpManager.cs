using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance;

    public List<PowerUpData> acquiredPowerUps = new();
    public List<IComboEffect> activeEffects = new();
    public List<IMissForgivenessEffect> missForgivenessEffects = new();
    public List<ITimedEffect> timedEffects = new();
    public List<ITargetDeathEffect> deathEffects = new();
    public List<ITargetSpawnedEffect> spawnEffects = new();

    [SerializeField] private PowerUpData testPowerUp; // assign via Inspector
    [Header("Power-Up Pool")]
    [SerializeField] private List<PowerUpData> availablePowerUps = new(); // All possible power-ups

    [Header("UI References")]
    [SerializeField] private PowerUpHUD powerUpHUD; // Reference to HUD display

    [Header("Skip Reward")]
    [SerializeField] private int skipRewardPoints = 500; // Points awarded for skipping power-up selection

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

    // Called by UI when player confirms their choice
    public void ApplyPowerUp(PowerUpData powerUp)
    {
        AddPowerUp(powerUp);
    }

    // Called by UI when player skips power-up selection
    public void SkipPowerUp()
    {
        // Give player points for skipping (risk/reward choice)
        GameManager.Instance.AddPoints(skipRewardPoints);
    }

    // Get current level of a specific power-up type
    public int GetPowerUpLevel(PowerUpEffectType effectType)
    {
        var existingPowerUp = acquiredPowerUps.FirstOrDefault(p => p.effectType == effectType);
        return existingPowerUp?.level ?? 0;
    }

    // Check if power-up can be upgraded (max level is 3)
    public bool CanUpgradePowerUp(PowerUpEffectType effectType)
    {
        return GetPowerUpLevel(effectType) < 3;
    }

    // Generate random power-up options with upgrade awareness
    public List<PowerUpData> GetRandomPowerUpOptions(int count = 2)
    {
        List<PowerUpData> options = new();

        if (availablePowerUps.Count == 0)
        {
            return options;
        }

        // Create weighted pool: existing upgradeable power-ups + new power-ups
        List<PowerUpData> weightedPool = new();

        // Add upgradeable existing power-ups (with higher weight)
        foreach (var acquired in acquiredPowerUps)
        {
            if (CanUpgradePowerUp(acquired.effectType))
            {
                var upgradedVersion = CreateUpgradeVersion(acquired);
                if (upgradedVersion != null)
                {
                    // Add twice for higher chance of upgrades
                    weightedPool.Add(upgradedVersion);
                    weightedPool.Add(upgradedVersion);
                }
            }
        }

        // Add new power-ups (that we don't have yet)
        foreach (var available in availablePowerUps)
        {
            if (GetPowerUpLevel(available.effectType) == 0)
            {
                weightedPool.Add(available);
            }
        }

        // Select random options without duplicates
        var shuffled = weightedPool.OrderBy(x => Random.value).ToList();
        for (int i = 0; i < count && i < shuffled.Count; i++)
        {
            // Ensure no duplicate effect types in same selection
            var candidate = shuffled[i];
            if (!options.Any(o => o.effectType == candidate.effectType))
            {
                options.Add(candidate);
            }
        }

        return options;
    }

    // Create an upgrade version of an existing power-up
    private PowerUpData CreateUpgradeVersion(PowerUpData basePowerUp)
    {
        if (!CanUpgradePowerUp(basePowerUp.effectType)) return null;

        // Create a runtime copy with upgraded stats
        var upgrade = ScriptableObject.CreateInstance<PowerUpData>();
        upgrade.powerUpName = basePowerUp.powerUpName;
        upgrade.icon = basePowerUp.icon;
        upgrade.effectType = basePowerUp.effectType;
        upgrade.upgradeable = basePowerUp.upgradeable;
        upgrade.level = basePowerUp.level + 1;

        // Generate upgrade description based on effect type and level
        upgrade.description = GenerateUpgradeDescription(basePowerUp.effectType, upgrade.level);

        return upgrade;
    }

    // Generate descriptions for upgraded power-ups
    private string GenerateUpgradeDescription(PowerUpEffectType effectType, int newLevel)
    {
        return effectType switch
        {
            PowerUpEffectType.ComboExplosion => newLevel switch
            {
                2 => "UPGRADE: Combo explosions now trigger at 8 combo instead of 10!",
                3 => "MAX UPGRADE: Combo explosions at 5 combo + larger blast radius!",
                _ => "Explosions trigger after reaching combo milestones."
            },
            PowerUpEffectType.MissForgiveness => newLevel switch
            {
                2 => "UPGRADE: Now forgives 2 misses instead of 1!",
                3 => "MAX UPGRADE: Forgiveness window - multiple misses forgiven for 1 second!",
                _ => "Forgives your first miss and preserves your combo."
            },
            PowerUpEffectType.TargetDeathExplosion => newLevel switch
            {
                2 => "UPGRADE: 20% explosion chance + larger radius (3.5m)!",
                3 => "MAX UPGRADE: 30% explosion chance + massive radius (4.5m)!",
                _ => "10% chance for killed ducks to explode and damage nearby targets."
            },
            PowerUpEffectType.TargetSpawnSlow => newLevel switch
            {
                2 => "UPGRADE: 50% of new ducks spawn slowed down!",
                3 => "MAX UPGRADE: 75% slow chance + glitter trail effect!",
                _ => "25% chance for newly spawned ducks to move slower."
            },
            PowerUpEffectType.Acquisition =>
                "UPGRADE: Gain another life! (This power-up grants multiple lives)",
            PowerUpEffectType.InstantFreeze =>
                "UPGRADE: Another 10-second freeze! (This power-up can be collected multiple times)",
            _ => "Upgraded power-up with enhanced effects!"
        };
    }

    public void AddPowerUp(PowerUpData powerUp)
    {
        // Check if we already have this power-up type
        var existingPowerUp = acquiredPowerUps.FirstOrDefault(p => p.effectType == powerUp.effectType);

        if (existingPowerUp != null && existingPowerUp.upgradeable)
        {
            // Create a new copy instead of modifying the existing one
            var upgradedCopy = ScriptableObject.CreateInstance<PowerUpData>();
            upgradedCopy.powerUpName = existingPowerUp.powerUpName;
            upgradedCopy.icon = existingPowerUp.icon;
            upgradedCopy.effectType = existingPowerUp.effectType;
            upgradedCopy.upgradeable = existingPowerUp.upgradeable;
            upgradedCopy.level = powerUp.level;
            upgradedCopy.description = powerUp.description;

            // Replace the existing power-up with the upgraded copy
            int index = acquiredPowerUps.IndexOf(existingPowerUp);

            // Destroy the old copy if it was runtime-created
            if (!availablePowerUps.Contains(existingPowerUp))
            {
                DestroyImmediate(existingPowerUp);
            }

            acquiredPowerUps[index] = upgradedCopy;

            // Remove old effects and add upgraded ones
            RemoveEffectsOfType(powerUp.effectType);
            AddEffectForPowerUp(upgradedCopy);
        }
        else
        {
            // For new power-ups, create a copy to avoid modifying the original asset
            var powerUpCopy = ScriptableObject.CreateInstance<PowerUpData>();
            powerUpCopy.powerUpName = powerUp.powerUpName;
            powerUpCopy.icon = powerUp.icon;
            powerUpCopy.effectType = powerUp.effectType;
            powerUpCopy.upgradeable = powerUp.upgradeable;
            powerUpCopy.level = powerUp.level;
            powerUpCopy.description = powerUp.description;

            // Add new power-up copy
            acquiredPowerUps.Add(powerUpCopy);
            AddEffectForPowerUp(powerUpCopy);
        }

        // Update HUD display
        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (powerUpHUD != null)
        {
            powerUpHUD.OnPowerUpChanged();
        }
    }

    // Remove effects of a specific type (for upgrades)
    private void RemoveEffectsOfType(PowerUpEffectType effectType)
    {
        switch (effectType)
        {
            case PowerUpEffectType.ComboExplosion:
                activeEffects.RemoveAll(e => e is ComboExplosionEffect);
                break;
            case PowerUpEffectType.MissForgiveness:
                missForgivenessEffects.RemoveAll(e => e is OopsieShieldEffect);
                timedEffects.RemoveAll(e => e is OopsieShieldEffect);
                break;
            case PowerUpEffectType.TargetDeathExplosion:
                deathEffects.RemoveAll(e => e is QuacksplosiveTendenciesEffect);
                break;
            case PowerUpEffectType.TargetSpawnSlow:
                spawnEffects.RemoveAll(e => e is DuckDilationEffect);
                break;
        }
    }

    // Add effect for a power-up (extracted from original AddPowerUp)
    private void AddEffectForPowerUp(PowerUpData powerUp)
    {
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

            case PowerUpEffectType.TargetSpawnSlow:
                var dilation = new DuckDilationEffect(powerUp.level);
                spawnEffects.Add(dilation);
                break;

            case PowerUpEffectType.Acquisition:
                var quack = new OneMoreQuackEffect();
                quack.ApplyEffect(); // Trigger instantly when picked
                break;

            case PowerUpEffectType.InstantFreeze:
                var quake = new CarnivalQuakeEffect();
                quake.ApplyEffect(); // Freeze ducks + VFX + unfreeze
                break;

            default:
                break;
        }
    }

    public void TriggerComboEffects(Vector3 position, int combo)
    {
        foreach (var effect in activeEffects)
        {
            effect.OnComboReached(position, combo);
        }
    }

    // Reset all power-ups when game restarts
    public void ResetPowerUps()
    {
        // Destroy any runtime-created PowerUpData instances to prevent persistence
        foreach (var powerUp in acquiredPowerUps)
        {
            // If this is a runtime-created upgrade (not an original asset), destroy it
            if (powerUp != null && !availablePowerUps.Contains(powerUp))
            {
                DestroyImmediate(powerUp);
            }
        }

        // Clear all acquired power-ups
        acquiredPowerUps.Clear();

        // Clear all active effects
        activeEffects.Clear();
        missForgivenessEffects.Clear();
        timedEffects.Clear();
        deathEffects.Clear();
        spawnEffects.Clear();

        // Update HUD to reflect empty state
        UpdateHUD();
    }
}