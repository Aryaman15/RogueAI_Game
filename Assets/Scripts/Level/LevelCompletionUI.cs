using RogueAI.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace RogueAI.Level
{
    public class LevelCompletionUI : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text messageText;
        [SerializeField] private Button continueButton;

        public void Configure(GameObject rootObject, Text message, Button button)
        {
            root = rootObject;
            messageText = message;
            continueButton = button;
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

            if (messageText)
            {
                messageText.text =
                    "POWER SECTOR COMPLETE\n\n" +
                    "POWER MODULE SECURED\n\n" +
                    "1 / 3 SHUTDOWN COMPONENTS ACQUIRED\n\n" +
                    "ACCESS TO NEXT SECTOR ESTABLISHED";
            }

            if (root)
            {
                root.SetActive(true);
            }
        }

        private void Hide()
        {
            if (root)
            {
                root.SetActive(false);
            }
        }

        private void RegisterButton()
        {
            if (!continueButton)
            {
                return;
            }

            continueButton.onClick.RemoveListener(ShowComingSoon);
            continueButton.onClick.AddListener(ShowComingSoon);
        }

        private void ShowComingSoon()
        {
            if (messageText)
            {
                messageText.text =
                    "NEXT SECTOR COMING SOON\n\n" +
                    "Prototype milestone complete.";
            }
        }
    }
}
