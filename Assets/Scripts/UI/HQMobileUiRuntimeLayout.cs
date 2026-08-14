using StarterAssets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RogueAI.UI
{
    public static class HQMobileUiRuntimeLayout
    {
        private const string GameplaySceneName = "HQ_Gameplay";
        private const float TouchLookSensitivity = 110f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void ApplyLayoutOnSceneLoad()
        {
            if (SceneManager.GetActiveScene().name != GameplaySceneName)
            {
                return;
            }

            ApplyLayout();
        }

        private static void ApplyLayout()
        {
            GameObject touchZones = GameObject.Find("UI_Canvas_StarterAssetsInputs_TouchZones");
            GameObject joysticks = GameObject.Find("UI_Canvas_StarterAssetsInputs_Joysticks");

            if (joysticks)
            {
                joysticks.SetActive(false);
            }

            if (touchZones)
            {
                touchZones.SetActive(true);
                ConfigureCanvas(touchZones, 0);
                ConfigureTouchZones(touchZones);
                ConfigureMobileButtons();
            }

            ConfigureCanvas(GameObject.Find("UI_Canvas_HQObjective"), 20);
            ConfigureCanvas(GameObject.Find("UI_Canvas_HQInteraction"), 40);
            ConfigureCanvas(GameObject.Find("UI_Canvas_HQTerminalChallenge"), 80);
            ConfigureObjectivePanel();
            ConfigureInteractPrompt();
            ConfigureTerminalChallengePanel();
        }

        private static void ConfigureCanvas(GameObject canvasObject, int sortingOrder)
        {
            if (!canvasObject)
            {
                return;
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = sortingOrder;
            }

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
            }

            canvasObject.transform.localScale = Vector3.one;
        }

        private static void ConfigureTouchZones(GameObject touchZones)
        {
            UICanvasControllerInput canvasInput = touchZones.GetComponent<UICanvasControllerInput>();
            StarterAssetsInputs playerInputs = Object.FindFirstObjectByType<StarterAssetsInputs>();
            if (canvasInput && playerInputs)
            {
                canvasInput.starterAssetsInputs = playerInputs;
            }

            foreach (global::UIVirtualTouchZone touchZone in touchZones.GetComponentsInChildren<global::UIVirtualTouchZone>(true))
            {
                string zoneName = touchZone.gameObject.name.ToLowerInvariant();
                bool isLookZone = zoneName.Contains("look");

                if (isLookZone)
                {
                    touchZone.magnitudeMultiplier = TouchLookSensitivity;
                    StretchRect(touchZone.GetComponent<RectTransform>(), new Vector2(0.52f, 0f), Vector2.one, Vector2.zero, GetSafeOffsetMax());
                }
                else
                {
                    StretchRect(touchZone.GetComponent<RectTransform>(), Vector2.zero, new Vector2(0.45f, 0.72f), GetSafeOffsetMin(), Vector2.zero);
                }

                MakeGraphicsTransparent(touchZone.gameObject, true);
            }
        }

        private static void ConfigureMobileButtons()
        {
            Vector4 safe = GetSafeMarginsInReferencePixels();
            ConfigureBottomRightButton("UI_Virtual_Button_Jump", new Vector2(-118f - safe.z, 126f + safe.w), new Vector2(138f, 138f));
            ConfigureBottomRightButton("UI_Virtual_Button_Sprint", new Vector2(-276f - safe.z, 126f + safe.w), new Vector2(138f, 138f));
        }

        private static void ConfigureBottomRightButton(string objectName, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject button = GameObject.Find(objectName);
            if (!button)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            if (!rect)
            {
                return;
            }

            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void ConfigureObjectivePanel()
        {
            GameObject panel = GameObject.Find("Panel_Objective");
            if (!panel)
            {
                return;
            }

            Vector4 safe = GetSafeMarginsInReferencePixels();
            SetTopLeftRect(panel.GetComponent<RectTransform>(), new Vector2(48f + safe.x, 46f + safe.y), new Vector2(640f, 230f));

            SetChildTopLeft(panel.transform, "Text_Title", new Vector2(30f, 24f), new Vector2(580f, 42f), 32);
            SetChildTopLeft(panel.transform, "Text_Primary", new Vector2(30f, 82f), new Vector2(580f, 52f), 34);
            SetChildTopLeft(panel.transform, "Text_Secondary", new Vector2(30f, 145f), new Vector2(580f, 58f), 28);
        }

        private static void ConfigureInteractPrompt()
        {
            GameObject prompt = GameObject.Find("Panel_InteractPrompt");
            if (!prompt)
            {
                return;
            }

            Vector4 safe = GetSafeMarginsInReferencePixels();
            RectTransform rect = prompt.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-96f - safe.z, 300f + safe.w);
            rect.sizeDelta = new Vector2(390f, 118f);
            SetChildStretch(prompt.transform, "Text_Interact", 42);
        }

        private static void ConfigureTerminalChallengePanel()
        {
            GameObject panel = GameObject.Find("Panel_TerminalChallenge");
            if (!panel)
            {
                return;
            }

            Vector4 safe = GetSafeMarginsInReferencePixels();
            RectTransform rect = panel.GetComponent<RectTransform>();
            if (rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = new Vector2(safe.x, safe.w);
                rect.offsetMax = new Vector2(-safe.z, -safe.y);
            }

            SetChildAnchored(panel.transform, "Text_Title", new Vector2(0f, -48f), new Vector2(0f, 62f), new Vector2(0.06f, 1f), new Vector2(0.94f, 1f), 42, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetChildAnchored(panel.transform, "Text_SystemStatus", new Vector2(0f, -116f), new Vector2(0f, 66f), new Vector2(0.08f, 1f), new Vector2(0.92f, 1f), 30, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetChildAnchored(panel.transform, "Text_Question", new Vector2(0f, -275f), new Vector2(0f, 240f), new Vector2(0.08f, 1f), new Vector2(0.92f, 1f), 42, TextAnchor.UpperLeft, FontStyle.Normal);
            SetChildAnchored(panel.transform, "Text_Code", new Vector2(0f, -470f), new Vector2(0f, 190f), new Vector2(0.08f, 1f), new Vector2(0.92f, 1f), 42, TextAnchor.UpperLeft, FontStyle.Normal);
            SetChildAnchored(panel.transform, "Input_Answer", new Vector2(0f, -680f), new Vector2(0f, 96f), new Vector2(0.14f, 1f), new Vector2(0.86f, 1f), 34, TextAnchor.MiddleLeft, FontStyle.Normal);
            SetChildAnchored(panel.transform, "Button_Execute", new Vector2(0f, -800f), new Vector2(420f, 96f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), 34, TextAnchor.MiddleCenter, FontStyle.Bold);
            SetChildAnchored(panel.transform, "Text_Feedback", new Vector2(0f, -910f), new Vector2(0f, 68f), new Vector2(0.10f, 1f), new Vector2(0.90f, 1f), 36, TextAnchor.MiddleCenter, FontStyle.Bold);

            Transform input = panel.transform.Find("Input_Answer");
            if (input)
            {
                SetChildStretch(input, "Text", 34, TextAnchor.MiddleLeft, FontStyle.Normal);
                SetChildStretch(input, "Placeholder", 32, TextAnchor.MiddleLeft, FontStyle.Italic);
            }

            Transform button = panel.transform.Find("Button_Execute");
            if (button)
            {
                SetChildStretch(button, "Text", 34, TextAnchor.MiddleCenter, FontStyle.Bold);
            }
        }

        private static void SetChildTopLeft(Transform parent, string childName, Vector2 topLeftOffset, Vector2 size, int fontSize = -1)
        {
            Transform child = parent.Find(childName);
            if (!child)
            {
                return;
            }

            SetTopLeftRect(child.GetComponent<RectTransform>(), topLeftOffset, size);
            SetTextFontSize(child, fontSize);
        }

        private static void SetChildStretch(Transform parent, string childName, int fontSize)
        {
            SetChildStretch(parent, childName, fontSize, TextAnchor.MiddleCenter, FontStyle.Normal);
        }

        private static void SetChildStretch(Transform parent, string childName, int fontSize, TextAnchor alignment, FontStyle style)
        {
            Transform child = parent.Find(childName);
            if (!child)
            {
                return;
            }

            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            ApplyTextSettings(child, fontSize, alignment, style);
        }

        private static void SetChildAnchored(Transform parent, string childName, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, int fontSize)
        {
            SetChildAnchored(parent, childName, anchoredPosition, size, anchorMin, anchorMax, fontSize, TextAnchor.MiddleCenter, FontStyle.Normal);
        }

        private static void SetChildAnchored(Transform parent, string childName, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, int fontSize, TextAnchor alignment, FontStyle style)
        {
            Transform child = parent.Find(childName);
            if (!child)
            {
                return;
            }

            RectTransform rect = child.GetComponent<RectTransform>();
            if (rect)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = size;
            }

            ApplyTextSettings(child, fontSize, alignment, style);
        }

        private static void SetTextFontSize(Transform target, int fontSize)
        {
            ApplyTextSettings(target, fontSize, null, null);
        }

        private static void ApplyTextSettings(Transform target, int fontSize, TextAnchor? alignment, FontStyle? style)
        {
            if (fontSize <= 0)
            {
                return;
            }

            Text text = target.GetComponent<Text>();
            if (text)
            {
                text.fontSize = fontSize;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.lineSpacing = 1.08f;

                if (alignment.HasValue)
                {
                    text.alignment = alignment.Value;
                }

                if (style.HasValue)
                {
                    text.fontStyle = style.Value;
                }
            }
        }

        private static void SetTopLeftRect(RectTransform rect, Vector2 topLeftOffset, Vector2 size)
        {
            if (!rect)
            {
                return;
            }

            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(topLeftOffset.x, -topLeftOffset.y);
            rect.sizeDelta = size;
        }

        private static void StretchRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (!rect)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void MakeGraphicsTransparent(GameObject root, bool keepRootRaycastTarget)
        {
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            foreach (Graphic graphic in graphics)
            {
                Color color = graphic.color;
                color.a = 0f;
                graphic.color = color;
                graphic.raycastTarget = keepRootRaycastTarget && graphic.gameObject == root;
            }
        }

        private static Vector2 GetSafeOffsetMin()
        {
            Vector4 safe = GetSafeMarginsInReferencePixels();
            return new Vector2(safe.x, safe.w);
        }

        private static Vector2 GetSafeOffsetMax()
        {
            Vector4 safe = GetSafeMarginsInReferencePixels();
            return new Vector2(-safe.z, -safe.y);
        }

        private static Vector4 GetSafeMarginsInReferencePixels()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                return Vector4.zero;
            }

            Rect safeArea = Screen.safeArea;
            Vector2 referenceResolution = new Vector2(1920f, 1080f);
            float scaleX = referenceResolution.x / Screen.width;
            float scaleY = referenceResolution.y / Screen.height;

            float left = safeArea.xMin * scaleX;
            float top = (Screen.height - safeArea.yMax) * scaleY;
            float right = (Screen.width - safeArea.xMax) * scaleX;
            float bottom = safeArea.yMin * scaleY;

            return new Vector4(left, top, right, bottom);
        }
    }
}
