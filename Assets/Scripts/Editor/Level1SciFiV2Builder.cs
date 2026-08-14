using System.Collections.Generic;
using RogueAI.Interaction;
using RogueAI.Level;
using StarterAssets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Level1SciFiV2Builder
{
    private const string LevelScenePath = "Assets/Scenes/Level1_PowerSector.unity";
    private const string FailedVisualRootName = "Level1_VisualEnvironment";
    private const string V2EnvironmentRootName = "Level1_Environment_V2";
    private const string V2CollisionRootName = "Level1_Collision";
    private const string V2LightingRootName = "Level1_Lighting";
    private const string V2AudioRootName = "Level1_Audio";
    private const string GreyboxRootName = "Level1_PowerSector_Greybox";
    private const string MaterialFolder = "Assets/Level1/V2Materials";
    private const string PackageRoot = "Assets/Creepy_Cat/3D Scifi Kit Starter Kit_HD";

    private const string Corridor01Path = PackageRoot + "/Your_Hown_Prefabs/Corridor_Test_01.prefab";
    private const string Corridor02Path = PackageRoot + "/Your_Hown_Prefabs/Corridor_Test_02.prefab";
    private const string GeneratorRoomPath = PackageRoot + "/Your_Hown_Prefabs/Simple_Room_02.prefab";
    private const string DoorPanelPath = PackageRoot + "/Prefabs/Doors/Door_Vert_01.prefab";
    private const string DoorFramePath = PackageRoot + "/Prefabs/Doors/DoorWay_01_Large.prefab";
    private const string TerminalConsolePath = PackageRoot + "/Prefabs/Walls/Wall_Console_01_Half.prefab";
    private const string GeneratorPropPath = PackageRoot + "/Prefabs/Props/Airing_01.prefab";

    private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();
    private static readonly List<Renderer> EmergencyRenderers = new List<Renderer>();
    private static readonly List<Renderer> PoweredRenderers = new List<Renderer>();
    private static readonly List<Renderer> SecurityStatusRenderers = new List<Renderer>();

    private struct ModulePlacement
    {
        public readonly string Name;
        public readonly string Path;
        public readonly Vector3 Center;
        public readonly bool CorridorShouldRunZ;

        public ModulePlacement(string name, string path, Vector3 center, bool corridorShouldRunZ)
        {
            Name = name;
            Path = path;
            Center = center;
            CorridorShouldRunZ = corridorShouldRunZ;
        }
    }

    [MenuItem("Tools/ClassQuest/Build Level 1 Sci-Fi V2")]
    public static void BuildStage1()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (!EnsureLevelSceneOpen())
        {
            return;
        }

        if (!ValidateRequiredAssets())
        {
            return;
        }

        CacheMaterials();
        EmergencyRenderers.Clear();
        PoweredRenderers.Clear();
        SecurityStatusRenderers.Clear();
        CleanupFailedAndV2Environment();
        RestoreGreybox();

        GameObject environmentRoot = new GameObject(V2EnvironmentRootName);
        GameObject entry = CreateChild(environmentRoot, "Entry");
        GameObject mainCorridor = CreateChild(environmentRoot, "MainCorridor");
        GameObject generatorRoom = CreateChild(environmentRoot, "GeneratorRoom");
        GameObject securityGate = CreateChild(environmentRoot, "SecurityGate");

        BuildPrebuiltStage1Modules(entry.transform, mainCorridor.transform, generatorRoom.transform);
        BuildStructuralFillers(environmentRoot.transform);
        BuildSecurityGateVisuals(securityGate.transform);
        MoveStage1GameplayAnchors(generatorRoom.transform, securityGate.transform);
        BuildStage1Collision();
        Level1VisualStateController visualState = BuildStage1LightingAndVisualState();
        Level1AudioDirector audioDirector = BuildStage1Audio();
        HideCoveredGreyboxRenderers();
        DisableGreyboxLighting();
        WireLevelFlow(audioDirector, visualState);

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "ClassQuest Sci-Fi V2",
            "Stage 1 built: Entry, Main Corridor, Generator Room, and Security Gate.\n\nOpen Play Mode and inspect from the first-person camera before continuing.",
            "OK");
    }

    [MenuItem("Tools/ClassQuest/Remove Level 1 Sci-Fi V2")]
    public static void RemoveV2()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        if (!EnsureLevelSceneOpen())
        {
            return;
        }

        DeleteIfExists(V2EnvironmentRootName);
        DeleteIfExists(V2CollisionRootName);
        DeleteIfExists(V2LightingRootName);
        DeleteIfExists(V2AudioRootName);
        RestoreGreybox();
        EnableGreyboxLighting();
        WireLevelFlow(null, null);

        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "ClassQuest Sci-Fi V2",
            "V2 environment removed. Greybox renderers and greybox lighting were restored.",
            "OK");
    }

    private static bool EnsureLevelSceneOpen()
    {
        if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(LevelScenePath))
        {
            EditorUtility.DisplayDialog("ClassQuest Sci-Fi V2", $"Could not find:\n\n{LevelScenePath}", "OK");
            return false;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.path != LevelScenePath)
        {
            EditorSceneManager.OpenScene(LevelScenePath, OpenSceneMode.Single);
        }

        return true;
    }

    private static bool ValidateRequiredAssets()
    {
        string[] required =
        {
            Corridor01Path,
            Corridor02Path,
            GeneratorRoomPath,
            DoorPanelPath,
            DoorFramePath,
            TerminalConsolePath,
            GeneratorPropPath
        };

        foreach (string path in required)
        {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(path))
            {
                EditorUtility.DisplayDialog("ClassQuest Sci-Fi V2", $"Missing required prefab:\n\n{path}", "OK");
                return false;
            }
        }

        return true;
    }

    private static void CleanupFailedAndV2Environment()
    {
        DeleteIfExists(FailedVisualRootName);
        DeleteIfExists(V2EnvironmentRootName);
        DeleteIfExists(V2CollisionRootName);
        DeleteIfExists(V2LightingRootName);
        DeleteIfExists(V2AudioRootName);

        foreach (Level1AudioDirector director in Object.FindObjectsByType<Level1AudioDirector>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(director.gameObject);
        }

        foreach (Level1VisualStateController visualState in Object.FindObjectsByType<Level1VisualStateController>(FindObjectsSortMode.None))
        {
            Object.DestroyImmediate(visualState.gameObject);
        }
    }

    private static void RestoreGreybox()
    {
        GameObject greybox = GameObject.Find(GreyboxRootName);
        if (!greybox)
        {
            Debug.LogWarning($"Could not restore greybox renderers because {GreyboxRootName} was not found.");
            return;
        }

        foreach (Renderer renderer in greybox.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = true;
        }

        foreach (Collider collider in greybox.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = true;
        }
    }

    private static void BuildPrebuiltStage1Modules(Transform entry, Transform mainCorridor, Transform generatorRoom)
    {
        ModulePlacement[] placements =
        {
            new ModulePlacement("Prebuilt_Entry_Corridor_Test_01", Corridor01Path, new Vector3(0f, 1.5f, -7.2f), true),
            new ModulePlacement("Prebuilt_Main_Corridor_Test_02", Corridor02Path, new Vector3(0f, 1.5f, 1.2f), true),
            new ModulePlacement("Prebuilt_Generator_Room_Simple_02", GeneratorRoomPath, new Vector3(11f, 1.5f, 3f), false)
        };

        InstantiatePrebuiltModule(placements[0], entry);
        InstantiatePrebuiltModule(placements[1], mainCorridor);
        InstantiatePrebuiltModule(placements[2], generatorRoom);
    }

    private static void BuildStructuralFillers(Transform parent)
    {
        GameObject fillers = CreateChild(parent.gameObject, "StructuralFillers");

        AddVisualBox(fillers.transform, "Ceiling_MainCorridor", new Vector3(0f, 3.08f, -1.9f), new Vector3(4.5f, 0.18f, 19.8f), "Ceiling");
        AddVisualBox(fillers.transform, "Ceiling_GeneratorBranch", new Vector3(4.6f, 3.08f, 3f), new Vector3(5.2f, 0.18f, 4.5f), "Ceiling");
        AddVisualBox(fillers.transform, "Ceiling_GeneratorRoom", new Vector3(11f, 3.08f, 3f), new Vector3(8.5f, 0.18f, 7.5f), "Ceiling");

        AddVisualBox(fillers.transform, "Entry_Back_Seal", new Vector3(0f, 1.55f, -12.15f), new Vector3(4.6f, 3.1f, 0.25f), "Wall");
        AddVisualBox(fillers.transform, "MainCorridor_Left_Seal", new Vector3(-2.22f, 1.55f, -1.9f), new Vector3(0.24f, 3.1f, 19.8f), "Wall");
        AddVisualBox(fillers.transform, "MainCorridor_Right_BeforeBranch_Seal", new Vector3(2.22f, 1.55f, -5.6f), new Vector3(0.24f, 3.1f, 12.9f), "Wall");
        AddVisualBox(fillers.transform, "MainCorridor_Right_AfterBranch_Seal", new Vector3(2.22f, 1.55f, 6.85f), new Vector3(0.24f, 3.1f, 2.35f), "Wall");

        AddVisualBox(fillers.transform, "GeneratorBranch_North_Seal", new Vector3(4.6f, 1.55f, 5.22f), new Vector3(5.2f, 3.1f, 0.24f), "Wall");
        AddVisualBox(fillers.transform, "GeneratorBranch_South_Seal", new Vector3(4.6f, 1.55f, 0.78f), new Vector3(5.2f, 3.1f, 0.24f), "Wall");

        AddVisualBox(fillers.transform, "GeneratorRoom_East_Seal", new Vector3(15.25f, 1.55f, 3f), new Vector3(0.24f, 3.1f, 7.5f), "Wall");
        AddVisualBox(fillers.transform, "GeneratorRoom_North_Seal", new Vector3(11f, 1.55f, 6.75f), new Vector3(8.5f, 3.1f, 0.24f), "Wall");
        AddVisualBox(fillers.transform, "GeneratorRoom_South_Seal", new Vector3(11f, 1.55f, -0.75f), new Vector3(8.5f, 3.1f, 0.24f), "Wall");
        AddVisualBox(fillers.transform, "GeneratorRoom_West_Lower_Seal", new Vector3(6.75f, 1.55f, 0.15f), new Vector3(0.24f, 3.1f, 1.8f), "Wall");
        AddVisualBox(fillers.transform, "GeneratorRoom_West_Upper_Seal", new Vector3(6.75f, 1.55f, 5.85f), new Vector3(0.24f, 3.1f, 1.8f), "Wall");

        AddVisualBox(fillers.transform, "Floor_MainCorridor_Dark", new Vector3(0f, -0.06f, -1.9f), new Vector3(4.5f, 0.12f, 19.8f), "Floor");
        AddVisualBox(fillers.transform, "Floor_GeneratorBranch_Dark", new Vector3(4.6f, -0.06f, 3f), new Vector3(5.2f, 0.12f, 4.5f), "Floor");
        AddVisualBox(fillers.transform, "Floor_GeneratorRoom_Dark", new Vector3(11f, -0.06f, 3f), new Vector3(8.5f, 0.12f, 7.5f), "Floor");

        AddVisualBox(fillers.transform, "PoweredStrip_MainCorridor_Left", new Vector3(-1.82f, 2.55f, -1.9f), new Vector3(0.08f, 0.08f, 15.5f), "Powered");
        AddVisualBox(fillers.transform, "PoweredStrip_MainCorridor_Right", new Vector3(1.82f, 2.55f, -1.9f), new Vector3(0.08f, 0.08f, 15.5f), "Powered");
        AddVisualBox(fillers.transform, "PoweredStrip_GeneratorRoom_Back", new Vector3(11f, 2.55f, 6.5f), new Vector3(5.8f, 0.08f, 0.08f), "Powered");
        AddVisualBox(fillers.transform, "PoweredStrip_GeneratorBranch", new Vector3(4.6f, 2.55f, 5.02f), new Vector3(4.4f, 0.08f, 0.08f), "Powered");
    }

    private static void BuildSecurityGateVisuals(Transform securityGate)
    {
        GameObject frame = InstantiatePrefab(DoorFramePath, securityGate, "Prebuilt_SecurityGate_DoorFrame");
        if (frame)
        {
            frame.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            CenterByRendererBounds(frame, new Vector3(0f, 1.55f, 8f));
            SanitizeVisualInstance(frame, false);
        }

        AddVisualBox(securityGate, "SecurityGate_Frame_Left", new Vector3(-1.86f, 1.55f, 8f), new Vector3(0.28f, 3.1f, 0.55f), "Frame");
        AddVisualBox(securityGate, "SecurityGate_Frame_Right", new Vector3(1.86f, 1.55f, 8f), new Vector3(0.28f, 3.1f, 0.55f), "Frame");
        AddVisualBox(securityGate, "SecurityGate_Frame_Top", new Vector3(0f, 3.05f, 8f), new Vector3(4.1f, 0.32f, 0.55f), "Frame");

        AddVisualBox(securityGate, "SecurityGate_RedStatus_Left", new Vector3(-1.45f, 2.4f, 7.67f), new Vector3(0.2f, 0.2f, 0.06f), "Emergency");
        AddVisualBox(securityGate, "SecurityGate_RedStatus_Right", new Vector3(1.45f, 2.4f, 7.67f), new Vector3(0.2f, 0.2f, 0.06f), "Emergency");
        AddWorldLabel(securityGate, "Sign_SecurityGate", "SECURITY GATE\nPOWER OFFLINE", new Vector3(0f, 2.55f, 7.64f), Quaternion.Euler(0f, 180f, 0f), 0.3f, "Emergency");
    }

    private static void MoveStage1GameplayAnchors(Transform generatorRoom, Transform securityGate)
    {
        GameObject terminal = GameObject.Find("Terminal_Placeholder");
        if (terminal)
        {
            MoveByRendererBounds(terminal, new Vector3(8.4f, 0.95f, 1.22f));
            terminal.transform.rotation = Quaternion.identity;
            HideChildRenderers(terminal);

            GameObject terminalVisual = InstantiatePrefab(TerminalConsolePath, terminal.transform, "Visual_Prebuilt_TerminalConsole");
            if (terminalVisual)
            {
                terminalVisual.transform.rotation = Quaternion.identity;
                CenterByRendererBounds(terminalVisual, new Vector3(8.4f, 1.05f, 1.05f));
                SanitizeVisualInstance(terminalVisual, false);
            }
        }

        GameObject generator = GameObject.Find("Generator_Placeholder");
        if (generator)
        {
            MoveByRendererBounds(generator, new Vector3(12.4f, 1.05f, 3.05f));
            generator.transform.rotation = Quaternion.identity;

            GameObject generatorVisual = InstantiatePrefab(GeneratorPropPath, generator.transform, "Visual_Prebuilt_GeneratorMachinery");
            if (generatorVisual)
            {
                generatorVisual.transform.rotation = Quaternion.identity;
                CenterByRendererBounds(generatorVisual, new Vector3(12.4f, 1.2f, 3.05f));
                SanitizeVisualInstance(generatorVisual, false);
            }
        }

        GameObject door = GameObject.Find("SecurityDoor_Locked_Placeholder");
        if (door)
        {
            DoorController doorController = door.GetComponent<DoorController>();
            door.transform.SetPositionAndRotation(new Vector3(0f, 1.45f, 8f), Quaternion.identity);
            HideChildRenderers(door);

            GameObject doorVisual = InstantiatePrefab(DoorPanelPath, door.transform, "Visual_Prebuilt_MovingDoorPanel");
            if (doorVisual)
            {
                doorVisual.transform.rotation = Quaternion.identity;
                CenterByRendererBounds(doorVisual, new Vector3(0f, 1.45f, 8f));
                SanitizeVisualInstance(doorVisual, false);
            }

            TextMesh statusLabel = GameObject.Find("Sign_SecurityGate")?.GetComponent<TextMesh>();
            if (doorController)
            {
                doorController.Configure(door.transform, statusLabel);
                doorController.Lock();
            }
        }
    }

    private static void BuildStage1Collision()
    {
        GameObject root = new GameObject(V2CollisionRootName);

        AddCollisionBox(root.transform, "Collision_Entry_Back", new Vector3(0f, 1.5f, -12.25f), new Vector3(4.8f, 3f, 0.32f));
        AddCollisionBox(root.transform, "Collision_Main_Left", new Vector3(-2.25f, 1.5f, -1.9f), new Vector3(0.32f, 3f, 20.1f));
        AddCollisionBox(root.transform, "Collision_Main_Right_BeforeBranch", new Vector3(2.25f, 1.5f, -5.65f), new Vector3(0.32f, 3f, 13.2f));
        AddCollisionBox(root.transform, "Collision_Main_Right_AfterBranch", new Vector3(2.25f, 1.5f, 6.85f), new Vector3(0.32f, 3f, 2.4f));

        AddCollisionBox(root.transform, "Collision_GeneratorBranch_North", new Vector3(4.6f, 1.5f, 5.25f), new Vector3(5.3f, 3f, 0.32f));
        AddCollisionBox(root.transform, "Collision_GeneratorBranch_South", new Vector3(4.6f, 1.5f, 0.75f), new Vector3(5.3f, 3f, 0.32f));

        AddCollisionBox(root.transform, "Collision_GeneratorRoom_East", new Vector3(15.28f, 1.5f, 3f), new Vector3(0.32f, 3f, 7.7f));
        AddCollisionBox(root.transform, "Collision_GeneratorRoom_North", new Vector3(11f, 1.5f, 6.78f), new Vector3(8.7f, 3f, 0.32f));
        AddCollisionBox(root.transform, "Collision_GeneratorRoom_South", new Vector3(11f, 1.5f, -0.78f), new Vector3(8.7f, 3f, 0.32f));
        AddCollisionBox(root.transform, "Collision_GeneratorRoom_West_Lower", new Vector3(6.72f, 1.5f, 0.1f), new Vector3(0.32f, 3f, 1.8f));
        AddCollisionBox(root.transform, "Collision_GeneratorRoom_West_Upper", new Vector3(6.72f, 1.5f, 5.9f), new Vector3(0.32f, 3f, 1.8f));

        AddCollisionBox(root.transform, "Collision_SecurityGate_Left_Block", new Vector3(-1.95f, 1.5f, 8f), new Vector3(0.35f, 3f, 0.7f));
        AddCollisionBox(root.transform, "Collision_SecurityGate_Right_Block", new Vector3(1.95f, 1.5f, 8f), new Vector3(0.35f, 3f, 0.7f));
    }

    private static Level1VisualStateController BuildStage1LightingAndVisualState()
    {
        GameObject root = new GameObject(V2LightingRootName);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.12f, 0.10f, 0.09f);

        Light emergencyEntry = AddPointLight(root.transform, "Emergency_Entry_Red", new Vector3(0f, 2.45f, -7f), 2.2f, 6.5f, new Color(1f, 0.25f, 0.16f));
        Light emergencyGate = AddPointLight(root.transform, "Emergency_SecurityGate_Red", new Vector3(0f, 2.55f, 6.6f), 2.5f, 6.5f, new Color(1f, 0.20f, 0.14f));
        Light emergencyGenerator = AddPointLight(root.transform, "Emergency_GeneratorRoom_Amber", new Vector3(11f, 2.6f, 3f), 2.4f, 7f, new Color(1f, 0.46f, 0.22f));

        Light poweredCorridor = AddPointLight(root.transform, "Powered_MainCorridor_Cyan", new Vector3(0f, 2.65f, -0.8f), 0f, 8f, new Color(0.68f, 0.95f, 1f));
        Light poweredGate = AddPointLight(root.transform, "Powered_SecurityGate_Cyan", new Vector3(0f, 2.65f, 8.4f), 0f, 7f, new Color(0.55f, 1f, 0.90f));
        Light poweredGenerator = AddPointLight(root.transform, "Powered_GeneratorRoom_White", new Vector3(11f, 2.65f, 3f), 0f, 8f, new Color(0.85f, 0.98f, 1f));

        FacilityPowerController powerController = root.AddComponent<FacilityPowerController>();
        powerController.Configure(
            new[] { emergencyEntry, emergencyGate, emergencyGenerator },
            new[] { poweredCorridor, poweredGate, poweredGenerator });
        powerController.ApplyInitialPowerOffState();

        Level1VisualStateController visualState = root.AddComponent<Level1VisualStateController>();
        visualState.Configure(
            EmergencyRenderers.ToArray(),
            PoweredRenderers.ToArray(),
            SecurityStatusRenderers.ToArray(),
            Materials["Emergency"],
            Materials["Powered"],
            Materials["Emergency"],
            Materials["Powered"]);
        visualState.ApplyPowerOffState();
        return visualState;
    }

    private static Level1AudioDirector BuildStage1Audio()
    {
        GameObject root = new GameObject(V2AudioRootName);

        AudioClip emergencyLoop = AssetDatabase.LoadAssetAtPath<AudioClip>(PackageRoot + "/Sounds/Ambiant_Loop.wav");
        AudioClip poweredLoop = AssetDatabase.LoadAssetAtPath<AudioClip>(PackageRoot + "/Sounds/Machin_Loop.wav");
        AudioClip doorClip = AssetDatabase.LoadAssetAtPath<AudioClip>(PackageRoot + "/Sounds/Pneumatic-door.wav");

        AudioSource emergencySource = AddAudioSource(root.transform, "Audio_EmergencyAmbience", new Vector3(0f, 2f, -2f), emergencyLoop, true, true, 0.35f, 12f);
        AudioSource poweredSource = AddAudioSource(root.transform, "Audio_PoweredMachinery", new Vector3(11f, 1.5f, 3f), poweredLoop, true, true, 0f, 10f);
        AudioSource generatorSource = AddAudioSource(root.transform, "Audio_Generator", new Vector3(12.4f, 1.4f, 3f), null, false, true, 1f, 8f);
        AudioSource doorSource = AddAudioSource(root.transform, "Audio_SecurityDoor", new Vector3(0f, 1.5f, 8f), null, false, true, 1f, 7f);
        AudioSource uiSource = AddAudioSource(root.transform, "Audio_UI", Vector3.zero, null, false, false, 1f, 1f);

        Level1AudioDirector director = root.AddComponent<Level1AudioDirector>();
        director.Configure(emergencySource, poweredSource, generatorSource, doorSource, uiSource, poweredLoop, doorClip, null, null);
        director.ApplyPowerOffState();
        return director;
    }

    private static void HideCoveredGreyboxRenderers()
    {
        GameObject greybox = GameObject.Find(GreyboxRootName);
        if (!greybox)
        {
            return;
        }

        string[] coveredGroups = { "Floors", "Walls", "DoorFrames", "Signage" };
        foreach (string groupName in coveredGroups)
        {
            Transform group = FindDeepChild(greybox.transform, groupName);
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

    private static void DisableGreyboxLighting()
    {
        Transform lighting = FindDeepChild(GameObject.Find(GreyboxRootName)?.transform, "Lighting");
        if (lighting)
        {
            lighting.gameObject.SetActive(false);
        }
    }

    private static void EnableGreyboxLighting()
    {
        Transform lighting = FindDeepChild(GameObject.Find(GreyboxRootName)?.transform, "Lighting");
        if (lighting)
        {
            lighting.gameObject.SetActive(true);
        }
    }

    private static void WireLevelFlow(Level1AudioDirector audioDirector, Level1VisualStateController visualState)
    {
        Level1FlowController flow = Object.FindFirstObjectByType<Level1FlowController>();
        TerminalInteractable terminal = Object.FindFirstObjectByType<TerminalInteractable>();
        GeneratorController generator = Object.FindFirstObjectByType<GeneratorController>();
        FacilityPowerController powerController = GameObject.Find(V2LightingRootName)?.GetComponent<FacilityPowerController>()
            ?? Object.FindFirstObjectByType<FacilityPowerController>();
        DoorController door = Object.FindFirstObjectByType<DoorController>();
        PlayerInteraction playerInteraction = Object.FindFirstObjectByType<PlayerInteraction>();
        PowerModulePickup powerModule = Object.FindFirstObjectByType<PowerModulePickup>();
        LevelExitTrigger levelExit = Object.FindFirstObjectByType<LevelExitTrigger>();
        HardwareHudController hud = Object.FindFirstObjectByType<HardwareHudController>();
        LevelCompletionUI completionUi = Object.FindFirstObjectByType<LevelCompletionUI>();

        if (!flow || !terminal || !generator || !powerController || !door || !playerInteraction || !powerModule || !levelExit)
        {
            Debug.LogWarning("Sci-Fi V2 could not fully rewire Level1FlowController. Gameplay objects may need inspection.");
            return;
        }

        flow.Configure(terminal, generator, powerController, door, playerInteraction, powerModule, levelExit, hud, completionUi, audioDirector, visualState);
    }

    private static GameObject InstantiatePrebuiltModule(ModulePlacement placement, Transform parent)
    {
        GameObject instance = InstantiatePrefab(placement.Path, parent, placement.Name);
        if (!instance)
        {
            return null;
        }

        instance.transform.rotation = Quaternion.identity;
        Bounds bounds = CalculateRendererBounds(instance);
        if (placement.CorridorShouldRunZ && bounds.size.x > bounds.size.z)
        {
            instance.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }

        CenterByRendererBounds(instance, placement.Center);
        SanitizeVisualInstance(instance, false);

        Bounds finalBounds = CalculateRendererBounds(instance);
        Debug.Log($"{placement.Name} placed at root position {instance.transform.position}, rotation {instance.transform.eulerAngles}, scale {instance.transform.localScale}, renderer bounds size {finalBounds.size}.");
        return instance;
    }

    private static GameObject InstantiatePrefab(string assetPath, Transform parent, string name)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (!prefab)
        {
            Debug.LogWarning($"Missing prefab: {assetPath}");
            return null;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        if (!instance)
        {
            return null;
        }

        instance.name = name;
        instance.transform.SetParent(parent, true);
        instance.transform.localScale = Vector3.one;
        return instance;
    }

    private static void SanitizeVisualInstance(GameObject instance, bool keepFirstFewLights)
    {
        foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
        {
            Object.DestroyImmediate(collider);
        }

        int retainedLights = 0;
        foreach (Light light in instance.GetComponentsInChildren<Light>(true))
        {
            light.shadows = LightShadows.None;
            light.intensity = Mathf.Min(light.intensity, 1.1f);

            if (keepFirstFewLights && retainedLights < 1)
            {
                retainedLights++;
                continue;
            }

            light.enabled = false;
        }

        foreach (MonoBehaviour behaviour in instance.GetComponentsInChildren<MonoBehaviour>(true))
        {
            Object.DestroyImmediate(behaviour);
        }

        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(true))
        {
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
        }
    }

    private static void MoveByRendererBounds(GameObject target, Vector3 desiredCenter)
    {
        Bounds bounds = CalculateRendererBounds(target);
        target.transform.position += desiredCenter - bounds.center;
    }

    private static void CenterByRendererBounds(GameObject target, Vector3 desiredCenter)
    {
        Bounds bounds = CalculateRendererBounds(target);
        target.transform.position += desiredCenter - bounds.center;
    }

    private static Bounds CalculateRendererBounds(GameObject target)
    {
        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
        bool initialized = false;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer)
            {
                continue;
            }

            if (!initialized)
            {
                bounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return bounds;
    }

    private static void HideChildRenderers(GameObject target)
    {
        foreach (Renderer renderer in target.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }
    }

    private static GameObject AddVisualBox(Transform parent, string name, Vector3 position, Vector3 scale, string materialKey)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, true);
        cube.transform.SetPositionAndRotation(position, Quaternion.identity);
        cube.transform.localScale = scale;

        Collider collider = cube.GetComponent<Collider>();
        if (collider)
        {
            Object.DestroyImmediate(collider);
        }

        if (cube.TryGetComponent(out Renderer renderer) && Materials.TryGetValue(materialKey, out Material material))
        {
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;

            bool isSecurityStatus = name.Contains("Status");
            if (isSecurityStatus)
            {
                SecurityStatusRenderers.Add(renderer);
            }
            else if (materialKey == "Emergency")
            {
                EmergencyRenderers.Add(renderer);
            }
            else if (materialKey == "Powered")
            {
                PoweredRenderers.Add(renderer);
            }
        }

        return cube;
    }

    private static void AddCollisionBox(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject box = new GameObject(name);
        box.transform.SetParent(parent, true);
        box.transform.SetPositionAndRotation(position, Quaternion.identity);
        box.transform.localScale = scale;

        BoxCollider collider = box.AddComponent<BoxCollider>();
        collider.size = Vector3.one;
    }

    private static Light AddPointLight(Transform parent, string name, Vector3 position, float intensity, float range, Color color)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.SetParent(parent, true);
        lightObject.transform.position = position;

        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = intensity;
        light.range = range;
        light.color = color;
        light.shadows = LightShadows.None;
        return light;
    }

    private static AudioSource AddAudioSource(Transform parent, string name, Vector3 position, AudioClip clip, bool loop, bool spatial, float volume, float maxDistance)
    {
        GameObject sourceObject = new GameObject(name);
        sourceObject.transform.SetParent(parent, true);
        sourceObject.transform.position = position;

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = loop;
        source.playOnAwake = false;
        source.volume = volume;
        source.spatialBlend = spatial ? 1f : 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = maxDistance;
        return source;
    }

    private static TextMesh AddWorldLabel(Transform parent, string name, string text, Vector3 position, Quaternion rotation, float size, string materialKey)
    {
        GameObject label = new GameObject(name);
        label.transform.SetParent(parent, true);
        label.transform.SetPositionAndRotation(position, rotation);

        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = size;
        textMesh.fontSize = 64;

        if (Materials.TryGetValue(materialKey, out Material material) && label.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = material;
        }

        return textMesh;
    }

    private static GameObject CreateChild(GameObject parent, string name)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform, true);
        return child;
    }

    private static void DeleteIfExists(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing)
        {
            Object.DestroyImmediate(existing);
        }
    }

    private static Transform FindDeepChild(Transform parent, string childName)
    {
        if (!parent)
        {
            return null;
        }

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

    private static void CacheMaterials()
    {
        Materials.Clear();
        EnsureFolder("Assets", "Level1");
        EnsureFolder("Assets/Level1", "V2Materials");

        Materials["Floor"] = GetOrCreateMaterial("M_CQ_V2_Floor_Graphite", new Color(0.07f, 0.08f, 0.09f), Color.black);
        Materials["Wall"] = GetOrCreateMaterial("M_CQ_V2_Wall_Steel", new Color(0.19f, 0.22f, 0.24f), Color.black);
        Materials["Frame"] = GetOrCreateMaterial("M_CQ_V2_Frame_DarkSteel", new Color(0.08f, 0.10f, 0.12f), Color.black);
        Materials["Ceiling"] = GetOrCreateMaterial("M_CQ_V2_Ceiling", new Color(0.06f, 0.07f, 0.08f), Color.black);
        Materials["Emergency"] = GetOrCreateMaterial("M_CQ_V2_Emergency_Red", new Color(0.8f, 0.08f, 0.04f), new Color(1.6f, 0.12f, 0.06f));
        Materials["Powered"] = GetOrCreateMaterial("M_CQ_V2_Powered_Cyan", new Color(0.08f, 0.72f, 0.78f), new Color(0.16f, 1.4f, 1.2f));
    }

    private static Material GetOrCreateMaterial(string name, Color baseColor, Color emission)
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

        if (emission.maxColorComponent > 0f)
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
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
