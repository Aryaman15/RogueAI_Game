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
        [SerializeField] private PowerModulePickup powerModule;
        [SerializeField] private LevelExitTrigger levelExit;
        [SerializeField] private HardwareHudController hardwareHud;
        [SerializeField] private LevelCompletionUI completionUI;
        [SerializeField] private Level1AudioDirector audioDirector;
        [SerializeField] private Level1VisualStateController visualStateController;
        [SerializeField] private Level1ProgressState progressState = new Level1ProgressState();

        private bool powerSequenceStarted;

        public void Configure(
            TerminalInteractable terminal,
            GeneratorController generator,
            FacilityPowerController powerController,
            DoorController door,
            PlayerInteraction interaction,
            PowerModulePickup module,
            LevelExitTrigger exit,
            HardwareHudController hud,
            LevelCompletionUI completion)
        {
            Configure(terminal, generator, powerController, door, interaction, module, exit, hud, completion, null, null);
        }

        public void Configure(
            TerminalInteractable terminal,
            GeneratorController generator,
            FacilityPowerController powerController,
            DoorController door,
            PlayerInteraction interaction,
            PowerModulePickup module,
            LevelExitTrigger exit,
            HardwareHudController hud,
            LevelCompletionUI completion,
            Level1AudioDirector audio,
            Level1VisualStateController visualState)
        {
            generatorTerminal = terminal;
            generatorController = generator;
            facilityPowerController = powerController;
            securityDoor = door;
            playerInteraction = interaction;
            powerModule = module;
            levelExit = exit;
            hardwareHud = hud;
            completionUI = completion;
            audioDirector = audio;
            visualStateController = visualState;

            if (powerModule)
            {
                powerModule.Configure(this, null, null);
            }

            if (levelExit)
            {
                levelExit.Configure(this, playerInteraction);
            }

            if (hardwareHud)
            {
                hardwareHud.SetInitialState();
            }

            if (audioDirector)
            {
                audioDirector.ApplyPowerOffState();
            }

            if (visualStateController)
            {
                visualStateController.ApplyPowerOffState();
            }

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

            progressState.terminalChallengeCompleted = true;
            powerSequenceStarted = true;
            StartCoroutine(RunPowerRestorationSequence());
        }

        public void CollectPowerModule(PowerModulePickup module)
        {
            if (progressState.powerModuleCollected)
            {
                return;
            }

            progressState.powerModuleCollected = true;

            if (module)
            {
                module.MarkCollected();
            }

            if (hardwareHud)
            {
                hardwareHud.SetPowerModuleAcquired();
            }

            if (audioDirector)
            {
                audioDirector.PlayPowerModulePickup();
            }

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("POWER MODULE ACQUIRED", 1.7f);
            }
        }

        public void TryCompleteLevel()
        {
            if (progressState.levelCompleted)
            {
                return;
            }

            if (!progressState.powerModuleCollected)
            {
                if (playerInteraction)
                {
                    playerInteraction.ShowStatusMessage("SHUTDOWN MODULE REQUIRED", 1.5f);
                }

                return;
            }

            progressState.levelCompleted = true;

            if (audioDirector)
            {
                audioDirector.PlayLevelComplete();
            }

            if (completionUI)
            {
                completionUI.Show(playerInteraction);
            }
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
                if (audioDirector)
                {
                    audioDirector.PlayPowerRestoredSequence();
                }

                if (visualStateController)
                {
                    visualStateController.StartPowerRestoredVisuals();
                }

                yield return generatorController.StartGenerator();
                progressState.generatorPowered = true;
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
                if (audioDirector)
                {
                    audioDirector.PlayDoorOpen();
                }

                yield return securityDoor.UnlockAndOpen();
                progressState.securityDoorUnlocked = true;
            }
        }
    }
}
