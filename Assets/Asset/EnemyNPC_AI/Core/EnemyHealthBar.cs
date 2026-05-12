using UnityEngine;
using UnityEngine.UI;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    public Transform target;            // Enemy to follow
    public Vector3 offset = new Vector3(0, 2.2f, 0); // Offset above the enemy

    [Header("UI")]
    public Slider healthSlider;         // Assign in prefab

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;

            // Make the health bar face the camera
            if (cam != null)
                transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
        }
    }

    public void SetHealth(float normalizedHealth)
    {
        if (healthSlider != null)
            healthSlider.value = Mathf.Clamp01(normalizedHealth);
    }
}
}