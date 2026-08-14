using System.Collections;
using UnityEngine;

namespace RogueAI.Level
{
    public class Level1VisualStateController : MonoBehaviour
    {
        [SerializeField] private Renderer[] emergencyRenderers;
        [SerializeField] private Renderer[] poweredRenderers;
        [SerializeField] private Renderer[] securityStatusRenderers;
        [SerializeField] private Material emergencyMaterial;
        [SerializeField] private Material poweredMaterial;
        [SerializeField] private Material lockedMaterial;
        [SerializeField] private Material unlockedMaterial;
        [SerializeField] private float transitionSeconds = 1.2f;

        public void Configure(
            Renderer[] emergency,
            Renderer[] powered,
            Renderer[] securityStatus,
            Material emergencyStateMaterial,
            Material poweredStateMaterial,
            Material locked,
            Material unlocked)
        {
            emergencyRenderers = emergency;
            poweredRenderers = powered;
            securityStatusRenderers = securityStatus;
            emergencyMaterial = emergencyStateMaterial;
            poweredMaterial = poweredStateMaterial;
            lockedMaterial = locked;
            unlockedMaterial = unlocked;
        }

        private void Awake()
        {
            ApplyPowerOffState();
        }

        public void ApplyPowerOffState()
        {
            SetRenderersEnabled(emergencyRenderers, true);
            SetRenderersEnabled(poweredRenderers, false);
            SetMaterial(securityStatusRenderers, lockedMaterial);
        }

        public void StartPowerRestoredVisuals()
        {
            if (isActiveAndEnabled)
            {
                StartCoroutine(PowerRestoredRoutine());
            }
        }

        private IEnumerator PowerRestoredRoutine()
        {
            SetRenderersEnabled(poweredRenderers, true);
            SetMaterial(securityStatusRenderers, poweredMaterial);
            yield return new WaitForSeconds(transitionSeconds);
            SetRenderersEnabled(emergencyRenderers, false);
            SetMaterial(securityStatusRenderers, unlockedMaterial);
        }

        private static void SetRenderersEnabled(Renderer[] renderers, bool enabled)
        {
            if (renderers == null)
            {
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer)
                {
                    renderer.enabled = enabled;
                }
            }
        }

        private static void SetMaterial(Renderer[] renderers, Material material)
        {
            if (renderers == null || !material)
            {
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                if (renderer)
                {
                    renderer.sharedMaterial = material;
                }
            }
        }
    }
}
