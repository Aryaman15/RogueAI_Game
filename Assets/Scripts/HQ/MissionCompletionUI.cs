using RogueAI.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace RogueAI.HQ
{
    public class MissionCompletionUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text messageText;
        [SerializeField] private Button exitButton;

        public void Configure(GameObject rootObject, Text message, Button button)
        {
            root = rootObject;
            messageText = message;
            exitButton = button;
            RegisterButton();
            Hide();
        }

        private void Awake()
        {
            RegisterButton();
            Hide();
        }

        public void Show(PlayerInteraction playerInteraction)
        {
            if (playerInteraction)
            {
                playerInteraction.LockGameplayForCompletion();
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (messageText)
            {
                messageText.text =
                    "MISSION COMPLETE\n\n" +
                    "ROGUE AI SHUTDOWN SUCCESSFUL\n\n" +
                    "Power restored.\n" +
                    "Surveillance disabled.\n" +
                    "Data recovered.\n" +
                    "The headquarters is secure.\n\n" +
                    "Your mission performance has been transmitted to ClassQuest.\n\n" +
                    "Your teacher can now review how you solved each challenge.";
            }

            if (root)
            {
                root.SetActive(true);
            }
        }

        private void RegisterButton()
        {
            if (!exitButton)
            {
                return;
            }

            exitButton.onClick.RemoveListener(ExitClassQuest);
            exitButton.onClick.AddListener(ExitClassQuest);
        }

        private void Hide()
        {
            if (root)
            {
                root.SetActive(false);
            }
        }

        private void ExitClassQuest()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
