using UnityEngine;

public class TargetMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveRange = 3f;

    private Vector3 startPos;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;
        direction = Random.Range(0, 2) == 0 ? -1 : 1; // Random left or right
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        // If moved too far from start, reverse direction
        if (Mathf.Abs(transform.position.x - startPos.x) > moveRange)
        {
            direction *= -1;
        }
    }
}
