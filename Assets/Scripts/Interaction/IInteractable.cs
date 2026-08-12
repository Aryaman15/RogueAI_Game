namespace RogueAI.Interaction
{
    public interface IInteractable
    {
        string InteractionPrompt { get; }
        bool CanInteract(PlayerInteraction player);
        void Interact(PlayerInteraction player);
    }
}
