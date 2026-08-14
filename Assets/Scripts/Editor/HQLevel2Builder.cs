using System.Collections.Generic;
using RogueAI.Challenges;
using RogueAI.HQ;
using RogueAI.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RogueAI.EditorTools
{
    public static class HQLevel2Builder
    {
        private const string GameplayScenePath = "Assets/Scenes/HQ_Gameplay.unity";
        private const string Level2RootName = "HQ_Level2_Gameplay";
        private const string MaterialFolder = "Assets/Generated/HQGameplay/Materials";

        private static readonly Dictionary<string, Material> Materials = new Dictionary<string, Material>();

        [MenuItem("Tools/ClassQuest/Create Missing HQ Level 2 Anchors")]
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

            Transform hallwayStart = FindAnchor("L2_CameraHallwayStart");
            if (!hallwayStart)
            {
                Debug.LogError("Cannot create Level 2 camera anchors because L2_CameraHallwayStart is missing.");
                return;
            }

            CreateAnchorIfMissing(anchorsRoot.transform, "L2_Camera01", hallwayStart.position + new Vector3(0f, 0f, 8f), hallwayStart.rotation);
            CreateAnchorIfMissing(anchorsRoot.transform, "L2_Camera02", hallwayStart.position + new Vector3(0f, 0f, 20f), hallwayStart.rotation);
            CreateAnchorIfMissing(anchorsRoot.transform, "L2_Camera03", hallwayStart.position + new Vector3(0f, 0f, 32f), hallwayStart.rotation);
            CreateAnchorIfMissing(anchorsRoot.transform, "L2_CameraHallwayEnd", hallwayStart.position + new Vector3(0f, 0f, 46f), hallwayStart.rotation);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("Created any missing Level 2 anchors under HQ_GameplayAnchors. You can move them manually, then run Tools > ClassQuest > Build HQ Level 2.");
        }

        [MenuItem("Tools/ClassQuest/Build HQ Level 2")]
        public static void BuildFromMenu()
        {
            if (!EditorUtility.DisplayDialog(
                    "Build HQ Level 2",
                    "This will recreate only HQ_Level2_Gameplay in Assets/Scenes/HQ_Gameplay.unity using the existing HQ_GameplayAnchors. It will not modify ProBuilder geometry.",
                    "Build Level 2",
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
            PlayerInteraction playerInteraction = Object.FindAnyObjectByType<PlayerInteraction>();
            TerminalChallengeUI challengeUi = Object.FindAnyObjectByType<TerminalChallengeUI>();

            if (!flowController || !playerInteraction || !challengeUi)
            {
                Debug.LogError("HQ Level 2 build failed. Required existing Level 1 systems were not found: HQFlowController, PlayerInteraction, TerminalChallengeUI.");
                ExitBatchIfNeeded(batchMode, 1);
                return;
            }

            RemoveExistingLevel2Root();

            GameObject level2Root = new GameObject(Level2RootName);
            GameObject terminalsRoot = CreateChild(level2Root.transform, "Terminals");
            GameObject camerasRoot = CreateChild(level2Root.transform, "SecurityCameras");
            GameObject triggersRoot = CreateChild(level2Root.transform, "Triggers");
            GameObject signsRoot = CreateChild(level2Root.transform, "Signs");

            TerminalInteractable securityTerminal = CreateSecurityTerminal(terminalsRoot.transform, anchors["L2_SecurityControlRoom"], challengeUi);
            SecurityCameraController[] cameras =
            {
                CreateSecurityCamera(camerasRoot.transform, "SecurityCamera01", anchors["L2_Camera01"], flowController, playerInteraction.transform, 52f, 0.32f),
                CreateSecurityCamera(camerasRoot.transform, "SecurityCamera02", anchors["L2_Camera02"], flowController, playerInteraction.transform, 62f, 0.42f),
                CreateSecurityCamera(camerasRoot.transform, "SecurityCamera03", anchors["L2_Camera03"], flowController, playerInteraction.transform, 56f, 0.36f)
            };

            CreateHallwayEndTrigger(triggersRoot.transform, anchors["L2_CameraHallwayEnd"], flowController);
            CreateSigns(signsRoot.transform, anchors);

            flowController.ConfigureLevel2(securityTerminal, cameras, anchors["L2_CameraHallwayStart"]);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("HQ Level 2 gameplay generated in Assets/Scenes/HQ_Gameplay.unity");
            ExitBatchIfNeeded(batchMode, 0);
        }

        private static Dictionary<string, Transform> FindRequiredAnchors()
        {
            string[] requiredNames =
            {
                "L2_SecurityControlRoom",
                "L2_CameraHallwayStart",
                "L2_Camera01",
                "L2_Camera02",
                "L2_Camera03",
                "L2_CameraHallwayEnd"
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
                Debug.LogError($"HQ Level 2 build failed. Missing required anchors under HQ_GameplayAnchors: {string.Join(", ", missing)}. Run Tools > ClassQuest > Create Missing HQ Level 2 Anchors, adjust their positions if needed, then run Tools > ClassQuest > Build HQ Level 2.");
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

        private static TerminalInteractable CreateSecurityTerminal(Transform parent, Transform anchor, TerminalChallengeUI challengeUi)
        {
            GameObject root = CreateChild(parent, "SecurityTerminal");
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            CreateCube(root.transform, "ConsoleBase", new Vector3(0f, 0.45f, 0f), new Vector3(1.45f, 0.9f, 0.75f), Materials["Steel"]);
            GameObject screen = CreateCube(root.transform, "Screen", new Vector3(0f, 1.25f, -0.33f), new Vector3(1.16f, 0.72f, 0.12f), Materials["TerminalScreen"]);
            Object.DestroyImmediate(screen.GetComponent<BoxCollider>());

            TerminalInteractable terminal = root.AddComponent<TerminalInteractable>();
            terminal.Configure("security-terminal", CreateSecurityFallbackChallenge(), challengeUi);
            return terminal;
        }

        private static SecurityCameraController CreateSecurityCamera(
            Transform parent,
            string name,
            Transform anchor,
            HQFlowController flowController,
            Transform player,
            float sweepAngle,
            float sweepSpeed)
        {
            GameObject root = CreateChild(parent, name);
            root.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            GameObject mount = CreateCube(root.transform, "Mount", Vector3.up * 2.25f, new Vector3(0.55f, 0.22f, 0.55f), Materials["Graphite"]);
            Object.DestroyImmediate(mount.GetComponent<BoxCollider>());

            GameObject head = CreateChild(root.transform, "CameraHead");
            head.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            head.transform.localRotation = Quaternion.identity;

            GameObject body = CreateCube(head.transform, "Body", Vector3.zero, new Vector3(0.65f, 0.36f, 0.45f), Materials["Steel"]);
            Object.DestroyImmediate(body.GetComponent<BoxCollider>());

            GameObject lens = CreateCube(head.transform, "StatusLight", new Vector3(0f, 0f, 0.28f), new Vector3(0.32f, 0.22f, 0.08f), Materials["AlertRed"]);
            Object.DestroyImmediate(lens.GetComponent<BoxCollider>());

            LineRenderer fov = root.AddComponent<LineRenderer>();
            fov.material = Materials["FovLine"];
            fov.widthMultiplier = 0.045f;
            fov.loop = true;
            fov.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            fov.receiveShadows = false;

            SecurityCameraController controller = root.AddComponent<SecurityCameraController>();
            controller.Configure(flowController, player, head.transform, lens.GetComponent<Renderer>(), Materials["AlertRed"], Materials["PoweredCyan"], fov);

            SerializedObject serialized = new SerializedObject(controller);
            serialized.FindProperty("sweepAngle").floatValue = sweepAngle;
            serialized.FindProperty("sweepSpeed").floatValue = sweepSpeed;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            return controller;
        }

        private static void CreateHallwayEndTrigger(Transform parent, Transform anchor, HQFlowController flowController)
        {
            GameObject triggerObject = CreateChild(parent, "L2_CameraHallwayEndTrigger");
            triggerObject.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

            BoxCollider collider = triggerObject.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(4f, 2.6f, 4f);
            collider.center = new Vector3(0f, 1.3f, 0f);

            Level2HallwayEndTrigger trigger = triggerObject.AddComponent<Level2HallwayEndTrigger>();
            trigger.Configure(flowController);
        }

        private static void CreateSigns(Transform parent, Dictionary<string, Transform> anchors)
        {
            CreateSign(parent, "Sign_SecurityControl", "SECURITY CONTROL ->", anchors["L2_SecurityControlRoom"].position + Vector3.up * 1.9f, anchors["L2_SecurityControlRoom"].rotation);
            CreateSign(parent, "Sign_SurveillanceGrid", "SURVEILLANCE GRID ->", anchors["L2_CameraHallwayStart"].position + Vector3.up * 1.9f, anchors["L2_CameraHallwayStart"].rotation);
            CreateSign(parent, "Sign_DataResearch", "DATA RESEARCH ->", anchors["L2_CameraHallwayEnd"].position + Vector3.up * 1.9f, anchors["L2_CameraHallwayEnd"].rotation);
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

        private static ChallengeData CreateSecurityFallbackChallenge()
        {
            return new ChallengeData
            {
                challengeId = "hq-level2-security-fallback",
                slotId = "security-terminal",
                title = "SECURITY CONTROL TERMINAL",
                statusText = "SURVEILLANCE GRID ACTIVE\nADMIN OVERRIDE REQUIRED",
                question = "Mission Question #2 is not loaded. Enter OVERRIDE for editor-only testing.",
                codeSnippet = string.Empty,
                expectedAnswer = "OVERRIDE",
                concept = "Security",
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

        private static void RemoveExistingLevel2Root()
        {
            GameObject existing = GameObject.Find(Level2RootName);
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
            Materials["PoweredCyan"] = GetOrCreateMaterial("M_HQ_PoweredCyan", new Color(0.04f, 0.42f, 0.48f), new Color(0.25f, 1f, 1f), 1.2f);
            Materials["FovLine"] = GetOrCreateMaterial("M_HQ_FovLine", new Color(1f, 0.08f, 0.04f), new Color(1f, 0.08f, 0.04f), 0.8f);
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
