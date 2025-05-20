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

                // Determine hit zone by tag
                if (hit.collider.CompareTag("DuckBeak"))
                {
                    scoreMultiplier = 3f;
                    Debug.Log("🟧 BEAK SHOT! Triple points!");
                }
                else if (hit.collider.CompareTag("DuckHead"))
                {
                    scoreMultiplier = 2f;
                    Debug.Log("🟨 HEADSHOT! Double points!");
                }
                else if (hit.collider.CompareTag("DuckBody"))
                {
                    scoreMultiplier = 1f;
                    Debug.Log("🟩 Body shot");
                }
                else
                {
                    GameManager.Instance.ResetCombo();
                    Debug.Log("Missed, reset combo.");
                    return;
                }

                GameManager.Instance.AddCombo();

                foreach (var powerUp in PowerUpManager.Instance.activePowerUps)
                {
                    if (powerUp.effect is IComboEffect comboEffect)
                    {
                        comboEffect.OnComboReached(this, GameManager.Instance.combo, hit.collider.transform.position);
                    }
                }

                Target target = hit.collider.GetComponentInParent<Target>();
                if (target != null)
                {
                    target.Hit(scoreMultiplier);
                }
            }
            else
            {
                GameManager.Instance.ResetCombo();
                Debug.Log("Missed everything.");
            }
        }
    }
}