using RogueAI.Interaction;
using UnityEngine;

namespace RogueAI.HQ
{
    public class DataFragmentPickup : MonoBehaviour, IInteractable
    {
        public enum FragmentId
        {
            A,
            B
        }

        [SerializeField] private string interactionPrompt = "INTERACT";
        [SerializeField] private FragmentId fragmentId;
        [SerializeField] private HQFlowController flowController;
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private Light pickupLight;
        [SerializeField] private float rotationSpeed = 80f;

        private bool collected;

        public string InteractionPrompt => interactionPrompt;
        public FragmentId Id => fragmentId;
        public bool IsCollected => collected;

        public void Configure(FragmentId id, HQFlowController flow, GameObject visual, Light light)
        {
            fragmentId = id;
            flowController = flow;
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
                player.ShowStatusMessage("DATA LINK OFFLINE", 1.5f);
                return;
            }

            flowController.OnDataFragmentCollected(fragmentId, this);
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
