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

    public void TriggerAOE(Vector3 position, float radius = 1f)
    {
        Debug.Log($"[VFX] Triggering AoE at {position} with radius {radius}");

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;

        // Scale the sphere to reflect the AoE radius (diameter = radius * 2)
        marker.transform.localScale = Vector3.one * radius * 2f;

        // Optional: Change color or make it transparent
        Renderer rend = marker.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = new Color(1f, 0f, 0f, 0.3f); // red with some transparency
        }

        Destroy(marker, 1.5f);
    }
}