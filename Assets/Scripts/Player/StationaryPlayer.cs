using UnityEngine;

public class StationaryPlayer : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    float xRotation = 0f;

    void Start()
    {
        // Remove cursor lock from here - GameManager will handle it
        transform.rotation = Quaternion.Euler(0f, 90f, 0f);
    }

    void Update()
    {
        // Only handle mouse look and shooting when game is active
        if (GameManager.Instance.isGameActive)
        {
            HandleMouseLook();
            HandleShoot();
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleShoot()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            Ray ray = playerCamera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                float scoreMultiplier = 1f;
                bool hitDuck = false;

                // Determine hit zone by tag
                if (hit.collider.CompareTag("DuckBeak"))
                {
                    scoreMultiplier = 3f;
                    Debug.Log("🟧 BEAK SHOT! Triple points!");
                    hitDuck = true;
                }
                else if (hit.collider.CompareTag("DuckHead"))
                {
                    scoreMultiplier = 2f;
                    Debug.Log("🟨 HEADSHOT! Double points!");
                    hitDuck = true;
                }
                else if (hit.collider.CompareTag("DuckBody"))
                {
                    scoreMultiplier = 1f;
                    Debug.Log("🟩 Body shot");
                    hitDuck = true;
                }
                else
                {
                    bool forgiven = false;

                    foreach (var effect in PowerUpManager.Instance.missForgivenessEffects)
                    {
                        if (effect.OnMiss())
                        {
                            forgiven = true;
                            Debug.Log("[OopsieShield] Miss was forgiven.");
                            break;
                        }
                    }

                    if (!forgiven)
                    {
                        GameManager.Instance.ResetCombo();
                        Debug.Log("Missed, reset combo.");
                    }

                    return;
                }

                if (hitDuck)
                {
                    // Play shoot SFX only when a duck is hit
                    GameManager.Instance.PlayShootSFX();

                    GameManager.Instance.AddCombo();

                    PowerUpManager.Instance.TriggerComboEffects(hit.collider.transform.position, GameManager.Instance.combo);

                    Target target = hit.collider.GetComponentInParent<Target>();
                    if (target != null)
                    {
                        target.Hit(scoreMultiplier);
                    }
                }
            }
            else
            {
                GameManager.Instance.ResetCombo();
                Debug.Log("Missed everything.");
                // (You can add miss SFX here later if you want)
            }
        }
    }
}
