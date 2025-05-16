using UnityEngine;

public class StationaryPlayer : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerCamera;

    float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleShoot();
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
                if (hit.collider.CompareTag("Target"))
                {
                    Debug.Log("🎯 Hit: " + hit.collider.name);
                    GameManager.Instance.AddCombo();

                    foreach (var powerUp in PowerUpManager.Instance.activePowerUps)
                    {
                        if (powerUp.effect is IComboEffect comboEffect)
                        {
                            comboEffect.OnComboReached(this, GameManager.Instance.combo, hit.collider.transform.position);
                        }
                    }

                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    GameManager.Instance.ResetCombo();
                    Debug.Log("Missed something else.");
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
