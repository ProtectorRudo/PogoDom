using System;
using System.IO;
using PogoDom.Runtime;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PogoDom.Editor
{
    public static class PogoDomAndroidBuild
    {
        private const string SceneFolder = "Assets/PogoDom/Scenes";
        private const string ScenePath = SceneFolder + "/PogoDom.unity";
        private const string OutputFolder = "Builds/Android";
        private const string ApkPath = OutputFolder + "/PogoDom-development.apk";

        [MenuItem("PogoDom/Build/1 - Prepare Unity Project")]
        public static void PrepareUnityProject()
        {
            ConfigurePlayerSettings();
            EnsurePlayableScene();
            Debug.Log("POGODOM Unity project prepared for Android. Scene: " + ScenePath);
        }

        [MenuItem("PogoDom/Build/2 - Build Android APK (Development)")]
        public static void BuildAndroidDevelopment()
        {
            EnsureAndroidBuildTarget();
            PrepareUnityProject();

            Directory.CreateDirectory(OutputFolder);
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("POGODOM Android build failed: " + summary.result + ". Check the Unity Console for the first compile/build error.");

            Debug.Log("POGODOM APK READY: " + Path.GetFullPath(ApkPath) +
                      " | size=" + summary.totalSize + " bytes | time=" + summary.totalTime);
        }

        // Terminal/CI example:
        // Unity.exe -batchmode -quit -buildTarget Android -projectPath <repo> -executeMethod PogoDom.Editor.PogoDomAndroidBuild.BuildAndroidDevelopmentBatch -logFile -
        public static void BuildAndroidDevelopmentBatch()
        {
            BuildAndroidDevelopment();
        }

        private static void EnsureAndroidBuildTarget()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
                return;

            if (Application.isBatchMode)
            {
                throw new InvalidOperationException(
                    "POGODOM batch build must start Unity with '-buildTarget Android'. " +
                    "Unity cannot reliably switch active build targets from an executeMethod while running in batch mode.");
            }

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Could not switch Unity build target to Android. Install Android Build Support from Unity Hub.");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "ProtectorRudo";
            PlayerSettings.productName = "POGODOM";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.protectorrudo.pogodom");
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // The first device gate is an installable development APK, not a Play Store release.
            // Keep the build simple and let the installed Unity Android toolchain choose its supported backend/SDK details.
            EditorUserBuildSettings.buildAppBundle = false;
        }

        private static void EnsurePlayableScene()
        {
            Directory.CreateDirectory(SceneFolder);

            var existing = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (existing != null)
            {
                EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
                return;
            }

            var previousScenePath = SceneManager.GetActiveScene().path;
            if (!string.IsNullOrEmpty(previousScenePath) && SceneManager.GetActiveScene().isDirty)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    throw new InvalidOperationException("Scene creation cancelled because the current Unity scene has unsaved changes.");
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "PogoDom";

            var root = new GameObject("PogoDomPrototype");
            root.AddComponent<PogoDomPrototypeBootstrap>();

            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save the POGODOM bootstrap scene at " + ScenePath);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
