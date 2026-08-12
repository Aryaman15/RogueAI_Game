using System.Collections.Generic;
using RogueAI.Challenges;
using RogueAI.Interaction;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Level1Builder
{
    private const string PlaygroundScenePath = "Assets/StarterAssets/FirstPersonController/Scenes/Playground.unity";
    private const string LevelScenePath = "Assets/Scenes/Level1_PowerSector.unity";
    private const string MaterialsFolder = "Assets/Level1/Materials";

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    [MenuItem("Tools/Rogue AI/Build Level 1")]
    public static void BuildLevel1()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(PlaygroundScenePath))
        {
            EditorUtility.DisplayDialog("Rogue AI Level Builder",
                $"Could not find the working Starter Assets scene:\n\n{PlaygroundScenePath}", "OK");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelScenePath))
        {
            bool overwrite = EditorUtility.DisplayDialog("Rebuild Level 1?",
                "Level1_PowerSector.unity already exists. Rebuild it from the working Playground scene?",
                "Rebuild", "Cancel");

            if (!overwrite)
            {
                return;
            }

            AssetDatabase.DeleteAsset(LevelScenePath);
        }

        EnsureFolder("Assets", "Scripts");
        EnsureFolder("Assets/Scripts", "Editor");
        EnsureFolder("Assets", "Level1");
        EnsureFolder("Assets/Level1", "Materials");

        AssetDatabase.CopyAsset(PlaygroundScenePath, LevelScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Scene scene = EditorSceneManager.OpenScene(LevelScenePath, OpenSceneMode.Single);
        CacheMaterials();

        RemoveStarterPlaygroundEnvironment();
        PreparePreservedPlayerSetup();
        BuildGreyboxLevel();
        ConfigureInteractionSystem();
        AddLevelToBuildSettingsPreservingPlayground();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Rogue AI Level Builder",
            "Level 1 Power Sector greybox has been built.\n\nThe working Starter Assets player, camera, and mobile controls were preserved from Playground.",
            "OK");
    }

    private static void RemoveStarterPlaygroundEnvironment()
    {
        string[] removeNames =
        {
            "Environment",
            "Lighting",
            "Reflection Probe",
            "Reflection Probe (1)",
            "Light Probe Group (1)"
        };

        foreach (string objectName in removeNames)
        {
            GameObject found = GameObject.Find(objectName);
            if (found)
            {
                Object.DestroyImmediate(found);
            }
        }
    }

    private static void PreparePreservedPlayerSetup()
    {
        GameObject player = GameObject.Find("PlayerCapsule");
        if (player)
        {
            player.transform.SetPositionAndRotation(new Vector3(0f, 1f, -10f), Quaternion.identity);
        }

        GameObject touchZones = GameObject.Find("UI_Canvas_StarterAssetsInputs_TouchZones");
        if (touchZones)
        {
            touchZones.SetActive(true);
            MakeTouchZoneTransparent(touchZones.transform, "UI_Virtual_TouchZone_Move");
            MakeTouchZoneTransparent(touchZones.transform, "UI_Virtual_TouchZone_Look");
        }

        GameObject joysticks = GameObject.Find("UI_Canvas_StarterAssetsInputs_Joysticks");
        if (joysticks)
        {
            joysticks.SetActive(false);
        }
    }

    private static void BuildGreyboxLevel()
    {
        GameObject root = new GameObject("Level1_PowerSector_Greybox");
        GameObject floors = CreateChild(root, "Floors");
        GameObject walls = CreateChild(root, "Walls");
        GameObject doorFrames = CreateChild(root, "DoorFrames");
        GameObject props = CreateChild(root, "Props");
        GameObject signage = CreateChild(root, "Signage");
        GameObject lighting = CreateChild(root, "Lighting");

        BuildFloors(floors.transform);
        BuildWalls(walls.transform);
        BuildSecurityDoor(doorFrames.transform, props.transform, signage.transform);
        BuildGeneratorRoomProps(props.transform, signage.transform);
        BuildPowerModuleArea(props.transform, signage.transform);
        BuildExitArea(props.transform, signage.transform);
        BuildNavigationSignage(signage.transform);
        BuildLighting(lighting.transform);
    }

    private static void BuildFloors(Transform parent)
    {
        AddCube(parent, "Floor_MainCorridor", new Vector3(0f, -0.1f, 7f), new Vector3(4f, 0.2f, 38f), "Floor");
        AddCube(parent, "Floor_GeneratorBranch", new Vector3(4.5f, -0.1f, 3f), new Vector3(5f, 0.2f, 4f), "Floor");
        AddCube(parent, "Floor_GeneratorRoom", new Vector3(11f, -0.1f, 3f), new Vector3(8f, 0.2f, 7f), "Floor");
        AddCube(parent, "Floor_PowerModuleRoom", new Vector3(-5f, -0.1f, 16f), new Vector3(4f, 0.2f, 4f), "Floor");
        AddCube(parent, "Floor_LevelExit", new Vector3(0f, -0.1f, 28.5f), new Vector3(5f, 0.2f, 5f), "Floor");
    }

    private static void BuildWalls(Transform parent)
    {
        const float wallY = 1.5f;
        const float wallThickness = 0.25f;
        const float wallHeight = 3f;

        AddCube(parent, "Wall_Main_Left_BeforeModuleOpening", new Vector3(-2.125f, wallY, 1f), new Vector3(wallThickness, wallHeight, 26f), "Wall");
        AddCube(parent, "Wall_Main_Left_AfterModuleOpening", new Vector3(-2.125f, wallY, 22f), new Vector3(wallThickness, wallHeight, 8f), "Wall");
        AddCube(parent, "Wall_Main_Right_BeforeGeneratorOpening", new Vector3(2.125f, wallY, -5.5f), new Vector3(wallThickness, wallHeight, 13f), "Wall");
        AddCube(parent, "Wall_Main_Right_AfterGeneratorOpening", new Vector3(2.125f, wallY, 15.5f), new Vector3(wallThickness, wallHeight, 21f), "Wall");
        AddCube(parent, "Wall_Entry_Back", new Vector3(0f, wallY, -12.125f), new Vector3(4.5f, wallHeight, wallThickness), "Wall");

        AddCube(parent, "Wall_GeneratorBranch_North", new Vector3(4.5f, wallY, 5.125f), new Vector3(5f, wallHeight, wallThickness), "Wall");
        AddCube(parent, "Wall_GeneratorBranch_South", new Vector3(4.5f, wallY, 0.875f), new Vector3(5f, wallHeight, wallThickness), "Wall");

        AddCube(parent, "Wall_GeneratorRoom_East", new Vector3(15.125f, wallY, 3f), new Vector3(wallThickness, wallHeight, 7f), "Wall");
        AddCube(parent, "Wall_GeneratorRoom_North", new Vector3(11f, wallY, 6.625f), new Vector3(8.25f, wallHeight, wallThickness), "Wall");
        AddCube(parent, "Wall_GeneratorRoom_South", new Vector3(11f, wallY, -0.625f), new Vector3(8.25f, wallHeight, wallThickness), "Wall");
        AddCube(parent, "Wall_GeneratorRoom_West_Lower", new Vector3(6.875f, wallY, 0.125f), new Vector3(wallThickness, wallHeight, 1.5f), "Wall");
        AddCube(parent, "Wall_GeneratorRoom_West_Upper", new Vector3(6.875f, wallY, 5.875f), new Vector3(wallThickness, wallHeight, 1.5f), "Wall");

        AddCube(parent, "Wall_PowerModuleRoom_West", new Vector3(-7.125f, wallY, 16f), new Vector3(wallThickness, wallHeight, 4.25f), "Wall");
        AddCube(parent, "Wall_PowerModuleRoom_North", new Vector3(-5f, wallY, 18.125f), new Vector3(4.25f, wallHeight, wallThickness), "Wall");
        AddCube(parent, "Wall_PowerModuleRoom_South", new Vector3(-5f, wallY, 13.875f), new Vector3(4.25f, wallHeight, wallThickness), "Wall");

        AddCube(parent, "Wall_Exit_Left", new Vector3(-2.625f, wallY, 28.5f), new Vector3(wallThickness, wallHeight, 5f), "Wall");
        AddCube(parent, "Wall_Exit_Right", new Vector3(2.625f, wallY, 28.5f), new Vector3(wallThickness, wallHeight, 5f), "Wall");
        AddCube(parent, "Wall_Exit_Back", new Vector3(0f, wallY, 31.125f), new Vector3(5.25f, wallHeight, wallThickness), "Wall");
    }

    private static void BuildSecurityDoor(Transform frameParent, Transform propsParent, Transform signageParent)
    {
        AddCube(frameParent, "SecurityDoor_Frame_Left", new Vector3(-1.85f, 1.5f, 8f), new Vector3(0.3f, 3f, 0.45f), "Frame");
        AddCube(frameParent, "SecurityDoor_Frame_Right", new Vector3(1.85f, 1.5f, 8f), new Vector3(0.3f, 3f, 0.45f), "Frame");
        AddCube(frameParent, "SecurityDoor_Frame_Top", new Vector3(0f, 3.05f, 8f), new Vector3(4f, 0.3f, 0.45f), "Frame");
        AddCube(propsParent, "SecurityDoor_Locked_Placeholder", new Vector3(0f, 1.45f, 8f), new Vector3(3f, 2.8f, 0.25f), "Door");
        AddLabel(signageParent, "Sign_SecurityDoorOffline", "SECURITY DOOR\nOFFLINE", new Vector3(0f, 2.35f, 7.72f), Quaternion.Euler(0f, 180f, 0f), 0.42f, "Warning");
    }

    private static void BuildGeneratorRoomProps(Transform propsParent, Transform signageParent)
    {
        AddCube(propsParent, "Generator_Placeholder_Base", new Vector3(12.3f, 0.55f, 3f), new Vector3(2.6f, 1.1f, 1.5f), "Generator");
        AddCube(propsParent, "Generator_Placeholder_Core", new Vector3(12.3f, 1.45f, 3f), new Vector3(1.5f, 0.7f, 1f), "GeneratorAccent");

        GameObject terminalRoot = CreateChild(propsParent.gameObject, "Terminal_Placeholder");
        terminalRoot.AddComponent<TerminalInteractable>();
        AddCube(terminalRoot.transform, "Terminal_Desk_Placeholder", new Vector3(8.4f, 0.45f, 1.2f), new Vector3(1.5f, 0.9f, 0.7f), "Terminal");
        AddCube(terminalRoot.transform, "Terminal_Monitor_Placeholder", new Vector3(8.4f, 1.25f, 0.85f), new Vector3(1.2f, 0.75f, 0.12f), "TerminalScreen");

        AddLabel(signageParent, "Sign_GeneratorRoom", "GENERATOR ROOM", new Vector3(3.9f, 2.25f, 0.7f), Quaternion.Euler(0f, 0f, 0f), 0.35f, "Info");
        AddLabel(signageParent, "Sign_RestorePower", "RESTORE\nSECTOR POWER", new Vector3(8.4f, 2.25f, 0.68f), Quaternion.Euler(0f, 0f, 0f), 0.32f, "Info");
    }

    private static void ConfigureInteractionSystem()
    {
        GameObject player = GameObject.Find("PlayerCapsule");
        Camera mainCamera = Camera.main;

        if (!player || !mainCamera)
        {
            Debug.LogWarning("Level1Builder could not configure interaction because PlayerCapsule or MainCamera was not found.");
            return;
        }

        GameObject existingUi = GameObject.Find("UI_Canvas_Interaction");
        if (existingUi)
        {
            Object.DestroyImmediate(existingUi);
        }

        InteractionUiRefs uiRefs = CreateInteractionUi();
        TerminalChallengeUI challengeUI = CreateTerminalChallengeUi();

        PlayerInteraction playerInteraction = player.GetComponent<PlayerInteraction>();
        if (!playerInteraction)
        {
            playerInteraction = player.AddComponent<PlayerInteraction>();
        }

        playerInteraction.Configure(mainCamera, uiRefs.PromptRoot, uiRefs.PromptText, uiRefs.InteractButton, uiRefs.StatusRoot, uiRefs.StatusText);
        playerInteraction.ConfigureGameplayLock(
            player.GetComponent<FirstPersonController>(),
            player.GetComponent<StarterAssetsInputs>(),
            GameObject.Find("UI_Canvas_StarterAssetsInputs_TouchZones"));

        GameObject terminalObject = GameObject.Find("Terminal_Placeholder");
        TerminalInteractable terminal = terminalObject ? terminalObject.GetComponent<TerminalInteractable>() : null;
        if (!terminal)
        {
            Debug.LogWarning("Level1Builder could not configure terminal challenge because Terminal_Placeholder was not found.");
            return;
        }

        terminal.Configure(CreateLevel1ChallengeData(), challengeUI);
    }

    private static void BuildPowerModuleArea(Transform propsParent, Transform signageParent)
    {
        AddCube(propsParent, "PowerModule_Pedestal", new Vector3(-5f, 0.35f, 16f), new Vector3(1.2f, 0.7f, 1.2f), "Frame");
        AddCube(propsParent, "PowerModule_Placeholder", new Vector3(-5f, 1.05f, 16f), new Vector3(0.55f, 0.55f, 0.55f), "PowerModule");
        AddLabel(signageParent, "Sign_PowerModule", "POWER MODULE", new Vector3(-5f, 2.2f, 13.95f), Quaternion.Euler(0f, 0f, 0f), 0.3f, "Info");
    }

    private static void BuildExitArea(Transform propsParent, Transform signageParent)
    {
        AddCube(propsParent, "LevelExit_Placeholder", new Vector3(0f, 1.5f, 30.65f), new Vector3(3.5f, 3f, 0.2f), "Exit");
        AddLabel(signageParent, "Sign_LevelExit", "LEVEL 1 EXIT", new Vector3(0f, 2.4f, 30.52f), Quaternion.Euler(0f, 180f, 0f), 0.38f, "Exit");
    }

    private static void BuildNavigationSignage(Transform parent)
    {
        AddLabel(parent, "Sign_MainObjective", "SECURITY DOOR AHEAD", new Vector3(0f, 2.35f, -3f), Quaternion.Euler(0f, 180f, 0f), 0.32f, "Info");
        AddLabel(parent, "Sign_GeneratorArrow", "GENERATOR ROOM ->", new Vector3(1.9f, 2.15f, 2.2f), Quaternion.Euler(0f, -90f, 0f), 0.32f, "Info");
    }

    private static void BuildLighting(Transform parent)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.38f, 0.42f, 0.46f);

        AddDirectionalLight(parent, "Directional Light", new Vector3(0f, 8f, -8f), Quaternion.Euler(50f, -30f, 0f), 0.8f);
        AddPointLight(parent, "Light_Entry", new Vector3(0f, 2.6f, -6f), 3.5f, 9f, new Color(0.95f, 0.9f, 0.78f));
        AddPointLight(parent, "Light_Door", new Vector3(0f, 2.6f, 6f), 3.2f, 8f, new Color(1f, 0.65f, 0.55f));
        AddPointLight(parent, "Light_GeneratorRoom", new Vector3(11f, 2.6f, 3f), 4f, 9f, new Color(0.7f, 0.9f, 1f));
        AddPointLight(parent, "Light_SecuredCorridor", new Vector3(0f, 2.6f, 17f), 3f, 9f, new Color(0.8f, 0.95f, 1f));
        AddPointLight(parent, "Light_Exit", new Vector3(0f, 2.6f, 28.5f), 3.5f, 8f, new Color(0.65f, 1f, 0.75f));
    }

    private class InteractionUiRefs
    {
        public GameObject PromptRoot;
        public Text PromptText;
        public Button InteractButton;
        public GameObject StatusRoot;
        public Text StatusText;
    }

    private static InteractionUiRefs CreateInteractionUi()
    {
        GameObject canvasObject = new GameObject("UI_Canvas_Interaction");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject buttonObject = new GameObject("Button_Interact");
        buttonObject.transform.SetParent(canvasObject.transform, false);
        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        SetAnchoredRect(buttonRect, new Vector2(0.5f, 0f), new Vector2(0f, 105f), new Vector2(300f, 92f));

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.02f, 0.08f, 0.10f, 0.9f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.02f, 0.08f, 0.10f, 0.9f);
        colors.highlightedColor = new Color(0.06f, 0.28f, 0.32f, 0.95f);
        colors.pressedColor = new Color(0.04f, 0.55f, 0.62f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        Text promptText = CreateUiText(buttonObject.transform, "Text_Interact", "INTERACT", 42, FontStyle.Bold, Color.white);
        StretchToParent(promptText.rectTransform);

        GameObject statusObject = new GameObject("Status_TerminalMessage");
        statusObject.transform.SetParent(canvasObject.transform, false);
        RectTransform statusRect = statusObject.AddComponent<RectTransform>();
        SetAnchoredRect(statusRect, new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(560f, 88f));

        Image statusImage = statusObject.AddComponent<Image>();
        statusImage.color = new Color(0.01f, 0.04f, 0.05f, 0.86f);

        Text statusText = CreateUiText(statusObject.transform, "Text_Status", "TERMINAL CONNECTED", 40, FontStyle.Bold, new Color(0.25f, 1f, 0.55f));
        StretchToParent(statusText.rectTransform);

        buttonObject.SetActive(false);
        statusObject.SetActive(false);

        return new InteractionUiRefs
        {
            PromptRoot = buttonObject,
            PromptText = promptText,
            InteractButton = button,
            StatusRoot = statusObject,
            StatusText = statusText
        };
    }

    private static ChallengeData CreateLevel1ChallengeData()
    {
        return new ChallengeData
        {
            challengeId = "level1_generator_python_range",
            title = "GENERATOR CONTROL TERMINAL",
            statusText = "POWER GRID OFFLINE\nMANUAL OVERRIDE REQUIRED",
            question = "What is the output of the following Python code?",
            codeSnippet = "for i in range(1, 4):\n    print(i)",
            expectedAnswer = "1 2 3",
            concept = "Python for loops and range",
            type = "short_answer"
        };
    }

    private static TerminalChallengeUI CreateTerminalChallengeUi()
    {
        GameObject existingUi = GameObject.Find("UI_Canvas_TerminalChallenge");
        if (existingUi)
        {
            Object.DestroyImmediate(existingUi);
        }

        GameObject canvasObject = new GameObject("UI_Canvas_TerminalChallenge");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600f, 900f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        TerminalChallengeUI challengeUI = canvasObject.AddComponent<TerminalChallengeUI>();

        GameObject panelObject = new GameObject("Panel_TerminalChallenge");
        panelObject.transform.SetParent(canvasObject.transform, false);
        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        StretchToParent(panelRect);

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0.015f, 0.03f, 0.04f, 0.96f);

        Text title = CreateUiText(panelObject.transform, "Text_Title", "GENERATOR CONTROL TERMINAL", 44, FontStyle.Bold, new Color(0.35f, 1f, 0.85f));
        SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -48f), new Vector2(1120f, 62f));

        Text status = CreateUiText(panelObject.transform, "Text_SystemStatus", "POWER GRID OFFLINE\nMANUAL OVERRIDE REQUIRED", 26, FontStyle.Bold, new Color(1f, 0.58f, 0.32f));
        SetAnchoredRect(status.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -115f), new Vector2(920f, 76f));

        Text question = CreateUiText(panelObject.transform, "Text_Question", "What is the output of the following Python code?", 30, FontStyle.Normal, Color.white);
        SetAnchoredRect(question.rectTransform, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(1100f, 52f));

        GameObject codePanel = new GameObject("Panel_CodeSnippet");
        codePanel.transform.SetParent(panelObject.transform, false);
        RectTransform codePanelRect = codePanel.AddComponent<RectTransform>();
        SetAnchoredRect(codePanelRect, new Vector2(0.5f, 1f), new Vector2(0f, -292f), new Vector2(940f, 116f));

        Image codePanelImage = codePanel.AddComponent<Image>();
        codePanelImage.color = new Color(0f, 0f, 0f, 0.58f);

        Text code = CreateUiText(codePanel.transform, "Text_CodeSnippet", "for i in range(1, 4):\n    print(i)", 32, FontStyle.Bold, new Color(0.88f, 1f, 0.9f));
        code.alignment = TextAnchor.MiddleLeft;
        RectTransform codeRect = code.rectTransform;
        codeRect.anchorMin = Vector2.zero;
        codeRect.anchorMax = Vector2.one;
        codeRect.offsetMin = new Vector2(28f, 8f);
        codeRect.offsetMax = new Vector2(-28f, -8f);

        InputField answerInput = CreateAnswerInput(panelObject.transform);
        Button executeButton = CreateExecuteButton(panelObject.transform);

        Text feedback = CreateUiText(panelObject.transform, "Text_Feedback", string.Empty, 38, FontStyle.Bold, new Color(0.25f, 1f, 0.55f));
        SetAnchoredRect(feedback.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, -268f), new Vector2(840f, 58f));

        challengeUI.Configure(panelObject, title, status, question, code, answerInput, executeButton, feedback);
        panelObject.SetActive(false);
        return challengeUI;
    }

    private static InputField CreateAnswerInput(Transform parent)
    {
        GameObject inputObject = new GameObject("Input_Answer");
        inputObject.transform.SetParent(parent, false);
        RectTransform inputRect = inputObject.AddComponent<RectTransform>();
        SetAnchoredRect(inputRect, new Vector2(0.5f, 0.5f), new Vector2(-175f, -168f), new Vector2(620f, 74f));

        Image inputImage = inputObject.AddComponent<Image>();
        inputImage.color = new Color(0.94f, 0.98f, 1f, 0.96f);

        InputField inputField = inputObject.AddComponent<InputField>();
        inputField.lineType = InputField.LineType.SingleLine;
        inputField.characterLimit = 80;
        inputField.shouldHideMobileInput = false;

        Text text = CreateUiText(inputObject.transform, "Text_InputValue", string.Empty, 34, FontStyle.Normal, Color.black);
        text.alignment = TextAnchor.MiddleLeft;
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 0f);
        textRect.offsetMax = new Vector2(-24f, 0f);

        Text placeholder = CreateUiText(inputObject.transform, "Text_Placeholder", "Enter answer, e.g. 1 2 3", 30, FontStyle.Italic, new Color(0.25f, 0.29f, 0.32f, 0.75f));
        placeholder.alignment = TextAnchor.MiddleLeft;
        RectTransform placeholderRect = placeholder.rectTransform;
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(24f, 0f);
        placeholderRect.offsetMax = new Vector2(-24f, 0f);

        inputField.textComponent = text;
        inputField.placeholder = placeholder;
        return inputField;
    }

    private static Button CreateExecuteButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("Button_Execute");
        buttonObject.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        SetAnchoredRect(buttonRect, new Vector2(0.5f, 0.5f), new Vector2(315f, -168f), new Vector2(260f, 78f));

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.04f, 0.42f, 0.46f, 0.98f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.04f, 0.42f, 0.46f, 0.98f);
        colors.highlightedColor = new Color(0.08f, 0.58f, 0.62f, 1f);
        colors.pressedColor = new Color(0.1f, 0.8f, 0.7f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        Text label = CreateUiText(buttonObject.transform, "Text_Execute", "EXECUTE", 36, FontStyle.Bold, Color.white);
        StretchToParent(label.rectTransform);
        return button;
    }

    private static Text CreateUiText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, Color color)
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
        if (!font)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return font;
    }

    private static void SetAnchoredRect(RectTransform rect, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject AddCube(Transform parent, string name, Vector3 position, Vector3 scale, string materialKey)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.SetPositionAndRotation(position, Quaternion.identity);
        cube.transform.localScale = scale;

        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer && Materials.TryGetValue(materialKey, out Material material))
        {
            renderer.sharedMaterial = material;
        }

        return cube;
    }

    private static void AddLabel(Transform parent, string name, string text, Vector3 position, Quaternion rotation, float size, string materialKey)
    {
        GameObject label = new GameObject(name);
        label.transform.SetParent(parent);
        label.transform.SetPositionAndRotation(position, rotation);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = size;
        textMesh.fontSize = 64;

        MeshRenderer renderer = label.GetComponent<MeshRenderer>();
        if (renderer && Materials.TryGetValue(materialKey, out Material material))
        {
            renderer.sharedMaterial = material;
        }
    }

    private static void AddDirectionalLight(Transform parent, string name, Vector3 position, Quaternion rotation, float intensity)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent);
        lightObject.transform.SetPositionAndRotation(position, rotation);

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = intensity;
    }

    private static void AddPointLight(Transform parent, string name, Vector3 position, float intensity, float range, Color color)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent);
        lightObject.transform.position = position;

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = intensity;
        light.range = range;
        light.color = color;
    }

    private static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform);
        return child;
    }

    private static void MakeTouchZoneTransparent(Transform root, string childName)
    {
        Transform child = FindDeepChild(root, childName);
        if (!child)
        {
            return;
        }

        UnityEngine.UI.Image image = child.GetComponent<UnityEngine.UI.Image>();
        if (!image)
        {
            return;
        }

        Color color = image.color;
        color.a = 0f;
        image.color = color;
        image.raycastTarget = true;
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform found = FindDeepChild(child, childName);
            if (found)
            {
                return found;
            }
        }

        return null;
    }

    private static void AddLevelToBuildSettingsPreservingPlayground()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(LevelScenePath, true));
        scenes.Add(new EditorBuildSettingsScene(PlaygroundScenePath, true));

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.path == LevelScenePath || scene.path == PlaygroundScenePath)
            {
                continue;
            }

            scenes.Add(scene);
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void CacheMaterials()
    {
        Materials.Clear();
        Materials["Floor"] = GetOrCreateMaterial("M_Greybox_Floor", new Color(0.22f, 0.34f, 0.40f));
        Materials["Wall"] = GetOrCreateMaterial("M_Greybox_Wall", new Color(0.52f, 0.58f, 0.61f));
        Materials["Frame"] = GetOrCreateMaterial("M_Greybox_Frame", new Color(0.18f, 0.20f, 0.22f));
        Materials["Door"] = GetOrCreateMaterial("M_SecurityDoor_Red", new Color(0.65f, 0.12f, 0.10f));
        Materials["Generator"] = GetOrCreateMaterial("M_Generator_Dark", new Color(0.12f, 0.18f, 0.20f));
        Materials["GeneratorAccent"] = GetOrCreateMaterial("M_Generator_Cyan", new Color(0.10f, 0.75f, 0.85f));
        Materials["Terminal"] = GetOrCreateMaterial("M_Terminal_Body", new Color(0.08f, 0.09f, 0.11f));
        Materials["TerminalScreen"] = GetOrCreateMaterial("M_Terminal_Screen", new Color(0.05f, 0.85f, 0.35f));
        Materials["PowerModule"] = GetOrCreateMaterial("M_PowerModule", new Color(1f, 0.82f, 0.2f));
        Materials["Exit"] = GetOrCreateMaterial("M_LevelExit_Green", new Color(0.2f, 0.85f, 0.35f));
        Materials["Warning"] = GetOrCreateMaterial("M_Sign_Warning", new Color(1f, 0.25f, 0.18f));
        Materials["Info"] = GetOrCreateMaterial("M_Sign_Info", new Color(0.25f, 0.9f, 1f));
    }

    private static Material GetOrCreateMaterial(string materialName, Color color)
    {
        string path = $"{MaterialsFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material)
        {
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string fullPath = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(fullPath))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
