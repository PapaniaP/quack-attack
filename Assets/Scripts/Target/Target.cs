using UnityEngine;

public class Target : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float lifetime = 5f;
    public float colorChangeThreshold = 1.5f;

    public GameObject boundsObject;
    private BoxCollider bounds;

    private Vector3 velocity;
    private float timer;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;

        float difficulty = GameManager.Instance != null ? GameManager.Instance.RunProgress : 0f;
        float adjustedSpeed = Mathf.Lerp(1f, moveSpeed, difficulty); // starts at 1, ramps to moveSpeed
        velocity = Random.onUnitSphere * adjustedSpeed;

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

        // LSD-style redirect on bounds exit
        if (bounds != null && !bounds.bounds.Contains(transform.position))
        {
            velocity = Random.onUnitSphere * moveSpeed;

            // Push the duck back inside the bounds slightly
            Vector3 center = bounds.bounds.center;
            Vector3 extents = bounds.bounds.extents;

            transform.position = center + new Vector3(
                Mathf.Clamp(transform.position.x - center.x, -extents.x * 0.95f, extents.x * 0.95f),
                Mathf.Clamp(transform.position.y - center.y, -extents.y * 0.95f, extents.y * 0.95f),
                Mathf.Clamp(transform.position.z - center.z, -extents.z * 0.95f, extents.z * 0.95f)
            );
        }

        // Timer and color change
        timer += Time.deltaTime;
        float timeLeft = lifetime - timer;

        if (timeLeft < colorChangeThreshold)
        {
            float t = 1f - (timeLeft / colorChangeThreshold);
            rend.material.color = Color.Lerp(originalColor, Color.red, t);
        }

        if (timer >= lifetime)
        {
            Explode();
        }
    }

    public void Hit()
    {
        Destroy(gameObject);
    }

    void Explode()
    {
        Debug.Log("💥 Duck exploded!");
        // GameManager.Instance.RemoveLife();
        Destroy(gameObject);
    }
}