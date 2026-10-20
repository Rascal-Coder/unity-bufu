using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Sideload test APK. IL2CPP ARM64 matches current phones; the debug keystore is enough to install.</summary>
public static class AndroidBuild {
    [MenuItem("不服开一局/Android/导出测试 APK")]
    public static void Export() {
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
            throw new InvalidOperationException("当前编辑器没有安装 Android Build Support。");
        string[] scenes={"Assets/Scenes/Intro.unity","Assets/Scenes/Main.unity"};
        if(!File.Exists(scenes[1]))throw new FileNotFoundException("主场景不存在",scenes[1]);
        PlayerSettings.companyName="REA Local";
        PlayerSettings.productName="TYCOON RUSH";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.rea.bufu");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
        PlayerSettings.Android.useCustomKeystore=false;
        // Use this editor's External Tools settings; Android support is optional.
        AppLogoSetup.Apply();
        const string output="Build/Android/TYCOON-RUSH-test.apk";
        Directory.CreateDirectory("Build/Android");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=scenes,target=BuildTarget.Android,locationPathName=output,options=BuildOptions.None});
        File.WriteAllText("Build/Android/build-result.json",JsonUtility.ToJson(new Result{
            result=report.summary.result.ToString(),editor=Application.unityVersion,
            errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,
            bytes=(long)report.summary.totalSize,output=Path.GetFullPath(output)},true));
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Android 打包失败："+report.summary.result);
        Debug.Log("BUFU_ANDROID_APK_OK "+Path.GetFullPath(output));
    }
    [Serializable] sealed class Result {public string result,editor,output;public int errors,warnings;public long bytes;}
}
