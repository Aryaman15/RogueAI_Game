using UnityEngine;
using UnityEngine.UI;

namespace RogueAI.Level
{
    public class HardwareHudController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text powerModuleStatus;
        [SerializeField] private Text authorizationChipStatus;
        [SerializeField] private Text overrideModuleStatus;

        public void Configure(GameObject rootObject, Text powerModule, Text authorizationChip, Text overrideModule)
        {
            root = rootObject;
            powerModuleStatus = powerModule;
            authorizationChipStatus = authorizationChip;
            overrideModuleStatus = overrideModule;
            SetInitialState();
        }

        private void Awake()
        {
            SetInitialState();
        }

        public void SetInitialState()
        {
            if (root)
            {
                root.SetActive(true);
            }

            SetText(powerModuleStatus, "NOT ACQUIRED", new Color(1f, 0.68f, 0.3f));
            SetText(authorizationChipStatus, "LOCKED", new Color(0.75f, 0.78f, 0.82f));
            SetText(overrideModuleStatus, "LOCKED", new Color(0.75f, 0.78f, 0.82f));
        }

        public void SetPowerModuleAcquired()
        {
            SetText(powerModuleStatus, "ACQUIRED", new Color(0.35f, 1f, 0.55f));
        }

        private static void SetText(Text label, string value, Color color)
        {
            if (!label)
            {
                return;
            }

            label.text = value;
            label.color = color;
        }
    }
}
