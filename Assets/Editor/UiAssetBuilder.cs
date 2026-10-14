using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using Bufu.Rebuild.UI;

public sealed class UiTexturePostprocessor:AssetPostprocessor {
    static bool IsUiTexture(string path)=>path.StartsWith("Assets/Resources/UI/")||path.StartsWith("Assets/Resources/Art/UI/");
    void OnPreprocessTexture() {
        if(!IsUiTexture(assetPath))return;
        var importer=(TextureImporter)assetImporter;
        importer.textureType=TextureImporterType.Default;importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;
        importer.alphaIsTransparency=true;importer.alphaSource=TextureImporterAlphaSource.FromInput;importer.sRGBTexture=true;
        importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.maxTextureSize=8192;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.isReadable=false;
    }
}

/// <summary>Rebuilds the original TMP font assets from Assets/BufuSource/Fonts (exported by Tools/export_ui.py).</summary>
public static class UiAssetBuilder {
    const string Source="Assets/BufuSource/Fonts",Target="Assets/Resources/UI/Fonts";
    [MenuItem("不服开一局/重建原版字体")]
    public static void BuildFonts() {
        if(!Directory.Exists(Source))return;
        Directory.CreateDirectory(Target);
        foreach(var folder in Directory.GetDirectories(Source))BuildFont(folder.Replace('\\','/'));
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
    }

    static void BuildFont(string folder) {
        var meta=(Dictionary<string,object>)MiniJson.Parse(File.ReadAllText(folder+"/meta.json"));
        string name=meta.Str("name");string path=Target+"/"+name+".asset";
        AssetDatabase.DeleteAsset(path);
        var font=ScriptableObject.CreateInstance<TMP_FontAsset>();font.name=name;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(folder+"/font.json"),font);
        AssetDatabase.CreateAsset(font,path);

        var atlases=new List<Texture2D>();
        foreach(string file in meta.Arr("atlases").Cast<string>()) {
            var png=new Texture2D(2,2,TextureFormat.RGBA32,false);png.LoadImage(File.ReadAllBytes(folder+"/"+file));
            var atlas=new Texture2D(png.width,png.height,TextureFormat.Alpha8,false){name=name+" Atlas",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            atlas.SetPixels32(png.GetPixels32());atlas.Apply(false,false);Object.DestroyImmediate(png);
            AssetDatabase.AddObjectToAsset(atlas,font);atlases.Add(atlas);
        }

        var exported=meta.Obj("material");
        var shader=Shader.Find(exported.Str("shader","TextMeshPro/Distance Field"))??Shader.Find("TextMeshPro/Mobile/Distance Field");
        var material=new Material(shader){name=exported.Str("name",name+" Material")};
        var floats=exported.Obj("floats");if(floats!=null)foreach(var kv in floats)if(material.HasProperty(kv.Key))material.SetFloat(kv.Key,System.Convert.ToSingle(kv.Value));
        var colors=exported.Obj("colors");
        if(colors!=null)foreach(var kv in colors) {
            var c=((List<object>)kv.Value).Select(System.Convert.ToSingle).ToArray();
            if(material.HasProperty(kv.Key))material.SetColor(kv.Key,new Color(c[0],c[1],c[2],c[3]));
        }
        if(atlases.Count>0) {
            material.SetTexture(ShaderUtilities.ID_MainTex,atlases[0]);
            material.SetFloat(ShaderUtilities.ID_TextureWidth,atlases[0].width);material.SetFloat(ShaderUtilities.ID_TextureHeight,atlases[0].height);
        }
        AssetDatabase.AddObjectToAsset(material,font);

        var so=new SerializedObject(font);
        var textures=so.FindProperty("m_AtlasTextures");textures.arraySize=atlases.Count;
        for(int i=0;i<atlases.Count;i++)textures.GetArrayElementAtIndex(i).objectReferenceValue=atlases[i];
        so.FindProperty("material").objectReferenceValue=material;
        var population=so.FindProperty("m_AtlasPopulationMode");if(population!=null)population.intValue=(int)AtlasPopulationMode.Static;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(font);
        font.ReadFontAssetDefinition();
        Debug.Log("Rebuilt TMP font "+name+": "+font.characterTable.Count+" characters, "+font.glyphTable.Count+" glyphs");
    }

    const string SpriteSource="Assets/BufuSource/SpriteAssets",SpriteTarget="Assets/Resources/UI/SpriteAssets";
    [MenuItem("不服开一局/重建原版图字")]
    public static void BuildSpriteAssets() {
        if(!Directory.Exists(SpriteSource))return;
        Directory.CreateDirectory(SpriteTarget);
        foreach(var folder in Directory.GetDirectories(SpriteSource))BuildSpriteAsset(folder.Replace('\\','/'));
        AssetDatabase.SaveAssets();AssetDatabase.Refresh();
    }

    static void BuildSpriteAsset(string folder) {
        var meta=(Dictionary<string,object>)MiniJson.Parse(File.ReadAllText(folder+"/meta.json"));
        string name=meta.Str("name");string path=SpriteTarget+"/"+name+".asset";
        AssetDatabase.DeleteAsset(path);
        var asset=ScriptableObject.CreateInstance<TMP_SpriteAsset>();asset.name=name;
        JsonUtility.FromJsonOverwrite(File.ReadAllText(folder+"/sprite.json"),asset);
        AssetDatabase.CreateAsset(asset,path);
        var sheet=new Texture2D(2,2,TextureFormat.RGBA32,false);sheet.LoadImage(File.ReadAllBytes(folder+"/atlas.png"));
        sheet.name=name+" Atlas";sheet.filterMode=FilterMode.Bilinear;sheet.wrapMode=TextureWrapMode.Clamp;sheet.Apply(false,false);
        AssetDatabase.AddObjectToAsset(sheet,asset);
        var material=new Material(Shader.Find("TextMeshPro/Sprite")){name=name+" Material"};
        material.SetTexture(ShaderUtilities.ID_MainTex,sheet);
        AssetDatabase.AddObjectToAsset(material,asset);
        asset.spriteSheet=sheet;asset.material=material;
        EditorUtility.SetDirty(asset);
        asset.UpdateLookupTables();
        Debug.Log("Rebuilt TMP sprite asset "+name+": "+asset.spriteCharacterTable.Count+" sprites");
    }
}
