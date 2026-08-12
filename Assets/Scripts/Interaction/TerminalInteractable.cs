using RogueAI.Challenges;
using UnityEngine;
using UnityEngine.Events;

namespace RogueAI.Interaction
{
    public class TerminalInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "INTERACT";
        [SerializeField] private ChallengeData challengeData;
        [SerializeField] private TerminalChallengeUI challengeUI;

        public UnityEvent<string> ChallengeCompleted = new UnityEvent<string>();

        public string InteractionPrompt => interactionPrompt;

        public void Configure(ChallengeData data, TerminalChallengeUI ui)
        {
            challengeData = data;
            challengeUI = ui;
        }

        public bool CanInteract(PlayerInteraction player)
        {
            return enabled && gameObject.activeInHierarchy;
        }

        public void Interact(PlayerInteraction player)
        {
            if (challengeUI && challengeUI.IsCompleted)
            {
                challengeUI.ShowAlreadyGranted(player, challengeData);
                return;
            }

            if (challengeUI && challengeData != null)
            {
                challengeUI.ChallengeCompleted.RemoveListener(HandleChallengeCompleted);
                challengeUI.ChallengeCompleted.AddListener(HandleChallengeCompleted);
                player.BeginTerminalChallenge(challengeUI, challengeData);
                return;
            }

            player.ShowStatusMessage("TERMINAL CONNECTED", 1.5f);
        }

        private void HandleChallengeCompleted(string challengeId)
        {
            ChallengeCompleted.Invoke(challengeId);
        }
    }
}
