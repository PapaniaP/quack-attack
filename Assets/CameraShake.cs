using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public float shakeDuration = 0.15f;   // How long the shake lasts
    public float shakeMagnitude = 0.1f;   // How intense the shake is
    public float dampingSpeed = 1.0f;     // How quickly the shake fades out

    private Vector3 initialPosition;
    private float currentShakeTime = 0f;

    void Update()
    {
        if (currentShakeTime > 0)
        {
            transform.position = initialPosition + Random.insideUnitSphere * shakeMagnitude;
            currentShakeTime -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            currentShakeTime = 0f;
            transform.position = initialPosition;
        }
    }

    public void Shake()
    {
        initialPosition = transform.position;
        currentShakeTime = shakeDuration;
    }

    // Optional: You could add an overload to control duration and magnitude per shake
    public void Shake(float duration, float magnitude)
    {
        initialPosition = transform.position;
        currentShakeTime = duration;
        shakeMagnitude = magnitude;
    }
}