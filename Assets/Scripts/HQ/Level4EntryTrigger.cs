using UnityEngine;

namespace RogueAI.HQ
{
    public class Level4EntryTrigger : MonoBehaviour
    {
        [SerializeField] private HQFlowController flowController;

        public void Configure(HQFlowController flow)
        {
            flowController = flow;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!flowController || !other.GetComponentInParent<CharacterController>())
            {
                return;
            }

            flowController.OnLevel4Entered();
        }
    }
}
