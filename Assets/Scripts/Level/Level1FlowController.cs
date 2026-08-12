using System.Collections;
using RogueAI.Interaction;
using UnityEngine;

namespace RogueAI.Level
{
    public class Level1FlowController : MonoBehaviour
    {
        [SerializeField] private TerminalInteractable generatorTerminal;
        [SerializeField] private GeneratorController generatorController;
        [SerializeField] private FacilityPowerController facilityPowerController;
        [SerializeField] private DoorController securityDoor;
        [SerializeField] private PlayerInteraction playerInteraction;

        private bool powerSequenceStarted;

        public void Configure(
            TerminalInteractable terminal,
            GeneratorController generator,
            FacilityPowerController powerController,
            DoorController door,
            PlayerInteraction interaction)
        {
            generatorTerminal = terminal;
            generatorController = generator;
            facilityPowerController = powerController;
            securityDoor = door;
            playerInteraction = interaction;
            RegisterTerminalEvent();
        }

        private void Awake()
        {
            RegisterTerminalEvent();
        }

        private void OnDestroy()
        {
            if (generatorTerminal)
            {
                generatorTerminal.ChallengeCompleted.RemoveListener(HandleChallengeCompleted);
            }
        }

        private void RegisterTerminalEvent()
        {
            if (!generatorTerminal)
            {
                return;
            }

            generatorTerminal.ChallengeCompleted.RemoveListener(HandleChallengeCompleted);
            generatorTerminal.ChallengeCompleted.AddListener(HandleChallengeCompleted);
        }

        private void HandleChallengeCompleted(string challengeId)
        {
            if (powerSequenceStarted)
            {
                return;
            }

            powerSequenceStarted = true;
            StartCoroutine(RunPowerRestorationSequence());
        }

        private IEnumerator RunPowerRestorationSequence()
        {
            yield return new WaitForSeconds(1.35f);

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("POWER RESTORING...", 1.2f);
            }

            if (generatorController)
            {
                yield return generatorController.StartGenerator();
            }

            if (facilityPowerController)
            {
                yield return facilityPowerController.RestorePower();
            }

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("POWER RESTORED", 1.4f);
            }

            yield return new WaitForSeconds(0.45f);

            if (securityDoor)
            {
                yield return securityDoor.UnlockAndOpen();
            }
        }
    }
}
