using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject targetPrefab;
    public GameObject boundsObject;

    [Header("Adaptive Spawning")]
    public float emergencySpawnIntervalMin = 0.1f;
    public float emergencySpawnIntervalMax = 0.3f;
    public int earlyGameMinTargets = 3;
    public int lateGameMinTargets = 5;
    public int earlyGameMaxTargets = 8; // TODO: Make this dynamic based on difficulty
    public int lateGameMaxTargets = 15;

    [Header("Spawn Intervals")]
    public float earlyGameBaseInterval = 1.5f;
    public float lateGameBaseInterval = 0.7f;
    public float slowSpawnMultiplier = 2.0f;

    private float spawnTimer;
    private BoxCollider bounds;
    private int currentTargetCount = 0; // Performance optimization: track internally

    void Start()
    {
        if (boundsObject != null)
        {
            bounds = boundsObject.GetComponent<BoxCollider>();
        }
        else
        {
            Debug.LogError("SpawnManager: boundsObject not assigned.");
        }

        // Spawn initial targets
        int initialCount = GetMinTargetCount();
        for (int i = 0; i < initialCount; i++)
        {
            SpawnTarget();
        }
    }

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;
        if (bounds == null) return;

        spawnTimer += Time.deltaTime;
        float currentSpawnInterval = GetCurrentSpawnInterval();

        if (spawnTimer >= currentSpawnInterval)
        {
            int maxTargets = GetMaxTargetCount();

            if (currentTargetCount < maxTargets)
            {
                SpawnTarget();
            }

            spawnTimer = 0f;
        }
    }

    private float GetCurrentSpawnInterval()
    {
        int minTargets = GetMinTargetCount();

        // Emergency mode: spawn rapidly when below minimum
        if (currentTargetCount < minTargets)
        {
            // Burst prevention: randomize emergency intervals
            return Random.Range(emergencySpawnIntervalMin, emergencySpawnIntervalMax);
        }

        // Adaptive intervals based on current target count
        float difficulty = GameManager.Instance.RunProgress;
        float baseInterval = Mathf.SmoothStep(earlyGameBaseInterval, lateGameBaseInterval, difficulty);

        int maxTargets = GetMaxTargetCount();
        float targetRatio = (float)currentTargetCount / maxTargets;

        // Scale interval based on how full the screen is
        if (targetRatio < 0.4f) // Less than 40% full
            return baseInterval * 0.7f; // Spawn faster
        else if (targetRatio < 0.7f) // 40-70% full
            return baseInterval; // Normal rate
        else // More than 70% full
            return baseInterval * slowSpawnMultiplier; // Spawn slower
    }

    private int GetMinTargetCount()
    {
        float difficulty = GameManager.Instance.RunProgress;
        return Mathf.RoundToInt(Mathf.SmoothStep(earlyGameMinTargets, lateGameMinTargets, difficulty));
    }

    private int GetMaxTargetCount()
    {
        float difficulty = GameManager.Instance.RunProgress;
        return Mathf.RoundToInt(Mathf.SmoothStep(earlyGameMaxTargets, lateGameMaxTargets, difficulty));
    }

    void SpawnTarget()
    {
        Vector3 center = bounds.bounds.center;
        Vector3 extents = bounds.bounds.extents;

        Vector3 spawnPos = center + new Vector3(
            Random.Range(-extents.x, extents.x),
            Random.Range(-extents.y, extents.y),
            Random.Range(0f, extents.z) // only positive Z
        );

        GameObject target = Instantiate(targetPrefab, spawnPos, Quaternion.identity);

        // Performance optimization: increment counter instead of searching
        currentTargetCount++;

        // Inject bounds into target
        Target targetScript = target.GetComponent<Target>();
        if (targetScript != null)
        {
            targetScript.boundsObject = boundsObject;

            // Apply spawn effects (like Duck Dilation)
            foreach (var effect in PowerUpManager.Instance.spawnEffects)
            {
                effect.OnTargetSpawned(targetScript);
            }
        }
    }

    // Call this when a target is destroyed
    public void OnTargetDestroyed()
    {
        currentTargetCount = Mathf.Max(0, currentTargetCount - 1);
    }

    public void ResetAndSpawn()
    {
        // Clear existing targets
        GameObject[] existingTargets = GameObject.FindGameObjectsWithTag("Target");
        foreach (GameObject target in existingTargets)
        {
            Destroy(target);
        }

        // Reset counters
        currentTargetCount = 0;
        spawnTimer = 0f;

        // Spawn initial targets
        int initialCount = GetMinTargetCount();
        for (int i = 0; i < initialCount; i++)
        {
            SpawnTarget();
        }
    }
}