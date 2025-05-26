using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("Renderer")]
    public Renderer rend;

    [Header("Target Options")]
    public float moveSpeed = 3f;
    public float lifetime = 5f; // This will be overridden by variable timing
    public float colorChangeThreshold = 1.5f;

    [Header("Variable Lifetime Settings")]
    public float earlyGameMinTime = 5f;
    public float earlyGameMaxTime = 10f;
    public float lateGameMinTime = 3f;
    public float lateGameMaxTime = 5f;

    [Header("Movement Settings")]
    public float bounceRandomization = 0.3f; // How much randomization to add to bounces
    public float earlyGameSpeedMultiplier = 0.7f; // Speed multiplier for early game
    public float lateGameSpeedMultiplier = 1.5f; // Speed multiplier for late game

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

    // Remove the CameraShake cameraShaker field, as we'll use the static Instance
    // private CameraShake cameraShaker;

    [Header("Hit Effect")]
    public GameObject HitExplosion; // Public variable to hold your smoke prefab

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
        originalColor = rend.material.color;
        originalScale = transform.localScale;

        // --- REMOVE THIS BLOCK OF CODE ---
        // Camera mainCamera = Camera.main;
        // if (mainCamera != null)
        // {
        //     cameraShaker = mainCamera.GetComponent<CameraShake>();
        // }
        // else
        // {
        //     Debug.LogError("No Main Camera found in the scene! Make sure your camera is tagged as 'MainCamera'.");
        // }
        // --- END OF REMOVED CODE BLOCK ---

        float difficulty = GameManager.Instance != null ? GameManager.Instance.RunProgress : 0f;

        // Variable lifetime based on difficulty
        float minTime = Mathf.Lerp(earlyGameMinTime, lateGameMinTime, difficulty);
        float maxTime = Mathf.Lerp(earlyGameMaxTime, lateGameMaxTime, difficulty);
        lifetime = Random.Range(minTime, maxTime);

        // Difficulty-scaled movement patterns
        float speedMultiplier = Mathf.Lerp(earlyGameSpeedMultiplier, lateGameSpeedMultiplier, difficulty);

        // Movement becomes more erratic with difficulty
        float directionRandomness = Mathf.Lerp(0.5f, 1.0f, difficulty); // Less random early, more random late
        Vector3 biasedDirection = new Vector3(
            Random.Range(-directionRandomness, directionRandomness),      // X movement scales with difficulty
            Random.Range(-directionRandomness * 0.5f, directionRandomness * 0.5f),  // Y movement (less than X)
            Random.Range(-0.2f, 0.2f)   // Z movement stays minimal
        ).normalized;

        // Speed variation also scales with difficulty
        float speedVariation = Mathf.Lerp(0.5f, 2.0f, difficulty); // Early: 0.5x-1.5x, Late: 0.5x-2.5x
        float randomSpeed = Random.Range(moveSpeed * (1 - speedVariation), moveSpeed * (1 + speedVariation));

        velocity = biasedDirection * randomSpeed * speedMultiplier;

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
        // Don't update if game is not active (paused, power-up selection, etc.)
        if (GameManager.Instance != null && !GameManager.Instance.isGameActive)
            return;

        // Don't update if frozen by power-ups
        if (GameManager.Instance != null && GameManager.Instance.IsFrozen)
            return;

        transform.position += velocity * Time.deltaTime;

        if (bounds != null && !bounds.bounds.Contains(transform.position))
        {
            Vector3 center = bounds.bounds.center;
            Vector3 extents = bounds.bounds.extents;
            Vector3 pos = transform.position - center;

            // Curved bouncing with physics-based reflection
            Vector3 wallNormal = Vector3.zero;
            bool hitWall = false;

            // Determine which wall(s) we hit and calculate the normal
            if (pos.x < -extents.x || pos.x > extents.x)
            {
                wallNormal.x = pos.x < 0 ? 1 : -1; // Normal points inward
                hitWall = true;
            }
            if (pos.y < -extents.y || pos.y > extents.y)
            {
                wallNormal.y = pos.y < 0 ? 1 : -1; // Normal points inward
                hitWall = true;
            }
            if (pos.z < -extents.z || pos.z > extents.z)
            {
                wallNormal.z = pos.z < 0 ? 1 : -1; // Normal points inward
                hitWall = true;
            }

            if (hitWall)
            {
                // Normalize the wall normal (important for corner bounces)
                wallNormal = wallNormal.normalized;

                // Physics-accurate reflection
                velocity = Vector3.Reflect(velocity, wallNormal);

                // Add slight randomization to prevent perfect bouncing patterns
                Vector3 randomization = Random.insideUnitSphere * bounceRandomization;
                randomization = Vector3.ProjectOnPlane(randomization, wallNormal); // Keep randomization parallel to wall
                velocity += randomization;

            }

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
        int hitZonePoints = Mathf.RoundToInt(basePoints * multiplier);

        // Use new method that applies combo multiplier to the hit zone points
        GameManager.Instance.AddPointsWithCombo(hitZonePoints);

        // Trigger camera shake using the Singleton instance
        if (CameraShake.Instance != null) // Accessing the Singleton instance
        {
            CameraShake.Instance.StartShake(); // Calling the correct method name
        }
        else
        {
            Debug.LogWarning("CameraShake.Instance is null. Cannot shake camera. Make sure CameraShake script is on Main Camera and correctly set up as a Singleton.");
        }

        // Instantiate the hit explosion prefab
        if (HitExplosion != null)
        {
            Instantiate(HitExplosion, transform.position, transform.rotation);
        }

        foreach (var effect in PowerUpManager.Instance.deathEffects)
        {
            effect.OnTargetKilled(this, transform.position);
        }

        // Notify SpawnManager that target was destroyed
        SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.OnTargetDestroyed();
        }

        Destroy(gameObject);
    }

    public void ApplySlow(float multiplier)
    {
        velocity *= multiplier;
    }

    // TODO: Implement glitter trail
    // public void EnableGlitterTrail()
    // {
    //     // Optional: if you have a trail or particle system, enable it here
    //     Debug.Log("✨ Glitter trail enabled! (Visual not implemented yet)");
    // }

    void Explode()
    {
        GameManager.Instance.RemoveLife();

        // Notify SpawnManager that target was destroyed
        SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.OnTargetDestroyed();
        }

        Destroy(gameObject);
    }
}