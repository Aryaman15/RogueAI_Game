using System.Collections;
using UnityEngine;

namespace RogueAI.Level
{
    public class FacilityPowerController : MonoBehaviour
    {
        [SerializeField] private Light[] emergencyLights;
        [SerializeField] private Light[] poweredLights;
        [SerializeField] private Color powerOffAmbient = new Color(0.28f, 0.18f, 0.16f);
        [SerializeField] private Color powerOnAmbient = new Color(0.38f, 0.42f, 0.46f);
        [SerializeField] private float transitionSeconds = 1.5f;

        public void Configure(Light[] emergency, Light[] powered)
        {
            emergencyLights = emergency;
            poweredLights = powered;
        }

        private void Awake()
        {
            ApplyInitialPowerOffState();
        }

        public void ApplyInitialPowerOffState()
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = powerOffAmbient;
            SetLights(emergencyLights, true, 2.8f);
            SetLights(poweredLights, false, 0f);
        }

        public IEnumerator RestorePower()
        {
            float elapsed = 0f;
            SetLights(poweredLights, true, 0f);

            while (elapsed < transitionSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / transitionSeconds);

                RenderSettings.ambientLight = Color.Lerp(powerOffAmbient, powerOnAmbient, t);
                FadeLights(emergencyLights, Mathf.Lerp(2.8f, 0.4f, t));
                FadeLights(poweredLights, Mathf.Lerp(0f, 3.2f, t));

                yield return null;
            }

            SetLights(emergencyLights, true, 0.4f);
            SetLights(poweredLights, true, 3.2f);
            RenderSettings.ambientLight = powerOnAmbient;
        }

        private static void SetLights(Light[] lights, bool enabled, float intensity)
        {
            if (lights == null)
            {
                return;
            }

            foreach (Light light in lights)
            {
                if (!light)
                {
                    continue;
                }

                light.enabled = enabled;
                light.intensity = intensity;
            }
        }

        private static void FadeLights(Light[] lights, float intensity)
        {
            if (lights == null)
            {
                return;
            }

            foreach (Light light in lights)
            {
                if (light)
                {
                    light.intensity = intensity;
                }
            }
        }
    }
}
