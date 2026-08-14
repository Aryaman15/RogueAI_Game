using UnityEngine;
#if ENABLE_INPUT_SYSTEM && (UNITY_IOS || UNITY_ANDROID)
using UnityEngine.InputSystem;
#endif

namespace RogueAI.Utilities
{
    public static class MobileDisableAutoSwitchControlsReferenceFixer
    {
#if ENABLE_INPUT_SYSTEM && (UNITY_IOS || UNITY_ANDROID)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AssignMissingPlayerInputReferences()
        {
            PlayerInput playerInput = Object.FindFirstObjectByType<PlayerInput>();
            if (!playerInput)
            {
                return;
            }

            foreach (global::MobileDisableAutoSwitchControls autoSwitchControl in Object.FindObjectsByType<global::MobileDisableAutoSwitchControls>(FindObjectsSortMode.None))
            {
                if (!autoSwitchControl.playerInput)
                {
                    autoSwitchControl.playerInput = playerInput;
                }
            }
        }
#endif
    }
}
