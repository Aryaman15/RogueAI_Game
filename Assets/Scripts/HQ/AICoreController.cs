using System.Collections;
using UnityEngine;

namespace RogueAI.HQ
{
    public class AICoreController : MonoBehaviour
    {
        [SerializeField] private Transform[] rotatingRings;
        [SerializeField] private Renderer coreRenderer;
        [SerializeField] private Light statusLight;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material offlineMaterial;
        [SerializeField] private float activeRotationSpeed = 42f;
        [SerializeField] private float shutdownSeconds = 2.4f;

        private bool active = true;
        private float currentRotationSpeed;

        public bool IsOffline => !active;

        public void Configure(Transform[] rings, Renderer core, Light light, Material activeMat, Material offlineMat)
        {
            rotatingRings = rings;
            coreRenderer = core;
            statusLight = light;
            activeMaterial = activeMat;
            offlineMaterial = offlineMat;
            ApplyActiveState();
        }

        private void Awake()
        {
            currentRotationSpeed = activeRotationSpeed;
            if (active)
            {
                ApplyActiveState();
            }
        }

        private void Update()
        {
            if (!active || rotatingRings == null)
            {
                return;
            }

            for (int i = 0; i < rotatingRings.Length; i++)
            {
                Transform ring = rotatingRings[i];
                if (!ring)
                {
                    continue;
                }

                float direction = i % 2 == 0 ? 1f : -1f;
                ring.Rotate(Vector3.up, direction * currentRotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        public IEnumerator ShutdownCore()
        {
            if (!active)
            {
                yield break;
            }

            float elapsed = 0f;
            float startSpeed = currentRotationSpeed;

            while (elapsed < shutdownSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / shutdownSeconds);
                currentRotationSpeed = Mathf.Lerp(startSpeed, 0f, t);

                if (statusLight)
                {
                    statusLight.intensity = Mathf.Lerp(3.4f, 0.15f, t);
                    statusLight.color = Color.Lerp(new Color(1f, 0.12f, 0.05f), new Color(0.15f, 0.25f, 0.28f), t);
                }

                yield return null;
            }

            ApplyOfflineState();
        }

        private void ApplyActiveState()
        {
            active = true;
            currentRotationSpeed = activeRotationSpeed;

            if (coreRenderer && activeMaterial)
            {
                coreRenderer.sharedMaterial = activeMaterial;
            }

            if (statusLight)
            {
                statusLight.enabled = true;
                statusLight.color = new Color(1f, 0.12f, 0.05f);
                statusLight.intensity = 3.4f;
            }
        }

        private void ApplyOfflineState()
        {
            active = false;
            currentRotationSpeed = 0f;

            if (coreRenderer && offlineMaterial)
            {
                coreRenderer.sharedMaterial = offlineMaterial;
            }

            if (statusLight)
            {
                statusLight.color = new Color(0.15f, 0.25f, 0.28f);
                statusLight.intensity = 0.15f;
            }
        }
    }
}
