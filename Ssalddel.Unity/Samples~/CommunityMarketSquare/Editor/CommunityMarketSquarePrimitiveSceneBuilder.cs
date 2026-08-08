using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ssalddel.Unity.Samples.CommunityMarketSquare.Editor
{
    public static class CommunityMarketSquarePrimitiveSceneBuilder
    {
        public const string ScenePath = "Assets/Ssalddel.Generated/CommunityMarketSquarePrimitive.unity";

        [MenuItem("Ssalddel/Samples/Create Community Market Square Primitive")]
        public static void CreateScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EnsureFolder("Assets", "Ssalddel.Generated");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Build(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            ValidateSavedScene();
        }

        public static void ValidateSavedScene()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var view = UnityEngine.Object.FindFirstObjectByType<CommunityMarketSquareView>();
            if (view == null || !view.ValidateWiring())
                throw new InvalidOperationException("CommunityMarketSquareWiringInvalid");
            if (UnityEngine.Object.FindFirstObjectByType<CommunityMarketSquareLifetimeScope>() == null)
                throw new InvalidOperationException("CommunityMarketSquareLifetimeScopeMissing");
            if (UnityEngine.Object.FindFirstObjectByType<CommunityMarketSquareSceneController>() == null)
                throw new InvalidOperationException("CommunityMarketSquareControllerMissing");
            Debug.Log("Community market square primitive wiring validated: " + scene.path);
        }

        private static void Build(Scene scene)
        {
            var root = new GameObject("CommunityMarketSquareZone");
            SceneManager.MoveGameObjectToScene(root, scene);
            var scope = root.AddComponent<CommunityMarketSquareLifetimeScope>();
            scope.ConfigureSimulation();
            var view = root.AddComponent<CommunityMarketSquareView>();
            root.AddComponent<CommunityMarketSquareSceneController>();

            CreateCube("Ground", root.transform, new Vector3(0f, -0.25f, -3f), new Vector3(18f, 0.5f, 13f), new Color(0.72f, 0.69f, 0.55f));
            CreateCube("CommunityBoard", root.transform, new Vector3(-6.5f, 1.5f, 2f), new Vector3(3f, 3f, 0.35f), new Color(0.20f, 0.43f, 0.32f));
            CreateCube("LedgerBoard", root.transform, new Vector3(6.5f, 1.5f, 2f), new Vector3(3f, 3f, 0.35f), new Color(0.52f, 0.38f, 0.66f));
            CreateCube("ActivityKiosk", root.transform, new Vector3(0f, 1f, 3f), new Vector3(2f, 2f, 1.2f), new Color(0.28f, 0.55f, 0.75f));

            var itemRoot = new GameObject("WorldItems").transform;
            itemRoot.SetParent(root.transform, false);
            itemRoot.localPosition = new Vector3(-5.2f, 0f, 0f);
            var templateRoot = CreateCube("CommunitySquareItemTemplate", root.transform, new Vector3(0f, -20f, 0f), new Vector3(2.8f, 0.8f, 2.2f), Color.gray);
            var itemView = templateRoot.AddComponent<CommunitySquareItemView>();
            var title = CreateText("Title", templateRoot.transform, new Vector3(0f, 0.7f, 0f), 0.22f, TextAnchor.MiddleCenter);
            var detail = CreateText("Detail", templateRoot.transform, new Vector3(0f, 0.42f, 0f), 0.14f, TextAnchor.MiddleCenter);
            itemView.Configure(templateRoot.GetComponent<Renderer>(), title, detail);
            templateRoot.SetActive(false);

            var status = CreateText("Status", root.transform, new Vector3(0f, 0.15f, 5.2f), 0.25f, TextAnchor.MiddleCenter);
            status.text = "Idle";
            view.Configure(itemRoot, itemView, status, 4, new Vector2(3.5f, 3f));

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true; camera.orthographicSize = 8f;
            cameraObject.transform.position = new Vector3(0f, 14f, -9f);
            cameraObject.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            var lightObject = new GameObject("Directional Light");
            lightObject.AddComponent<Light>().type = LightType.Directional;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static GameObject CreateCube(string name, Transform parent, Vector3 position, Vector3 scale, Color color)
        {
            var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
            item.name = name; item.transform.SetParent(parent, false); item.transform.localPosition = position; item.transform.localScale = scale;
            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            item.GetComponent<Renderer>().sharedMaterial = new Material(shader) { color = color };
            return item;
        }

        private static TextMesh CreateText(string name, Transform parent, Vector3 position, float size, TextAnchor anchor)
        {
            var gameObject = new GameObject(name); gameObject.transform.SetParent(parent, false); gameObject.transform.localPosition = position;
            gameObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var text = gameObject.AddComponent<TextMesh>(); text.anchor = anchor; text.alignment = TextAlignment.Center; text.characterSize = size; text.color = Color.black;
            return text;
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
