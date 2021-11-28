using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;
using Bridge.Core.App.Manager;
using System;
using Bridge.Core.UnityEditor.App.Manager;
using Bridge.Core.UnityEditor.Debugger;

public static class AppBuildConfig
{
    public static void BuildApp()
    {
        try
        {
            BuildSettingsData settings = AppBuildManagerEditor.GetBuildSettings(AppBuildManagerEditor.GetDefaultStorageInfo());
            BuildApplication(settings, GetBuildScenes(), settings.buildAndRun);
        }
        catch (Exception exception)
        {
            DebugConsole.Log(Bridge.Core.Debug.LogLevel.Error, $"Build failed with error : {exception.Message}");
            throw exception;
        }
    }

    private static void BuildApplication(BuildSettingsData buildSettings, string[] scenes, bool runApp)
        {
            try
            {
                BuildPlayerOptions buildOptions = new BuildPlayerOptions();
                buildOptions.scenes = scenes;

                string buildDirectory = Directory.GetParent(Directory.GetCurrentDirectory()) + $"/App Builds/{buildSettings.configurations.platform.ToString()}";

                if (!Directory.Exists(buildDirectory))
                {
                    Directory.CreateDirectory(buildDirectory);
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Debug, $"A new build directory has been created @ : {buildDirectory}");
                }
   
                buildSettings.configurations.targetBuildDirectory = GetBuildFolderPath(buildSettings);
                buildSettings.configurations.buildLocation = GetBuildFilePath(buildSettings);

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
                    EditorWindow.FocusWindowIfItsOpen<AppBuildManagerEditor>();
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Success, "App build completed successfully.");
                }

                if (summary.result == BuildResult.Failed)
                {
                    DebugConsole.Log(Bridge.Core.Debug.LogLevel.Error, "App build failed.");
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

    private static string[] GetBuildScenes()
    {
        int sceneCount = SceneManager.sceneCount;
        string[] scenePath = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            scenePath[i] = SceneUtility.GetScenePathByBuildIndex(i);
            DebugConsole.Log(Bridge.Core.Debug.LogLevel.Debug, $"Found scene @ : {scenePath[i]}");
        }

        return scenePath;
    }

    public static string GetBuildFilePath(BuildSettingsData buildSettings)
        {
            string buildDir = Directory.GetCurrentDirectory() + "/Builds";

            if (!Directory.Exists(buildDir))
            {
                Directory.CreateDirectory(buildDir);
            }

            return Path.Combine(buildDir, buildSettings.appInfo.appName + $".{PlatformSpecificData.GetFileExtension(buildSettings.configurations.platform)}");
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
