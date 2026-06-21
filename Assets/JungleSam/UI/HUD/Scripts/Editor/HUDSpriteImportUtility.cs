#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class HUDSpriteImportUtility
{
    private const string ArtRoot = "Assets/JungleSam/UI/Art/JungleSam_HUD_UI_Assets/Cleaned/";

    [MenuItem("JungleSam/HUD/Apply HUD Sprite Import Settings")]
    public static void ApplyImportSettings()
    {
        ConfigureSheet("UI_Panel_Background.png");
        ConfigureSheet("UI_Panel_Frame.png");
        ConfigureSheet("UI_Button_Dark.png");
        ConfigureSheet("UI_Button_Selected.png");
        ConfigureSingle("UI_Crosshair.png", Vector4.zero);
        ConfigureSheet("UI_Overlay_Scanlines.png");
        ConfigureSingle("UI_FieldRadio_Story.png", Vector4.zero);

        ConfigureSheet("UI_Icons_Combat_Sheet.png");
        ConfigureSheet("UI_Icons_Story_Sheet.png");
        ConfigureSheet("UI_Bars_Sheet.png");
        ConfigureSheet("UI_Decor_Sheet.png");

        AssetDatabase.Refresh();
        Debug.Log("JungleSam HUD sprite import settings applied. Slice sprite sheets manually in Sprite Editor if needed.");
    }

    private static void ConfigureSingle(string fileName, Vector4 border)
    {
        TextureImporter importer = GetImporter(fileName);

        if (importer == null)
            return;

        ApplyCommonSettings(importer);
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spriteBorder = border;
        importer.SaveAndReimport();
    }

    private static void ConfigureSheet(string fileName)
    {
        TextureImporter importer = GetImporter(fileName);

        if (importer == null)
            return;

        ApplyCommonSettings(importer);
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.SaveAndReimport();
    }

    private static TextureImporter GetImporter(string fileName)
    {
        string path = ArtRoot + fileName;
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

        if (importer == null)
            Debug.LogWarning($"HUD sprite not found or not a texture: {path}");

        return importer;
    }

    private static void ApplyCommonSettings(TextureImporter importer)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.npotScale = TextureImporterNPOTScale.None;
    }
}
#endif
