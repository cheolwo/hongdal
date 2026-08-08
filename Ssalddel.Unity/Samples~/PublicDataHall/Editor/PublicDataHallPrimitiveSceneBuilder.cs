using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ssalddel.Unity.Samples.PublicDataHall.Editor
{
    public static class PublicDataHallPrimitiveSceneBuilder
    {
        private const string SceneDirectory = "Assets/Ssalddel/Scenes";
        private const string ScenePath = SceneDirectory + "/PublicDataHallPrimitive.unity";

        [MenuItem("Ssalddel/Samples/Create Public Data Hall Primitive Scene")]
        public static void CreateScene()
        {
            if (!CanReplaceCurrentScene())
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("PublicDataHallZone");
            CreateCube("WorldMapTable", root.transform, Vector3.zero,
                new Vector3(20f, 0.5f, 12f), new Color(0.18f, 0.28f, 0.34f));

            var markerRoot = new GameObject("ObservationMarkers");
            markerRoot.transform.SetParent(root.transform, false);
            var markerTemplate = CreateMarkerTemplate(markerRoot.transform);
            markerTemplate.gameObject.SetActive(false);
            var status = CreateText("PublicDataStatus", root.transform,
                new Vector3(-9f, 2.2f, 5f), "Idle");

            var view = root.AddComponent<PublicDataHallView>();
            view.Configure(markerRoot.transform, markerTemplate, status, new Vector2(18f, 10f));
            root.AddComponent<PublicDataHallSceneController>();
            var scope = root.AddComponent<PublicDataHallLifetimeScope>();
            scope.ConfigureSimulation();

            CreateCamera();
            CreateLight();
            Directory.CreateDirectory(SceneDirectory);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("Created public data hall primitive scene: " + ScenePath);
        }

        [MenuItem("Ssalddel/Samples/Validate Public Data Hall Primitive Scene")]
        public static void ValidateGeneratedScene()
        {
            if (!File.Exists(ScenePath))
            {
                throw new FileNotFoundException("Public data hall scene was not generated.", ScenePath);
            }

            if (!CanReplaceCurrentScene())
            {
                return;
            }

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var view = Object.FindFirstObjectByType<PublicDataHallView>(FindObjectsInactive.Include);
            var controller = Object.FindFirstObjectByType<PublicDataHallSceneController>();
            var scope = Object.FindFirstObjectByType<PublicDataHallLifetimeScope>();
            if (view == null || controller == null || scope == null || !view.ValidateWiring())
            {
                throw new MissingReferenceException("Public data hall wiring is invalid after scene reload.");
            }

            Debug.Log("Validated public data hall marker template, View, Controller, and LifetimeScope wiring.");
        }

        private static PublicObservationMarkerView CreateMarkerTemplate(Transform parent)
        {
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "ObservationMarkerTemplate";
            marker.transform.SetParent(parent, false);
            marker.transform.localScale = new Vector3(0.35f, 0.25f, 0.35f);
            marker.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial("ObservationMarkerMaterial", new Color(0.94f, 0.56f, 0.24f));
            var label = CreateText("Title", marker.transform, new Vector3(0f, 1.2f, 0f), "Observation");
            var view = marker.AddComponent<PublicObservationMarkerView>();
            view.Configure(marker.GetComponent<Renderer>(), label);
            return view;
        }

        private static GameObject CreateCube(
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
            Vector3 position,
            string text)
        {
            var value = new GameObject(name);
            value.transform.SetParent(parent, false);
            value.transform.localPosition = position;
            var label = value.AddComponent<TextMesh>();
            label.text = text;
            label.characterSize = 0.22f;
            label.fontSize = 48;
            label.anchor = TextAnchor.MiddleLeft;
            return label;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard");
            var material = new Material(shader)
            {
                name = name,
                color = color,
            };
            return material;
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 9f;
            cameraObject.transform.position = new Vector3(0f, 14f, -10f);
            cameraObject.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
        }

        private static void CreateLight()
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static bool CanReplaceCurrentScene()
        {
            if (Application.isBatchMode)
            {
                return !EditorSceneManager.GetActiveScene().isDirty;
            }

            return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }
    }
}
