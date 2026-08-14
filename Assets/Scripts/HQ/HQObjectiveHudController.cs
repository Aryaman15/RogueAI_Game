using UnityEngine;
using UnityEngine.UI;

namespace RogueAI.HQ
{
    public class HQObjectiveHudController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Text titleText;
        [SerializeField] private Text primaryText;
        [SerializeField] private Text secondaryText;

        public void Configure(GameObject rootObject, Text title, Text primary, Text secondary)
        {
            root = rootObject;
            titleText = title;
            primaryText = primary;
            secondaryText = secondary;
        }

        public void SetObjective(string primary, string secondary)
        {
            if (root)
            {
                root.SetActive(true);
            }

            if (titleText)
            {
                titleText.text = "CURRENT OBJECTIVE";
            }

            if (primaryText)
            {
                primaryText.text = primary;
            }

            if (secondaryText)
            {
                secondaryText.text = secondary;
            }
        }
    }
}
