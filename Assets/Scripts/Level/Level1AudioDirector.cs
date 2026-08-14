using System.Collections;
using UnityEngine;

namespace RogueAI.Level
{
    public class Level1AudioDirector : MonoBehaviour
    {
        [SerializeField] private AudioSource emergencyAmbience;
        [SerializeField] private AudioSource poweredAmbience;
        [SerializeField] private AudioSource generatorSource;
        [SerializeField] private AudioSource doorSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioClip generatorStartupClip;
        [SerializeField] private AudioClip doorOpenClip;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioClip completionClip;
        [SerializeField] private float ambienceFadeSeconds = 1.6f;

        public void Configure(
            AudioSource emergencyLoop,
            AudioSource poweredLoop,
            AudioSource generator,
            AudioSource door,
            AudioSource ui,
            AudioClip generatorStartup,
            AudioClip doorOpen,
            AudioClip pickup,
            AudioClip completion)
        {
            emergencyAmbience = emergencyLoop;
            poweredAmbience = poweredLoop;
            generatorSource = generator;
            doorSource = door;
            uiSource = ui;
            generatorStartupClip = generatorStartup;
            doorOpenClip = doorOpen;
            pickupClip = pickup;
            completionClip = completion;
        }

        private void Awake()
        {
            ApplyPowerOffState();
        }

        public void ApplyPowerOffState()
        {
            if (emergencyAmbience)
            {
                emergencyAmbience.volume = 0.38f;
                if (!emergencyAmbience.isPlaying)
                {
                    emergencyAmbience.Play();
                }
            }

            if (poweredAmbience)
            {
                poweredAmbience.volume = 0f;
                if (!poweredAmbience.isPlaying)
                {
                    poweredAmbience.Play();
                }
            }
        }

        public void PlayPowerRestoredSequence()
        {
            if (generatorSource && generatorStartupClip)
            {
                generatorSource.PlayOneShot(generatorStartupClip, 0.8f);
            }

            if (isActiveAndEnabled)
            {
                StartCoroutine(FadeAmbience());
            }
        }

        public void PlayDoorOpen()
        {
            if (doorSource && doorOpenClip)
            {
                doorSource.PlayOneShot(doorOpenClip, 0.85f);
            }
        }

        public void PlayPowerModulePickup()
        {
            if (uiSource && pickupClip)
            {
                uiSource.PlayOneShot(pickupClip, 0.8f);
            }
        }

        public void PlayLevelComplete()
        {
            if (uiSource && completionClip)
            {
                uiSource.PlayOneShot(completionClip, 0.65f);
            }
        }

        private IEnumerator FadeAmbience()
        {
            float elapsed = 0f;
            while (elapsed < ambienceFadeSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / ambienceFadeSeconds);

                if (emergencyAmbience)
                {
                    emergencyAmbience.volume = Mathf.Lerp(0.38f, 0.08f, t);
                }

                if (poweredAmbience)
                {
                    poweredAmbience.volume = Mathf.Lerp(0f, 0.28f, t);
                }

                yield return null;
            }
        }
    }
}
