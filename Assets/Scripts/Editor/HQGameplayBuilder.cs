using System.Collections.Generic;
using RogueAI.Challenges;
using RogueAI.ClassQuest;
using RogueAI.HQ;
using RogueAI.Interaction;
using RogueAI.Level;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace RogueAI.EditorTools
{
    public static class HQGameplayBuilder
    {
        private const string SourceScenePath = "Assets/Scenes/HQ_BLOCKOUT_Shortening.unity";
        private const string GameplayScenePath = "Assets/Scenes/HQ_Gameplay.unity";
        private const string GameplayRootName = "HQ_Level1_Gameplay";
        private const string MaterialFolder = "Assets/Generated/HQGameplay/Materials";
        private const string TouchZonesPrefabPath = "Assets/StarterAssets/Mobile/Prefabs/CanvasInputs/UI_Canvas_StarterAssetsInputs_TouchZones.prefab";
        private const float TouchLookSensitivity = 110f;

        private static readonly Vector3 PlayerSpawnPosition = new Vector3(-58.864f, 1.15f, -58.393f);
        private static readonly Vector3 PlayerSpawnRotation = new Vector3(0f, 45f, 0f);
        private static readonly Vector3 GeneratorPosition = new Vector3(55.2f, 0f, -72f);
        private static readonly Vector3 TerminalPosition = new Vector3(52.8f, 0f, -66.5f);
        private static readonly Vector3 PowerLeverPosition = new Vector3(-10.5f, 0f, -75f);
        private static readonly Vector3 GatePosition = new Vector3(-58.86f, 1.45f, -36.22f);

        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("Tools/ClassQuest/Build HQ Gameplay")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Build HQ Gameplay",
                    "This will recreate Assets/Scenes/HQ_Gameplay.unity from HQ_BLOCKOUT_Shortening.unity and add Level 1 gameplay placeholders. The master blockout scene will not be modified.",
                    "Build",
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
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath))
            {
                Debug.LogError($"HQ gameplay build failed. Source scene missing: {SourceScenePath}");
                ExitBatchIfNeeded(batchMode, 1);
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameplayScenePath))
            {
                AssetDatabase.DeleteAsset(GameplayScenePath);
            }

            if (!AssetDatabase.CopyAsset(SourceScenePath, GameplayScenePath))
            {
                Debug.LogError($"HQ gameplay build failed. Could not copy scene to: {GameplayScenePath}");
                ExitBatchIfNeeded(batchMode, 1);
                return;
            }

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(GameplayScenePath, OpenSceneMode.Single);

            Materials.Clear();
            EnsureGeneratedFolders();
            EnsureMaterials();

            RemoveExistingGeneratedRoot();

            GameObject gameplayRoot = new GameObject(GameplayRootName);
            GameObject generatedObjects = CreateChild(gameplayRoot.transform, "GeneratedObjects");
            GameObject uiRoot = CreateChild(gameplayRoot.transform, "UI");
            GameObject lightingRoot = CreateChild(gameplayRoot.transform, "Lighting");
            GameObject signsRoot = CreateChild(gameplayRoot.transform, "Signs");

            PlayerInteraction playerInteraction = ConfigurePlayer();
            if (!playerInteraction)
            {
                Debug.LogError("HQ gameplay build failed. PlayerInteraction could not be configured.");
                ExitBatchIfNeeded(batchMode, 1);
                return;
            }

            EnsureEventSystem();

            InteractionUiRefs interactionUi = CreateInteractionUi(uiRoot.transform);
            TerminalChallengeUI challengeUi = CreateTerminalChallengeUi(uiRoot.transform);
            HQObjectiveHudController objectiveHud = CreateObjectiveHud(uiRoot.transform);

            playerInteraction.Configure(
                Camera.main,
                interactionUi.PromptRoot,
                interactionUi.PromptText,
                interactionUi.InteractButton,
                interactionUi.StatusRoot,
                interactionUi.StatusText);

            GameObject touchControls = EnsureMobileInputCanvases(playerInteraction);
            playerInteraction.ConfigureGameplayLock(
                playerInteraction.GetComponent<FirstPersonController>(),
                playerInteraction.GetComponent<StarterAssetsInputs>(),
                touchControls);

            GeneratorController generator = CreateGenerator(generatedObjects.transform);
            TerminalInteractable terminal = CreateGeneratorTerminal(generatedObjects.transform, challengeUi);
            PowerLeverPickup lever = CreatePowerLever(generatedObjects.transform);
            DoorController gate = CreateLevel1Gate(generatedObjects.transform);
            FacilityPowerController powerController = CreateFacilityPowerController(lightingRoot.transform);
            CreateSigns(signsRoot.transform);
            EnsureClassQuestApiConfig();

            HQFlowController flowController = gameplayRoot.AddComponent<HQFlowController>();
            lever.Configure(flowController, lever.transform.Find("Visual").gameObject, lever.GetComponentInChildren<Light>());
            flowController.Configure(terminal, generator, powerController, gate, lever, playerInteraction, objectiveHud);

            AddGameplaySceneToBuildSettings();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HQ Gameplay scene generated at Assets/Scenes/HQ_Gameplay.unity");
            ExitBatchIfNeeded(batchMode, 0);
        }

        private static PlayerInteraction ConfigurePlayer()
        {
            GameObject player = GameObject.Find("PlayerCapsule");
            if (!player)
            {
                Debug.LogError("PlayerCapsule was not found in the copied HQ blockout scene.");
                return null;
            }

            player.transform.SetPositionAndRotation(
                PlayerSpawnPosition,
                Quaternion.Euler(PlayerSpawnRotation));

            PlayerInteraction interaction = player.GetComponent<PlayerInteraction>();
            if (!interaction)
            {
                interaction = player.AddComponent<PlayerInteraction>();
            }

            Camera mainCamera = Camera.main;
            if (!mainCamera)
            {
                GameObject cameraObject = GameObject.Find("MainCamera") ?? GameObject.Find("Main Camera");
                if (cameraObject)
                {
                    mainCamera = cameraObject.GetComponent<Camera>();
                }
            }

            if (mainCamera)
            {
                mainCamera.tag = "MainCamera";
            }

            return interaction;
        }

        private static GeneratorController CreateGenerator(Transform parent)
        {
            GameObject root = CreateChild(parent, "Generator");
            root.transform.position = GeneratorPosition;

            GameObject baseObject = CreateCube(root.transform, "Base", new Vector3(0f, 0.45f, 0f), new Vector3(2.6f, 0.9f, 1.5f), Materials["Graphite"]);
            GameObject core = CreateCube(root.transform, "StatusCore", new Vector3(0f, 1.25f, 0f), new Vector3(1.4f, 0.7f, 1f), Materials["GeneratorOff"]);
            GameObject rotor = CreateCube(root.transform, "RotatingPowerCoupler", new Vector3(0f, 1.8f, 0f), new Vector3(1.8f, 0.16f, 0.16f), Materials["PoweredCyan"]);
            Object.DestroyImmediate(baseObject.GetComponent<BoxCollider>());
            Object.DestroyImmediate(rotor.GetComponent<BoxCollider>());

            Light runningLight = new GameObject("GeneratorRunningLight").AddComponent<Light>();
            runningLight.transform.SetParent(root.transform);
            runningLight.transform.localPosition = new Vector3(0f, 2.15f, 0f);
            runningLight.type = LightType.Point;
            runningLight.range = 5f;
            runningLight.intensity = 0f;
            runningLight.color = new Color(0.35f, 0.95f, 1f);
            runningLight.shadows = LightShadows.None;
            runningLight.enabled = false;

            GeneratorController controller = root.AddComponent<GeneratorController>();
            controller.Configure(core.GetComponent<Renderer>(), Materials["GeneratorOff"], Materials["GeneratorOn"], runningLight, rotor.transform);
            return controller;
        }

        private static TerminalInteractable CreateGeneratorTerminal(Transform parent, TerminalChallengeUI challengeUi)
        {
            GameObject root = CreateChild(parent, "GeneratorTerminal");
            root.transform.position = TerminalPosition;
            root.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            CreateCube(root.transform, "ConsoleBase", new Vector3(0f, 0.45f, 0f), new Vector3(1.5f, 0.9f, 0.75f), Materials["Steel"]);
            GameObject screen = CreateCube(root.transform, "Screen", new Vector3(0f, 1.25f, -0.33f), new Vector3(1.2f, 0.72f, 0.12f), Materials["TerminalScreen"]);
            Object.DestroyImmediate(screen.GetComponent<BoxCollider>());

            TerminalInteractable terminal = root.AddComponent<TerminalInteractable>();
            terminal.Configure(CreateGeneratorFallbackChallenge(), challengeUi);
            return terminal;
        }

        private static PowerLeverPickup CreatePowerLever(Transform parent)
        {
            GameObject root = CreateChild(parent, "PowerLeverPickup");
            root.transform.position = PowerLeverPosition;

            CreateCube(root.transform, "Pedestal", new Vector3(0f, 0.35f, 0f), new Vector3(1.3f, 0.7f, 1.3f), Materials["Graphite"]);

            GameObject visual = CreateChild(root.transform, "Visual");
            CreateCube(visual.transform, "LeverHandle", new Vector3(0f, 1.15f, 0f), new Vector3(0.25f, 1f, 0.25f), Materials["PoweredCyan"]);
            CreateCube(visual.transform, "LeverGrip", new Vector3(0f, 1.72f, 0f), new Vector3(0.75f, 0.25f, 0.25f), Materials["PoweredCyan"]);

            Light pickupLight = new GameObject("PowerLeverLight").AddComponent<Light>();
            pickupLight.transform.SetParent(root.transform);
            pickupLight.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            pickupLight.type = LightType.Point;
            pickupLight.color = new Color(0.2f, 0.95f, 1f);
            pickupLight.intensity = 2.5f;
            pickupLight.range = 4f;
            pickupLight.shadows = LightShadows.None;

            PowerLeverPickup pickup = root.AddComponent<PowerLeverPickup>();
            pickup.Configure(null, visual, pickupLight);
            return pickup;
        }

        private static DoorController CreateLevel1Gate(Transform parent)
        {
            GameObject root = CreateChild(parent, "Level1SecurityGate");

            GameObject frameLeft = CreateCube(root.transform, "Frame_Left", new Vector3(-65.2f, 1.45f, -36.22f), new Vector3(0.35f, 2.9f, 0.55f), Materials["Graphite"]);
            GameObject frameRight = CreateCube(root.transform, "Frame_Right", new Vector3(-52.55f, 1.45f, -36.22f), new Vector3(0.35f, 2.9f, 0.55f), Materials["Graphite"]);
            GameObject frameTop = CreateCube(root.transform, "Frame_Top", new Vector3(-58.86f, 2.8f, -36.22f), new Vector3(12.8f, 0.25f, 0.55f), Materials["Graphite"]);
            Object.DestroyImmediate(frameLeft.GetComponent<BoxCollider>());
            Object.DestroyImmediate(frameRight.GetComponent<BoxCollider>());
            Object.DestroyImmediate(frameTop.GetComponent<BoxCollider>());

            GameObject doorPanel = CreateCube(root.transform, "DoorPanel", GatePosition, new Vector3(11.5f, 2.8f, 0.35f), Materials["EmergencyRed"]);

            TextMesh label = new GameObject("GateStatusLabel").AddComponent<TextMesh>();
            label.transform.SetParent(root.transform);
            label.transform.position = new Vector3(-58.86f, 2.15f, -36.62f);
            label.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.22f;
            label.fontSize = 42;
            label.color = new Color(1f, 0.28f, 0.22f);
            label.text = "LEVEL 1 GATE\nLOCKED";

            DoorController controller = root.AddComponent<DoorController>();
            controller.Configure(doorPanel.transform, label);
            controller.Lock();
            return controller;
        }

        private static FacilityPowerController CreateFacilityPowerController(Transform parent)
        {
            GameObject root = CreateChild(parent, "FacilityPowerController");

            Light emergencyA = CreatePointLight(root.transform, "EmergencyLight_Start", new Vector3(-40f, 2.4f, -58f), new Color(1f, 0.22f, 0.12f), 2.4f, 12f);
            Light emergencyB = CreatePointLight(root.transform, "EmergencyLight_Generator", new Vector3(54f, 2.4f, -68f), new Color(1f, 0.22f, 0.12f), 2.2f, 11f);
            Light poweredA = CreatePointLight(root.transform, "PoweredLight_MainCorridor", new Vector3(0f, 2.55f, -58f), new Color(0.72f, 0.95f, 1f), 0f, 15f);
            Light poweredB = CreatePointLight(root.transform, "PoweredLight_Gate", new Vector3(-58.8f, 2.55f, -33f), new Color(0.72f, 0.95f, 1f), 0f, 12f);

            FacilityPowerController controller = root.AddComponent<FacilityPowerController>();
            controller.Configure(new[] { emergencyA, emergencyB }, new[] { poweredA, poweredB });
            controller.ApplyInitialPowerOffState();
            return controller;
        }

        private static Light CreatePointLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range)
        {
            Light light = new GameObject(name).AddComponent<Light>();
            light.transform.SetParent(parent);
            light.transform.position = position;
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            return light;
        }

        private static void CreateSigns(Transform parent)
        {
            CreateSign(parent, "Sign_GeneratorControl", "GENERATOR CONTROL ->", new Vector3(28f, 1.9f, -60.8f), Quaternion.Euler(0f, 0f, 0f));
            CreateSign(parent, "Sign_PowerStorage", "<- POWER STORAGE", new Vector3(-22f, 1.9f, -60.8f), Quaternion.Euler(0f, 0f, 0f));
            CreateSign(parent, "Sign_SecuritySector", "SECURITY SECTOR", new Vector3(-58.86f, 1.8f, -41.2f), Quaternion.Euler(0f, 180f, 0f));
        }

        private static void CreateSign(Transform parent, string name, string message, Vector3 position, Quaternion rotation)
        {
            TextMesh sign = new GameObject(name).AddComponent<TextMesh>();
            sign.transform.SetParent(parent);
            sign.transform.position = position;
            sign.transform.rotation = rotation;
            sign.text = message;
            sign.anchor = TextAnchor.MiddleCenter;
            sign.alignment = TextAlignment.Center;
            sign.characterSize = 0.22f;
            sign.fontSize = 44;
            sign.color = new Color(0.35f, 0.95f, 1f);
        }

        private static HQObjectiveHudController CreateObjectiveHud(Transform parent)
        {
            GameObject canvasObject = CreateCanvas(parent, "UI_Canvas_HQObjective", 0);
            GameObject panel = CreateUiPanel(canvasObject.transform, "Panel_Objective", new Vector2(640f, 230f), new Vector2(48f, -46f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0f, 0f, 0f, 0.62f), new Vector2(0f, 1f));
            Text title = CreateUiText(panel.transform, "Text_Title", "CURRENT OBJECTIVE", 32, FontStyle.Bold, new Color(0.35f, 0.95f, 1f));
            Text primary = CreateUiText(panel.transform, "Text_Primary", "Restore Facility Power", 34, FontStyle.Bold, Color.white);
            Text secondary = CreateUiText(panel.transform, "Text_Secondary", "Reach the Generator Control Room", 28, FontStyle.Normal, new Color(0.82f, 0.9f, 0.92f));

            SetRect(title.rectTransform, new Vector2(30f, -24f), new Vector2(580f, 42f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            SetRect(primary.rectTransform, new Vector2(30f, -82f), new Vector2(580f, 52f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));
            SetRect(secondary.rectTransform, new Vector2(30f, -145f), new Vector2(580f, 58f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f));

            HQObjectiveHudController controller = canvasObject.AddComponent<HQObjectiveHudController>();
            controller.Configure(panel, title, primary, secondary);
            controller.SetObjective("Restore Facility Power", "Reach the Generator Control Room");
            return controller;
        }

        private static InteractionUiRefs CreateInteractionUi(Transform parent)
        {
            GameObject canvasObject = CreateCanvas(parent, "UI_Canvas_HQInteraction", 10);

            GameObject prompt = CreateUiPanel(canvasObject.transform, "Panel_InteractPrompt", new Vector2(390f, 118f), new Vector2(-96f, 300f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Color(0f, 0f, 0f, 0.68f), new Vector2(1f, 0f));
            Text promptText = CreateUiText(prompt.transform, "Text_Interact", "INTERACT", 42, FontStyle.Bold, Color.white);
            Button promptButton = prompt.AddComponent<Button>();
            ColorBlock colors = promptButton.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.8f, 1f, 1f);
            colors.pressedColor = new Color(0.45f, 0.9f, 1f);
            promptButton.colors = colors;
            SetRect(promptText.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

            GameObject status = CreateUiPanel(canvasObject.transform, "Panel_Status", new Vector2(360f, 70f), new Vector2(0f, 120f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Color(0f, 0f, 0f, 0.7f));
            Text statusText = CreateUiText(status.transform, "Text_Status", "", 24, FontStyle.Bold, Color.white);
            SetRect(statusText.rectTransform, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one);

            prompt.SetActive(false);
            status.SetActive(false);
            return new InteractionUiRefs(prompt, promptText, promptButton, status, statusText);
        }

        private static TerminalChallengeUI CreateTerminalChallengeUi(Transform parent)
        {
            GameObject canvasObject = CreateCanvas(parent, "UI_Canvas_HQTerminalChallenge", 30);
            GameObject panel = CreateUiPanel(canvasObject.transform, "Panel_TerminalChallenge", Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one, new Color(0.02f, 0.035f, 0.045f, 0.96f));

            Text title = CreateUiText(panel.transform, "Text_Title", "GENERATOR CONTROL TERMINAL", 42, FontStyle.Bold, new Color(0.3f, 1f, 0.7f));
            Text status = CreateUiText(panel.transform, "Text_SystemStatus", "POWER GRID OFFLINE\nMANUAL OVERRIDE REQUIRED", 30, FontStyle.Bold, new Color(1f, 0.58f, 0.32f));
            Text question = CreateUiText(panel.transform, "Text_Question", "What is the output?", 42, FontStyle.Normal, Color.white);
            question.alignment = TextAnchor.UpperLeft;
            question.lineSpacing = 1.08f;

            Text code = CreateUiText(panel.transform, "Text_Code", "for i in range(1, 4):\n    print(i)", 42, FontStyle.Normal, new Color(0.82f, 0.98f, 1f));
            code.alignment = TextAnchor.UpperLeft;
            code.lineSpacing = 1.08f;

            InputField answerInput = CreateInputField(panel.transform, "Input_Answer");
            Button executeButton = CreateButton(panel.transform, "Button_Execute", "EXECUTE", 34);
            Text feedback = CreateUiText(panel.transform, "Text_Feedback", "", 36, FontStyle.Bold, Color.white);

            SetRect(title.rectTransform, new Vector2(0f, -48f), new Vector2(0f, 62f), new Vector2(0.06f, 1f), new Vector2(0.94f, 1f));
            SetRect(status.rectTransform, new Vector2(0f, -116f), new Vector2(0f, 66f), new Vector2(0.08f, 1f), new Vector2(0.92f, 1f));
            SetRect(question.rectTransform, new Vector2(0f, -275f), new Vector2(0f, 240f), new Vector2(0.08f, 1f), new Vector2(0.92f, 1f));
            SetRect(code.rectTransform, new Vector2(0f, -470f), new Vector2(0f, 190f), new Vector2(0.08f, 1f), new Vector2(0.92f, 1f));
            SetRect(answerInput.GetComponent<RectTransform>(), new Vector2(0f, -680f), new Vector2(0f, 96f), new Vector2(0.14f, 1f), new Vector2(0.86f, 1f));
            SetRect(executeButton.GetComponent<RectTransform>(), new Vector2(0f, -800f), new Vector2(420f, 96f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            SetRect(feedback.rectTransform, new Vector2(0f, -910f), new Vector2(0f, 68f), new Vector2(0.1f, 1f), new Vector2(0.9f, 1f));

            TerminalChallengeUI challengeUI = canvasObject.AddComponent<TerminalChallengeUI>();
            challengeUI.Configure(panel, title, status, question, code, answerInput, executeButton, feedback);
            panel.SetActive(false);
            return challengeUI;
        }

        private static GameObject EnsureMobileInputCanvases(PlayerInteraction playerInteraction)
        {
            StarterAssetsInputs inputs = playerInteraction.GetComponent<StarterAssetsInputs>();
            GameObject touchZones = FindOrInstantiatePrefab(TouchZonesPrefabPath, "UI_Canvas_StarterAssetsInputs_TouchZones");
            GameObject joysticks = GameObject.Find("UI_Canvas_StarterAssetsInputs_Joysticks");

            ConfigureMobileCanvas(touchZones, inputs, 0);
            ConfigureTouchZoneLayout(touchZones);

            if (joysticks)
            {
                joysticks.SetActive(false);
            }

#if ENABLE_INPUT_SYSTEM
            UnityEngine.InputSystem.PlayerInput playerInput = playerInteraction.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput && joysticks)
            {
                global::MobileDisableAutoSwitchControls autoSwitch = joysticks.GetComponent<global::MobileDisableAutoSwitchControls>();
                if (autoSwitch)
                {
                    autoSwitch.playerInput = playerInput;
                }
            }
#endif

            return touchZones ? touchZones : joysticks;
        }

        private static GameObject FindOrInstantiatePrefab(string prefabPath, string objectName)
        {
            GameObject existing = GameObject.Find(objectName);
            if (existing)
            {
                return existing;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (!prefab)
            {
                Debug.LogWarning($"Mobile input prefab missing: {prefabPath}");
                return null;
            }

            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = objectName;
            return instance;
        }

        private static void ConfigureMobileCanvas(GameObject canvasObject, StarterAssetsInputs inputs, int sortingOrder)
        {
            if (!canvasObject)
            {
                return;
            }

            UICanvasControllerInput canvasInput = canvasObject.GetComponent<UICanvasControllerInput>();
            if (canvasInput)
            {
                canvasInput.starterAssetsInputs = inputs;
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
        }

        private static void ConfigureTouchZoneLayout(GameObject touchZones)
        {
            if (!touchZones)
            {
                return;
            }

            touchZones.SetActive(true);

            foreach (global::UIVirtualTouchZone touchZone in touchZones.GetComponentsInChildren<global::UIVirtualTouchZone>(true))
            {
                RectTransform rect = touchZone.GetComponent<RectTransform>();
                if (!rect)
                {
                    continue;
                }

                bool isLookZone = touchZone.gameObject.name.ToLowerInvariant().Contains("look");
                if (isLookZone)
                {
                    touchZone.magnitudeMultiplier = TouchLookSensitivity;
                    rect.anchorMin = new Vector2(0.52f, 0f);
                    rect.anchorMax = Vector2.one;
                }
                else
                {
                    rect.anchorMin = Vector2.zero;
                    rect.anchorMax = new Vector2(0.45f, 0.72f);
                }

                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                MakeGraphicsTransparent(touchZone.gameObject, true);
            }

            ConfigureTouchButton("UI_Virtual_Button_Jump", new Vector2(-118f, 126f), new Vector2(138f, 138f));
            ConfigureTouchButton("UI_Virtual_Button_Sprint", new Vector2(-276f, 126f), new Vector2(138f, 138f));
        }

        private static void ConfigureTouchButton(string objectName, Vector2 anchoredPosition, Vector2 size)
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

        private static ChallengeData CreateGeneratorFallbackChallenge()
        {
            return new ChallengeData
            {
                challengeId = "hq-level1-generator-fallback",
                slotId = "generator-terminal",
                title = "GENERATOR CONTROL TERMINAL",
                statusText = "POWER GRID OFFLINE\nMANUAL OVERRIDE REQUIRED",
                question = "What is the output of the following Python code?",
                codeSnippet = "for i in range(1, 4):\n    print(i)",
                expectedAnswer = "1 2 3",
                concept = "Python loops",
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
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvasObject;
        }

        private static GameObject CreateUiPanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            return CreateUiPanel(parent, name, size, anchoredPosition, anchorMin, anchorMax, color, new Vector2(0.5f, 0.5f));
        }

        private static GameObject CreateUiPanel(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Vector2 anchorMin, Vector2 anchorMax, Color color, Vector2 pivot)
        {
            GameObject panel = CreateChild(parent, name);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            SetRect(panel.GetComponent<RectTransform>(), anchoredPosition, size, anchorMin, anchorMax, pivot);
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

        private static InputField CreateInputField(Transform parent, string name)
        {
            GameObject inputObject = CreateUiPanel(parent, name, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, new Color(0.94f, 0.98f, 1f, 1f));
            InputField input = inputObject.AddComponent<InputField>();

            Text text = CreateUiText(inputObject.transform, "Text", "", 34, FontStyle.Normal, Color.black);
            text.alignment = TextAnchor.MiddleLeft;
            SetRect(text.rectTransform, new Vector2(16f, 0f), new Vector2(-32f, 0f), Vector2.zero, Vector2.one);

            Text placeholder = CreateUiText(inputObject.transform, "Placeholder", "Enter answer", 32, FontStyle.Italic, new Color(0.25f, 0.28f, 0.3f, 0.65f));
            placeholder.alignment = TextAnchor.MiddleLeft;
            SetRect(placeholder.rectTransform, new Vector2(16f, 0f), new Vector2(-32f, 0f), Vector2.zero, Vector2.one);

            input.textComponent = text;
            input.placeholder = placeholder;
            input.lineType = InputField.LineType.MultiLineNewline;
            input.keyboardType = TouchScreenKeyboardType.Default;
            return input;
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
            SetRect(rect, anchoredPosition, sizeDelta, anchorMin, anchorMax, new Vector2(0.5f, 0.5f));
        }

        private static void SetRect(RectTransform rect, Vector2 anchoredPosition, Vector2 sizeDelta, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = position;
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

        private static void RemoveExistingGeneratedRoot()
        {
            GameObject existing = GameObject.Find(GameplayRootName);
            if (existing)
            {
                Object.DestroyImmediate(existing);
            }
        }

        private static void EnsureClassQuestApiConfig()
        {
            if (Object.FindFirstObjectByType<ClassQuestApiConfig>())
            {
                return;
            }

            GameObject configObject = new GameObject("ClassQuest_ApiConfig");
            configObject.AddComponent<ClassQuestApiConfig>();
        }

        private static void EnsureEventSystem()
        {
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (!eventSystem)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystem = eventSystemObject.AddComponent<EventSystem>();
            }

#if ENABLE_INPUT_SYSTEM
            if (!eventSystem.GetComponent<InputSystemUIInputModule>())
            {
                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (!eventSystem.GetComponent<StandaloneInputModule>())
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
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
            Materials["EmergencyRed"] = GetOrCreateMaterial("M_HQ_EmergencyRed", new Color(0.6f, 0.06f, 0.04f), new Color(1f, 0.18f, 0.08f), 0.65f);
            Materials["PoweredCyan"] = GetOrCreateMaterial("M_HQ_PoweredCyan", new Color(0.04f, 0.42f, 0.48f), new Color(0.25f, 1f, 1f), 1.2f);
            Materials["GeneratorOff"] = GetOrCreateMaterial("M_HQ_GeneratorOff", new Color(0.18f, 0.08f, 0.07f), new Color(0.9f, 0.08f, 0.02f), 0.4f);
            Materials["GeneratorOn"] = GetOrCreateMaterial("M_HQ_GeneratorOn", new Color(0.04f, 0.4f, 0.45f), new Color(0.18f, 1f, 1f), 1.4f);
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

        private static void AddGameplaySceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == GameplayScenePath)
                {
                    scene.enabled = true;
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(GameplayScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void ExitBatchIfNeeded(bool batchMode, int code)
        {
            if (batchMode)
            {
                EditorApplication.Exit(code);
            }
        }

        private readonly struct InteractionUiRefs
        {
            public readonly GameObject PromptRoot;
            public readonly Text PromptText;
            public readonly Button InteractButton;
            public readonly GameObject StatusRoot;
            public readonly Text StatusText;

            public InteractionUiRefs(GameObject promptRoot, Text promptText, Button interactButton, GameObject statusRoot, Text statusText)
            {
                PromptRoot = promptRoot;
                PromptText = promptText;
                InteractButton = interactButton;
                StatusRoot = statusRoot;
                StatusText = statusText;
            }
        }
    }
}
