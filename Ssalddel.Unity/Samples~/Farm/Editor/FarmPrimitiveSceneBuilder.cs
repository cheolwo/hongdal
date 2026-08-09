using System.IO;
using Ssalddel.Unity.Farm;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

namespace Ssalddel.Unity.Samples.Farm.Editor
{
    public static class FarmPrimitiveSceneBuilder
    {
        private const string SceneDirectory = "Assets/SsalddelGenerated/Farm";
        private const string ScenePath = SceneDirectory + "/FarmPrimitive.unity";

        [MenuItem("Ssalddel/Samples/Create Farm Primitive Scene")]
        public static void CreateScene()
        {
            if (!CanReplaceCurrentScene()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("FarmZone");
            Primitive("Ground", root.transform, new Vector3(0f, -0.3f, 0f),
                new Vector3(18f, 0.5f, 14f), new Color(0.35f, 0.28f, 0.18f));
            var status = Text("ZoneStatus", root.transform,
                "FARM · SERVER-AUTHORIZED CONDITION", new Vector3(0f, 4.8f, 4.5f), 0.03f);

            var plotObject = Primitive("FarmTile", root.transform, new Vector3(0f, 0.1f, 0f),
                new Vector3(10f, 0.35f, 7f), new Color(0.42f, 0.28f, 0.15f));
            var plotLabel = Text("PlotLabel", plotObject.transform, "WAITING FOR PLOT",
                new Vector3(0f, 1.1f, 2.6f), 0.025f, true);
            var plot = plotObject.AddComponent<FarmTileView>();
            plot.Configure("farm-plot:a.1", plotObject.GetComponent<Renderer>(), plotLabel);

            var cropObject = Primitive("Crop", plotObject.transform, new Vector3(-2f, 0.8f, 0f),
                new Vector3(3f, 1.2f, 3f), new Color(0.25f, 0.65f, 0.25f), true);
            var cropLabel = Text("CropLabel", cropObject.transform, "WAITING FOR CROP",
                new Vector3(0f, 1.2f, 0f), 0.02f, true);
            var crop = cropObject.AddComponent<CropView>();
            crop.Configure("cultivation:a.potato.2026", cropObject.GetComponent<Renderer>(), cropLabel);

            var sensorObject = Primitive("SoilMoistureSensor", plotObject.transform,
                new Vector3(2.5f, 1.2f, 0f), new Vector3(0.8f, 2.2f, 0.8f), Color.gray, true);
            var sensorLabel = Text("SensorLabel", sensorObject.transform, "WAITING FOR SENSOR",
                new Vector3(0f, 1.5f, 0f), 0.018f, true);
            var sensor = sensorObject.AddComponent<SensorView>();
            sensor.Configure("sensor:a.soil-moisture.1", sensorObject.GetComponent<Renderer>(), sensorLabel);

            var fieldWaypoint = Waypoint("FieldWaypoint", root.transform, "farm.field-a", new Vector3(-2f, 0f, -2f));
            var sensorWaypoint = Waypoint("SensorWaypoint", root.transform, "farm.sensor-a", new Vector3(2.5f, 0f, -2f));
            var entryWaypoint = Waypoint("EntryWaypoint", root.transform, "farm.entry", new Vector3(0f, 0f, -5f));
            var packoutWaypoint = Waypoint("PackoutWaypoint", root.transform, "farm.packout", new Vector3(5f, 0f, 3f));
            var workerObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            workerObject.name = "FarmWorker";
            workerObject.transform.SetParent(root.transform, false);
            workerObject.transform.position = fieldWaypoint.position;
            workerObject.GetComponent<Renderer>().sharedMaterial = Material("FarmWorkerMaterial", new Color(0.2f, 0.45f, 0.8f));
            var worker = workerObject.AddComponent<FarmWorkerView>();
            worker.Configure(
                "farm-worker:a.1",
                workerObject.AddComponent<NavMeshAgent>(),
                null,
                workerObject.GetComponent<Renderer>(),
                new[]
                {
                    new FarmWorkerView.WaypointBinding { Key = "farm.entry", Transform = entryWaypoint },
                    new FarmWorkerView.WaypointBinding { Key = "farm.field-a", Transform = fieldWaypoint },
                    new FarmWorkerView.WaypointBinding { Key = "farm.sensor-a", Transform = sensorWaypoint },
                    new FarmWorkerView.WaypointBinding { Key = "farm.packout", Transform = packoutWaypoint },
                });

            var view = root.AddComponent<FarmView>();
            view.Configure(new[] { plot }, new[] { crop }, new[] { sensor }, new[] { worker }, status);
            root.AddComponent<FarmSceneController>();
            var tokenProvider = root.AddComponent<FarmSessionTokenProvider>();
            root.AddComponent<FarmLifetimeScope>().ConfigureSimulationApi(tokenProvider);

            CreateCamera();
            CreateLight();
            Directory.CreateDirectory(SceneDirectory);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Selection.activeGameObject = root;
            Debug.Log("Created farm primitive scene: " + ScenePath);
        }

        [MenuItem("Ssalddel/Samples/Validate Farm Primitive Scene")]
        public static void ValidateGeneratedScene()
        {
            if (!File.Exists(ScenePath)) throw new FileNotFoundException("Farm scene was not generated.", ScenePath);
            if (!CanReplaceCurrentScene()) return;

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var view = Object.FindAnyObjectByType<FarmView>();
            if (view == null || !view.ValidateWiring())
                throw new MissingReferenceException("Farm View socket is invalid.");
            if (Object.FindAnyObjectByType<FarmSceneController>() == null)
                throw new MissingReferenceException("Farm Controller is missing.");
            if (Object.FindAnyObjectByType<FarmLifetimeScope>() == null)
                throw new MissingReferenceException("Farm LifetimeScope is missing.");
            if (Object.FindAnyObjectByType<FarmSessionTokenProvider>() == null)
                throw new MissingReferenceException("Farm token provider is missing.");
            Debug.Log("Validated FarmTile, Crop, Sensor, FarmWorker and VContainer wiring.");
        }

        private static GameObject Primitive(
            string name, Transform parent, Vector3 position, Vector3 scale, Color color, bool local = false)
        {
            var value = GameObject.CreatePrimitive(PrimitiveType.Cube);
            value.name = name;
            value.transform.SetParent(parent, false);
            if (local) value.transform.localPosition = position; else value.transform.position = position;
            value.transform.localScale = scale;
            value.GetComponent<Renderer>().sharedMaterial = Material(name + "Material", color);
            return value;
        }

        private static TextMesh Text(
            string name, Transform parent, string text, Vector3 position, float size, bool local = false)
        {
            var value = new GameObject(name);
            value.transform.SetParent(parent, false);
            if (local) value.transform.localPosition = position; else value.transform.position = position;
            value.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var mesh = value.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.characterSize = size;
            mesh.fontSize = 48;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.color = Color.white;
            return mesh;
        }

        private static Material Material(string name, Color color)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")
                ?? throw new System.InvalidOperationException("No compatible shader found.");
            return new Material(shader) { name = name, color = color };
        }

        private static Transform Waypoint(string name, Transform parent, string key, Vector3 position)
        {
            var value = new GameObject(name);
            value.transform.SetParent(parent, false);
            value.transform.position = position;
            value.name = name + " [" + key + "]";
            return value.transform;
        }

        private static void CreateCamera()
        {
            var value = new GameObject("Main Camera");
            value.tag = "MainCamera";
            var camera = value.AddComponent<Camera>();
            camera.transform.position = new Vector3(0f, 14f, -15f);
            camera.transform.rotation = Quaternion.Euler(38f, 0f, 0f);
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.17f, 0.13f);
        }

        private static void CreateLight()
        {
            var value = new GameObject("Directional Light");
            var light = value.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
        }

        private static bool CanReplaceCurrentScene()
        {
            if (!Application.isBatchMode) return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            for (var index = 0; index < SceneManager.sceneCount; index++)
            {
                var scene = SceneManager.GetSceneAt(index);
                if (scene.isDirty)
                    throw new System.InvalidOperationException("Batch mode refuses modified scene: " + scene.name);
            }
            return true;
        }
    }
}
