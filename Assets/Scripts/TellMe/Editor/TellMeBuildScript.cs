#if UNITY_EDITOR
using System;
using System.IO;
using TellMe.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace TellMe.Editor
{
    /// <summary>
    /// Editor Automation & Android APK Builder untuk TellMe Edu (Siswa & Siswi SD Tunarungu).
    /// Menyediakan menu 1-Klik di Unity Editor dan method CLI batchmode untuk build APK Android.
    /// </summary>
    public static class TellMeBuildScript
    {
        private const string MainScenePath = "Assets/Scenes/MainScene.unity";
        private const string ApkRelativeOutputPath = "Builds/Android/TellMe_Offline_Release.apk";

        [MenuItem("TellMe Edu/1. Setup & Buka Main Scene (Siap Play)", false, 10)]
        public static void SetupMainScene()
        {
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Buat Main Camera
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(1f, 0.976f, 0.949f, 1f); // #FFF9F2 Sunny Cream
            cam.orthographic = true;
            camGo.AddComponent<AudioListener>();

            // 2. Buat Directional Light untuk pencahayaan 3D Stage
            var lightGo = new GameObject("Directional Light");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.15f;
            lightGo.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

            // 3. Buat Master UI Root
            var masterGo = new GameObject("[TellMe_MasterUI]");
            masterGo.AddComponent<TellMeMasterUI>();

            EditorSceneManager.SaveScene(scene, MainScenePath);

            // Set sebagai satu-satunya scene aktif di Build Settings agar langsung masuk ke aplikasi
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };

            AssetDatabase.SaveAssets();
            Debug.Log("[TellMeBuildScript] MainScene berhasil disiapkan di: " + MainScenePath);
        }

        [MenuItem("TellMe Edu/2. Build Android APK (TellMe_Offline_Release.apk)", false, 20)]
        public static void BuildAndroidAPK()
        {
            Debug.Log("[TellMeBuildScript] Memulai persiapan Build Android APK TellMe Edu...");

            // Pastikan MainScene sudah siap dengan TellMeMasterUI
            SetupMainScene();

            // Konfigurasi PlayerSettings Android (Smartphone Portrait)
            PlayerSettings.companyName = "DingBuzzer";
            PlayerSettings.productName = "TellMe Edu";
            PlayerSettings.bundleVersion = "2.0.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            var androidTarget = NamedBuildTarget.Android;
            PlayerSettings.SetApplicationIdentifier(androidTarget, "com.dingbuzzer.tellme");

            // Gunakan OpenGLES3 agar kompatibel di semua HP Android & tidak bentrok dengan ARCore/Vulkan
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

            // Konfigurasi Android SDK & Architecture
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.SetScriptingBackend(androidTarget, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // Pastikan tidak menggunakan App Bundle (.aab) melainkan file .apk langsung
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            string fullApkPath = Path.GetFullPath(ApkRelativeOutputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullApkPath));

            var buildOptions = new BuildPlayerOptions
            {
                scenes = new[] { MainScenePath },
                locationPathName = fullApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            Debug.Log("[TellMeBuildScript] Menjalankan BuildPipeline.BuildPlayer ke: " + fullApkPath);
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[TellMeBuildScript] BUILD SUKSES! Ukuran: {summary.totalSize / (1024 * 1024)} MB | Lokasi: {fullApkPath}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(0);
                }
            }
            else
            {
                Debug.LogError($"[TellMeBuildScript] BUILD GAGAL: {summary.result} | Total Error: {summary.totalErrors}");
                if (Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }
            }
        }
    }
}
#endif
