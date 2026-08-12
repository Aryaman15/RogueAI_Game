using System.Collections;
using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace RogueAI.Interaction
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private Camera interactionCamera;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private LayerMask interactionLayers = ~0;

        [Header("UI")]
        [SerializeField] private GameObject promptRoot;
        [SerializeField] private Text promptText;
        [SerializeField] private Button interactButton;
        [SerializeField] private GameObject statusRoot;
        [SerializeField] private Text statusText;

        private IInteractable currentInteractable;
        private Coroutine statusRoutine;

        public void Configure(Camera targetCamera, GameObject prompt, Text promptLabel, Button button, GameObject status, Text statusLabel)
        {
            interactionCamera = targetCamera;
            promptRoot = prompt;
            promptText = promptLabel;
            interactButton = button;
            statusRoot = status;
            statusText = statusLabel;
        }

        private void Awake()
        {
            if (!interactionCamera)
            {
                interactionCamera = Camera.main;
            }

            if (interactButton)
            {
                interactButton.onClick.RemoveListener(InteractWithCurrentTarget);
                interactButton.onClick.AddListener(InteractWithCurrentTarget);
            }

            SetPromptVisible(false);
            SetStatusVisible(false);
        }

        private void Update()
        {
            UpdateCurrentTarget();

#if ENABLE_INPUT_SYSTEM
            if (currentInteractable != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                InteractWithCurrentTarget();
            }
#endif
        }

        public void ShowStatusMessage(string message, float seconds)
        {
            if (statusRoutine != null)
            {
                StopCoroutine(statusRoutine);
            }

            statusRoutine = StartCoroutine(ShowStatusRoutine(message, seconds));
        }

        private void UpdateCurrentTarget()
        {
            IInteractable target = FindInteractableInView();
            if (target != null && !target.CanInteract(this))
            {
                target = null;
            }

            currentInteractable = target;

            if (currentInteractable != null)
            {
                if (promptText)
                {
                    promptText.text = currentInteractable.InteractionPrompt;
                }

                SetPromptVisible(true);
            }
            else
            {
                SetPromptVisible(false);
            }
        }

        private IInteractable FindInteractableInView()
        {
            if (!interactionCamera)
            {
                return null;
            }

            Ray ray = new Ray(interactionCamera.transform.position, interactionCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayers, QueryTriggerInteraction.Collide))
            {
                return null;
            }

            MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IInteractable interactable)
                {
                    return interactable;
                }
            }

            return null;
        }

        private void InteractWithCurrentTarget()
        {
            if (currentInteractable == null || !currentInteractable.CanInteract(this))
            {
                return;
            }

            currentInteractable.Interact(this);
        }

        private IEnumerator ShowStatusRoutine(string message, float seconds)
        {
            if (statusText)
            {
                statusText.text = message;
            }

            SetStatusVisible(true);
            yield return new WaitForSeconds(seconds);
            SetStatusVisible(false);
            statusRoutine = null;
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptRoot)
            {
                promptRoot.SetActive(visible);
            }
        }

        private void SetStatusVisible(bool visible)
        {
            if (statusRoot)
            {
                statusRoot.SetActive(visible);
            }
        }
    }
}
