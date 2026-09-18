#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class LookTextureImport
{
    static LookTextureImport()
    {
        EditorApplication.delayCall += Apply;
    }

    static void Apply()
    {
        Normal("Assets/Resources/Look/Wood_Normal.jpg");
        Normal("Assets/Resources/Look/Fabric_Normal.jpg");
        Normal("Assets/Resources/Look/Metal_Normal.jpg");
        Linear("Assets/Resources/Look/Wood_Rough.jpg");
        Linear("Assets/Resources/Look/Fabric_Rough.jpg");
        Linear("Assets/Resources/Look/Metal_Rough.jpg");
        Linear("Assets/Resources/Look/Metal_Metallic.jpg");
    }

    static void Normal(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null)
        {
            return;
        }

        if (imp.textureType == TextureImporterType.NormalMap && !imp.sRGBTexture)
        {
            return;
        }

        imp.textureType = TextureImporterType.NormalMap;
        imp.sRGBTexture = false;
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.anisoLevel = 4;
        imp.SaveAndReimport();
    }

    static void Linear(string path)
    {
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp == null)
        {
            return;
        }

        if (!imp.sRGBTexture)
        {
            return;
        }

        imp.sRGBTexture = false;
        imp.wrapMode = TextureWrapMode.Repeat;
        imp.SaveAndReimport();
    }
}
#endif
