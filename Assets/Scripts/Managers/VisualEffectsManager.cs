using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void TriggerAOE(Vector3 position)
    {
        Debug.Log($"[VFX] Triggering AoE effect at: {position}");

        // TODO: Add particle effects, camera shake, etc.
    }
}