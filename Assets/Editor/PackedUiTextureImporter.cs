using UnityEditor;
using UnityEngine;

/// <summary>Shared sheets and standalone UI use the same platform texture profile.</summary>
public sealed class PackedUiTextureImporter:AssetPostprocessor {
    public override int GetPostprocessOrder()=>200;
    public static bool Handles(string path)=>path.StartsWith("Assets/Resources/PackedUI/")||path.StartsWith("Assets/Resources/CommerceArt/")||path.StartsWith("Assets/Resources/UI/Sprites/");
    void OnPreprocessTexture(){if(Handles(assetPath))Configure((TextureImporter)assetImporter);}
    public static void Configure(TextureImporter importer) {
        importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;
        importer.mipmapEnabled=false;importer.isReadable=false;importer.sRGBTexture=true;
        importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
        importer.maxTextureSize=8192;importer.textureCompression=TextureImporterCompression.CompressedHQ;
        Set(importer,"Standalone",TextureImporterFormat.BC7);
        Set(importer,"Android",TextureImporterFormat.ASTC_4x4);
        Set(importer,"iPhone",TextureImporterFormat.ASTC_4x4);
    }
    static void Set(TextureImporter importer,string target,TextureImporterFormat format) {
        var setting=importer.GetPlatformTextureSettings(target);setting.name=target;setting.overridden=true;
        setting.maxTextureSize=8192;setting.format=format;setting.compressionQuality=100;
        setting.crunchedCompression=false;importer.SetPlatformTextureSettings(setting);
    }
}
