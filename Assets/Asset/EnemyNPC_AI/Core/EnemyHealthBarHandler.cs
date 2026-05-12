 using UnityEngine;
using UnityEngine.UI;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{

public class EnemyHealthBarHandler : MonoBehaviour
{
    public GameObject healthBarPrefab;
    private Slider healthSlider;
    private Transform cam;

    private EnemyHealth enemyHealth;
    private Transform barTransform;

    private void Start()
    {
        cam = Camera.main.transform;

        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null) return;

        GameObject bar = Instantiate(healthBarPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
        barTransform = bar.transform;
        healthSlider = bar.GetComponentInChildren<Slider>();
        bar.transform.SetParent(null); // Keep it in world space
    }

    private void Update()
    {
        if (healthSlider == null || enemyHealth == null || barTransform == null)
            return;

        healthSlider.value = enemyHealth.CurrentHealthNormalized();
        barTransform.position = transform.position + Vector3.up * 2f;
        barTransform.LookAt(barTransform.position + cam.forward); // Face camera
    }
}
}
