using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer rend;

    [Header("Target Options")]
    public float moveSpeed = 3f;
    public float lifetime = 5f;
    public float colorChangeThreshold = 1.5f;

    [Header("Visual Feedback")]
    public float pulseMinScale = 0.999f;
    public float pulseMaxScale = 1.001f;
    public float pulseSpeed = 1f;

    public GameObject boundsObject;
    private BoxCollider bounds;

    private Vector3 velocity;
    private float timer;
    private Color originalColor;
    private Vector3 originalScale;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        originalColor = rend.material.color;
        originalScale = transform.localScale;

        float difficulty = GameManager.Instance != null ? GameManager.Instance.RunProgress : 0f;
        float adjustedSpeed = Mathf.Lerp(1f, moveSpeed, difficulty); // starts at 1, ramps to moveSpeed
        Vector3 biasedDirection = new Vector3(
            Random.Range(-1f, 1f),     // Full X range (left-right)
            Random.Range(-0.5f, 0.5f), // Less Y movement (up-down)
            Random.Range(-0.2f, 0.2f)  // Tiny Z movement (depth)
        ).normalized;

        float randomSpeed = Random.Range(-1f * moveSpeed, 2f * moveSpeed);
        velocity = biasedDirection * randomSpeed;

        if (boundsObject != null)
        {
            bounds = boundsObject.GetComponent<BoxCollider>();
        }

        // Optional: random start position inside bounds
        if (bounds != null)
        {
            Vector3 center = bounds.bounds.center;
            Vector3 extents = bounds.bounds.extents;
            transform.position = center + new Vector3(
                Random.Range(-extents.x, extents.x),
                Random.Range(-extents.y, extents.y),
                Random.Range(-extents.z, extents.z)
            );
        }
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;

        if (bounds != null && !bounds.bounds.Contains(transform.position))
        {
            Vector3 center = bounds.bounds.center;
            Vector3 extents = bounds.bounds.extents;
            Vector3 pos = transform.position - center;

            // Check which axis is out of bounds and reflect velocity accordingly
            if (pos.x < -extents.x || pos.x > extents.x)
                velocity.x = -velocity.x;
            if (pos.y < -extents.y || pos.y > extents.y)
                velocity.y = -velocity.y;
            if (pos.z < -extents.z || pos.z > extents.z)
                velocity.z = -velocity.z;

            // Clamp position just inside the bounds
            transform.position = center + new Vector3(
                Mathf.Clamp(pos.x, -extents.x * 0.95f, extents.x * 0.95f),
                Mathf.Clamp(pos.y, -extents.y * 0.95f, extents.y * 0.95f),
                Mathf.Clamp(pos.z, -extents.z * 0.95f, extents.z * 0.95f)
            );
        }

        // Timer and visual feedback
        timer += Time.deltaTime;
        float timeLeft = lifetime - timer;
        UpdateVisualFeedback(timeLeft);

        if (timer >= lifetime)
        {
            Explode();
        }
    }

    void UpdateVisualFeedback(float timeLeft)
    {
        // Color change as time runs out
        if (timeLeft < colorChangeThreshold)
        {
            float t = 1f - (timeLeft / colorChangeThreshold);

            // More dramatic color shift (yellow -> orange -> red)
            Color warningColor;
            if (t < 0.5f)
                warningColor = Color.Lerp(originalColor, Color.yellow, t * 2);
            else
                warningColor = Color.Lerp(Color.yellow, Color.red, (t - 0.5f) * 2);

            rend.material.color = warningColor;

            // Pulsing effect that intensifies as time runs out
            float pulseIntensity = Mathf.Lerp(0.1f, 0.5f, t); // More intense pulsing as t approaches 1
            float pulseFactor = Mathf.Lerp(
                1 - (pulseMinScale * pulseIntensity),
                1 + (pulseMaxScale * pulseIntensity),
                (Mathf.Sin(Time.time * pulseSpeed * (1 + t)) + 1) / 2
            );

            transform.localScale = originalScale * pulseFactor;

            // Optionally, could add emission intensity increase here if material supports it
            if (rend.material.HasProperty("_EmissionColor"))
            {
                Color emissionColor = warningColor * t * 2;
                rend.material.SetColor("_EmissionColor", emissionColor);
            }
        }
        else
        {
            // Reset to original appearance
            rend.material.color = originalColor;
            transform.localScale = originalScale;

            if (rend.material.HasProperty("_EmissionColor"))
            {
                rend.material.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    public void Hit(float multiplier = 1f)
    {
        int basePoints = 100;
        int finalPoints = Mathf.RoundToInt(basePoints * multiplier);

        GameManager.Instance.AddPoints(finalPoints);
        Debug.Log($"[Target] Hit! Multiplier: {multiplier}, Points: {finalPoints}");

        Destroy(gameObject);
    }

    void Explode()
    {
        Debug.Log("💥 Duck exploded!");
        GameManager.Instance.RemoveLife();
        Destroy(gameObject);
    }
}