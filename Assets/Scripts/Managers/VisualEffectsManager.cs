using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CartoonFX;  // Add CFXR namespace

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Instance;

    [Header("Screen Tint Overlay")]
    public Image screenOverlay; // Assign a fullscreen UI Image in the Inspector
    public Color quakeTintColor = new Color(1f, 1f, 1f, 0.7f); // whitish color
    private Coroutine tintRoutine;

    [Header("Damage Effect")]
    public Q_Vignette_Split damageVignette;  // Reference to the Q_Vignette component
    public float damageEffectDuration = 0.8f;
    public Color damageColor = new Color(1f, 0f, 0f, 1f);  // Red with FULL opacity
    public float minVignetteScale = 0.8f;  // Starting scale (subtle but visible)
    public float maxVignetteScale = 1.5f;  // Maximum dramatic scale
    private Color originalMainColor;
    private Color originalSkyColor;
    private float originalMainScale;
    private float originalSkyScale;

    [Header("Camera Shake (CFXR)")]
    public CFXR_Effect.CameraShake cameraShake = new CFXR_Effect.CameraShake();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Store original vignette settings
        if (damageVignette != null)
        {
            originalMainColor = damageVignette.mainColor;
            originalSkyColor = damageVignette.skyColor;
            originalMainScale = damageVignette.mainScale;
            originalSkyScale = damageVignette.skyScale;
        }

        // Configure CFXR camera shake for damage effect
        if (cameraShake != null)
        {
            cameraShake.enabled = true;
            cameraShake.useMainCamera = true;
            cameraShake.duration = 0.3f;
            cameraShake.shakeStrength = new Vector3(0.4f, 0.4f, 0.1f);
            cameraShake.shakeSpace = CFXR_Effect.CameraShake.ShakeSpace.Screen;
            cameraShake.fetchCameras();
        }
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
        if (cameraShake == null) return;

        // Configure the shake parameters dynamically
        cameraShake.duration = duration;
        cameraShake.shakeStrength = new Vector3(intensity, intensity, intensity * 0.5f);

        // Start the shake - CFXR will handle the animation automatically
        cameraShake.StartShake();

        // Start a coroutine to handle the CFXR animation
        StartCoroutine(HandleCFXRShake(duration));
    }

    private System.Collections.IEnumerator HandleCFXRShake(float duration)
    {
        // CFXR handles the shake animation internally, but we need to call animate()
        float elapsed = 0f;
        while (elapsed < duration)
        {
            cameraShake.animate(elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraShake.StopShake();
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

    // 👇 Generic VFX spawning
    public void PlayEffect(GameObject vfxPrefab, Vector3 position)
    {
        if (vfxPrefab == null) return;
        Instantiate(vfxPrefab, position, Quaternion.identity);
    }

    // 👇 DAMAGE EFFECT (when life is lost)
    public void PlayDamageEffect()
    {
        // Play damage sound via AudioManager (proper separation of concerns)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDamageSFX();
        }

        // Trigger vignette animation
        if (damageVignette != null)
        {
            StartCoroutine(AnimateDamageVignette());
        }

        // Use CFXR camera shake system (professional and smooth)
        CameraShake(0.4f, 0.3f);
    }

    private System.Collections.IEnumerator AnimateDamageVignette()
    {
        // Force vignette to invisible state before starting animation
        Color invisibleMainColor = new Color(originalMainColor.r, originalMainColor.g, originalMainColor.b, 0f);
        Color invisibleSkyColor = new Color(originalSkyColor.r, originalSkyColor.g, originalSkyColor.b, 0f);
        damageVignette.mainColor = invisibleMainColor;
        damageVignette.skyColor = invisibleSkyColor;
        damageVignette.mainScale = 0f;  // Start completely invisible
        damageVignette.skyScale = 0f;

        float elapsedTime = 0f;

        // Phase 1: Quick flash in (20% of total time)
        float flashInTime = damageEffectDuration * 0.2f;

        while (elapsedTime < flashInTime)
        {
            float t = elapsedTime / flashInTime;

            // Animate color (fade in the red)
            Color currentMainColor = Color.Lerp(invisibleMainColor, damageColor, t);
            damageVignette.mainColor = currentMainColor;
            Color currentSkyColor = Color.Lerp(invisibleSkyColor, damageColor, t);
            damageVignette.skyColor = currentSkyColor;

            // Animate scale (grow the vignette dramatically)
            float currentMainScale = Mathf.Lerp(0f, maxVignetteScale, t);
            damageVignette.mainScale = currentMainScale;
            float currentSkyScale = Mathf.Lerp(0f, maxVignetteScale, t);
            damageVignette.skyScale = currentSkyScale;

            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            yield return null;
        }

        // Phase 2: Brief hold (10% of total time)
        yield return new WaitForSecondsRealtime(damageEffectDuration * 0.1f);

        // Phase 3: Smooth fade out (70% of total time)
        float fadeOutTime = damageEffectDuration * 0.7f;
        elapsedTime = 0f;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;

            // Animate color (fade out the red)
            Color currentMainColor = Color.Lerp(damageColor, invisibleMainColor, t);
            damageVignette.mainColor = currentMainColor;
            Color currentSkyColor = Color.Lerp(damageColor, invisibleSkyColor, t);
            damageVignette.skyColor = currentSkyColor;

            // Animate scale (shrink back to invisible)
            float currentMainScale = Mathf.Lerp(maxVignetteScale, 0f, t);
            damageVignette.mainScale = currentMainScale;
            float currentSkyScale = Mathf.Lerp(maxVignetteScale, 0f, t);
            damageVignette.skyScale = currentSkyScale;

            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Ensure we're back to invisible state
        damageVignette.mainColor = invisibleMainColor;
        damageVignette.skyColor = invisibleSkyColor;
        damageVignette.mainScale = 0f;
        damageVignette.skyScale = 0f;
    }
}