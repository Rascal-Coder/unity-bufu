using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class LayoutFixBuild {
    [Serializable] public sealed class BuildResult { public string result,editor,path;public int errors,warnings;public string[] warningMessages; }
    public static void Build() {
        string root=Path.GetFullPath("../..");
        foreach(string asset in Directory.GetFiles("Assets/Resources","*.png",SearchOption.AllDirectories).Select(p=>p.Replace('\\','/')).Where(PackedUiTextureImporter.Handles)) {
            AssetDatabase.ImportAsset(asset,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(asset);
            PackedUiTextureImporter.Configure(importer);importer.SaveAndReimport();
        }
        string player=Path.Combine(root,"Build/LayoutFixCommercialPlayer/BufuRebuild.exe");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Intro.unity","Assets/Scenes/Main.unity"},locationPathName=player,target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
        var result=new BuildResult{result=report.summary.result.ToString(),editor=Application.unityVersion,path=player,errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,warningMessages=report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Warning).Select(m=>m.content).Distinct().ToArray()};
        File.WriteAllText(Path.Combine(root,"Documentation/LayoutFix-2026-10-20/unity-build-result.json"),JsonUtility.ToJson(result,true));
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Layout player build failed");
        Debug.Log("LAYOUT_BUILD_OK "+player);
    }
}
