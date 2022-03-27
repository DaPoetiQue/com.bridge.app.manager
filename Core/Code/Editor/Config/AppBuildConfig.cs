using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;
using Bridge.Core.App.Manager;
using System;
using Bridge.Core.UnityCustomEditor.App.Manager;
using Bridge.Core.UnityCustomEditor.Debugger;
using System.Collections.Generic;

public static class AppBuildConfig
{
    public static void BuildApp()
    {
        try
        {
            BuildSettings settings = BuildManager.GetBuildSettings(BuildManager.GetDefaultStorageInfo()).ToInstance();
            BuildApplication(settings.ToSerializable(), GetBuildScenes(SerializableInstanceDataConverter.GetBuildScenes(settings.buildScenes)), settings.buildAndRun);
        }
        catch (Exception exception)
        {
            DebugConsole.Log(Bridge.Core.Debug.LogLevel.Error, $"Build failed with error : {exception.Message}");
            throw exception;
        }
    }

    /// <summary>
    /// This function builds the app using the provided build settings data.
    /// </summary>
    /// <param name="buildSettings"></param>
    /// <param name="scenes"></param>
    /// <param name="runApp">Determines if the app should run after build</param>
    private static void BuildApplication(BuildSettingsData buildSettings, string[] scenes, bool runApp)
        {
            try
            {
                if (buildSettings.configurations.platform == BuildTarget.NoTarget)
                {
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Warning, $"Invalid Build Target Platform : There is no build target assigned ");
                    return;
                }

                BuildPlayerOptions buildOptions = new BuildPlayerOptions();
                buildOptions.scenes = scenes;

                string buildDirectory = Directory.GetParent(Directory.GetCurrentDirectory()) + $"/App Builds/{buildSettings.configurations.platform.ToString()}";

                if (!Directory.Exists(buildDirectory))
                {
                    Directory.CreateDirectory(buildDirectory);
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Debug, $"A new build directory has been created @ : {buildDirectory}");
                }
   
                buildSettings.configurations.targetBuildDirectory = GetBuildFolderPath(buildSettings);
                buildSettings.configurations.targetBuildDirectory.Replace(" ", string.Empty);
                buildSettings.configurations.buildLocation = GetBuildFilePath(buildSettings).Replace(" ", string.Empty);

                if (string.IsNullOrEmpty(buildSettings.configurations.buildLocation))
                {
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Warning, $"Build location not assigned.");
                    return;
                }

                if (File.Exists(buildSettings.configurations.buildLocation))
                {
                    File.Delete(buildSettings.configurations.buildLocation);
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Warning, $"Removing file @ : {buildSettings.configurations.buildLocation}");
                }

                if(GetRuntimeOs(buildSettings) != RuntimeOS.None)
                {
                    buildOptions.locationPathName = buildSettings.configurations.buildLocation;

                    buildOptions.target = buildSettings.configurations.platform;

                    if (runApp)
                    {
                        buildOptions.options = BuildOptions.AutoRunPlayer;
                    }
                    else
                    {
                        buildOptions.options = BuildOptions.None;
                    }

                    BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                    BuildSummary summary = report.summary;

                    if (summary.result == BuildResult.Succeeded)
                    {
                        EditorWindow.FocusWindowIfItsOpen<BuildManagerWindow>();
                        DebugConsole.Log(Bridge.Core.Debug.LogLevel.Success, "App build completed successfully.");
                    }

                    if (summary.result == BuildResult.Failed)
                    {
                        DebugConsole.Log(Bridge.Core.Debug.LogLevel.Error, "App build failed.");
                    }
                }
                else
                {
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Warning, $"Failed to get runtime os group for platform : : {buildSettings.configurations.platform.ToString()}");
                    return;
                } 
              
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

    public static string[] GetBuildScenes(BuildSceneData[] buildScenes)
    {

        if(buildScenes == null)
        {
            DebugConsole.Log(Bridge.Core.Debug.LogLevel.Warning, $"Build scenes can not Be NULL : Returning NULL");
            return null;
        }

        int sceneCount = buildScenes.Length;
        List<string> activeSceneList = new List<string>();

        for (int i = 0; i < sceneCount; i++)
        {
            if(buildScenes[i].isActive && activeSceneList.Contains(buildScenes[i].scenePath) == false)
            {
                activeSceneList.Add(buildScenes[i].scenePath);
            }
        }

        return activeSceneList.ToArray();
    }

    public static BuildSceneData[] GetBuildSceneData()
    {
        int sceneCount = SceneManager.sceneCount;
        BuildSceneData[] scenePath = new BuildSceneData[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            scenePath[i].scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            DebugConsole.Log(Bridge.Core.Debug.LogLevel.Debug, $"Found scene @ : {scenePath[i]}");
        }

        return scenePath;
    }


    /// <summary>
    /// This function is for getting a runtime platform group for the currently selected platform.
    /// </summary>
    /// <param name="buildSettings"></param>
    /// <returns>Runtime OS for the currently selected runtime platform.</returns>
    public static RuntimeOS GetRuntimeOs(BuildSettingsData buildSettings)
    {
        RuntimeOS runtimeOS = RuntimeOS.None;

        switch(buildSettings.configurations.platform)
        {
            case BuildTarget.Android:

                runtimeOS = RuntimeOS.Mobile;

                break;

            case BuildTarget.iOS:

                runtimeOS = RuntimeOS.Mobile;

                break;

            case BuildTarget.StandaloneWindows:


                runtimeOS = RuntimeOS.Standalone;

                break;

            case BuildTarget.StandaloneWindows64:

                runtimeOS = RuntimeOS.Standalone;

                break;

            case BuildTarget.StandaloneOSX:

                runtimeOS = RuntimeOS.Standalone;

                break;

            case BuildTarget.StandaloneLinux64:

                runtimeOS = RuntimeOS.Standalone;

                break;

            case BuildTarget.EmbeddedLinux:

                runtimeOS = RuntimeOS.Standalone;

                break;

            case BuildTarget.WebGL:

                runtimeOS = RuntimeOS.Web;

                break;

            case BuildTarget.WSAPlayer:

                runtimeOS = RuntimeOS.Web;

                break;

            case BuildTarget.CloudRendering:

                runtimeOS = RuntimeOS.Web;

                break;

            case BuildTarget.Switch:

                runtimeOS = RuntimeOS.Console;

                break;

            case BuildTarget.Stadia:

                runtimeOS = RuntimeOS.Console;

                break;

            case BuildTarget.XboxOne:

                runtimeOS = RuntimeOS.Console;

                break;

            case BuildTarget.GameCoreXboxOne:

                runtimeOS = RuntimeOS.Console;

                break;

            case BuildTarget.GameCoreXboxSeries:

                runtimeOS = RuntimeOS.Console;

                break;

            case BuildTarget.PS4:

                runtimeOS = RuntimeOS.Console;

                break;

            case BuildTarget.PS5:

                runtimeOS = RuntimeOS.Console;

                break;

            case BuildTarget.tvOS:

                runtimeOS = RuntimeOS.TV;

                break;
        }

        return runtimeOS;
    }

    public static string GetBuildFilePath(BuildSettingsData buildSettings)
        {
            string buildDir = Directory.GetCurrentDirectory() + "/Builds";

            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            return Path.Combine(buildDir, buildSettings.appInfo.displayName + $".{PlatformSpecificData.GetFileExtension(buildSettings.configurations.platform)}");
        }

    public static string GetBuildFolderPath(BuildSettingsData buildSettings)
    {
        string buildDir = Directory.GetCurrentDirectory() + "/Builds";

        if (!Directory.Exists(buildDir))
        {
            Directory.CreateDirectory(buildDir);
        }

        return buildDir;
    }
}
