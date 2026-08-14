using RogueAI.Challenges;
using RogueAI.ClassQuest;
using RogueAI.HQ;
using UnityEngine;
using UnityEngine.Events;

namespace RogueAI.Interaction
{
    public class FinalTerminalInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "INTERACT";
        [SerializeField] private string slotId = "exit-terminal";
        [SerializeField] private bool allowEditorFallbackChallenge = true;
        [SerializeField] private ChallengeData challengeData;
        [SerializeField] private TerminalChallengeUI challengeUI;
        [SerializeField] private HQFlowController flowController;

        public UnityEvent<string> ChallengeCompleted = new UnityEvent<string>();

        private bool completed;

        public string InteractionPrompt => interactionPrompt;
        public bool IsCompleted => completed;

        public void Configure(string terminalSlotId, ChallengeData data, TerminalChallengeUI ui, HQFlowController flow)
        {
            slotId = terminalSlotId;
            challengeData = data;
            challengeUI = ui;
            flowController = flow;
        }

        public bool CanInteract(PlayerInteraction player)
        {
            return enabled && gameObject.activeInHierarchy && !completed && (!flowController || !flowController.MissionCompleted);
        }

        public void Interact(PlayerInteraction player)
        {
            if (completed || (flowController && flowController.MissionCompleted))
            {
                player.ShowStatusMessage("AI CORE OFFLINE", 1.5f);
                return;
            }

            ChallengeData activeChallenge = ResolveChallengeData();
            if (challengeUI && activeChallenge != null)
            {
                if (flowController)
                {
                    flowController.OnFinalAuthorizationStarted();
                }

                challengeUI.ChallengeCompleted.RemoveAllListeners();
                challengeUI.ChallengeCompleted.AddListener(HandleChallengeCompleted);
                player.BeginTerminalChallenge(challengeUI, activeChallenge);
                return;
            }

            player.ShowStatusMessage("MISSION DATA NOT LOADED", 1.5f);
        }

        private void HandleChallengeCompleted(string challengeId)
        {
            completed = true;
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
