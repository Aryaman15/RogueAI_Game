using System.Collections.Generic;
using RogueAI.Challenges;
using RogueAI.HQ;
using RogueAI.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RogueAI.EditorTools
{
    public static class HQLevel4Builder
    {
        private const string GameplayScenePath = "Assets/Scenes/HQ_Gameplay.unity";
        private const string Level4RootName = "HQ_Level4_Gameplay";
        private const string MaterialFolder = "Assets/Generated/HQGameplay/Materials";
        private const string FinalTerminalSlotId = "exit-terminal";

        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("Tools/ClassQuest/Create Missing HQ Level 4 Anchors")]
        public static void CreateMissingAnchorsFromMenu()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != GameplayScenePath)
            {
                scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            }

            GameObject anchorsRoot = GameObject.Find("HQ_GameplayAnchors");
            if (!anchorsRoot)
            {
                anchorsRoot = new GameObject("HQ_GameplayAnchors");
            }

            Transform baseAnchor = FindAnchor("L3_HQGate") ?? FindAnchor("L3_DataTerminal");
            if (!baseAnchor)
            {
                Debug.LogError("Cannot create Level 4 anchors because neither L3_HQGate nor L3_DataTerminal exists.");
                return;
            }

            CreateAnchorIfMissing(anchorsRoot.transform, "L4_FinalTerminal", baseAnchor.position + baseAnchor.forward * 16f + Vector3.right * 3f, baseAnchor.rotation);
            CreateAnchorIfMissing(anchorsRoot.transform, "L4_AICore", baseAnchor.position + baseAnchor.forward * 24f, baseAnchor.rotation);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("Created any missing Level 4 anchors under HQ_GameplayAnchors. Move them into the AI Core room, save, then run Tools > ClassQuest > Build HQ Level 4.");
        }

        [MenuItem("Tools/ClassQuest/Build HQ Level 4")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Build HQ Level 4",
                    "This will recreate only HQ_Level4_Gameplay in Assets/Scenes/HQ_Gameplay.unity using L4_FinalTerminal and L4_AICore anchors. It will not modify ProBuilder geometry.",
                    "Build Level 4",
                    "Cancel"))
            {
                return;
            }

            Build(false);
        }

        public static void BuildFromCommandLine()
        {
            Build(true);
        }

        private static void Build(bool batchMode)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != GameplayScenePath)
            {
                scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);
            }

            Materials.Clear();
            EnsureGeneratedFolders();
            EnsureMaterials();

            Dictionary<string, Transform> anchors = FindRequiredAnchors();
            if (anchors == null)
            {
                ExitBatchIfNeeded(batchMode, 1);
                return;
            }

            HQFlowController flowController = Object.FindAnyObjectByType<HQFlowController>();
            TerminalChallengeUI challengeUi = Object.FindAnyObjectByType<TerminalChallengeUI>();
            if (!flowController || !challengeUi)
            {
                Debug.LogError("HQ Level 4 build failed. Required existing systems were not found: HQFlowController and TerminalChallengeUI.");
                ExitBatchIfNeeded(batchMode, 1);
                return;
            }

            RemoveExistingLevel4Root();

            GameObject level4Root = new GameObject(Level4RootName);
            GameObject coreRoot = CreateChild(level4Root.transform, "AICore");
            GameObject terminalsRoot = CreateChild(level4Root.transform, "Terminals");
            GameObject triggersRoot = CreateChild(level4Root.transform, "Triggers");
            GameObject uiRoot = CreateChild(level4Root.transform, "UI");
            GameObject signsRoot = CreateChild(level4Root.transform, "Signs");

            AICoreController aiCore = CreateAiCore(coreRoot.transform, anchors["L4_AICore"]);
            FinalTerminalInteractable finalTerminal = CreateFinalTerminal(terminalsRoot.transform, anchors["L4_FinalTerminal"], challengeUi, flowController);
            Level4EntryTrigger entryTrigger = CreateLevel4EntryTrigger(triggersRoot.transform, anchors["L4_AICore"], flowController);
            MissionCompletionUI completionUi = CreateMissionCompletionUi(uiRoot.transform);
            CreateSigns(signsRoot.transform, anchors);

            flowController.ConfigureLevel4(finalTerminal, aiCore, completionUi, entryTrigger);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HQ Level 4 gameplay generated in Assets/Scenes/HQ_Gameplay.unity");
            ExitBatchIfNeeded(batchMode, 0);
        }

        private static Dictionary<string, Transform> FindRequiredAnchors()
        {
            string[] requiredNames =
            {
                "L4_FinalTerminal",
                "L4_AICore"
            };

            Dictionary<string, Transform> anchors = new Dictionary<string, Transform>();
            List<string> missing = new List<string>();

            foreach (string anchorName in requiredNames)
            {
                GameObject anchor = GameObject.Find(anchorName);
                if (!anchor)
                {
                    missing.Add(anchorName);
                    continue;
                }

                anchors[anchorName] = anchor.transform;
            }

            if (missing.Count > 0)
            {
                Debug.LogError($"HQ Level 4 build failed. Missing required anchors under HQ_GameplayAnchors: {string.Join(", ", missing)}. Create/save these anchors first, then run Tools > ClassQuest > Build HQ Level 4.");
                return null;
            }

            return anchors;
        }

        private static Transform FindAnchor(string anchorName)
        {
            GameObject anchor = GameObject.Find(anchorName);
            return anchor ? anchor.transform : null;
        }

        private static void CreateAnchorIfMissing(Transform parent, string anchorName, Vector3 position, Quaternion rotation)
        {
            if (GameObject.Find(anchorName))
            {
                return;
            }

            GameObject anchor = new GameObject(anchorName);
            anchor.transform.SetParent(parent);
            anchor.transform.SetPositionAndRotation(position, rotation);
            anchor.transform.localScale = Vector3.one;
        }

        private static AICoreController CreateAiCore(Transform parent, Transform anchor)
        {
            GameObject root = CreateChild(parent, "AI_Core");
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            GameObject baseObject = CreateCube(root.transform, "CoreBase", new Vector3(0f, 0.28f, 0f), new Vector3(3.2f, 0.56f, 3.2f), Materials["Graphite"]);
            Object.DestroyImmediate(baseObject.GetComponent<BoxCollider>());

            GameObject energy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            energy.name = "CoreEnergy";
            energy.transform.SetParent(root.transform);
            energy.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            energy.transform.localRotation = Quaternion.identity;
            energy.transform.localScale = new Vector3(1.15f, 1.15f, 1.15f);
            energy.GetComponent<Renderer>().sharedMaterial = Materials["AICoreActive"];
            Object.DestroyImmediate(energy.GetComponent<SphereCollider>());

            Transform ringOne = CreateRing(root.transform, "CoreRing_01", new Vector3(0f, 1.55f, 0f), 2.05f, Materials["AlertRed"]);
            Transform ringTwo = CreateRing(root.transform, "CoreRing_02", new Vector3(0f, 1.55f, 0f), 2.65f, Materials["AlertRed"]);
            ringTwo.localRotation = Quaternion.Euler(0f, 45f, 0f);

            Light coreLight = new GameObject("CoreStatusLight").AddComponent<Light>();
            coreLight.transform.SetParent(root.transform);
            coreLight.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            coreLight.type = LightType.Point;
            coreLight.color = new Color(1f, 0.12f, 0.05f);
            coreLight.intensity = 3.4f;
            coreLight.range = 7f;
            coreLight.shadows = LightShadows.None;

            AICoreController controller = root.AddComponent<AICoreController>();
            controller.Configure(
                new[] { ringOne, ringTwo },
                energy.GetComponent<Renderer>(),
                coreLight,
                Materials["AICoreActive"],
                Materials["AICoreOffline"]);
            return controller;
        }

        private static Transform CreateRing(Transform parent, string name, Vector3 localPosition, float diameter, Material material)
        {
            GameObject ring = CreateChild(parent, name);
            ring.transform.localPosition = localPosition;

            float barLength = diameter;
            float barThickness = 0.08f;
            GameObject north = CreateCube(ring.transform, "North", new Vector3(0f, 0f, diameter * 0.5f), new Vector3(barLength, barThickness, barThickness), material);
            GameObject south = CreateCube(ring.transform, "South", new Vector3(0f, 0f, -diameter * 0.5f), new Vector3(barLength, barThickness, barThickness), material);
            GameObject east = CreateCube(ring.transform, "East", new Vector3(diameter * 0.5f, 0f, 0f), new Vector3(barThickness, barThickness, barLength), material);
            GameObject west = CreateCube(ring.transform, "West", new Vector3(-diameter * 0.5f, 0f, 0f), new Vector3(barThickness, barThickness, barLength), material);
            Object.DestroyImmediate(north.GetComponent<BoxCollider>());
            Object.DestroyImmediate(south.GetComponent<BoxCollider>());
            Object.DestroyImmediate(east.GetComponent<BoxCollider>());
            Object.DestroyImmediate(west.GetComponent<BoxCollider>());

            return ring.transform;
        }

        private static FinalTerminalInteractable CreateFinalTerminal(
            Transform parent,
            Transform anchor,
            TerminalChallengeUI challengeUi,
            HQFlowController flowController)
        {
            GameObject root = CreateChild(parent, "FinalAICoreTerminal");
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            CreateCube(root.transform, "ConsoleBase", new Vector3(0f, 0.45f, 0f), new Vector3(1.7f, 0.9f, 0.82f), Materials["Steel"]);
            GameObject screen = CreateCube(root.transform, "Screen", new Vector3(0f, 1.28f, -0.36f), new Vector3(1.36f, 0.78f, 0.12f), Materials["TerminalScreen"]);
            Object.DestroyImmediate(screen.GetComponent<BoxCollider>());

            TextMesh label = new GameObject("TerminalLabel").AddComponent<TextMesh>();
            label.transform.SetParent(root.transform);
            label.transform.localPosition = new Vector3(0f, 1.82f, -0.42f);
            label.transform.localRotation = Quaternion.identity;
            label.text = "AI CORE\nAUTHORIZATION";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.18f;
            label.fontSize = 40;
            label.color = new Color(1f, 0.28f, 0.22f);

            FinalTerminalInteractable terminal = root.AddComponent<FinalTerminalInteractable>();
            terminal.Configure(FinalTerminalSlotId, CreateFinalFallbackChallenge(), challengeUi, flowController);
            return terminal;
        }

        private static Level4EntryTrigger CreateLevel4EntryTrigger(Transform parent, Transform anchor, HQFlowController flowController)
        {
            GameObject trigger = CreateChild(parent, "Level4EntryTrigger");
            trigger.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            BoxCollider collider = trigger.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(9f, 3f, 9f);
            collider.center = new Vector3(0f, 1.5f, 0f);

            Level4EntryTrigger entryTrigger = trigger.AddComponent<Level4EntryTrigger>();
            entryTrigger.Configure(flowController);
            return entryTrigger;
        }

        private static MissionCompletionUI CreateMissionCompletionUi(Transform parent)
        {
            GameObject canvasObject = CreateCanvas(parent, "UI_Canvas_MissionComplete", 90);
            GameObject panel = CreateUiPanel(canvasObject.transform, "Panel_MissionComplete", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, new Color(0.01f, 0.025f, 0.032f, 0.97f));

            Text title = CreateUiText(panel.transform, "Text_Title", "MISSION COMPLETE", 38, FontStyle.Bold, new Color(0.35f, 1f, 0.72f));
            Text message = CreateUiText(panel.transform, "Text_Message", "", 24, FontStyle.Normal, Color.white);
            Button exitButton = CreateButton(panel.transform, "Button_ExitClassQuest", "EXIT CLASSQUEST", 26);

            SetRect(title.rectTransform, new Vector2(0f, -70f), new Vector2(0f, 58f), new Vector2(0.08f, 1f), new Vector2(0.92f, 1f));
            SetRect(message.rectTransform, new Vector2(0f, -275f), new Vector2(0f, 310f), new Vector2(0.14f, 1f), new Vector2(0.86f, 1f));
            SetRect(exitButton.GetComponent<RectTransform>(), new Vector2(0f, 82f), new Vector2(340f, 72f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));

            MissionCompletionUI completionUi = canvasObject.AddComponent<MissionCompletionUI>();
            completionUi.Configure(panel, message, exitButton);
            panel.SetActive(false);
            return completionUi;
        }

        private static void CreateSigns(Transform parent, Dictionary<string, Transform> anchors)
        {
            CreateSign(parent, "Sign_AICoreTerminal", "AI CORE TERMINAL ->", anchors["L4_FinalTerminal"].position + Vector3.up * 1.9f, anchors["L4_FinalTerminal"].rotation);
            CreateSign(parent, "Sign_AICore", "ROGUE AI CORE", anchors["L4_AICore"].position + Vector3.up * 2.85f, anchors["L4_AICore"].rotation);
        }

        private static void CreateSign(Transform parent, string name, string message, Vector3 position, Quaternion rotation)
        {
            TextMesh sign = new GameObject(name).AddComponent<TextMesh>();
            sign.transform.SetParent(parent);
            sign.transform.SetPositionAndRotation(position, rotation);
            sign.text = message;
            sign.anchor = TextAnchor.MiddleCenter;
            sign.alignment = TextAlignment.Center;
            sign.characterSize = 0.22f;
            sign.fontSize = 44;
            sign.color = new Color(0.35f, 0.95f, 1f);
        }

        private static ChallengeData CreateFinalFallbackChallenge()
        {
            return new ChallengeData
            {
                challengeId = "hq-level4-final-fallback",
                slotId = FinalTerminalSlotId,
                title = "FINAL AI CORE SHUTDOWN AUTHORIZATION",
                statusText = "ROGUE AI CORE ACTIVE\nFINAL OVERRIDE REQUIRED",
                question = "Mission Question #4 is not loaded. Enter SHUTDOWN for editor-only testing.",
                codeSnippet = string.Empty,
                expectedAnswer = "SHUTDOWN",
                concept = "Final authorization",
                type = "short-answer"
            };
        }

        private static GameObject CreateCanvas(Transform parent, string name, int sortOrder)
        {
            GameObject canvasObject = CreateChild(parent, name);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        private static GameObject CreateUiPanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            GameObject panel = CreateChild(parent, name);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            SetRect(panel.GetComponent<RectTransform>(), anchoredPosition, size, anchorMin, anchorMax);
            return panel;
        }

        private static Text CreateUiText(Transform parent, string name, string text, int fontSize, FontStyle style, Color color)
        {
            GameObject textObject = CreateChild(parent, name);
            Text label = textObject.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(Transform parent, string name, string text, int fontSize)
        {
            GameObject buttonObject = CreateUiPanel(parent, name, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.1f, 0.68f, 0.8f, 1f));
            Button button = buttonObject.AddComponent<Button>();
            Text label = CreateUiText(buttonObject.transform, "Text", text, fontSize, FontStyle.Bold, Color.white);
            SetRect(label.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);
            return button;
        }

        private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 anchorMin, Vector2 anchorMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = scale;

            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer && material)
            {
                renderer.sharedMaterial = material;
            }

            return cube;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static void RemoveExistingLevel4Root()
        {
            GameObject existing = GameObject.Find(Level4RootName);
            if (existing)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static void EnsureGeneratedFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Generated"))
            {
                AssetDatabase.CreateFolder("Assets", "Generated");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Generated/HQGameplay"))
            {
                AssetDatabase.CreateFolder("Assets/Generated", "HQGameplay");
            }

            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                AssetDatabase.CreateFolder("Assets/Generated/HQGameplay", "Materials");
            }
        }

        private static void EnsureMaterials()
        {
            Materials["Graphite"] = GetOrCreateMaterial("M_HQ_Graphite", new Color(0.07f, 0.09f, 0.1f));
            Materials["Steel"] = GetOrCreateMaterial("M_HQ_Steel", new Color(0.42f, 0.48f, 0.52f));
            Materials["TerminalScreen"] = GetOrCreateMaterial("M_HQ_TerminalScreen", new Color(0.02f, 0.35f, 0.38f), new Color(0.1f, 0.95f, 1f), 1.5f);
            Materials["AlertRed"] = GetOrCreateMaterial("M_HQ_AlertRed", new Color(0.6f, 0.04f, 0.03f), new Color(1f, 0.08f, 0.04f), 1.2f);
            Materials["AICoreActive"] = GetOrCreateMaterial("M_HQ_AICoreActive", new Color(0.65f, 0.05f, 0.04f), new Color(1f, 0.12f, 0.05f), 1.7f);
            Materials["AICoreOffline"] = GetOrCreateMaterial("M_HQ_AICoreOffline", new Color(0.1f, 0.14f, 0.15f), new Color(0.04f, 0.08f, 0.09f), 0.2f);
        }

        private static Material GetOrCreateMaterial(string name, Color baseColor)
        {
            return GetOrCreateMaterial(name, baseColor, Color.black, 0f);
        }

        private static Material GetOrCreateMaterial(string name, Color baseColor, Color emissionColor, float emissionIntensity)
        {
            string path = $"{MaterialFolder}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!material)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = baseColor;
            if (emissionIntensity > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", emissionColor * emissionIntensity);
            }
            else
            {
                material.DisableKeyword("_EMISSION");
            }

            return material;
        }

        private static void ExitBatchIfNeeded(bool batchMode, int code)
        {
            if (batchMode)
            {
                EditorApplication.Exit(code);
            }
        }
    }
}
