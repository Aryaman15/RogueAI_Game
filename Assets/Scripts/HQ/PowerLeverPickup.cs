using RogueAI.Interaction;
using UnityEngine;

namespace RogueAI.HQ
{
    public class PowerLeverPickup : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "INTERACT";
        [SerializeField] private HQFlowController flowController;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Light pickupLight;
        [SerializeField] private float rotationSpeed = 70f;

        private bool collected;

        public string InteractionPrompt => interactionPrompt;
        public bool IsCollected => collected;

        public void Configure(HQFlowController controller, GameObject visual, Light light)
        {
            flowController = controller;
            visualRoot = visual;
            pickupLight = light;
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
            return enabled && gameObject.activeInHierarchy && !collected;
        }

        public void Interact(PlayerInteraction player)
        {
            if (collected)
            {
                return;
            }

            if (!flowController)
            {
                player.ShowStatusMessage("POWER LEVER OFFLINE", 1.5f);
                return;
            }

            flowController.CollectPowerLever(this);
        }

        public void MarkCollected()
        {
            collected = true;

            if (visualRoot)
            {
                visualRoot.SetActive(false);
            }

            if (pickupLight)
            {
                pickupLight.enabled = false;
            }

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider pickupCollider in colliders)
            {
                pickupCollider.enabled = false;
            }
        }
    }
}
