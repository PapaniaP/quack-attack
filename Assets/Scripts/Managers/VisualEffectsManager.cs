using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CartoonFX;  // Add CFXR namespace

public enum AoEType
{
    General,        // General/fallback AoE effects
    Combo,          // Combo explosion effects
    Quacksplosive   // Quacksplosive Tendencies effect
}

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Instance;

    [Header("Screen Tint Overlay")]
    public Image screenOverlay; // Assign a fullscreen UI Image in the Inspector
    public Color quakeTintColor = new Color(1f, 1f, 1f, 0.7f); // whitish color
    private Coroutine tintRoutine;

    [Header("Damage Effect")]
    public Q_Vignette_Split damageVignette;  // Reference to the Q_Vignette component
    public float damageEffectDuration = 0.5f;  // Reduced from 0.8f for snappier effect
    public Color damageColor = new Color(1f, 0f, 0f, 1f);  // Red with FULL opacity
    public float minVignetteScale = 0.8f;  // Starting scale (subtle but visible)
    public float maxVignetteScale = 2.0f;  // Increased from 1.5f for more dramatic scale
    private Color originalMainColor;
    private Color originalSkyColor;
    private float originalMainScale;
    private float originalSkyScale;

    [Header("Camera Shake (CFXR)")]
    public CFXR_Effect.CameraShake cameraShake = new CFXR_Effect.CameraShake();

    [Header("AoE Explosion Prefabs")]
    [SerializeField] private GameObject generalAoEPrefab;           // For general/fallback AoE effects
    [SerializeField] private GameObject comboExplosionPrefab;       // For combo explosions (bigger/flashier)
    [SerializeField] private GameObject quacksplosiveExplosionPrefab; // For Quacksplosive Tendencies effect
    [SerializeField] private float explosionLifetime = 3f;         // How long explosions stay in scene

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

    // 👇 NEW PREFAB-BASED AOE SYSTEM
    public void TriggerAOE(Vector3 position, float radius = 1f, AoEType explosionType = AoEType.General)
    {
        Debug.Log($"[VFX] Triggering {explosionType} AoE at {position} with radius {radius}");

        GameObject prefabToSpawn = GetExplosionPrefab(explosionType);

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"[VFX] No prefab assigned for explosion type: {explosionType}. Using fallback.");
            CreateFallbackExplosion(position, radius);
            return;
        }

        // Instantiate the explosion prefab
        GameObject explosion = Instantiate(prefabToSpawn, position, Quaternion.identity);

        // Scale the explosion based on radius
        float scale = Mathf.Max(0.5f, radius / 2f); // Minimum scale of 0.5x, scale roughly based on radius
        explosion.transform.localScale = Vector3.one * scale;

        // Auto-destroy after lifetime (if it doesn't destroy itself)
        if (explosionLifetime > 0)
        {
            Destroy(explosion, explosionLifetime);
        }
    }

    // Overloaded method for backward compatibility (no explosion type specified)
    public void TriggerAOE(Vector3 position, float radius = 1f)
    {
        TriggerAOE(position, radius, AoEType.General);
    }

    // Method for specific explosion types
    public void TriggerComboExplosion(Vector3 position, float radius = 5f)
    {
        TriggerAOE(position, radius, AoEType.Combo);
    }

    public void TriggerQuacksplosion(Vector3 position, float radius = 2f)
    {
        TriggerAOE(position, radius, AoEType.Quacksplosive);
    }

    private GameObject GetExplosionPrefab(AoEType explosionType)
    {
        return explosionType switch
        {
            AoEType.Combo => comboExplosionPrefab ?? generalAoEPrefab,
            AoEType.Quacksplosive => quacksplosiveExplosionPrefab ?? generalAoEPrefab,
            AoEType.General => generalAoEPrefab,
            _ => generalAoEPrefab
        };
    }

    // Fallback for when no prefab is assigned (keeps old sphere behavior)
    private void CreateFallbackExplosion(Vector3 position, float radius)
    {
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.transform.position = position;
        marker.transform.localScale = Vector3.one * radius * 2f;

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

        // Phase 1: HARSH flash in (10% of total time - much faster!)
        float flashInTime = damageEffectDuration * 0.1f;

        while (elapsedTime < flashInTime)
        {
            // Use easing for more dramatic flash-in effect
            float t = elapsedTime / flashInTime;
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic for sharp start

            // Animate color (fade in the red)
            Color currentMainColor = Color.Lerp(invisibleMainColor, damageColor, easedT);
            damageVignette.mainColor = currentMainColor;
            Color currentSkyColor = Color.Lerp(invisibleSkyColor, damageColor, easedT);
            damageVignette.skyColor = currentSkyColor;

            // Animate scale (grow the vignette dramatically)
            float currentMainScale = Mathf.Lerp(0f, maxVignetteScale, easedT);
            damageVignette.mainScale = currentMainScale;
            float currentSkyScale = Mathf.Lerp(0f, maxVignetteScale, easedT);
            damageVignette.skyScale = currentSkyScale;

            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time in case game is paused
            yield return null;
        }

        // Phase 2: Very brief hold (5% of total time - minimal hold for harsh effect)
        yield return new WaitForSecondsRealtime(damageEffectDuration * 0.05f);

        // Phase 3: Fast fade out (85% of total time)
        float fadeOutTime = damageEffectDuration * 0.85f;
        elapsedTime = 0f;

        while (elapsedTime < fadeOutTime)
        {
            float t = elapsedTime / fadeOutTime;
            // Use ease-in for sharper fade-out
            float easedT = Mathf.Pow(t, 2f); // Ease in quadratic for sharp end

            // Animate color (fade out the red)
            Color currentMainColor = Color.Lerp(damageColor, invisibleMainColor, easedT);
            damageVignette.mainColor = currentMainColor;
            Color currentSkyColor = Color.Lerp(damageColor, invisibleSkyColor, easedT);
            damageVignette.skyColor = currentSkyColor;

            // Animate scale (shrink back to invisible)
            float currentMainScale = Mathf.Lerp(maxVignetteScale, 0f, easedT);
            damageVignette.mainScale = currentMainScale;
            float currentSkyScale = Mathf.Lerp(maxVignetteScale, 0f, easedT);
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