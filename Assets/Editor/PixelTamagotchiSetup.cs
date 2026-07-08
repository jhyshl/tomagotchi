#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace PixelTamagotchi.EditorTools
{
    public static class PixelTamagotchiSetup
    {
        [MenuItem("PixelTamagotchi/创建启动场景")]
        public static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera));
            camera.tag = "MainCamera";
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = new Color(0.12f, 0.13f, 0.15f);
            var app = new GameObject("PixelTamagotchiApp");
            app.AddComponent<PixelTamagotchi.PixelTamagotchiApp>();
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Main.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
            Debug.Log("已创建 Assets/Scenes/Main.unity，并加入 Build Settings。");
        }
    }
}
#endif
