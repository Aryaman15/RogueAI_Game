using UnityEngine;

namespace RogueAI.Interaction
{
    public class TerminalInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "INTERACT";
        [SerializeField] private string connectedMessage = "TERMINAL CONNECTED";
        [SerializeField] private float messageSeconds = 1.5f;

        public string InteractionPrompt => interactionPrompt;

        public bool CanInteract(PlayerInteraction player)
        {
            return enabled && gameObject.activeInHierarchy;
        }

        public void Interact(PlayerInteraction player)
        {
            player.ShowStatusMessage(connectedMessage, messageSeconds);
        }
    }
}
