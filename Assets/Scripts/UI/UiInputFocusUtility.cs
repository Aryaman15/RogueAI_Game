using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace RogueAI.UI
{
    public static class UiInputFocusUtility
    {
        public static void EnsureEventSystem()
        {
            if (EventSystem.current)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
            eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
        }

        public static void FocusInputField(MonoBehaviour owner, InputField inputField)
        {
            if (!owner || !inputField)
            {
                return;
            }

            owner.StartCoroutine(FocusInputFieldRoutine(inputField));
        }

        private static IEnumerator FocusInputFieldRoutine(InputField inputField)
        {
            yield return null;

            EnsureEventSystem();

            if (!inputField || !inputField.isActiveAndEnabled)
            {
                yield break;
            }

            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            inputField.Select();
            inputField.ActivateInputField();
            inputField.MoveTextEnd(false);
        }
    }
}
