using UnityEngine;
public interface IComboEffect {
    void OnComboReached(StationaryPlayer player, int comboCount, Vector3 lastHitPosition);
}