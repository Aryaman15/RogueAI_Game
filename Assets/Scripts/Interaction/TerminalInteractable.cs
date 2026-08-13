using RogueAI.Challenges;
using RogueAI.ClassQuest;
using UnityEngine;
using UnityEngine.Events;

namespace RogueAI.Interaction
{
    public class TerminalInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "INTERACT";
        [SerializeField] private string slotId = "generator-terminal";
        [SerializeField] private bool allowEditorFallbackChallenge = true;
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
            ChallengeData activeChallenge = ResolveChallengeData();

            if (challengeUI && challengeUI.IsCompleted)
            {
                challengeUI.ShowAlreadyGranted(player, activeChallenge);
                return;
            }

            if (challengeUI && activeChallenge != null)
            {
                challengeUI.ChallengeCompleted.RemoveListener(HandleChallengeCompleted);
                challengeUI.ChallengeCompleted.AddListener(HandleChallengeCompleted);
                player.BeginTerminalChallenge(challengeUI, activeChallenge);
                return;
            }

            player.ShowStatusMessage("MISSION DATA NOT LOADED", 1.5f);
        }

        private void HandleChallengeCompleted(string challengeId)
        {
            ChallengeCompleted.Invoke(challengeId);
        }

        private ChallengeData ResolveChallengeData()
        {
            if (ClassQuestMissionRuntime.TryGetChallenge(slotId, out ClassQuestChallengeDto backendChallenge))
            {
                return ClassQuestMissionRuntime.ToChallengeData(backendChallenge, challengeData);
            }

            if (Application.isEditor && allowEditorFallbackChallenge)
            {
                if (challengeData != null && string.IsNullOrWhiteSpace(challengeData.slotId))
                {
                    challengeData.slotId = slotId;
                }

                return challengeData;
            }

            return null;
        }
    }
}
