using System.Collections;
using UnityEngine;

namespace RogueAI.Level
{
    public class GeneratorController : MonoBehaviour
    {
        [SerializeField] private Renderer statusRenderer;
        [SerializeField] private Material offMaterial;
        [SerializeField] private Material onMaterial;
        [SerializeField] private Light runningLight;
        [SerializeField] private Transform rotatingPart;
        [SerializeField] private float startupSeconds = 1.5f;
        [SerializeField] private float runningRotationSpeed = 180f;

        private bool isPowered;
        private bool isStarting;

        public bool IsPowered => isPowered;

        public void Configure(Renderer status, Material offStateMaterial, Material onStateMaterial, Light indicatorLight, Transform spinner)
        {
            statusRenderer = status;
            offMaterial = offStateMaterial;
            onMaterial = onStateMaterial;
            runningLight = indicatorLight;
            rotatingPart = spinner;
        }

        private void Awake()
        {
            ApplyPowerState(false);
        }

        private void Update()
        {
            if (isPowered && rotatingPart)
            {
                rotatingPart.Rotate(Vector3.up, runningRotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        public IEnumerator StartGenerator()
        {
            if (isPowered || isStarting)
            {
                yield break;
            }

            isStarting = true;

            float elapsed = 0f;
            while (elapsed < startupSeconds)
            {
                elapsed += Time.deltaTime;

                if (runningLight)
                {
                    runningLight.enabled = true;
                    runningLight.intensity = Mathf.Lerp(0.4f, 4f, elapsed / startupSeconds);
                }

                if (rotatingPart)
                {
                    rotatingPart.Rotate(Vector3.up, Mathf.Lerp(40f, runningRotationSpeed, elapsed / startupSeconds) * Time.deltaTime, Space.Self);
                }

                yield return null;
            }

            isStarting = false;
            ApplyPowerState(true);
        }

        private void ApplyPowerState(bool powered)
        {
            isPowered = powered;

            if (statusRenderer)
            {
                Material targetMaterial = powered ? onMaterial : offMaterial;
                if (targetMaterial)
                {
                    statusRenderer.sharedMaterial = targetMaterial;
                }
            }

            if (runningLight)
            {
                runningLight.enabled = powered;
                runningLight.intensity = powered ? 4f : 0f;
            }
        }
    }
}
