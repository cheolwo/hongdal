using System.IO;
using Ssalddel.Unity.TraditionalMarkets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ssalddel.Unity.Samples.TraditionalMarketHub.Editor
{
    public static class 전통시장물류거점PrimitiveSceneBuilder
    {
        private const string SceneDirectory = "Assets/Ssalddel/Scenes";
        private const string ScenePath = SceneDirectory + "/TraditionalMarketHubPrimitive.unity";

        [MenuItem("Ssalddel/Samples/Create Traditional Market Hub Primitive Scene")]
        public static void CreateScene()
        {
            if (!CanReplaceCurrentScene())
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("TraditionalMarketHubZone");

            CreateGround(root.transform);
            var marketBuilding = CreateMarketBuilding(root.transform);
            var logisticsHub = CreateLogisticsHub(root.transform);
            var informationPanel = CreatePanel(
                root.transform,
                "InformationPanel",
                new Vector3(0f, 0.9f, -4.8f),
                new Vector3(8f, 1.5f, 0.35f),
                "전통시장 물류거점 Loading...");
            var detailPanel = CreatePanel(
                root.transform,
                "HubDetailPanel",
                new Vector3(6.4f, 2.2f, -1.8f),
                new Vector3(4.2f, 4.2f, 0.35f),
                string.Empty);
            detailPanel.Root.SetActive(false);

            var hubView = root.AddComponent<전통시장물류거점View>();
            hubView.Configure(
                marketBuilding,
                logisticsHub,
                informationPanel.Root,
                informationPanel.Text,
                detailPanel.Root,
                detailPanel.Text);

            var controller = root.AddComponent<전통시장물류거점SceneController>();
            root.AddComponent<전통시장물류거점LifetimeScope>();

            CreateCamera();
            CreateLight();

            Directory.CreateDirectory(SceneDirectory);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("Created traditional market hub primitive scene: " + ScenePath);
        }

        public static void ValidateGeneratedScene()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Traditional market hub scene was not generated.", ScenePath);
            }

            if (!CanReplaceCurrentScene())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var hubView = Object.FindFirstObjectByType<전통시장물류거점View>();
            var controller = Object.FindFirstObjectByType<전통시장물류거점SceneController>();
            var lifetimeScope = Object.FindFirstObjectByType<전통시장물류거점LifetimeScope>();
            if (hubView == null || controller == null || lifetimeScope == null)
            {
                throw new MissingReferenceException(
                    "Traditional market hub View, SceneController, or LifetimeScope is missing.");
            }

            if (!hubView.ValidateWiring())
            {
                throw new MissingReferenceException("Traditional market hub wiring is invalid after scene reload.");
            }

            var fixture = new Simulated전통시장물류거점조회UseCase().조회Async().GetAwaiter().GetResult();
            var errors = new 전통시장물류거점ScreenModelValidator().Validate(fixture);
            if (errors.Length > 0)
            {
                throw new System.InvalidOperationException(
                    "Traditional market hub fixture contract is invalid: " + string.Join(", ", errors));
            }

            Debug.Log("Validated traditional market hub scene reload, wiring, and fixture.");
        }

        private static bool CanReplaceCurrentScene()
        {
            if (!Application.isBatchMode)
            {
                return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            for (var index = 0; index < SceneManager.sceneCount; index++)
            {
                var scene = SceneManager.GetSceneAt(index);
                if (scene.isDirty)
                {
                    throw new System.InvalidOperationException(
                        "Batch mode refuses to replace a modified scene: " + scene.name);
                }
            }

            return true;
        }

        private static void CreateGround(Transform parent)
        {
            var ground = Primitive(
                "Ground",
                parent,
                new Vector3(0f, -0.25f, 0f),
                new Vector3(18f, 0.5f, 13f),
                new Color(0.68f, 0.72f, 0.64f));
            ground.isStatic = true;
        }

        private static 시장건물View CreateMarketBuilding(Transform parent)
        {
            var root = new GameObject("MarketBuilding");
            root.transform.SetParent(parent, false);
            Primitive(
                "BuildingBody",
                root.transform,
                new Vector3(-3.8f, 2f, 3.3f),
                new Vector3(7f, 4f, 4f),
                new Color(0.74f, 0.34f, 0.24f));
            Primitive(
                "MarketRoof",
                root.transform,
                new Vector3(-3.8f, 4.2f, 3.3f),
                new Vector3(7.6f, 0.6f, 4.6f),
                new Color(0.32f, 0.18f, 0.14f));
            var name = CreateText(
                "MarketName",
                root.transform,
                "샘플 중앙전통시장",
                new Vector3(-3.8f, 3.2f, 1.2f),
                0.045f,
                Color.white);
            var region = CreateText(
                "Region",
                root.transform,
                "샘플시 중앙구",
                new Vector3(-3.8f, 2.6f, 1.2f),
                0.028f,
                Color.white);
            var view = root.AddComponent<시장건물View>();
            view.Configure(root, name, region);
            return view;
        }

        private static 물류거점View CreateLogisticsHub(Transform parent)
        {
            var root = new GameObject("PublicLogisticsHub");
            root.transform.SetParent(parent, false);
            var body = Primitive(
                "HubBody",
                root.transform,
                new Vector3(3.9f, 1.7f, 3.3f),
                new Vector3(6.2f, 3.4f, 4f),
                new Color(0.92f, 0.62f, 0.16f));
            var socket = body.AddComponent<InteractionSocket>();
            socket.Configure(body.GetComponent<Collider>());
            CreateLoadingDock(root.transform, new Vector3(2f, 0.45f, 0.8f), "InboundDock");
            CreateLoadingDock(root.transform, new Vector3(5.8f, 0.45f, 0.8f), "PickupDock");
            var status = CreateText(
                "HubStatus",
                root.transform,
                "물류거점 Pilot",
                new Vector3(3.9f, 2.8f, 1.15f),
                0.038f,
                Color.black);
            var capability = CreateText(
                "Capabilities",
                root.transform,
                "대량입고 / 분류 / 주민픽업",
                new Vector3(3.9f, 2.1f, 1.15f),
                0.022f,
                Color.black);
            var source = CreateText(
                "Source",
                root.transform,
                "SIMULATED",
                new Vector3(3.9f, 1.45f, 1.15f),
                0.018f,
                Color.black);
            var view = root.AddComponent<물류거점View>();
            view.Configure(root, body.GetComponent<Renderer>(), status, capability, source, socket);
            return view;
        }

        private static void CreateLoadingDock(Transform parent, Vector3 position, string name)
        {
            Primitive(
                name,
                parent,
                position,
                new Vector3(2.4f, 0.35f, 1.8f),
                new Color(0.24f, 0.29f, 0.34f));
        }

        private static Panel CreatePanel(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            string initialText)
        {
            var root = Primitive(name, parent, position, scale, new Color(0.12f, 0.16f, 0.2f));
            var value = CreateText(
                "Text",
                root.transform,
                initialText,
                new Vector3(0f, 0f, -0.65f),
                0.025f,
                Color.white,
                true);
            return new Panel(root, value);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8.5f;
            cameraObject.transform.position = new Vector3(0f, 13f, -15f);
            cameraObject.transform.LookAt(new Vector3(0f, 1.5f, 1.5f));
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
        }

        private static GameObject Primitive(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color)
        {
            var value = GameObject.CreatePrimitive(PrimitiveType.Cube);
            value.name = name;
            value.transform.SetParent(parent, false);
            value.transform.localPosition = position;
            value.transform.localScale = scale;
            value.GetComponent<Renderer>().sharedMaterial = CreateMaterial(name + "Material", color);
            return value;
        }

        private static TextMesh CreateText(
            string name,
            Transform parent,
            string value,
            Vector3 position,
            float characterSize,
            Color color,
            bool local = false)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            textObject.transform.localPosition = position;
            var text = textObject.AddComponent<TextMesh>();
            text.text = value;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 64;
            text.characterSize = characterSize;
            text.color = color;
            return text;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader)
            {
                name = name,
                color = color,
            };
        }

        private readonly struct Panel
        {
            public Panel(GameObject root, TextMesh text)
            {
                Root = root;
                Text = text;
            }

            public GameObject Root { get; }

            public TextMesh Text { get; }
        }
    }
}
