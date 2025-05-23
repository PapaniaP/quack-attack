using UnityEngine;
using UnityEngine.UI;

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Instance;

    [Header("Screen Tint Overlay")]
    public Image screenOverlay; // Assign a fullscreen UI Image in the Inspector
    public Color quakeTintColor = new Color(1f, 0.3f, 0.9f, 0.7f); // carnival purple-pink
    private Coroutine tintRoutine;

    [Header("Camera Shake")]
    public Transform cameraTransform;
    private Vector3 originalCamPos;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (cameraTransform != null)
            originalCamPos = cameraTransform.localPosition;
    }

    // 👇 SCREEN FLASH (quick white fade)
    public void ScreenFlash()
    {
        if (screenOverlay == null) return;

        screenOverlay.color = Color.white;
        screenOverlay.canvasRenderer.SetAlpha(1f);
        screenOverlay.CrossFadeAlpha(0f, 0.4f, false);
    }

    // 👇 TINT FADE OUT
    public void ScreenTintFadeOut(float duration)
    {
        if (screenOverlay == null) return;

        if (tintRoutine != null)
            StopCoroutine(tintRoutine);
        tintRoutine = StartCoroutine(FadeTintRoutine(duration));
    }

    private System.Collections.IEnumerator FadeTintRoutine(float duration)
    {
        float time = 0f;
        screenOverlay.color = quakeTintColor;
        screenOverlay.canvasRenderer.SetAlpha(1f);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = 1f - (time / duration);
            screenOverlay.canvasRenderer.SetAlpha(t);
            yield return null;
        }

        screenOverlay.canvasRenderer.SetAlpha(0f);
    }

    // 👇 CAMERA SHAKE
    public void CameraShake(float intensity = 0.3f, float duration = 0.5f)
    {
        if (cameraTransform == null) return;
        StartCoroutine(ShakeCamera(intensity, duration));
    }

    private System.Collections.IEnumerator ShakeCamera(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cameraTransform.localPosition = originalCamPos + Random.insideUnitSphere * intensity;
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalCamPos;
    }

    // 👇 EXISTING AOE MARKER FUNCTION (unchanged)
    public void TriggerAOE(Vector3 position, float radius = 1f)
    {
        Debug.Log($"[VFX] Triggering AoE at {position} with radius {radius}");

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;

        // Scale the sphere to reflect the AoE radius (diameter = radius * 2)
        marker.transform.localScale = Vector3.one * radius * 2f;

        // Optional: Change color or make it transparent
        Renderer rend = marker.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = new Color(1f, 0f, 0f, 0.3f); // red with transparency
        }

        Destroy(marker, 1.5f);
    }
}