using UnityEngine;
using System.Collections; // Required for Coroutines

public class CameraShake : MonoBehaviour // <--- Class name is now CameraShake
{
    // Singleton pattern
    public static CameraShake Instance; // <--- Instance type is now CameraShake

    [Header("Shake Settings")]
    public float defaultShakeDuration = 0.15f;    // How long the shake lasts by default
    public float defaultShakeMagnitude = 0.1f;    // How intense the shake is by default
    public float defaultDampingSpeed = 1.0f;      // How quickly the shake fades out by default (higher = faster fade)

    private Vector3 initialLocalPosition; // Stores the camera's original local position
    private bool isShaking = false;       // Flag to prevent multiple concurrent shakes

    private void Awake()
    {
        // Implement the Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // Ensure this GameObject is not destroyed when loading new scenes if you want global access
            // DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            // If another instance already exists, destroy this one
            Debug.LogWarning("Duplicate CameraShake instance found, destroying this one.", this);
            Destroy(gameObject);
            return; // Exit to prevent further execution for this duplicate
        }

        // Store the camera's initial local position immediately upon awake.
        // This is the "home" position the camera should always return to.
        initialLocalPosition = transform.localPosition;
    }

    private void OnDisable()
    {
        // When the camera or script is disabled, stop any shake and reset position.
        StopAllCoroutines();
        if (transform.localPosition != initialLocalPosition)
        {
            transform.localPosition = initialLocalPosition;
        }
        isShaking = false;
    }

    /// <summary>
    /// Initiates a camera shake with the default settings.
    /// </summary>
    public void StartShake()
    {
        StartShake(defaultShakeDuration, defaultShakeMagnitude, defaultDampingSpeed);
    }

    /// <summary>
    /// Initiates a camera shake with custom duration, magnitude, and damping speed.
    /// </summary>
    /// <param name="duration">How long the shake should last.</param>
    /// <param name="magnitude">How intense the shake should be.</param>
    /// <param name="dampingSpeed">How quickly the shake fades out.</param>
    public void StartShake(float duration, float magnitude, float dampingSpeed)
    {
        if (isShaking)
        {
            // If already shaking, stop the current shake before starting a new one
            // This prevents overlapping and maintains control.
            StopAllCoroutines();
            transform.localPosition = initialLocalPosition; // Snap back before new shake
            isShaking = false; // Reset flag
        }

        // Start the shake coroutine
        StartCoroutine(ShakeEffectCoroutine(duration, magnitude, dampingSpeed));
    }

    /// <summary>
    /// The coroutine that handles the actual camera shaking effect.
    /// </summary>
    private IEnumerator ShakeEffectCoroutine(float duration, float magnitude, float dampingSpeed)
    {
        isShaking = true; // Set the flag that a shake is active

        float elapsed = 0f; // Time elapsed during the shake

        // Loop for the duration of the shake
        while (elapsed < duration)
        {
            // Calculate a random offset for the shake
            // Random.insideUnitSphere generates a random point inside a sphere with radius 1
            Vector3 randomOffset = Random.insideUnitSphere * magnitude;

            // Apply the shake relative to the initial local position
            transform.localPosition = initialLocalPosition + randomOffset;

            // Increase elapsed time and gradually reduce magnitude (damping)
            elapsed += Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Ensure the camera snaps back to its exact initial local position when the shake is done
        transform.localPosition = initialLocalPosition;

        isShaking = false; // Reset the flag
    }

    /// <summary>
    /// Stops any active camera shake and resets the camera's local position.
    /// </summary>
    public void StopShake()
    {
        StopAllCoroutines();
        if (transform.localPosition != initialLocalPosition)
        {
            transform.localPosition = initialLocalPosition;
        }
        isShaking = false;
    }
}