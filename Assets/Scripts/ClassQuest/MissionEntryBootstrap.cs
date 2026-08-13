using RogueAI.Interaction;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogueAI.ClassQuest
{
    public static class MissionEntryBootstrap
    {
        private static bool bootstrappedForScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            TryCreateMissionEntry();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            bootstrappedForScene = false;
            TryCreateMissionEntry();
        }

        private static void TryCreateMissionEntry()
        {
            if (bootstrappedForScene || ClassQuestMissionRuntime.HasMission)
            {
                return;
            }

            if (!Object.FindAnyObjectByType<PlayerInteraction>() || !Object.FindAnyObjectByType<TerminalInteractable>())
            {
                return;
            }

            GameObject entryObject = new GameObject("ClassQuest_MissionEntry");
            entryObject.AddComponent<MissionEntryController>();
            bootstrappedForScene = true;
        }
    }
}
