using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>Export the existing game to Xcode on an editor with iOS Build Support installed.</summary>
public static class IosBuild {
    [MenuItem("不服开一局/iOS/导出 Xcode 工程")]
    public static void Export() {
        if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS,BuildTarget.iOS))
            throw new InvalidOperationException("当前编辑器没有安装 iOS Build Support。请在装有该模块的团结引擎中导出；生成 IPA 需要 Mac、Xcode 和 Apple 签名。");
        const string scene="Assets/Scenes/Main.unity";
        if(!File.Exists(scene))throw new FileNotFoundException("主场景不存在",scene);
        NativeParticleBuilder.Build();RebuildEditor.VerifyCore();
        string identifier=Environment.GetEnvironmentVariable("BUFU_IOS_BUNDLE_ID");
        if(string.IsNullOrWhiteSpace(identifier))identifier="com.rea.bufu";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS,identifier);
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS,ScriptingImplementation.IL2CPP);
        PlayerSettings.SetArchitecture(BuildTargetGroup.iOS,1); // ARM64
        PlayerSettings.iOS.targetOSVersionString="13.0";
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
        string team=Environment.GetEnvironmentVariable("BUFU_APPLE_TEAM_ID");
        if(!string.IsNullOrWhiteSpace(team)) {
            PlayerSettings.iOS.appleDeveloperTeamID=team;
            PlayerSettings.iOS.appleEnableAutomaticSigning=true;
        }
        const string output="Build/iOS/Xcode";
        Directory.CreateDirectory(output);
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
            scenes=new[]{scene},target=BuildTarget.iOS,locationPathName=output,options=BuildOptions.None});
        Directory.CreateDirectory("Documentation");
        File.WriteAllText("Documentation/ios-build-result.json",JsonUtility.ToJson(new Result{
            result=report.summary.result.ToString(),editor=Application.unityVersion,
            errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,output=output,bundleIdentifier=identifier},true));
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("iOS 导出失败："+report.summary.result);
        Debug.Log("BUFU_IOS_XCODE_EXPORT_OK "+Path.GetFullPath(output));
    }
    [Serializable] sealed class Result {public string result,editor,output,bundleIdentifier;public int errors,warnings;}
}
