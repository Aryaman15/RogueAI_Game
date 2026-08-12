using RogueAI.Interaction;
using UnityEngine;

namespace RogueAI.Level
{
    public class LevelExitTrigger : MonoBehaviour
    {
        [SerializeField] private Level1FlowController levelFlowController;
        [SerializeField] private PlayerInteraction playerInteraction;
        [SerializeField] private BoxCollider triggerArea;

        private bool playerWasInside;

        public void Configure(Level1FlowController flowController, PlayerInteraction player)
        {
            levelFlowController = flowController;
            playerInteraction = player;
            CacheTriggerArea();
        }

        private void Awake()
        {
            CacheTriggerArea();
        }

        private void Update()
        {
            if (!levelFlowController)
            {
                return;
            }

            if (!playerInteraction)
            {
                playerInteraction = Object.FindFirstObjectByType<PlayerInteraction>();
            }

            if (!playerInteraction || !triggerArea)
            {
                return;
            }

            bool playerInside = triggerArea.bounds.Contains(playerInteraction.transform.position);
            if (playerInside && !playerWasInside)
            {
                levelFlowController.TryCompleteLevel();
            }

            playerWasInside = playerInside;
        }

        private void OnTriggerEnter(Collider other)
        {
            PlayerInteraction player = other.GetComponentInParent<PlayerInteraction>();
            if (!player || !levelFlowController)
            {
                return;
            }

            levelFlowController.TryCompleteLevel();
        }

        private void CacheTriggerArea()
        {
            if (!triggerArea)
            {
                triggerArea = GetComponent<BoxCollider>();
            }
        }
    }
}
