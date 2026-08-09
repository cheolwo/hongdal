using System.IO;
using Ssalddel.Unity.ResidentialPickup;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ssalddel.Unity.Samples.ResidentialPickup.Editor
{
    public static class ResidentialPickupPrimitiveSceneBuilder
    {
        private const string SceneDirectory = "Assets/Ssalddel/Scenes";
        private const string ScenePath = SceneDirectory + "/ResidentialPickupPrimitive.unity";

        [MenuItem("Ssalddel/Samples/Create Residential Pickup Primitive Scene")]
        public static void CreateScene()
        {
            if (!CanReplaceCurrentScene())
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("ResidentialPickupZone");
            CreateGround(root.transform);
            CreateBuilding(root.transform);
            var status = CreateText(
                "ZoneStatus",
                root.transform,
                "RESIDENTIAL PICKUP · READ ONLY",
                new Vector3(0f, 4.5f, 3.8f),
                0.035f,
                Color.white);
            var points = new[]
            {
                CreatePickupPoint("PickupPoint_91", "residential-pickup:91", root.transform, new Vector3(-3f, 0.8f, 0f)),
                CreatePickupPoint("PickupPoint_92", "residential-pickup:92", root.transform, new Vector3(3f, 0.8f, 0f)),
            };
            var view = root.AddComponent<ResidentialPickupView>();
            view.Configure(points, status);
            var controller = root.AddComponent<ResidentialPickupSceneController>();
            CreateRoleSwitch(
                "OrdererRoleSwitch",
                ResidentialPickupRoleCodes.Orderer,
                root.transform,
                controller,
                new Vector3(-2.2f, 0.5f, -4f),
                new Color(0.25f, 0.58f, 0.92f));
            CreateRoleSwitch(
                "TransporterRoleSwitch",
                ResidentialPickupRoleCodes.Transporter,
                root.transform,
                controller,
                new Vector3(2.2f, 0.5f, -4f),
                new Color(0.95f, 0.58f, 0.18f));
            var tokenProvider = root.AddComponent<ResidentialPickupSessionTokenProvider>();
            var scope = root.AddComponent<ResidentialPickupLifetimeScope>();
            scope.ConfigureSimulationApi(tokenProvider);

            CreateCamera();
            CreateLight();
            Directory.CreateDirectory(SceneDirectory);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("Created residential pickup primitive scene: " + ScenePath);
        }

        [MenuItem("Ssalddel/Samples/Validate Residential Pickup Primitive Scene")]
        public static void ValidateGeneratedScene()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Residential pickup scene was not generated.", ScenePath);
            }

            if (!CanReplaceCurrentScene())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var view = Object.FindAnyObjectByType<ResidentialPickupView>();
            var controller = Object.FindAnyObjectByType<ResidentialPickupSceneController>();
            var scope = Object.FindAnyObjectByType<ResidentialPickupLifetimeScope>();
            var tokenProvider = Object.FindAnyObjectByType<ResidentialPickupSessionTokenProvider>();
            var switches = Object.FindObjectsByType<ResidentialPickupRoleSwitchView>();
            if (view == null)
            {
                throw new MissingReferenceException("Residential pickup View is missing.");
            }

            if (controller == null)
            {
                throw new MissingReferenceException("Residential pickup Controller is missing.");
            }

            if (scope == null)
            {
                throw new MissingReferenceException("Residential pickup LifetimeScope is missing.");
            }

            if (tokenProvider == null)
            {
                throw new MissingReferenceException("Residential pickup token provider is missing.");
            }

            if (switches.Length != 2)
            {
                throw new MissingReferenceException(
                    "Residential pickup role switch count is invalid: " + switches.Length);
            }

            if (!view.ValidateWiring())
            {
                throw new MissingReferenceException("Residential pickup View socket is invalid.");
            }

            foreach (var roleSwitch in switches)
            {
                if (!roleSwitch.ValidateWiring())
                {
                    throw new MissingReferenceException("Residential pickup role switch is invalid.");
                }
            }

            Debug.Log("Validated residential pickup View, role switches, token provider, and VContainer wiring.");
        }

        private static ResidentialPickupPointView CreatePickupPoint(
            string name,
            string stableId,
            Transform parent,
            Vector3 position)
        {
            var root = Primitive(
                name,
                parent,
                position,
                new Vector3(3.6f, 1.2f, 2.2f),
                new Color(0.6f, 0.62f, 0.65f));
            var badge = Primitive(
                "RoleBadge",
                root.transform,
                new Vector3(0f, 1f, 0f),
                new Vector3(2.4f, 0.3f, 0.25f),
                Color.gray,
                true);
            badge.SetActive(false);
            var text = CreateText(
                "PickupLabel",
                root.transform,
                "WAITING FOR AUTHORIZED SNAPSHOT",
                new Vector3(0f, 1.7f, 0f),
                0.025f,
                Color.white,
                true);
            var view = root.AddComponent<ResidentialPickupPointView>();
            view.Configure(stableId, root.GetComponent<Renderer>(), text, badge);
            return view;
        }

        private static void CreateRoleSwitch(
            string name,
            string roleCode,
            Transform parent,
            ResidentialPickupSceneController controller,
            Vector3 position,
            Color color)
        {
            var root = Primitive(name, parent, position, new Vector3(3f, 1f, 1.6f), color);
            var view = root.AddComponent<ResidentialPickupRoleSwitchView>();
            view.Configure(roleCode, controller);
            CreateText(name + "Label", root.transform, roleCode, new Vector3(0f, 0.8f, 0f), 0.03f, Color.white, true);
        }

        private static void CreateGround(Transform parent)
        {
            var ground = Primitive(
                "Ground",
                parent,
                new Vector3(0f, -0.25f, 0f),
                new Vector3(18f, 0.5f, 14f),
                new Color(0.55f, 0.58f, 0.52f));
            ground.isStatic = true;
        }

        private static void CreateBuilding(Transform parent)
        {
            Primitive(
                "ResidentialBuilding",
                parent,
                new Vector3(0f, 3f, 5f),
                new Vector3(12f, 6f, 2f),
                new Color(0.72f, 0.7f, 0.66f));
        }

        private static GameObject Primitive(
            string name,
            Transform parent,
            Vector3 position,
            Vector3 scale,
            Color color,
            bool local = false)
        {
            var value = GameObject.CreatePrimitive(PrimitiveType.Cube);
            value.name = name;
            value.transform.SetParent(parent, false);
            if (local)
            {
                value.transform.localPosition = position;
            }
            else
            {
                value.transform.position = position;
            }

            value.transform.localScale = scale;
            value.GetComponent<Renderer>().sharedMaterial = Material(name + "Material", color);
            return value;
        }

        private static TextMesh CreateText(
            string name,
            Transform parent,
            string text,
            Vector3 position,
            float size,
            Color color,
            bool local = false)
        {
            var value = new GameObject(name);
            value.transform.SetParent(parent, false);
            if (local)
            {
                value.transform.localPosition = position;
            }
            else
            {
                value.transform.position = position;
            }

            value.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var mesh = value.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = color;
            return mesh;
        }

        private static Material Material(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? throw new System.InvalidOperationException("No compatible shader found.");
            var material = new Material(shader) { name = name, color = color };
            return material;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 14f, -15f);
            camera.transform.rotation = Quaternion.Euler(38f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.16f, 0.2f);
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
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
    }
}
