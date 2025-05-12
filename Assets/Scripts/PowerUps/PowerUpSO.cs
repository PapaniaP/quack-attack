using UnityEngine;

public enum PowerUpType {
    Passive,
    Unique
}

[CreateAssetMenu(menuName = "PowerUps/New PowerUp")]
public class PowerUpSO : ScriptableObject {
    public string powerUpName;
    [TextArea]
    public string description;
    public Sprite icon;
    public PowerUpType type;
    public bool stackable;
    public int level;

[Tooltip("Assign a ScriptableObject that implements an effect interface like IComboEffect or IKillEffect")]
public ScriptableObject effect;
}
