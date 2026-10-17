using System;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

/// <summary>Light rounded icon is the application icon. Dark rounded icon is the launch logo.</summary>
[InitializeOnLoad]
public static class AppLogoSetup {
    const string LightPath="Assets/AppLogoLight.png";
    const string DarkPath="Assets/AppLogoDark.png";
    static int attempts;

    static AppLogoSetup() { EditorApplication.delayCall+=Apply; }

    [MenuItem("不服开一局/设置应用图标")]
    public static void Apply() {
        if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling||EditorApplication.isUpdating) {
            if(attempts++<8) EditorApplication.delayCall+=Apply;
            return;
        }
        var lightImporter=AssetImporter.GetAtPath(LightPath) as TextureImporter;
        var darkImporter=AssetImporter.GetAtPath(DarkPath) as TextureImporter;
        if(lightImporter==null||darkImporter==null) {
            if(attempts++<8) EditorApplication.delayCall+=Apply;
            return;
        }
        Prepare(lightImporter,false);
        Prepare(darkImporter,true);
        Assign();
    }

    static void Prepare(TextureImporter importer,bool sprite) {
        bool changed=false;
        var type=sprite?TextureImporterType.Sprite:TextureImporterType.Default;
        if(importer.textureType!=type) { importer.textureType=type; changed=true; }
        if(sprite&&importer.spriteImportMode!=SpriteImportMode.Single) { importer.spriteImportMode=SpriteImportMode.Single; changed=true; }
        if(importer.mipmapEnabled) { importer.mipmapEnabled=false; changed=true; }
        if(!importer.alphaIsTransparency) { importer.alphaIsTransparency=true; changed=true; }
        if(importer.npotScale!=TextureImporterNPOTScale.None) { importer.npotScale=TextureImporterNPOTScale.None; changed=true; }
        if(importer.textureCompression!=TextureImporterCompression.Uncompressed) {
            importer.textureCompression=TextureImporterCompression.Uncompressed; changed=true;
        }
        var platform=importer.GetDefaultPlatformTextureSettings();
        if(platform.textureCompression!=TextureImporterCompression.Uncompressed||platform.maxTextureSize<1024) {
            platform.textureCompression=TextureImporterCompression.Uncompressed;
            platform.maxTextureSize=2048;
            importer.SetPlatformTextureSettings(platform);
            changed=true;
        }
        if(changed) importer.SaveAndReimport();
    }

    static void Assign() {
        var light=AssetDatabase.LoadAssetAtPath<Texture2D>(LightPath);
        var darkSprite=AssetDatabase.LoadAssetAtPath<Sprite>(DarkPath);
        if(light==null) {
            Debug.LogWarning("APP_LOGO_MISSING "+LightPath);
            return;
        }
        bool changed=false;
        changed|=AssignGroup(NamedBuildTarget.Standalone,light);
        changed|=AssignGroup(NamedBuildTarget.Android,light);
        changed|=AssignGroup(NamedBuildTarget.iOS,light);
        var legacy=PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown);
        if(legacy==null||legacy.Length!=1||legacy[0]!=light) {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown,new[]{light});
            changed=true;
        }
        if(darkSprite!=null) {
            var logos=PlayerSettings.SplashScreen.logos;
            if(logos==null||logos.Length!=1||logos[0].logo!=darkSprite) {
                PlayerSettings.SplashScreen.logos=new[]{PlayerSettings.SplashScreenLogo.Create(2f,darkSprite)};
                changed=true;
            }
        }
        if(!changed) return;
        AssetDatabase.SaveAssets();
        Debug.Log("APP_LOGO_OK light=AppLogoLight dark=AppLogoDark");
    }

    static bool AssignGroup(NamedBuildTarget target,Texture2D icon) {
        try { return AssignGroupCore(target,icon); }
        catch(Exception e) { Debug.LogWarning("APP_LOGO_SKIP "+target+" "+e.Message); return false; }
    }

    static bool AssignGroupCore(NamedBuildTarget target,Texture2D icon) {
        int[] sizes=PlayerSettings.GetIconSizes(target,IconKind.Application);
        if(sizes==null||sizes.Length==0) return false;
        var current=PlayerSettings.GetIcons(target,IconKind.Application);
        if(current!=null&&current.Length==sizes.Length) {
            bool filled=true;
            for(int i=0;i<current.Length;i++) if(current[i]!=icon) { filled=false; break; }
            if(filled) return false;
        }
        var icons=new Texture2D[sizes.Length];
        for(int i=0;i<icons.Length;i++) icons[i]=icon;
        PlayerSettings.SetIcons(target,icons,IconKind.Application);
        return true;
    }
}
