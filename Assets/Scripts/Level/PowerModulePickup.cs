using RogueAI.Interaction;
using UnityEngine;

namespace RogueAI.Level
{
    public class PowerModulePickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "INTERACT";
        [SerializeField] private Level1FlowController levelFlowController;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Light moduleLight;
        [SerializeField] private float rotationSpeed = 65f;

        private bool collected;

        public string InteractionPrompt => interactionPrompt;

        public void Configure(Level1FlowController flowController, GameObject visuals, Light light)
        {
            levelFlowController = flowController;
            if (visuals)
            {
                visualRoot = visuals;
            }

            if (light)
            {
                moduleLight = light;
            }
        }

        private void Update()
        {
            if (!collected && visualRoot)
            {
                visualRoot.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            }
        }

        public bool CanInteract(PlayerInteraction player)
        {
            return !collected && enabled && gameObject.activeInHierarchy;
        }

        public void Interact(PlayerInteraction player)
        {
            if (!levelFlowController)
            {
                player.ShowStatusMessage("POWER MODULE ACQUIRED", 1.5f);
                MarkCollected();
                return;
            }

            levelFlowController.CollectPowerModule(this);
        }

        public void MarkCollected()
        {
            collected = true;

            if (moduleLight)
            {
                moduleLight.enabled = false;
            }

            if (visualRoot)
            {
                visualRoot.SetActive(false);
            }
        }
    }
}
