using System.IO;
using PogoDom.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PogoDom.Editor
{
    public static class PogoDomQuickStart
    {
        private const string SceneDirectory = "Assets/PogoDom/Scenes";
        private const string ScenePath = SceneDirectory + "/PogoDom_M0.unity";

        [MenuItem("PogoDom/Create M0 Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            if (!Directory.Exists(SceneDirectory))
                Directory.CreateDirectory(SceneDirectory);

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("PogoDom Prototype");
            root.AddComponent<PogoDomPrototypeBootstrap>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = root;
            AssetDatabase.Refresh();
            Debug.Log($"PogoDom M0 scene created at {ScenePath}. Press Play.");
        }
    }
}
