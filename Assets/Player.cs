using UnityEngine;

public class Player : MonoBehaviour
{
    void Update()
{
    if (Input.GetMouseButtonDown(0)) // Left-click
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Target"))
            {
                // Hit a duck 🎯
                GameManager.Instance.AddCombo();

                // Trigger any combo-based power-up effects
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
                // Hit something else = miss
                GameManager.Instance.ResetCombo();
            }
        }
        else
        {
            // Nothing hit = miss
            GameManager.Instance.ResetCombo();
        }
    }
}
}