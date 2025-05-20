using UnityEngine;

public class DestroyAfterEffect : MonoBehaviour
{
    public float lifetime = 1.0f; // Adjust this to match the duration of your smoke effect

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
}