using UnityEngine;
using UnityEngine.UI;
using SuperHorizon.EnemyNPCai;

namespace SuperHorizon.EnemyNPCai
{
    public class UIHealthBar : MonoBehaviour
    {
        public static UIHealthBar Instance;

        [SerializeField] private Slider slider;

        private void Awake()
        {
            Instance = this;
        }

        public void SetHealth(int current, int max)
        {
            slider.value = (float)current / max;
        }
    }
}