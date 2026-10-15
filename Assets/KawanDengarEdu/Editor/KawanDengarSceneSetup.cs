#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using KawanDengarEdu.UI;

namespace KawanDengarEdu.EditorTools
{
    public static class KawanDengarSceneSetup
    {
        [MenuItem("KawanDengar Edu/✨ Bangun Scene Lengkap (1-Click Scene Setup)", false, 1)]
        public static void SetupCompleteMainScene()
        {
            // Buat scene baru
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Directional Light (Warm White, Intensitas: 1.2)
            var lightObj = new GameObject("Directional Light", typeof(Light));
            var light = lightObj.GetComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.90f);
            light.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

            // 2. Main Camera (Daylight Sky Color #E0F2FE, FOV 60)
            var camObj = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            var cam = camObj.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            ColorUtility.TryParseHtmlString("#E0F2FE", out var skyCol);
            cam.backgroundColor = skyCol;
            cam.fieldOfView = 60f;
            camObj.transform.position = new Vector3(0, 1.40f, -1.8f);
            camObj.transform.LookAt(new Vector3(0, 1.40f, 2.5f));
            camObj.tag = "MainCamera";

            // 3. Master App GameObject & Bangun Seluruh 11 Layar Figma + Stage 3D AR
            var masterObj = new GameObject("[KawanDengarEdu_App]", typeof(KawanDengarMasterUI));
            var masterUI = masterObj.GetComponent<KawanDengarMasterUI>();
            masterUI.BuildFullSceneInEditor();

            // Simpan Scene
            string scenePath = "Assets/Scenes/MainScene.unity";
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
            EditorSceneManager.SaveScene(newScene, scenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[KawanDengar Edu] Scene berhasil dibangun lengkap dan disimpan ke " + scenePath);

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Sukses!", 
                    "Scene 'MainScene.unity' berhasil dibangun lengkap!\n\n" +
                    "• 11 Layar antarmuka sesuai Figma telah tersusun di Canvas.\n" +
                    "• Panggung 3D AR Studio (Apel 4 Tahap, Rotator Trackball & Kebun) siap digunakan.\n" +
                    "• Tekan tombol Play (▶️) di Unity Editor untuk langsung mencoba seluruh fiturnya!", 
                    "Bagus!");
            }
        }

        [MenuItem("KawanDengar Edu/🔄 Setup Ulang Scene Aktif (In Current Scene)", false, 2)]
        public static void SetupCurrentActiveScene()
        {
            var oldCanvas = GameObject.Find("KawanDengarCanvas");
            if (oldCanvas != null) KawanDengarMasterUI.SafeDestroy(oldCanvas);
            var oldStage = GameObject.Find("3D_AR_Stage");
            if (oldStage != null) KawanDengarMasterUI.SafeDestroy(oldStage);

            var masterUI = Object.FindFirstObjectByType<KawanDengarMasterUI>();
            if (masterUI == null)
            {
                var masterObj = new GameObject("[KawanDengarEdu_App]", typeof(KawanDengarMasterUI));
                masterUI = masterObj.GetComponent<KawanDengarMasterUI>();
            }

            masterUI.BuildFullSceneInEditor();

            var currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(currentScene);
            AssetDatabase.SaveAssets();

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("Sukses!", 
                    $"Scene aktif '{currentScene.name}' berhasil diperbarui lengkap!\n\n" +
                    "• 11 Layar antarmuka sesuai Figma telah tersusun di Canvas.\n" +
                    "• Panggung 3D AR Studio & Kebun siap digunakan.\n" +
                    "• Tekan tombol Play (▶️) di Unity Editor!", 
                    "Bagus!");
            }
        }
    }
}
#endif
