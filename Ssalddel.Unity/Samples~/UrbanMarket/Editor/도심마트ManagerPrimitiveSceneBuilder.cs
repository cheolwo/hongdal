using System;
using System.IO;
using System.Linq;
using Ssalddel.Unity.Samples.UrbanMarket;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ssalddel.Unity.Samples.UrbanMarket.Editor
{
    public static class 도심마트ManagerPrimitiveSceneBuilder
    {
        private const string SceneDirectory = "Assets/Ssalddel/Scenes";
        private const string ScenePath = SceneDirectory + "/UrbanMarketManagerPrimitive.unity";

        [MenuItem("Ssalddel/Samples/Create Urban Market Manager Primitive Scene")]
        public static void CreateScene()
        {
            if (!CanReplaceCurrentScene()) return;
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("UrbanMarketManagerZone");
            Primitive("Ground", root.transform, new Vector3(0f, -0.25f, 1f), new Vector3(18f, 0.5f, 13f), new Color(0.72f, 0.75f, 0.68f));

            var status = Panel(root.transform, "RuntimeStatus", "Loading...", new Vector3(0f, 5.4f, 4.8f), new Vector3(9f, 0.9f, 0.25f));
            var summary = Panel(root.transform, "ManagerSummary", string.Empty, new Vector3(0f, 4.2f, 4.8f), new Vector3(12f, 1.1f, 0.25f));
            var queue = Panel(root.transform, "PriorityQueue", string.Empty, new Vector3(-5.5f, 2.2f, 3.8f), new Vector3(6f, 4.2f, 0.25f));
            var tasks = Panel(root.transform, "TaskMarkers", string.Empty, new Vector3(5.5f, 3.1f, 3.8f), new Vector3(4.5f, 2.2f, 0.25f));
            var sourcePlans = Panel(root.transform, "SourcePlans", string.Empty, new Vector3(5.5f, 0.7f, 3.8f), new Vector3(4.5f, 2.2f, 0.25f));
            var details = Panel(root.transform, "Details", string.Empty, new Vector3(0f, 0.8f, -4.3f), new Vector3(11f, 2.3f, 0.25f));
            var shelves = new[]
            {
                Shelf(root.transform, "PotatoManagerShelf", "urban-market-shelf:market-shelf:potato", new Vector3(-2.5f, 1f, -0.5f)),
                Shelf(root.transform, "OnionManagerShelf", "urban-market-shelf:market-shelf:onion", new Vector3(2.5f, 1f, -0.5f)),
            };

            var view = root.AddComponent<도심마트ManagerSurfaceView>();
            view.Configure(status, summary, queue, tasks, sourcePlans, details, shelves);
            root.AddComponent<도심마트ManagerSceneController>();
            var lifetimeScope = root.AddComponent<도심마트LifetimeScope>();
            lifetimeScope.ConfigureManagerSimulation();

            CreateCamera();
            CreateLight();
            Directory.CreateDirectory(SceneDirectory);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("Created urban market manager primitive scene: " + ScenePath);
        }

        public static void ValidateGeneratedScene()
        {
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Urban market manager scene was not generated.", ScenePath);
            if (!CanReplaceCurrentScene()) return;
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var view = UnityEngine.Object.FindFirstObjectByType<도심마트ManagerSurfaceView>();
            var controller = UnityEngine.Object.FindFirstObjectByType<도심마트ManagerSceneController>();
            var scope = UnityEngine.Object.FindFirstObjectByType<도심마트LifetimeScope>();
            if (view == null || controller == null || scope == null || !view.ValidateWiring())
                throw new MissingReferenceException("Urban market manager View, Controller, or LifetimeScope wiring is invalid.");
            if (UnityEngine.Object.FindObjectsByType<도심마트ManagerShelfView>(FindObjectsSortMode.None).Length != 2)
                throw new InvalidOperationException("Urban market manager fixture requires two shelf surfaces.");
            Debug.Log("Validated urban market manager scene reload and surface wiring.");
        }

        private static 도심마트ManagerShelfView Shelf(Transform parent, string name, string stableId, Vector3 position)
        {
            var root = Primitive(name, parent, position, new Vector3(3.8f, 1.5f, 2.2f), new Color(0.35f, 0.24f, 0.16f));
            var socket = root.AddComponent<InteractionSocket>();
            socket.Configure(root.GetComponent<Collider>());
            var quantity = Text("Quantity", root.transform, string.Empty, new Vector3(0f, 1.2f, -1.2f), 0.026f);
            var boxes = Enumerable.Range(0, 12)
                .Select(index => Primitive(
                    "DisplayBox_" + (index + 1),
                    root.transform,
                    new Vector3(-1.2f + (index % 4) * 0.8f, 0.9f + (index / 4) * 0.55f, 0f),
                    new Vector3(0.55f, 0.45f, 0.55f),
                    new Color(0.72f, 0.58f, 0.32f),
                    true))
                .ToArray();
            var view = root.AddComponent<도심마트ManagerShelfView>();
            view.Configure(stableId, root.GetComponent<Renderer>(), quantity, boxes, socket);
            return view;
        }

        private static TextMesh Panel(Transform parent, string name, string value, Vector3 position, Vector3 scale)
        {
            var panel = Primitive(name, parent, position, scale, new Color(0.12f, 0.16f, 0.2f));
            return Text("Text", panel.transform, value, new Vector3(0f, 0f, -0.65f), 0.018f);
        }

        private static GameObject Primitive(
            string name, Transform parent, Vector3 position, Vector3 scale, Color color, bool local = false)
        {
            var value = GameObject.CreatePrimitive(PrimitiveType.Cube);
            value.name = name;
            value.transform.SetParent(parent, false);
            if (local) value.transform.localPosition = position; else value.transform.position = position;
            value.transform.localScale = scale;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            value.GetComponent<Renderer>().sharedMaterial = new Material(shader) { name = name + "Material", color = color };
            return value;
        }

        private static TextMesh Text(
            string name, Transform parent, string value, Vector3 position, float characterSize)
        {
            var target = new GameObject(name);
            target.transform.SetParent(parent, false);
            target.transform.localPosition = position;
            var text = target.AddComponent<TextMesh>();
            text.text = value;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = characterSize;
            text.color = Color.white;
            return text;
        }

        private static void CreateCamera()
        {
            var target = new GameObject("Main Camera");
            target.tag = "MainCamera";
            var camera = target.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            target.transform.position = new Vector3(0f, 12f, -16f);
            target.transform.LookAt(new Vector3(0f, 1.8f, 1f));
        }

        private static void CreateLight()
        {
            var target = new GameObject("Directional Light");
            var light = target.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            target.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static bool CanReplaceCurrentScene()
        {
            if (!UnityEngine.Application.isBatchMode) return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            for (var index = 0; index < SceneManager.sceneCount; index++)
            {
                var scene = SceneManager.GetSceneAt(index);
                if (scene.isDirty) throw new InvalidOperationException("Batch mode refuses to replace a modified scene: " + scene.name);
            }
            return true;
        }
    }
}
