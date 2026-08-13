using RogueAI.Interaction;
using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

namespace RogueAI.ClassQuest
{
    public class MissionEntryController : MonoBehaviour
    {
        private InputField missionCodeInput;
        private InputField studentNameInput;
        private Text statusText;
        private Button startButton;
        private GameObject root;
        private FirstPersonController firstPersonController;
        private StarterAssetsInputs starterAssetsInputs;
        private GameObject gameplayTouchControls;
        private bool connecting;

        private void Awake()
        {
            CacheGameplayReferences();
            SetGameplayEnabled(false);
            CreateUi();
        }

        private void OnDestroy()
        {
            if (!ClassQuestMissionRuntime.HasMission)
            {
                SetGameplayEnabled(true);
            }
        }

        private void StartMission()
        {
            if (connecting)
            {
                return;
            }

            string missionCode = missionCodeInput ? missionCodeInput.text.Trim().ToUpperInvariant() : string.Empty;
            string studentName = studentNameInput ? studentNameInput.text.Trim() : string.Empty;

            if (string.IsNullOrWhiteSpace(missionCode) || string.IsNullOrWhiteSpace(studentName))
            {
                SetStatus("MISSION CODE AND STUDENT NAME REQUIRED", true);
                return;
            }

            connecting = true;
            startButton.interactable = false;
            SetStatus($"CONNECTING TO CLASSQUEST...\n{ClassQuestApiConfig.CurrentBaseUrl}", false);

            StartCoroutine(ClassQuestApiClient.GetMissionByCode(
                missionCode,
                mission =>
                {
                    ClassQuestMissionRuntime.SetMission(mission, studentName);
                    SetStatus("MISSION LOADED", false);
                    SetGameplayEnabled(true);
                    Destroy(root);
                    Destroy(gameObject);
                },
                error =>
                {
                    connecting = false;
                    startButton.interactable = true;
                    SetStatus(error == "MISSION NOT FOUND" ? "MISSION NOT FOUND" : error, true);
                }));
        }

        private void CacheGameplayReferences()
        {
            GameObject player = GameObject.Find("PlayerCapsule");

            if (player)
            {
                firstPersonController = player.GetComponent<FirstPersonController>();
                starterAssetsInputs = player.GetComponent<StarterAssetsInputs>();
            }

            GameObject touchZones = GameObject.Find("UI_Canvas_StarterAssetsInputs_TouchZones");
            gameplayTouchControls = touchZones;
        }

        private void SetGameplayEnabled(bool enabled)
        {
            if (firstPersonController)
            {
                firstPersonController.enabled = enabled;
            }

            if (gameplayTouchControls)
            {
                gameplayTouchControls.SetActive(enabled);
            }

            if (!enabled && starterAssetsInputs)
            {
                starterAssetsInputs.MoveInput(Vector2.zero);
                starterAssetsInputs.LookInput(Vector2.zero);
                starterAssetsInputs.JumpInput(false);
                starterAssetsInputs.SprintInput(false);
            }
        }

        private void CreateUi()
        {
            root = new GameObject("UI_Canvas_MissionEntry");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Panel_MissionEntry");
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.01f, 0.025f, 0.035f, 0.98f);

            Text eyebrow = CreateText(panel.transform, "Text_Eyebrow", "CLASSQUEST MISSION LINK", 28, FontStyle.Bold, new Color(0.35f, 1f, 0.85f));
            SetRect(eyebrow.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(900f, 46f));

            Text title = CreateText(panel.transform, "Text_Title", "ROGUE AI HEADQUARTERS", 54, FontStyle.Bold, Color.white);
            SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -172f), new Vector2(1100f, 70f));

            Text subtitle = CreateText(panel.transform, "Text_Subtitle", "Enter the mission code provided by your teacher.", 30, FontStyle.Normal, new Color(0.72f, 0.82f, 0.86f));
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -232f), new Vector2(1000f, 52f));

            Text codeLabel = CreateText(panel.transform, "Text_MissionCodeLabel", "MISSION CODE", 25, FontStyle.Bold, new Color(0.35f, 1f, 0.85f));
            SetRect(codeLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 86f), new Vector2(620f, 42f));
            missionCodeInput = CreateInput(panel.transform, "Input_MissionCode", "CQ-XXXX", new Vector2(0f, 28f));

            Text nameLabel = CreateText(panel.transform, "Text_StudentNameLabel", "STUDENT NAME", 25, FontStyle.Bold, new Color(0.35f, 1f, 0.85f));
            SetRect(nameLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -70f), new Vector2(620f, 42f));
            studentNameInput = CreateInput(panel.transform, "Input_StudentName", "Aryaman", new Vector2(0f, -128f));

            startButton = CreateButton(panel.transform);
            statusText = CreateText(panel.transform, "Text_Status", string.Empty, 24, FontStyle.Bold, new Color(0.35f, 1f, 0.85f));
            SetRect(statusText.rectTransform, new Vector2(0.5f, 0f), new Vector2(0f, 110f), new Vector2(1120f, 96f));
        }

        private InputField CreateInput(Transform parent, string name, string placeholderText, Vector2 position)
        {
            GameObject inputObject = new GameObject(name);
            inputObject.transform.SetParent(parent, false);
            RectTransform inputRect = inputObject.AddComponent<RectTransform>();
            SetRect(inputRect, new Vector2(0.5f, 0.5f), position, new Vector2(620f, 76f));

            Image image = inputObject.AddComponent<Image>();
            image.color = new Color(0.94f, 0.98f, 1f, 0.96f);

            InputField input = inputObject.AddComponent<InputField>();
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 48;
            input.shouldHideMobileInput = false;

            Text text = CreateText(inputObject.transform, "Text_Value", string.Empty, 34, FontStyle.Normal, Color.black);
            text.alignment = TextAnchor.MiddleLeft;
            StretchWithPadding(text.rectTransform, 24f);

            Text placeholder = CreateText(inputObject.transform, "Text_Placeholder", placeholderText, 30, FontStyle.Italic, new Color(0.25f, 0.29f, 0.32f, 0.75f));
            placeholder.alignment = TextAnchor.MiddleLeft;
            StretchWithPadding(placeholder.rectTransform, 24f);

            input.textComponent = text;
            input.placeholder = placeholder;
            return input;
        }

        private Button CreateButton(Transform parent)
        {
            GameObject buttonObject = new GameObject("Button_StartMission");
            buttonObject.transform.SetParent(parent, false);
            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            SetRect(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(0f, -238f), new Vector2(430f, 82f));

            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.04f, 0.58f, 0.62f, 1f);

            Button button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(StartMission);

            Text label = CreateText(buttonObject.transform, "Text_StartMission", "START MISSION", 34, FontStyle.Bold, Color.white);
            Stretch(label.rectTransform);
            return button;
        }

        private void SetStatus(string message, bool warning)
        {
            if (!statusText)
            {
                return;
            }

            statusText.text = message;
            statusText.color = warning ? new Color(1f, 0.45f, 0.25f) : new Color(0.35f, 1f, 0.85f);
        }

        private static Text CreateText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            Text uiText = textObject.AddComponent<Text>();
            uiText.text = text;
            uiText.font = GetBuiltinFont();
            uiText.fontSize = fontSize;
            uiText.fontStyle = fontStyle;
            uiText.color = color;
            uiText.alignment = TextAnchor.MiddleCenter;
            uiText.raycastTarget = false;
            return uiText;
        }

        private static Font GetBuiltinFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return font ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchWithPadding(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, 0f);
            rect.offsetMax = new Vector2(-padding, 0f);
        }
    }
}
