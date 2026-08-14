using System.Collections.Generic;
using RogueAI.Challenges;
using RogueAI.HQ;
using RogueAI.Interaction;
using RogueAI.Level;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogueAI.EditorTools
{
    public static class HQLevel3Builder
    {
        private const string GameplayScenePath = "Assets/Scenes/HQ_Gameplay.unity";
        private const string Level3RootName = "HQ_Level3_Gameplay";
        private const string MaterialFolder = "Assets/Generated/HQGameplay/Materials";
        private const string DataTerminalSlotId = "power-module-terminal";

        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("Tools/ClassQuest/Create Missing HQ Level 3 Anchors")]
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

            Transform baseAnchor = FindAnchor("L2_CameraHallwayEnd") ?? FindAnchor("L2_CameraHallwayStart");
            if (!baseAnchor)
            {
                Debug.LogError("Cannot create Level 3 anchors because neither L2_CameraHallwayEnd nor L2_CameraHallwayStart exists.");
                return;
            }

            CreateAnchorIfMissing(anchorsRoot.transform, "L3_FragmentA", baseAnchor.position + new Vector3(8f, 0f, 10f), baseAnchor.rotation);
            CreateAnchorIfMissing(anchorsRoot.transform, "L3_FragmentB", baseAnchor.position + new Vector3(-8f, 0f, 22f), baseAnchor.rotation);
            CreateAnchorIfMissing(anchorsRoot.transform, "L3_DataTerminal", baseAnchor.position + new Vector3(0f, 0f, 34f), baseAnchor.rotation);
            CreateAnchorIfMissing(anchorsRoot.transform, "L3_HQGate", baseAnchor.position + new Vector3(0f, 0f, 46f), baseAnchor.rotation);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("Created any missing Level 3 anchors under HQ_GameplayAnchors. You can move them manually, then run Tools > ClassQuest > Build HQ Level 3.");
        }

        [MenuItem("Tools/ClassQuest/Build HQ Level 3")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Build HQ Level 3",
                    "This will recreate only HQ_Level3_Gameplay in Assets/Scenes/HQ_Gameplay.unity using the existing HQ_GameplayAnchors. It will not modify ProBuilder geometry.",
                    "Build Level 3",
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
                Debug.LogError("HQ Level 3 build failed. Required existing systems were not found: HQFlowController and TerminalChallengeUI.");
                ExitBatchIfNeeded(batchMode, 1);
                return;
            }

            RemoveExistingLevel3Root();

            GameObject level3Root = new GameObject(Level3RootName);
            GameObject fragmentsRoot = CreateChild(level3Root.transform, "DataFragments");
            GameObject terminalsRoot = CreateChild(level3Root.transform, "Terminals");
            GameObject gatesRoot = CreateChild(level3Root.transform, "Gates");
            GameObject signsRoot = CreateChild(level3Root.transform, "Signs");

            DataFragmentPickup fragmentA = CreateDataFragment(fragmentsRoot.transform, "DataFragmentA", DataFragmentPickup.FragmentId.A, anchors["L3_FragmentA"], flowController);
            DataFragmentPickup fragmentB = CreateDataFragment(fragmentsRoot.transform, "DataFragmentB", DataFragmentPickup.FragmentId.B, anchors["L3_FragmentB"], flowController);
            DataTerminalInteractable terminal = CreateDataTerminal(terminalsRoot.transform, anchors["L3_DataTerminal"], challengeUi, flowController);
            DoorController hqGate = CreateHqGate(gatesRoot.transform, anchors["L3_HQGate"]);

            CreateSigns(signsRoot.transform, anchors);
            flowController.ConfigureLevel3(terminal, fragmentA, fragmentB, hqGate);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HQ Level 3 gameplay generated in Assets/Scenes/HQ_Gameplay.unity");
            ExitBatchIfNeeded(batchMode, 0);
        }

        private static Dictionary<string, Transform> FindRequiredAnchors()
        {
            string[] requiredNames =
            {
                "L3_FragmentA",
                "L3_FragmentB",
                "L3_DataTerminal",
                "L3_HQGate"
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
                Debug.LogError($"HQ Level 3 build failed. Missing required anchors under HQ_GameplayAnchors: {string.Join(", ", missing)}. Run Tools > ClassQuest > Create Missing HQ Level 3 Anchors, adjust their positions if needed, then run Tools > ClassQuest > Build HQ Level 3.");
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

        private static DataFragmentPickup CreateDataFragment(
            Transform parent,
            string name,
            DataFragmentPickup.FragmentId fragmentId,
            Transform anchor,
            HQFlowController flowController)
        {
            GameObject root = CreateChild(parent, name);
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            CreateCube(root.transform, "Pedestal", new Vector3(0f, 0.28f, 0f), new Vector3(1.1f, 0.56f, 1.1f), Materials["Graphite"]);

            GameObject visual = CreateChild(root.transform, "Visual");
            GameObject fragment = CreateCube(visual.transform, "FragmentCore", new Vector3(0f, 1.1f, 0f), new Vector3(0.65f, 0.65f, 0.65f), Materials["DataGlow"]);
            Object.DestroyImmediate(fragment.GetComponent<BoxCollider>());

            TextMesh label = new GameObject("FragmentLabel").AddComponent<TextMesh>();
            label.transform.SetParent(visual.transform);
            label.transform.localPosition = new Vector3(0f, 1.58f, 0f);
            label.transform.localRotation = Quaternion.identity;
            label.text = fragmentId == DataFragmentPickup.FragmentId.A ? "A" : "B";
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.35f;
            label.fontSize = 64;
            label.color = Color.white;

            Light pickupLight = new GameObject("DataFragmentLight").AddComponent<Light>();
            pickupLight.transform.SetParent(root.transform);
            pickupLight.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            pickupLight.type = LightType.Point;
            pickupLight.color = new Color(0.25f, 1f, 1f);
            pickupLight.intensity = 2.2f;
            pickupLight.range = 4f;
            pickupLight.shadows = LightShadows.None;

            DataFragmentPickup pickup = root.AddComponent<DataFragmentPickup>();
            pickup.Configure(fragmentId, flowController, visual, pickupLight);
            return pickup;
        }

        private static DataTerminalInteractable CreateDataTerminal(
            Transform parent,
            Transform anchor,
            TerminalChallengeUI challengeUi,
            HQFlowController flowController)
        {
            GameObject root = CreateChild(parent, "DataReconstructionTerminal");
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            CreateCube(root.transform, "ConsoleBase", new Vector3(0f, 0.45f, 0f), new Vector3(1.55f, 0.9f, 0.75f), Materials["Steel"]);
            GameObject screen = CreateCube(root.transform, "Screen", new Vector3(0f, 1.25f, -0.33f), new Vector3(1.24f, 0.72f, 0.12f), Materials["TerminalScreen"]);
            Object.DestroyImmediate(screen.GetComponent<BoxCollider>());

            DataTerminalInteractable terminal = root.AddComponent<DataTerminalInteractable>();
            terminal.Configure(DataTerminalSlotId, CreateDataFallbackChallenge(), challengeUi, flowController);
            return terminal;
        }

        private static DoorController CreateHqGate(Transform parent, Transform anchor)
        {
            GameObject root = CreateChild(parent, "Level3HQGate");
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            GameObject frameLeft = CreateCube(root.transform, "Frame_Left", new Vector3(-2.2f, 1.45f, 0f), new Vector3(0.3f, 2.9f, 0.55f), Materials["Graphite"]);
            GameObject frameRight = CreateCube(root.transform, "Frame_Right", new Vector3(2.2f, 1.45f, 0f), new Vector3(0.3f, 2.9f, 0.55f), Materials["Graphite"]);
            GameObject frameTop = CreateCube(root.transform, "Frame_Top", new Vector3(0f, 2.8f, 0f), new Vector3(4.7f, 0.25f, 0.55f), Materials["Graphite"]);
            Object.DestroyImmediate(frameLeft.GetComponent<BoxCollider>());
            Object.DestroyImmediate(frameRight.GetComponent<BoxCollider>());
            Object.DestroyImmediate(frameTop.GetComponent<BoxCollider>());

            GameObject doorPanel = CreateCube(root.transform, "DoorPanel", new Vector3(0f, 1.45f, 0f), new Vector3(4.1f, 2.8f, 0.35f), Materials["AlertRed"]);

            TextMesh label = new GameObject("GateStatusLabel").AddComponent<TextMesh>();
            label.transform.SetParent(root.transform);
            label.transform.localPosition = new Vector3(0f, 2.15f, -0.4f);
            label.transform.localRotation = Quaternion.identity;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.22f;
            label.fontSize = 42;
            label.color = new Color(1f, 0.28f, 0.22f);
            label.text = "HQ GATE\nLOCKED";

            DoorController controller = root.AddComponent<DoorController>();
            controller.Configure(doorPanel.transform, label);
            controller.SetStatusMessages("HQ GATE\nLOCKED", "AI CORE ACCESS\nAVAILABLE");
            controller.Lock();
            return controller;
        }

        private static void CreateSigns(Transform parent, Dictionary<string, Transform> anchors)
        {
            CreateSign(parent, "Sign_DataArchiveA", "DATA ARCHIVE A ->", anchors["L3_FragmentA"].position + Vector3.up * 1.9f, anchors["L3_FragmentA"].rotation);
            CreateSign(parent, "Sign_DataArchiveB", "DATA ARCHIVE B ->", anchors["L3_FragmentB"].position + Vector3.up * 1.9f, anchors["L3_FragmentB"].rotation);
            CreateSign(parent, "Sign_DataReconstruction", "DATA RECONSTRUCTION ->", anchors["L3_DataTerminal"].position + Vector3.up * 1.9f, anchors["L3_DataTerminal"].rotation);
            CreateSign(parent, "Sign_AiCore", "AI CORE / MAIN HQ ->", anchors["L3_HQGate"].position + Vector3.up * 1.9f, anchors["L3_HQGate"].rotation);
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

        private static ChallengeData CreateDataFallbackChallenge()
        {
            return new ChallengeData
            {
                challengeId = "hq-level3-data-fallback",
                slotId = DataTerminalSlotId,
                title = "DATA RECONSTRUCTION TERMINAL",
                statusText = "SHUTDOWN PROTOCOL ENCRYPTED\nFRAGMENT ASSEMBLY REQUIRED",
                question = "Mission Question #3 is not loaded. Enter DECRYPT for editor-only testing.",
                codeSnippet = string.Empty,
                expectedAnswer = "DECRYPT",
                concept = "Data reconstruction",
                type = "short-answer"
            };
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

        private static void RemoveExistingLevel3Root()
        {
            GameObject existing = GameObject.Find(Level3RootName);
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
            Materials["DataGlow"] = GetOrCreateMaterial("M_HQ_DataGlow", new Color(0.04f, 0.38f, 0.55f), new Color(0.25f, 1f, 1f), 1.5f);
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
