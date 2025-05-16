using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Target Settings")]
    public GameObject targetPrefab;
    public GameObject boundsObject;

    [Header("Spawning Settings")]
    public float spawnInterval = 2f;
    public int minTargets = 3;
    public int maxTargets = 20;

    private float spawnTimer;
    private BoxCollider bounds;

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
    }

    void Update()
    {
        if (!GameManager.Instance.isGameActive) return;
        if (bounds == null) return;

        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            int currentTargetCount = GameObject.FindGameObjectsWithTag("Target").Length;
            int allowedCount = GetAllowedTargetCount();

            if (currentTargetCount < allowedCount)
            {
                SpawnTarget();
            }

            spawnTimer = 0f;
        }
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

        // Inject bounds into target
        Target targetScript = target.GetComponent<Target>();
        if (targetScript != null)
        {
            targetScript.boundsObject = boundsObject;
        }
    }

    int GetAllowedTargetCount()
    {
        float t = GameManager.Instance.RunProgress; // 0 to 1 over time
        return Mathf.RoundToInt(Mathf.Lerp(minTargets, maxTargets, t));
    }
}