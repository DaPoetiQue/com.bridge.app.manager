using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEditor.Build.Reporting;
using Bridge.Core.App.Manager;
using System;
using Bridge.Core.UnityEditor.App.Manager;
using UnityEngine;

public static class AppBuildConfig
{
        public static void BuildApp()
        {
            try
            {
                int sceneCount = SceneManager.sceneCount;
                string[] scenePath = new string[sceneCount];

                for (int i = 0; i < sceneCount; i++)
                {
                    scenePath[i] = SceneUtility.GetScenePathByBuildIndex(i);
                    UnityEngine.Debug.Log($"--> Found Scene @ : {scenePath[i]}");
                }

                Build(AppBuildManagerEditor.GetBuildSettings(AppBuildManagerEditor.GetDefaultStorageInfo()), scenePath, false);
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public static void Build(BuildSettingsData buildSettings, string[] scenePath, bool runApp)
        {
            switch (buildSettings.configurations.platform)
            {
                case BuildTarget.Android:

                    BuildAndroid(buildSettings, scenePath, runApp);

                    break;

                case BuildTarget.iOS:


                    if (BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                    {

                    }

                    break;

                case BuildTarget.StandaloneWindows:

                    BuildStandaloneWindows(buildSettings, scenePath, runApp);

                    break;
            }
        }

        private static void BuildStandaloneWindows(BuildSettingsData settings, string[] scenes, bool runApp)
        {

        }


        private static void BuildAndroid(BuildSettingsData settings, string[] scenes, bool runApp)
        {
            try
            {
                BuildPlayerOptions buildOptions = new BuildPlayerOptions();
                buildOptions.scenes = scenes;

                string buildDirectory = Directory.GetParent(Directory.GetCurrentDirectory()) + $"/App Builds/{settings.configurations.platform.ToString()}";

                UnityEngine.Debug.Log($"--> Dir : {buildDirectory}");

                if (!Directory.Exists(buildDirectory))
                {
                    Directory.CreateDirectory(buildDirectory);
                }

            
                string appFilePath = Path.Combine("D:/Documents/Work/Tools/Builds", settings.appInfo.appName + ".apk");
                // string appFilePath = Path.Combine(buildDirectory, settings.appInfo.appName + ".apk");

            settings.configurations.buildLocation = appFilePath;

            UnityEngine.Debug.Log($"--> Building App At Path : {settings.configurations.buildLocation}");

                if (string.IsNullOrEmpty(settings.configurations.buildLocation))
                {
                    return;
                }

                if (File.Exists(settings.configurations.buildLocation))
                {
                    File.Delete(settings.configurations.buildLocation);
                }

                buildOptions.locationPathName = settings.configurations.buildLocation;

                buildOptions.target = BuildTarget.Android;

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
                    //Debugger.Log(DebugData.LogType.LogInfo, "App build completed successfully.");
                }

                if (summary.result == BuildResult.Failed)
                {
                    //DebugConsole.Log(LogLevel.Success, $"App build failed.");
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
