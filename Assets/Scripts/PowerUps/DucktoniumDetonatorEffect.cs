using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/Effects/Ducktonium Detonator")]
// This class is a reusable asset I can create in the editor. It knows how to react when a combo happens because it follows the IComboEffect rules.
public class DucktoniumDetonatorEffect : ScriptableObject, IComboEffect {
    public void OnComboReached(Player player, int comboCount, Vector3 lastHitPosition) {
        // Search through the player’s active power-ups and see if they have one called ‘Ducktonium Detonator’. If they do, give it to me.
        var powerUp = PowerUpManager.Instance.activePowerUps
            .Find(p => p.powerUpName == "Ducktonium Detonator");

        if (powerUp == null) return;

        // Determine combo threshold based on upgrade level
        int threshold = powerUp.level switch {
            1 => 10,
            2 => 8,
            3 => 5,
            _ => 10
        };

        // If combo is high enough, trigger AoE explosion
        if (comboCount >= threshold) {
            VisualEffectsManager.Instance.TriggerAOE(lastHitPosition);
        }
    }
}