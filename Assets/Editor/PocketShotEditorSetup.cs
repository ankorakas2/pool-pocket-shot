#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class PocketShotEditorSetup
{
    static PocketShotEditorSetup()
    {
        EditorApplication.delayCall += Apply;
    }

    public static void Apply()
    {
        PlayerSettings.companyName = "Anestis";
        PlayerSettings.productName = "PocketShot";
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.anestis.pocketshot");
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, "com.anestis.pocketshot");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        EnsureUrp();
        if (File.Exists("Assets/Scenes/Main.unity"))
        {
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/Scenes/Main.unity", true) };
        }
    }

    static void EnsureUrp()
    {
        const string dir = "Assets/Settings";
        if (!AssetDatabase.IsValidFolder(dir))
        {
            AssetDatabase.CreateFolder("Assets", "Settings");
        }

        var rendererPath = dir + "/PocketShotRenderer.asset";
        var pipePath = dir + "/PocketShotURP.asset";
        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(renderer, rendererPath);
        }

        var pipe = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipePath);
        if (pipe == null)
        {
            pipe = UniversalRenderPipelineAsset.Create(renderer);
            AssetDatabase.CreateAsset(pipe, pipePath);
        }

        GraphicsSettings.defaultRenderPipeline = pipe;
        QualitySettings.renderPipeline = pipe;
    }
}

public static class AndroidApkBuild
{
    [MenuItem("PocketShot/Build Android APK")]
    public static void BuildApk()
    {
        PocketShotEditorSetup.Apply();
        Directory.CreateDirectory("Builds");
        var apk = Path.Combine("Builds", "PocketShot.apk");
        var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Main.unity" },
            locationPathName = apk,
            target = BuildTarget.Android,
            options = BuildOptions.None
        });
        if (result.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("APK ready: " + Path.GetFullPath(apk));
            EditorUtility.RevealInFinder(apk);
        }
        else
        {
            Debug.LogError("Android build failed. In Unity Hub (Personal, free) add Android Build Support: SDK, NDK, OpenJDK.");
        }
    }
}
#endif
