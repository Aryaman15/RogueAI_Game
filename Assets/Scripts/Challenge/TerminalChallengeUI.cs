using System;
using System.Collections;
using System.Text.RegularExpressions;
using RogueAI.Interaction;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RogueAI.Challenges
{
    public class TerminalChallengeUI : MonoBehaviour
    {
        [Serializable]
        public class ChallengeCompletedEvent : UnityEvent<string> { }

        [Header("UI Root")]
        [SerializeField] private GameObject root;

        [Header("Text")]
        [SerializeField] private Text titleText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text questionText;
        [SerializeField] private Text codeText;
        [SerializeField] private Text feedbackText;

        [Header("Input")]
        [SerializeField] private InputField answerInput;
        [SerializeField] private Button executeButton;

        [Header("Timing")]
        [SerializeField] private float closeDelaySeconds = 1.25f;

        public ChallengeCompletedEvent ChallengeCompleted = new ChallengeCompletedEvent();

        private ChallengeData challengeData;
        private PlayerInteraction playerInteraction;
        private bool completed;
        private int attemptCount;
        private Coroutine closeRoutine;

        public bool IsCompleted => completed;
        public int AttemptCount => attemptCount;

        public void Configure(
            GameObject rootObject,
            Text title,
            Text status,
            Text question,
            Text code,
            InputField answer,
            Button execute,
            Text feedback)
        {
            root = rootObject;
            titleText = title;
            statusText = status;
            questionText = question;
            codeText = code;
            answerInput = answer;
            executeButton = execute;
            feedbackText = feedback;
        }

        private void Awake()
        {
            if (executeButton)
            {
                executeButton.onClick.RemoveListener(SubmitAnswer);
                executeButton.onClick.AddListener(SubmitAnswer);
            }

            CloseImmediate();
        }

        public void Open(ChallengeData data, PlayerInteraction player)
        {
            challengeData = data;
            playerInteraction = player;

            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
                closeRoutine = null;
            }

            ApplyChallengeText();

            if (answerInput)
            {
                answerInput.text = string.Empty;
                answerInput.ActivateInputField();
            }

            if (feedbackText)
            {
                feedbackText.text = string.Empty;
            }

            if (root)
            {
                root.SetActive(true);
            }
        }

        public void ShowAlreadyGranted(PlayerInteraction player, ChallengeData data)
        {
            player.ShowStatusMessage("SYSTEM ACCESS ALREADY GRANTED", 1.5f);
        }

        public void CloseImmediate()
        {
            if (root)
            {
                root.SetActive(false);
            }
        }

        private void SubmitAnswer()
        {
            if (challengeData == null || completed)
            {
                return;
            }

            attemptCount++;
            string submitted = answerInput ? answerInput.text : string.Empty;

            if (NormalizeAnswer(submitted) != NormalizeAnswer(challengeData.expectedAnswer))
            {
                if (feedbackText)
                {
                    feedbackText.text = "ACCESS DENIED";
                    feedbackText.color = new Color(1f, 0.25f, 0.2f);
                }

                if (answerInput)
                {
                    answerInput.ActivateInputField();
                }

                return;
            }

            completed = true;

            if (feedbackText)
            {
                feedbackText.text = "ACCESS GRANTED";
                feedbackText.color = new Color(0.25f, 1f, 0.55f);
            }

            ChallengeCompleted.Invoke(challengeData.challengeId);

            if (closeRoutine != null)
            {
                StopCoroutine(closeRoutine);
            }

            closeRoutine = StartCoroutine(CloseAfterDelay());
        }

        private IEnumerator CloseAfterDelay()
        {
            yield return new WaitForSeconds(closeDelaySeconds);
            CloseImmediate();

            if (playerInteraction)
            {
                playerInteraction.EndTerminalChallenge();
            }

            closeRoutine = null;
        }

        private void ApplyChallengeText()
        {
            if (titleText)
            {
                titleText.text = challengeData.title;
            }

            if (statusText)
            {
                statusText.text = challengeData.statusText;
            }

            if (questionText)
            {
                questionText.text = challengeData.question;
            }

            if (codeText)
            {
                codeText.text = challengeData.codeSnippet;
            }
        }

        private static string NormalizeAnswer(string value)
        {
            return Regex.Replace(value ?? string.Empty, @"\s+", " ").Trim();
        }
    }
}
