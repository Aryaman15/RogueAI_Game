using StarterAssets;
using UnityEngine;

namespace RogueAI.HQ
{
    public class Level2HallwayEndTrigger : MonoBehaviour
    {
        [SerializeField] private HQFlowController flowController;

        public void Configure(HQFlowController flow)
        {
            flowController = flow;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!flowController || other.GetComponentInParent<StarterAssetsInputs>() == null)
            {
                return;
            }

            flowController.OnSecurityHallwayCleared();
        }
    }
}
