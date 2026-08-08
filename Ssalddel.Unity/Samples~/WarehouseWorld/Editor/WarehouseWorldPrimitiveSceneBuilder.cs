using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

namespace Ssalddel.Unity.Samples.WarehouseWorld.Editor
{
    public static class WarehouseWorldPrimitiveSceneBuilder
    {
        public const string ScenePath = "Assets/Ssalddel.Generated/WarehouseWorldPrimitive.unity";
        [MenuItem("Ssalddel/Samples/Create Warehouse World Primitive")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (!AssetDatabase.IsValidFolder("Assets/Ssalddel.Generated")) AssetDatabase.CreateFolder("Assets", "Ssalddel.Generated");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single); Build(scene);
            EditorSceneManager.SaveScene(scene, ScenePath); ValidateSavedScene();
        }
        public static void ValidateSavedScene()
        {
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var view = UnityEngine.Object.FindFirstObjectByType<WarehouseWorldView>();
            if (view == null || !view.ValidateWiring()) throw new InvalidOperationException("WarehouseWorldWiringInvalid");
            if (UnityEngine.Object.FindFirstObjectByType<WarehouseWorldLifetimeScope>() == null) throw new InvalidOperationException("WarehouseWorldLifetimeScopeMissing");
            if (UnityEngine.Object.FindFirstObjectByType<WarehouseWorldSceneController>() == null) throw new InvalidOperationException("WarehouseWorldControllerMissing");
            Debug.Log("Warehouse world primitive wiring validated: " + ScenePath);
        }
        private static void Build(Scene scene)
        {
            var root = new GameObject("WarehouseWorldZone"); SceneManager.MoveGameObjectToScene(root, scene);
            var scope = root.AddComponent<WarehouseWorldLifetimeScope>(); scope.ConfigureSimulation();
            var view = root.AddComponent<WarehouseWorldView>(); var controller = root.AddComponent<WarehouseWorldSceneController>(); controller.ConfigureWarehouse(7);
            root.AddComponent<WarehouseRuntimeSessionTokenProvider>();
            Cube("Ground", root.transform, new Vector3(0, -0.25f, 0), new Vector3(20, .5f, 14), new Color(.58f, .58f, .54f));
            var inbound = Waypoint("InboundDock", root.transform, new Vector3(-7, 0, 4));
            var storage = Waypoint("StorageZone", root.transform, new Vector3(-4, 0, -4));
            var rack = Waypoint("RackZone", root.transform, new Vector3(4, 0, -4));
            var outbound = Waypoint("OutboundStaging", root.transform, new Vector3(7, 0, 4));
            for (var i = 0; i < 4; i++) Cube("Rack_" + i, root.transform, new Vector3(-3f + i * 2f, 1.2f, -1f), new Vector3(1.2f, 2.4f, 4f), new Color(.35f, .38f, .42f));
            var objectRoot = new GameObject("WorldObjects").transform; objectRoot.SetParent(root.transform, false); objectRoot.localPosition = new Vector3(-5, 0, 3);
            var objectTemplateRoot = Cube("WorldObjectTemplate", root.transform, new Vector3(0, -20, 0), new Vector3(2.2f, .8f, 1.6f), Color.gray);
            var objectTemplate = objectTemplateRoot.AddComponent<WarehouseWorldObjectView>(); objectTemplate.Configure(objectTemplateRoot.GetComponent<Renderer>(), Text("Label", objectTemplateRoot.transform, new Vector3(0, .7f, 0), .16f)); objectTemplateRoot.SetActive(false);
            var npcTemplateRoot = GameObject.CreatePrimitive(PrimitiveType.Capsule); npcTemplateRoot.name = "NpcTemplate"; npcTemplateRoot.transform.SetParent(root.transform, false); npcTemplateRoot.transform.position = new Vector3(0, -20, 0);
            var agent = npcTemplateRoot.AddComponent<NavMeshAgent>(); agent.speed = 2.5f; agent.stoppingDistance = .2f;
            var animator = npcTemplateRoot.AddComponent<Animator>(); var npcTemplate = npcTemplateRoot.AddComponent<WarehouseNpcView>(); npcTemplate.Configure(agent, animator, Text("NpcLabel", npcTemplateRoot.transform, new Vector3(0, 1.5f, 0), .12f)); npcTemplateRoot.SetActive(false);
            var status = Text("Status", root.transform, new Vector3(0, .1f, 6), .24f); status.text = "Idle";
            view.Configure(objectRoot, objectTemplate, npcTemplate, status, inbound, storage, rack, outbound);
            var cameraObject = new GameObject("Main Camera"); cameraObject.tag = "MainCamera"; var camera = cameraObject.AddComponent<Camera>(); camera.orthographic = true; camera.orthographicSize = 9; cameraObject.transform.position = new Vector3(0, 15, -11); cameraObject.transform.rotation = Quaternion.Euler(55, 0, 0);
            var light = new GameObject("Directional Light"); light.AddComponent<Light>().type = LightType.Directional; light.transform.rotation = Quaternion.Euler(50, -30, 0);
        }
        private static Transform Waypoint(string name, Transform parent, Vector3 position) { var item = new GameObject(name); item.transform.SetParent(parent, false); item.transform.localPosition = position; Cube(name + "Marker", item.transform, Vector3.zero, new Vector3(1.4f, .1f, 1.4f), new Color(.2f, .7f, .45f)); return item.transform; }
        private static GameObject Cube(string name, Transform parent, Vector3 position, Vector3 scale, Color color) { var item = GameObject.CreatePrimitive(PrimitiveType.Cube); item.name = name; item.transform.SetParent(parent, false); item.transform.localPosition = position; item.transform.localScale = scale; var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"); item.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color }; return item; }
        private static TextMesh Text(string name, Transform parent, Vector3 position, float size) { var item = new GameObject(name); item.transform.SetParent(parent, false); item.transform.localPosition = position; item.transform.localRotation = Quaternion.Euler(90, 0, 0); var text = item.AddComponent<TextMesh>(); text.anchor = TextAnchor.MiddleCenter; text.alignment = TextAlignment.Center; text.characterSize = size; text.color = Color.black; return text; }
    }
}
