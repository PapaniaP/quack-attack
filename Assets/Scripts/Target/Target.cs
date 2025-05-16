using UnityEngine;

public class Target : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float lifetime = 5f;
    public float colorChangeThreshold = 1.5f;

    public BoxCollider bounds; // Assign via Inspector

    private Vector3 velocity;
    private float timer;
    private Renderer rend;
    private Color originalColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;

        velocity = Random.onUnitSphere * moveSpeed;

        // Optional: random start position inside bounds
        if (bounds != null)
        {
            Vector3 center = bounds.transform.position;
            Vector3 size = bounds.size * 0.5f;
            transform.position = center + new Vector3(
                Random.Range(-size.x, size.x),
                Random.Range(-size.y, size.y),
                Random.Range(-size.z, size.z)
            );
        }
    }

    void Update()
    {
        transform.position += velocity * Time.deltaTime;

        // Bounce off bounds
        if (bounds != null && !bounds.bounds.Contains(transform.position))
        {
            Vector3 pos = transform.position;
            Vector3 min = bounds.bounds.min;
            Vector3 max = bounds.bounds.max;

            if (pos.x < min.x || pos.x > max.x) velocity.x *= -1;
            if (pos.y < min.y || pos.y > max.y) velocity.y *= -1;
            if (pos.z < min.z || pos.z > max.z) velocity.z *= -1;
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