using System.Collections;
using RogueAI.Interaction;
using RogueAI.Level;
using StarterAssets;
using UnityEngine;

namespace RogueAI.HQ
{
    public class HQFlowController : MonoBehaviour
    {
        [Header("Level 1 State")]
        [SerializeField] private bool powerRestored;
        [SerializeField] private bool leverCollected;
        [SerializeField] private bool level1GateOpened;

        [Header("Future HQ State")]
        [SerializeField] private bool securityDisabled;
        [SerializeField] private bool securityHallwayCleared;
        [SerializeField] private bool fragmentACollected;
        [SerializeField] private bool fragmentBCollected;
        [SerializeField] private bool shutdownProtocolRecovered;
        [SerializeField] private bool hqGateOpened;
        [SerializeField] private bool aiCoreShutdown;
        [SerializeField] private bool missionCompleted;

        [Header("References")]
        [SerializeField] private TerminalInteractable generatorTerminal;
        [SerializeField] private GeneratorController generatorController;
        [SerializeField] private FacilityPowerController facilityPowerController;
        [SerializeField] private DoorController level1Gate;
        [SerializeField] private PowerLeverPickup powerLeverPickup;
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private HQObjectiveHudController objectiveHud;

        [Header("Level 2 References")]
        [SerializeField] private TerminalInteractable securityTerminal;
        [SerializeField] private SecurityCameraController[] securityCameras;
        [SerializeField] private Transform level2ResetAnchor;
        [SerializeField] private float level2ResetPlayerHeight = 1.15f;

        [Header("Level 3 References")]
        [SerializeField] private DataTerminalInteractable dataTerminal;
        [SerializeField] private DataFragmentPickup fragmentAPickup;
        [SerializeField] private DataFragmentPickup fragmentBPickup;
        [SerializeField] private DoorController hqGate;

        [Header("Level 4 References")]
        [SerializeField] private FinalTerminalInteractable finalTerminal;
        [SerializeField] private AICoreController aiCoreController;
        [SerializeField] private MissionCompletionUI missionCompletionUi;
        [SerializeField] private Level4EntryTrigger level4EntryTrigger;

        private Coroutine powerRoutine;
        private Coroutine gateRoutine;
        private Coroutine cameraResetRoutine;
        private Coroutine hqGateRoutine;
        private Coroutine aiCoreShutdownRoutine;

        public bool PowerRestored => powerRestored;
        public bool LeverCollected => leverCollected;
        public bool Level1GateOpened => level1GateOpened;
        public bool SecurityDisabled => securityDisabled;
        public bool SecurityHallwayCleared => securityHallwayCleared;
        public bool FragmentACollected => fragmentACollected;
        public bool FragmentBCollected => fragmentBCollected;
        public bool ShutdownProtocolRecovered => shutdownProtocolRecovered;
        public bool HqGateOpened => hqGateOpened;
        public bool AiCoreShutdown => aiCoreShutdown;
        public bool MissionCompleted => missionCompleted;
        public bool HasAllDataFragments => fragmentACollected && fragmentBCollected;

        public void Configure(
            TerminalInteractable terminal,
            GeneratorController generator,
            FacilityPowerController powerController,
            DoorController gate,
            PowerLeverPickup lever,
            PlayerInteraction interaction,
            HQObjectiveHudController objective)
        {
            generatorTerminal = terminal;
            generatorController = generator;
            facilityPowerController = powerController;
            level1Gate = gate;
            powerLeverPickup = lever;
            playerInteraction = interaction;
            objectiveHud = objective;

            RegisterEvents();
            ApplyInitialState();
        }

        public void ConfigureLevel2(
            TerminalInteractable terminal,
            SecurityCameraController[] cameras,
            Transform resetAnchor)
        {
            securityTerminal = terminal;
            securityCameras = cameras;
            level2ResetAnchor = resetAnchor;

            RegisterEvents();
            ApplyLevel2InitialState();
        }

        public void ConfigureLevel3(
            DataTerminalInteractable terminal,
            DataFragmentPickup fragmentA,
            DataFragmentPickup fragmentB,
            DoorController gate)
        {
            dataTerminal = terminal;
            fragmentAPickup = fragmentA;
            fragmentBPickup = fragmentB;
            hqGate = gate;

            RegisterEvents();
            ApplyLevel3InitialState();
        }

        public void ConfigureLevel4(
            FinalTerminalInteractable terminal,
            AICoreController coreController,
            MissionCompletionUI completionUi,
            Level4EntryTrigger entryTrigger)
        {
            finalTerminal = terminal;
            aiCoreController = coreController;
            missionCompletionUi = completionUi;
            level4EntryTrigger = entryTrigger;

            RegisterEvents();
            ApplyLevel4InitialState();
        }

        private void Awake()
        {
            RegisterEvents();
            ApplyInitialState();
            ApplyLevel2InitialState();
            ApplyLevel3InitialState();
            ApplyLevel4InitialState();
        }

        private void OnDestroy()
        {
            if (generatorTerminal)
            {
                generatorTerminal.ChallengeCompleted.RemoveListener(HandleGeneratorChallengeCompleted);
            }

            if (securityTerminal)
            {
                securityTerminal.ChallengeCompleted.RemoveListener(HandleSecurityChallengeCompleted);
            }

            if (dataTerminal)
            {
                dataTerminal.ChallengeCompleted.RemoveListener(HandleDataChallengeCompleted);
            }

            if (finalTerminal)
            {
                finalTerminal.ChallengeCompleted.RemoveListener(HandleFinalChallengeCompleted);
            }
        }

        public void OnPowerRestored()
        {
            if (powerRestored || powerRoutine != null)
            {
                return;
            }

            powerRoutine = StartCoroutine(RestorePowerRoutine());
        }

        public void CollectPowerLever(PowerLeverPickup pickup)
        {
            if (leverCollected)
            {
                return;
            }

            leverCollected = true;

            if (pickup)
            {
                pickup.MarkCollected();
            }

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("POWER LEVER ACQUIRED", 1.8f);
            }

            if (objectiveHud)
            {
                objectiveHud.SetObjective("Reach Security Sector", "Proceed to the Sector Gate");
            }

            TryOpenLevel1Gate();
        }

        public void OnSecurityDisabled()
        {
            if (securityDisabled)
            {
                return;
            }

            securityDisabled = true;

            if (securityCameras != null)
            {
                foreach (SecurityCameraController securityCamera in securityCameras)
                {
                    if (securityCamera)
                    {
                        securityCamera.DisableCamera();
                    }
                }
            }

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("SURVEILLANCE OFFLINE", 1.8f);
            }

            if (objectiveHud)
            {
                objectiveHud.SetObjective("Cross Security Hall", "Proceed through the disabled surveillance corridor");
            }
        }

        public void OnSecurityHallwayCleared()
        {
            if (!securityDisabled)
            {
                if (playerInteraction)
                {
                    playerInteraction.ShowStatusMessage("SURVEILLANCE ACTIVE", 1.5f);
                }

                return;
            }

            if (securityHallwayCleared)
            {
                return;
            }

            securityHallwayCleared = true;

            if (objectiveHud)
            {
                UpdateDataFragmentsObjective();
            }
        }

        public void OnDataFragmentCollected(DataFragmentPickup.FragmentId fragmentId, DataFragmentPickup pickup)
        {
            if (fragmentId == DataFragmentPickup.FragmentId.A)
            {
                if (fragmentACollected)
                {
                    return;
                }

                fragmentACollected = true;
            }
            else
            {
                if (fragmentBCollected)
                {
                    return;
                }

                fragmentBCollected = true;
            }

            if (pickup)
            {
                pickup.MarkCollected();
            }

            if (playerInteraction)
            {
                string label = fragmentId == DataFragmentPickup.FragmentId.A ? "A" : "B";
                playerInteraction.ShowStatusMessage($"DATA FRAGMENT {label} ACQUIRED", 1.6f);
            }

            if (HasAllDataFragments)
            {
                if (playerInteraction)
                {
                    playerInteraction.ShowStatusMessage("ALL DATA FRAGMENTS ACQUIRED", 1.7f);
                }

                if (objectiveHud)
                {
                    objectiveHud.SetObjective("RECONSTRUCT SHUTDOWN PROTOCOL", "Reach Data Terminal");
                }

                return;
            }

            UpdateDataFragmentsObjective();
        }

        public void OnShutdownProtocolRecovered()
        {
            if (shutdownProtocolRecovered || hqGateRoutine != null)
            {
                return;
            }

            shutdownProtocolRecovered = true;

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("SHUTDOWN PROTOCOL DECRYPTED", 1.8f);
            }

            if (objectiveHud)
            {
                objectiveHud.SetObjective("ENTER THE AI CORE", "Proceed to Main Headquarters");
            }

            hqGateRoutine = StartCoroutine(OpenHqGateRoutine());
        }

        public void OnLevel4Entered()
        {
            if (missionCompleted || aiCoreShutdown)
            {
                return;
            }

            if (objectiveHud)
            {
                objectiveHud.SetObjective("SHUT DOWN ROGUE AI", "Reach the AI Core Terminal");
            }
        }

        public void OnFinalAuthorizationStarted()
        {
            if (missionCompleted || aiCoreShutdown)
            {
                return;
            }

            if (objectiveHud)
            {
                objectiveHud.SetObjective("FINAL AUTHORIZATION", "Complete the shutdown protocol");
            }
        }

        public void OnAICoreShutdown()
        {
            if (aiCoreShutdown || missionCompleted || aiCoreShutdownRoutine != null)
            {
                return;
            }

            aiCoreShutdownRoutine = StartCoroutine(AICoreShutdownRoutine());
        }

        public void CompleteMission()
        {
            if (missionCompleted)
            {
                return;
            }

            missionCompleted = true;

            if (objectiveHud)
            {
                objectiveHud.SetObjective("MISSION COMPLETE", "Rogue AI shutdown successful");
            }

            if (missionCompletionUi)
            {
                missionCompletionUi.Show(playerInteraction);
            }
            else if (playerInteraction)
            {
                playerInteraction.LockGameplayForCompletion();
            }
        }

        public void HandleSecurityCameraDetection(SecurityCameraController securityCamera)
        {
            if (securityDisabled || cameraResetRoutine != null)
            {
                return;
            }

            cameraResetRoutine = StartCoroutine(SecurityCameraDetectionRoutine());
        }

        private void HandleGeneratorChallengeCompleted(string challengeId)
        {
            OnPowerRestored();
        }

        private void HandleSecurityChallengeCompleted(string challengeId)
        {
            OnSecurityDisabled();
        }

        private void HandleDataChallengeCompleted(string challengeId)
        {
            OnShutdownProtocolRecovered();
        }

        private void HandleFinalChallengeCompleted(string challengeId)
        {
            OnAICoreShutdown();
        }

        private IEnumerator AICoreShutdownRoutine()
        {
            yield return new WaitForSeconds(1.35f);

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("SHUTDOWN AUTHORIZATION ACCEPTED", 1.1f);
            }

            yield return new WaitForSeconds(1f);

            if (objectiveHud)
            {
                objectiveHud.SetObjective("TERMINATING ROGUE AI...", "AI Core shutdown in progress");
            }

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("TERMINATING ROGUE AI...", 1.4f);
            }

            if (aiCoreController)
            {
                yield return aiCoreController.ShutdownCore();
            }
            else
            {
                yield return new WaitForSeconds(2.4f);
            }

            aiCoreShutdown = true;
            aiCoreShutdownRoutine = null;

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("AI CORE OFFLINE", 1.2f);
            }

            yield return new WaitForSeconds(0.75f);
            CompleteMission();
        }

        private IEnumerator SecurityCameraDetectionRoutine()
        {
            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("SURVEILLANCE ALERT\nPLAYER DETECTED", 1.2f);
            }

            yield return new WaitForSeconds(0.75f);
            ResetPlayerToLevel2Checkpoint();
            cameraResetRoutine = null;
        }

        private IEnumerator RestorePowerRoutine()
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

            powerRestored = true;
            powerRoutine = null;

            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("POWER RESTORED", 1.8f);
            }

            if (objectiveHud)
            {
                objectiveHud.SetObjective("Retrieve Power Lever", "Search the Power Storage Room");
            }

            TryOpenLevel1Gate();
        }

        private void TryOpenLevel1Gate()
        {
            if (!powerRestored || !leverCollected || level1GateOpened || gateRoutine != null)
            {
                return;
            }

            gateRoutine = StartCoroutine(OpenLevel1GateRoutine());
        }

        private IEnumerator OpenLevel1GateRoutine()
        {
            if (playerInteraction)
            {
                playerInteraction.ShowStatusMessage("LEVEL 1 GATE UNLOCKED", 1.2f);
            }

            yield return new WaitForSeconds(0.35f);

            if (level1Gate)
            {
                yield return level1Gate.UnlockAndOpen();
            }

            level1GateOpened = true;
            gateRoutine = null;

            if (objectiveHud)
            {
                objectiveHud.SetObjective("Disable Surveillance", "Reach Security Control");
            }
        }

        private IEnumerator OpenHqGateRoutine()
        {
            yield return new WaitForSeconds(0.35f);

            if (hqGate)
            {
                yield return hqGate.UnlockAndOpen();
            }

            hqGateOpened = true;
            hqGateRoutine = null;
        }

        private void RegisterEvents()
        {
            if (generatorTerminal)
            {
                generatorTerminal.ChallengeCompleted.RemoveListener(HandleGeneratorChallengeCompleted);
                generatorTerminal.ChallengeCompleted.AddListener(HandleGeneratorChallengeCompleted);
            }

            if (securityTerminal)
            {
                securityTerminal.ChallengeCompleted.RemoveListener(HandleSecurityChallengeCompleted);
                securityTerminal.ChallengeCompleted.AddListener(HandleSecurityChallengeCompleted);
            }

            if (dataTerminal)
            {
                dataTerminal.ChallengeCompleted.RemoveListener(HandleDataChallengeCompleted);
                dataTerminal.ChallengeCompleted.AddListener(HandleDataChallengeCompleted);
            }

            if (finalTerminal)
            {
                finalTerminal.ChallengeCompleted.RemoveListener(HandleFinalChallengeCompleted);
                finalTerminal.ChallengeCompleted.AddListener(HandleFinalChallengeCompleted);
            }
        }

        private void ApplyInitialState()
        {
            if (objectiveHud)
            {
                objectiveHud.SetObjective("Restore Facility Power", "Reach Generator Control");
            }

            if (level1Gate && !level1GateOpened)
            {
                level1Gate.Lock();
            }

            if (facilityPowerController && !powerRestored)
            {
                facilityPowerController.ApplyInitialPowerOffState();
            }
        }

        private void ApplyLevel2InitialState()
        {
            if (securityDisabled && securityCameras != null)
            {
                foreach (SecurityCameraController securityCamera in securityCameras)
                {
                    if (securityCamera)
                    {
                        securityCamera.DisableCamera();
                    }
                }
            }
        }

        private void ApplyLevel3InitialState()
        {
            if (hqGate && !hqGateOpened)
            {
                hqGate.Lock();
            }
        }

        private void ApplyLevel4InitialState()
        {
            if (level4EntryTrigger)
            {
                level4EntryTrigger.Configure(this);
            }

            if (missionCompleted && missionCompletionUi)
            {
                missionCompletionUi.Show(playerInteraction);
            }
        }

        private void UpdateDataFragmentsObjective()
        {
            if (!objectiveHud)
            {
                return;
            }

            string fragmentAStatus = fragmentACollected ? "ACQUIRED" : "NOT FOUND";
            string fragmentBStatus = fragmentBCollected ? "ACQUIRED" : "NOT FOUND";
            objectiveHud.SetObjective("RECOVER DATA FRAGMENTS", $"Fragment A: {fragmentAStatus}\nFragment B: {fragmentBStatus}");
        }

        private void ResetPlayerToLevel2Checkpoint()
        {
            if (!playerInteraction || !level2ResetAnchor)
            {
                return;
            }

            Transform player = playerInteraction.transform;
            CharacterController characterController = player.GetComponent<CharacterController>();
            StarterAssetsInputs inputs = player.GetComponent<StarterAssetsInputs>();

            if (inputs)
            {
                inputs.MoveInput(Vector2.zero);
                inputs.LookInput(Vector2.zero);
                inputs.JumpInput(false);
                inputs.SprintInput(false);
            }

            if (characterController)
            {
                characterController.enabled = false;
            }

            player.SetPositionAndRotation(
                level2ResetAnchor.position + Vector3.up * level2ResetPlayerHeight,
                level2ResetAnchor.rotation);

            if (characterController)
            {
                characterController.enabled = true;
            }
        }
    }
}
