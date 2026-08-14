using System.Collections.Generic;
using RogueAI.Interaction;
using RogueAI.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class Level1SciFiEnvironmentBuilder
{
    private const string VisualRootName = "Level1_VisualEnvironment";
    private const string GreyboxRootName = "Level1_PowerSector_Greybox";
    private const string LevelScenePath = "Assets/Scenes/Level1_PowerSector.unity";
    private const string AssetBase = "Assets/Creepy_Cat/3D Scifi Kit Starter Kit_HD";
    private const string MaterialsFolder = "Assets/Level1/VisualMaterials";

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

    [MenuItem("Tools/ClassQuest/Build Level 1 Sci-Fi Environment")]
    public static void BuildEnvironment()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EnsureLevelSceneOpen();

        if (!ValidateLevelScene())
        {
            return;
        }

        EnsureFolder("Assets", "Level1");
        EnsureFolder("Assets/Level1", "VisualMaterials");
        CacheMaterials();
        OptimizeAudioImports();

        RemoveEnvironmentInternal(true);

        GameObject root = new GameObject(VisualRootName);
        CreateSection(root, "EntrySector");
        CreateSection(root, "MainPowerCorridor");
        CreateSection(root, "GeneratorControlRoom");
        CreateSection(root, "SecurityGateArea");
        CreateSection(root, "SecuredMaintenanceCorridor");
        CreateSection(root, "PowerModuleVault");
        CreateSection(root, "SectorExit");
        CreateSection(root, "Ceiling");
        CreateSection(root, "DecorativeProps");
        CreateSection(root, "Signage");
        CreateSection(root, "Lighting");
        CreateSection(root, "Audio");

        HideGreyboxRenderers();
        BuildEntrySector(root.transform.Find("EntrySector"));
        BuildMainPowerCorridor(root.transform.Find("MainPowerCorridor"), root.transform.Find("Ceiling"));
        BuildGeneratorControlRoom(root.transform.Find("GeneratorControlRoom"), root.transform.Find("DecorativeProps"));
        BuildSecurityGateArea(root.transform.Find("SecurityGateArea"));
        BuildSecuredMaintenanceCorridor(root.transform.Find("SecuredMaintenanceCorridor"), root.transform.Find("Ceiling"));
        BuildPowerModuleVault(root.transform.Find("PowerModuleVault"), root.transform.Find("DecorativeProps"));
        BuildSectorExit(root.transform.Find("SectorExit"));
        BuildSignage(root.transform.Find("Signage"));
        BuildLighting(root.transform.Find("Lighting"));
        BuildAudio(root.transform.Find("Audio"));
        WireVisualAndAudioState();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("ClassQuest Environment",
            "Level 1 sci-fi visual/audio environment has been built.\n\nGameplay objects and greybox colliders were preserved.",
            "OK");
    }

    [MenuItem("Tools/ClassQuest/Remove Level 1 Sci-Fi Environment")]
    public static void RemoveEnvironment()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EnsureLevelSceneOpen();
        RemoveEnvironmentInternal(true);
        RestoreGreyboxRenderers();
        RemoveVisualAudioDirectors();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("ClassQuest Environment",
            "Level 1 sci-fi environment has been removed and greybox MeshRenderers were restored.",
            "OK");
    }

    private static bool ValidateLevelScene()
    {
        if (!GameObject.Find("PlayerCapsule") || !GameObject.Find(GreyboxRootName))
        {
            EditorUtility.DisplayDialog("ClassQuest Environment",
                "Open Assets/Scenes/Level1_PowerSector.unity before building the sci-fi environment.",
                "OK");
            return false;
        }

        if (!AssetDatabase.LoadAssetAtPath<GameObject>($"{AssetBase}/Your_Hown_Prefabs/Corridor_Test_01.prefab"))
        {
            EditorUtility.DisplayDialog("ClassQuest Environment",
                $"Could not find imported sci-fi package at:\n{AssetBase}",
                "OK");
            return false;
        }

        return true;
    }

    private static void EnsureLevelSceneOpen()
    {
        if (SceneManager.GetActiveScene().path == LevelScenePath)
        {
            return;
        }

        SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelScenePath);
        if (sceneAsset)
        {
            EditorSceneManager.OpenScene(LevelScenePath, OpenSceneMode.Single);
        }
    }

    private static void BuildEntrySector(Transform parent)
    {
        InstantiateVisualPrefab(parent, "Entry_CorridorShell", $"{AssetBase}/Your_Hown_Prefabs/Corridor_Test_01.prefab",
            new Vector3(0f, 0f, -6f), Quaternion.identity, new Vector3(0.72f, 0.72f, 0.86f));

        AddPanel(parent, "Entry_BackWall_Reinforcement", new Vector3(0f, 1.6f, -12.02f), new Vector3(4.6f, 3.2f, 0.12f), Materials["Graphite"]);
        AddPanel(parent, "Entry_FloorOverlay", new Vector3(0f, 0.02f, -5.5f), new Vector3(4.05f, 0.04f, 13f), Materials["Floor"]);
        AddWallStrips(parent, -1.94f, -10f, 1f, true, Materials["EmergencyRed"]);
        AddWallStrips(parent, 1.94f, -10f, 1f, true, Materials["EmergencyRed"]);
    }

    private static void BuildMainPowerCorridor(Transform parent, Transform ceilingParent)
    {
        InstantiateVisualPrefab(parent, "MainCorridorShell_A", $"{AssetBase}/Your_Hown_Prefabs/Corridor_Test_02.prefab",
            new Vector3(0f, 0f, 0f), Quaternion.identity, new Vector3(0.74f, 0.74f, 0.9f));
        InstantiateVisualPrefab(parent, "MainCorridorShell_B", $"{AssetBase}/Your_Hown_Prefabs/Corridor_Test_02.prefab",
            new Vector3(0f, 0f, 11f), Quaternion.identity, new Vector3(0.74f, 0.74f, 0.9f));

        AddPanel(parent, "MainCorridor_FloorOverlay", new Vector3(0f, 0.025f, 7f), new Vector3(4.05f, 0.04f, 38f), Materials["Floor"]);
        AddRepeatedPrefab(parent, $"{AssetBase}/Prefabs/Floors/Floor_Third_01_Strip_2x6.prefab", "FloorStrip_Main", new Vector3(-0.85f, 0.055f, -8f), 7, new Vector3(0f, 0f, 5.5f), Quaternion.identity, Vector3.one);
        AddRepeatedPrefab(parent, $"{AssetBase}/Prefabs/Floors/Floor_Third_02_Strip_2x6.prefab", "FloorStrip_Main_R", new Vector3(0.85f, 0.055f, -8f), 7, new Vector3(0f, 0f, 5.5f), Quaternion.identity, Vector3.one);

        AddWallPanels(parent, -2.02f, -9f, 30f, true);
        AddWallPanels(parent, 2.02f, -9f, 30f, true);
        AddCeilingPanels(ceilingParent, 0f, 7f, 38f, 4f);
        AddPoweredStrip(parent, "MainCorridor_PoweredStrip_Left", new Vector3(-1.92f, 2.45f, 7f), new Vector3(0.08f, 0.08f, 32f));
        AddPoweredStrip(parent, "MainCorridor_PoweredStrip_Right", new Vector3(1.92f, 2.45f, 7f), new Vector3(0.08f, 0.08f, 32f));
    }

    private static void BuildGeneratorControlRoom(Transform parent, Transform propsParent)
    {
        InstantiateVisualPrefab(parent, "GeneratorRoomShell", $"{AssetBase}/Your_Hown_Prefabs/Simple_Room_02.prefab",
            new Vector3(11f, 0f, 3f), Quaternion.identity, new Vector3(1.08f, 0.82f, 0.95f));

        AddPanel(parent, "GeneratorRoom_FloorOverlay", new Vector3(11f, 0.03f, 3f), new Vector3(8f, 0.04f, 7f), Materials["Floor"]);
        AddWallPanels(parent, 15.0f, -0.3f, 6.7f, false);
        AddWallPanels(parent, 7.0f, -0.3f, 6.7f, false);
        AddPoweredStrip(parent, "GeneratorRoom_PoweredRing_North", new Vector3(11f, 2.5f, 6.48f), new Vector3(7.6f, 0.08f, 0.08f));
        AddPoweredStrip(parent, "GeneratorRoom_PoweredRing_South", new Vector3(11f, 2.5f, -0.48f), new Vector3(7.6f, 0.08f, 0.08f));

        Transform generator = FindTransform("Generator_Placeholder");
        if (generator)
        {
            GameObject visual = new GameObject("Visual_ScifiGeneratorAssembly");
            visual.transform.SetParent(generator, false);
            InstantiateVisualPrefab(visual.transform, "Generator_Airing", $"{AssetBase}/Prefabs/Props/Airing_01.prefab",
                new Vector3(0f, 0.65f, 0f), Quaternion.Euler(0f, 90f, 0f), new Vector3(1.7f, 1.4f, 1.7f));
            AddPanel(visual.transform, "Generator_EnergyCore", new Vector3(0f, 1.35f, 0f), new Vector3(1.8f, 0.35f, 1.8f), Materials["PoweredCyan"]);
        }

        Transform terminal = FindTransform("Terminal_Placeholder");
        if (terminal)
        {
            GameObject visual = new GameObject("Visual_ScifiConsole");
            visual.transform.SetParent(terminal, false);
            InstantiateVisualPrefab(visual.transform, "Terminal_WallConsole", $"{AssetBase}/Prefabs/Walls/Wall_Console_01_Half.prefab",
                new Vector3(0f, 0.3f, 0.15f), Quaternion.Euler(0f, 180f, 0f), new Vector3(0.55f, 0.55f, 0.55f));
            AddPanel(visual.transform, "Terminal_ActiveScreen", new Vector3(0f, 1.28f, -0.36f), new Vector3(1.05f, 0.58f, 0.035f), Materials["ScreenCyan"]);
        }

        AddDecorProp(propsParent, "Generator_Crate_A", $"{AssetBase}/Prefabs/Props/Crate_01.prefab", new Vector3(13.9f, 0.05f, 5.2f), Quaternion.Euler(0f, 30f, 0f), Vector3.one * 0.8f);
        AddDecorProp(propsParent, "Generator_Pipes_Wall", $"{AssetBase}/Prefabs/Stuff/Pipes_01.prefab", new Vector3(14.7f, 1.25f, 1.1f), Quaternion.Euler(0f, -90f, 0f), Vector3.one);
    }

    private static void BuildSecurityGateArea(Transform parent)
    {
        InstantiateVisualPrefab(parent, "SecurityGate_Frame", $"{AssetBase}/Prefabs/Doors/Vertical_Doors_Kit.prefab",
            new Vector3(0f, 0f, 8f), Quaternion.identity, new Vector3(0.72f, 0.82f, 0.72f));

        Transform door = FindTransform("SecurityDoor_Locked_Placeholder");
        if (door)
        {
            DisableChildRenderers(door);
            GameObject visual = new GameObject("Visual_ScifiSecurityDoor");
            visual.transform.SetParent(door, false);
            InstantiateVisualPrefab(visual.transform, "Door_Vert_Visual", $"{AssetBase}/Prefabs/Doors/Door_Vert_01.prefab",
                Vector3.zero, Quaternion.identity, new Vector3(0.86f, 1.08f, 0.12f));
            Renderer status = AddPanel(visual.transform, "Door_StatusLight", new Vector3(0f, 0.92f, -0.18f), new Vector3(1.6f, 0.08f, 0.04f), Materials["EmergencyRed"]).GetComponent<Renderer>();
            MarkSecurityStatus(status);
        }

        AddPanel(parent, "SecurityGate_FloorHazard", new Vector3(0f, 0.06f, 7.25f), new Vector3(3.5f, 0.04f, 0.8f), Materials["HazardAmber"]);
        AddEmergencyStrip(parent, "SecurityGate_RedIndicator", new Vector3(0f, 2.75f, 7.7f), new Vector3(2.5f, 0.08f, 0.05f));
    }

    private static void BuildSecuredMaintenanceCorridor(Transform parent, Transform ceilingParent)
    {
        InstantiateVisualPrefab(parent, "SecuredCorridorShell", $"{AssetBase}/Your_Hown_Prefabs/Corridor_Test_01.prefab",
            new Vector3(0f, 0f, 18f), Quaternion.identity, new Vector3(0.74f, 0.74f, 0.9f));

        AddPanel(parent, "SecuredCorridor_FloorOverlay", new Vector3(0f, 0.025f, 19.2f), new Vector3(4.05f, 0.04f, 12f), Materials["Floor"]);
        AddWallPanels(parent, -2.02f, 13f, 25f, true);
        AddWallPanels(parent, 2.02f, 13f, 25f, true);
        AddCeilingPanels(ceilingParent, 0f, 19f, 12f, 4f);
        AddPoweredStrip(parent, "SecuredCorridor_PoweredStrip_Left", new Vector3(-1.92f, 2.45f, 19f), new Vector3(0.08f, 0.08f, 11f));
        AddPoweredStrip(parent, "SecuredCorridor_PoweredStrip_Right", new Vector3(1.92f, 2.45f, 19f), new Vector3(0.08f, 0.08f, 11f));
    }

    private static void BuildPowerModuleVault(Transform parent, Transform propsParent)
    {
        InstantiateVisualPrefab(parent, "PowerModuleVaultShell", $"{AssetBase}/Your_Hown_Prefabs/Simple_Room_01.prefab",
            new Vector3(-5f, 0f, 16f), Quaternion.identity, new Vector3(0.68f, 0.76f, 0.68f));

        AddPanel(parent, "PowerModuleVault_FloorOverlay", new Vector3(-5f, 0.035f, 16f), new Vector3(4f, 0.04f, 4f), Materials["Floor"]);
        AddPoweredStrip(parent, "PowerModuleVault_FocusLight", new Vector3(-5f, 2.35f, 16f), new Vector3(2f, 0.08f, 2f));
        AddPanel(parent, "PowerModuleVault_PedestalGlow", new Vector3(-5f, 0.72f, 16f), new Vector3(1.5f, 0.06f, 1.5f), Materials["PoweredCyan"]);
        AddDecorProp(propsParent, "Vault_Fence_Left", $"{AssetBase}/Prefabs/Fences/Fence_Short_01.prefab", new Vector3(-6.25f, 0.1f, 17.35f), Quaternion.identity, Vector3.one * 0.7f);
        AddDecorProp(propsParent, "Vault_Fence_Right", $"{AssetBase}/Prefabs/Fences/Fence_Short_01.prefab", new Vector3(-3.75f, 0.1f, 17.35f), Quaternion.identity, Vector3.one * 0.7f);
    }

    private static void BuildSectorExit(Transform parent)
    {
        InstantiateVisualPrefab(parent, "SectorExitDoorway", $"{AssetBase}/Prefabs/Doors/DoorWay_01_Large.prefab",
            new Vector3(0f, 0f, 30.55f), Quaternion.identity, new Vector3(0.95f, 0.95f, 0.95f));
        AddPanel(parent, "SectorExit_GlowPanel", new Vector3(0f, 1.45f, 30.48f), new Vector3(3.2f, 2.6f, 0.05f), Materials["ExitGreen"]);
        AddPoweredStrip(parent, "SectorExit_CyanHeader", new Vector3(0f, 2.85f, 30.38f), new Vector3(3.6f, 0.08f, 0.05f));
    }

    private static void BuildSignage(Transform parent)
    {
        AddWorldLabel(parent, "Sign_EntryPowerResearchSector", "POWER RESEARCH SECTOR\nEMERGENCY ACCESS", new Vector3(0f, 2.15f, -9.6f), Quaternion.Euler(0f, 180f, 0f), 0.22f, Materials["TextCyan"]);
        AddWorldLabel(parent, "Sign_LockdownStatus", "AI CONTROL STATUS:\nLOCKDOWN", new Vector3(-1.9f, 1.75f, -3.5f), Quaternion.Euler(0f, 90f, 0f), 0.16f, Materials["TextRed"]);
        AddWorldLabel(parent, "Sign_SecurityGrid", "SECURITY GRID", new Vector3(0f, 2.15f, 5.8f), Quaternion.Euler(0f, 180f, 0f), 0.2f, Materials["TextRed"]);
        AddWorldLabel(parent, "Sign_GeneratorControl", "GENERATOR CONTROL ->", new Vector3(1.95f, 2.2f, 2.2f), Quaternion.Euler(0f, -90f, 0f), 0.17f, Materials["TextCyan"]);
        AddWorldLabel(parent, "Sign_PowerModuleVault", "POWER CONTROL MODULE", new Vector3(-5f, 2.25f, 13.95f), Quaternion.identity, 0.17f, Materials["TextCyan"]);
        AddWorldLabel(parent, "Sign_NextSector", "NEXT SECTOR\nSECURITY NETWORK", new Vector3(0f, 2.15f, 30.25f), Quaternion.Euler(0f, 180f, 0f), 0.18f, Materials["TextCyan"]);
    }

    private static void BuildLighting(Transform parent)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.14f, 0.11f, 0.11f);

        AddSceneLight(parent, "CQ_Key_Emergency_Entry", LightType.Point, new Vector3(0f, 2.4f, -5f), Quaternion.identity, new Color(1f, 0.28f, 0.16f), 2.2f, 8f, LightShadows.None);
        AddSceneLight(parent, "CQ_Key_Emergency_SecurityGate", LightType.Point, new Vector3(0f, 2.6f, 6.8f), Quaternion.identity, new Color(1f, 0.24f, 0.16f), 2.6f, 8f, LightShadows.None);
        AddSceneLight(parent, "CQ_Key_GeneratorRoom", LightType.Point, new Vector3(11f, 2.5f, 3f), Quaternion.identity, new Color(0.55f, 0.9f, 1f), 2.3f, 8f, LightShadows.None);
        AddSceneLight(parent, "CQ_Key_PowerModule", LightType.Point, new Vector3(-5f, 2.2f, 16f), Quaternion.identity, new Color(0.5f, 1f, 0.9f), 3.4f, 5f, LightShadows.None);
        AddSceneLight(parent, "CQ_Key_Exit", LightType.Point, new Vector3(0f, 2.4f, 28.8f), Quaternion.identity, new Color(0.45f, 1f, 0.75f), 2.4f, 7f, LightShadows.None);
    }

    private static void BuildAudio(Transform parent)
    {
        AudioClip ambience = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AssetBase}/Sounds/Ambiant_Loop.wav");
        AudioClip machinery = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AssetBase}/Sounds/Machin_Loop.wav");
        AudioClip computer = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AssetBase}/Sounds/Computer_loop.wav");
        AudioClip door = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AssetBase}/Sounds/Pneumatic-door.wav");

        Level1AudioDirector director = parent.gameObject.AddComponent<Level1AudioDirector>();
        AudioSource emergencySource = AddAudioSource(parent, "Audio_EmergencyAmbience_2D", ambience, Vector3.zero, true, false, 0.38f, 0f, 1f, 500f);
        AudioSource poweredSource = AddAudioSource(parent, "Audio_PoweredMachinery_2D", machinery, Vector3.zero, true, false, 0f, 0f, 1f, 500f);
        AudioSource generatorSource = AddAudioSource(parent, "Audio_Generator_3D", machinery, new Vector3(12.3f, 1.4f, 3f), false, true, 0.9f, 1f, 1f, 12f);
        AudioSource doorSource = AddAudioSource(parent, "Audio_SecurityDoor_3D", door, new Vector3(0f, 1.5f, 8f), false, true, 0.85f, 1f, 1f, 10f);
        AudioSource uiSource = AddAudioSource(parent, "Audio_UI_2D", computer, Vector3.zero, false, false, 0.75f, 0f, 1f, 500f);
        director.Configure(emergencySource, poweredSource, generatorSource, doorSource, uiSource, machinery, door, computer, computer);
    }

    private static void OptimizeAudioImports()
    {
        ConfigureAudioImport($"{AssetBase}/Sounds/Ambiant_Loop.wav", AudioClipLoadType.CompressedInMemory);
        ConfigureAudioImport($"{AssetBase}/Sounds/Machin_Loop.wav", AudioClipLoadType.CompressedInMemory);
        ConfigureAudioImport($"{AssetBase}/Sounds/Pneumatic-door.wav", AudioClipLoadType.CompressedInMemory);
        ConfigureAudioImport($"{AssetBase}/Sounds/Computer_loop.wav", AudioClipLoadType.Streaming);
    }

    private static void ConfigureAudioImport(string assetPath, AudioClipLoadType loadType)
    {
        AudioImporter importer = AssetImporter.GetAtPath(assetPath) as AudioImporter;
        if (!importer)
        {
            return;
        }

        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.55f;
        settings.loadType = loadType;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = false;
        EditorUtility.SetDirty(importer);
        importer.SaveAndReimport();
    }

    private static void WireVisualAndAudioState()
    {
        Level1FlowController flow = Object.FindFirstObjectByType<Level1FlowController>();
        TerminalInteractable terminal = Object.FindFirstObjectByType<TerminalInteractable>();
        GeneratorController generator = Object.FindFirstObjectByType<GeneratorController>();
        FacilityPowerController power = Object.FindFirstObjectByType<FacilityPowerController>();
        DoorController door = Object.FindFirstObjectByType<DoorController>();
        PlayerInteraction player = Object.FindFirstObjectByType<PlayerInteraction>();
        PowerModulePickup module = Object.FindFirstObjectByType<PowerModulePickup>();
        LevelExitTrigger exit = Object.FindFirstObjectByType<LevelExitTrigger>();
        HardwareHudController hud = Object.FindFirstObjectByType<HardwareHudController>();
        LevelCompletionUI completion = Object.FindFirstObjectByType<LevelCompletionUI>();
        Level1AudioDirector audio = Object.FindFirstObjectByType<Level1AudioDirector>();

        Level1VisualStateController visualState = GameObject.Find(VisualRootName).AddComponent<Level1VisualStateController>();
        visualState.Configure(
            FindMarkedRenderers("CQ_Emergency"),
            FindMarkedRenderers("CQ_Powered"),
            FindMarkedRenderers("CQ_SecurityStatus"),
            Materials["EmergencyRed"],
            Materials["PoweredCyan"],
            Materials["EmergencyRed"],
            Materials["ExitGreen"]);

        if (flow && terminal && generator && power && door && player && module && exit)
        {
            flow.Configure(terminal, generator, power, door, player, module, exit, hud, completion, audio, visualState);
        }
    }

    private static GameObject InstantiateVisualPrefab(Transform parent, string name, string assetPath, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (!prefab)
        {
            Debug.LogWarning($"ClassQuest environment missing prefab: {assetPath}");
            return AddPanel(parent, $"{name}_Fallback", position + Vector3.up * 1.5f, Vector3.one, Materials["Graphite"]);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        instance.name = name;
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.transform.localScale = scale;
        SanitizeImportedVisual(instance);
        return instance;
    }

    private static void AddRepeatedPrefab(Transform parent, string assetPath, string namePrefix, Vector3 start, int count, Vector3 step, Quaternion rotation, Vector3 scale)
    {
        for (int i = 0; i < count; i++)
        {
            InstantiateVisualPrefab(parent, $"{namePrefix}_{i + 1:00}", assetPath, start + step * i, rotation, scale);
        }
    }

    private static void AddDecorProp(Transform parent, string name, string assetPath, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        InstantiateVisualPrefab(parent, name, assetPath, position, rotation, scale);
    }

    private static void SanitizeImportedVisual(GameObject root)
    {
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }

        foreach (Light light in root.GetComponentsInChildren<Light>(true))
        {
            light.enabled = false;
            light.shadows = LightShadows.None;
        }

        foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
        {
            Object.DestroyImmediate(behaviour);
        }

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            renderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
    }

    private static void HideGreyboxRenderers()
    {
        GameObject greybox = GameObject.Find(GreyboxRootName);
        if (!greybox)
        {
            return;
        }

        string[] hideGroups = { "Floors", "Walls", "DoorFrames", "Signage" };
        foreach (string groupName in hideGroups)
        {
            Transform group = greybox.transform.Find(groupName);
            if (!group)
            {
                continue;
            }

            foreach (Renderer renderer in group.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }
        }
    }

    private static void RestoreGreyboxRenderers()
    {
        GameObject greybox = GameObject.Find(GreyboxRootName);
        if (!greybox)
        {
            return;
        }

        foreach (Renderer renderer in greybox.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = true;
        }
    }

    private static void RemoveEnvironmentInternal(bool restoreGreybox)
    {
        GameObject existing = GameObject.Find(VisualRootName);
        if (existing)
        {
            Object.DestroyImmediate(existing);
        }

        RemoveVisualAudioDirectors();

        if (restoreGreybox)
        {
            RestoreGreyboxRenderers();
        }
    }

    private static void RemoveVisualAudioDirectors()
    {
        foreach (Level1AudioDirector director in Object.FindObjectsByType<Level1AudioDirector>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(director);
        }

        foreach (Level1VisualStateController controller in Object.FindObjectsByType<Level1VisualStateController>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(controller);
        }
    }

    private static void BuildWallPanel(Transform parent, string name, Vector3 position, Vector3 scale, Quaternion rotation)
    {
        GameObject panel = InstantiateVisualPrefab(parent, name, $"{AssetBase}/Prefabs/Walls/Wall_Simple_01_Long.prefab", position, rotation, scale);
        panel.transform.localScale = scale;
    }

    private static void AddWallPanels(Transform parent, float x, float zStart, float zEnd, bool alongZ)
    {
        float step = 5.8f;
        int count = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(zEnd - zStart) / step));
        for (int i = 0; i < count; i++)
        {
            float z = Mathf.Lerp(zStart, zEnd, count == 1 ? 0.5f : i / (float)(count - 1));
            Quaternion rotation = alongZ ? Quaternion.Euler(0f, x < 0f ? 90f : -90f, 0f) : Quaternion.identity;
            Vector3 position = alongZ ? new Vector3(x, 1.55f, z) : new Vector3(x, 1.55f, z);
            BuildWallPanel(parent, $"WallPanel_{parent.name}_{i + 1:00}", position, Vector3.one * 0.68f, rotation);
        }
    }

    private static void AddCeilingPanels(Transform parent, float x, float zCenter, float length, float width)
    {
        int count = Mathf.Max(1, Mathf.CeilToInt(length / 5.8f));
        float start = zCenter - length * 0.5f + 2.9f;
        for (int i = 0; i < count; i++)
        {
            InstantiateVisualPrefab(parent, $"CeilingPanel_{i + 1:00}", $"{AssetBase}/Prefabs/Roofs/Roof_Squared_01_6x6.prefab",
                new Vector3(x, 3.05f, start + i * 5.8f), Quaternion.identity, new Vector3(width / 6f, 0.65f, 0.96f));
        }
    }

    private static void AddWallStrips(Transform parent, float x, float zStart, float zEnd, bool alongZ, Material material)
    {
        float center = (zStart + zEnd) * 0.5f;
        float length = Mathf.Abs(zEnd - zStart);
        AddPanel(parent, $"EmergencyWallStrip_{x}_{center}", new Vector3(x, 2.35f, center), new Vector3(0.06f, 0.08f, length), material);
    }

    private static Renderer AddPoweredStrip(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        Renderer renderer = AddPanel(parent, name, position, scale, Materials["PoweredCyan"]).GetComponent<Renderer>();
        MarkPowered(renderer);
        return renderer;
    }

    private static Renderer AddEmergencyStrip(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        Renderer renderer = AddPanel(parent, name, position, scale, Materials["EmergencyRed"]).GetComponent<Renderer>();
        MarkEmergency(renderer);
        return renderer;
    }

    private static GameObject AddPanel(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        Object.DestroyImmediate(cube.GetComponent<Collider>());
        Renderer renderer = cube.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return cube;
    }

    private static void AddWorldLabel(Transform parent, string name, string text, Vector3 position, Quaternion rotation, float size, Material material)
    {
        GameObject label = new GameObject(name);
        label.transform.SetParent(parent);
        label.transform.SetPositionAndRotation(position, rotation);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = size;
        textMesh.fontSize = 72;

        MeshRenderer renderer = label.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static Light AddSceneLight(Transform parent, string name, LightType type, Vector3 position, Quaternion rotation, Color color, float intensity, float range, LightShadows shadows)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent);
        lightObject.transform.SetPositionAndRotation(position, rotation);
        Light light = lightObject.AddComponent<Light>();
        light.type = type;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = shadows;
        return light;
    }

    private static AudioSource AddAudioSource(Transform parent, string name, AudioClip clip, Vector3 position, bool loop, bool spatial, float volume, float spatialBlend, float minDistance, float maxDistance)
    {
        GameObject audioObject = new GameObject(name);
        audioObject.transform.SetParent(parent);
        audioObject.transform.position = position;
        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = spatial ? spatialBlend : 0f;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        return source;
    }

    private static Transform CreateSection(GameObject root, string name)
    {
        GameObject section = new GameObject(name);
        section.transform.SetParent(root.transform);
        return section.transform;
    }

    private static Transform FindTransform(string name)
    {
        GameObject found = GameObject.Find(name);
        return found ? found.transform : null;
    }

    private static void DisableChildRenderers(Transform root)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }
    }

    private static readonly List<Renderer> EmergencyRenderers = new List<Renderer>();
    private static readonly List<Renderer> PoweredRenderers = new List<Renderer>();
    private static readonly List<Renderer> SecurityStatusRenderers = new List<Renderer>();

    private static void MarkEmergency(Renderer renderer)
    {
        if (renderer && !EmergencyRenderers.Contains(renderer))
        {
            EmergencyRenderers.Add(renderer);
        }
    }

    private static void MarkPowered(Renderer renderer)
    {
        if (renderer && !PoweredRenderers.Contains(renderer))
        {
            PoweredRenderers.Add(renderer);
        }
    }

    private static void MarkSecurityStatus(Renderer renderer)
    {
        if (renderer && !SecurityStatusRenderers.Contains(renderer))
        {
            SecurityStatusRenderers.Add(renderer);
        }
    }

    private static Renderer[] FindMarkedRenderers(string marker)
    {
        if (marker == "CQ_Emergency")
        {
            return EmergencyRenderers.ToArray();
        }

        if (marker == "CQ_Powered")
        {
            return PoweredRenderers.ToArray();
        }

        return SecurityStatusRenderers.ToArray();
    }

    private static void CacheMaterials()
    {
        Materials.Clear();
        Materials["Graphite"] = GetOrCreateMaterial("CQ_Graphite", new Color(0.07f, 0.08f, 0.09f), Color.black, false);
        Materials["Floor"] = GetOrCreateMaterial("CQ_DarkSteelFloor", new Color(0.18f, 0.21f, 0.23f), Color.black, false);
        Materials["EmergencyRed"] = GetOrCreateMaterial("CQ_EmergencyRed_Emissive", new Color(0.7f, 0.06f, 0.04f), new Color(1f, 0.08f, 0.03f) * 1.5f, true);
        Materials["HazardAmber"] = GetOrCreateMaterial("CQ_HazardAmber", new Color(0.9f, 0.48f, 0.08f), new Color(1f, 0.42f, 0.02f) * 0.7f, true);
        Materials["PoweredCyan"] = GetOrCreateMaterial("CQ_PoweredCyan_Emissive", new Color(0.05f, 0.55f, 0.65f), new Color(0.1f, 1f, 0.85f) * 1.8f, true);
        Materials["ScreenCyan"] = GetOrCreateMaterial("CQ_ScreenCyan_Unlit", new Color(0.08f, 0.9f, 0.75f), new Color(0.08f, 0.9f, 0.75f) * 2f, true);
        Materials["ExitGreen"] = GetOrCreateMaterial("CQ_ExitGreen_Emissive", new Color(0.1f, 0.75f, 0.32f), new Color(0.2f, 1f, 0.45f) * 1.6f, true);
        Materials["TextCyan"] = GetOrCreateMaterial("CQ_TextCyan", new Color(0.5f, 1f, 0.95f), new Color(0.2f, 1f, 0.9f), true);
        Materials["TextRed"] = GetOrCreateMaterial("CQ_TextRed", new Color(1f, 0.28f, 0.18f), new Color(1f, 0.1f, 0.05f), true);

        EmergencyRenderers.Clear();
        PoweredRenderers.Clear();
        SecurityStatusRenderers.Clear();
    }

    private static Material GetOrCreateMaterial(string materialName, Color baseColor, Color emission, bool emissive)
    {
        string path = $"{MaterialsFolder}/{materialName}.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = Shader.Find("Universal Render Pipeline/Lit");
        material.SetColor("_BaseColor", baseColor);
        material.color = baseColor;

        if (emissive)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission);
        }
        else
        {
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
        }

        EditorUtility.SetDirty(material);
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
