using System;
using UnityEditor;
// using Bridge.Core.App.Data.Storage;
using UnityEngine;

namespace Bridge.Core.App.Manager
{
    #region App Info

    public enum RuntimePlatform : byte
    {
        StandaloneOSX = 1, 
        StandaloneWindows, 
        iOS, 
        Android,
        StandaloneWindows64,
        WebGL, 
        WSAPlayer, 
        StandaloneLinux64, 
        PS4, 
        XBoxOne, 
        TVOS, 
        Switch, 
        Lumin, 
        Stadia, 
        CloudRendering, 
        GameCoreXBoxSeries, 
        GameCoreXBoxOne, 
        PS5, 
        EmbeddedLinux, 
        NoTarget
    }

    public enum BuildFileExtension : byte
    {
        apk,
        exe,
        none
    }

    public enum RuntimeOS : byte
    {
        Console = 1,
        Mobile,
        None,
        Standalone,
        TV,
        Web
    }

    [Serializable]
    public struct AppResolution
    {
        public int width;

        [Space(5)]
        public int height;
    }

    /// <summary>
    /// This class contains the app build settings.
    /// </summary>
    public class BuildSettings : ScriptableObject
    {
        [Space(5)]
        public AppInfo appInfo;

        [Space(5)]
        public BuildConfig configurations;

        #region Display Settings

        [Space(5)]
        public ConsoleDisplaySettings consoleDisplaySettings;

        [Space(5)]
        public MobileDisplaySettings mobileDisplaySettings;

        [Space(5)]
        public StandaloneDisplaySettings standaloneDisplaySettings;

        [Space(5)]
        public WebDisplaySettings webDisplaySettings;

        #endregion

        [Space(5)]
        public AndroidBuildSettings androidSettings;

        [Space(5)]
        public bool includeBuildScripts;

        #region Converted Settings

       public BuildSettingsData ToSerializable()
        {
            return new BuildSettingsData
            {
                appInfo = this.appInfo,
                configurations = this.configurations,
                consoleDisplaySettings = this.consoleDisplaySettings,
                mobileDisplaySettings = this.mobileDisplaySettings,
                standaloneDisplaySettings = this.standaloneDisplaySettings,
                webDisplaySettings = this.webDisplaySettings,
                androidSettings = this.androidSettings,
                includeBuildScripts = this.includeBuildScripts
            };
        }

        #endregion
    }

    /// <summary>
    /// This class contains the serializable app build settings.
    /// </summary>
    [Serializable]
    public class BuildSettingsData
    {
        [Space(5)]
        public AppInfo appInfo;

        [Space(5)]
        public BuildConfig configurations;

        #region Display Settings

        [Space(5)]
        public ConsoleDisplaySettings consoleDisplaySettings;

        [Space(5)]
        public MobileDisplaySettings mobileDisplaySettings;

        [Space(5)]
        public StandaloneDisplaySettings standaloneDisplaySettings;

        [Space(5)]
        public WebDisplaySettings webDisplaySettings;

        #endregion

        [Space(5)]
        public AndroidBuildSettings androidSettings;

        [Space(5)]
        public bool includeBuildScripts;

        public BuildSettings ToInstance()
        {
            BuildSettings buildSettings = ScriptableObject.CreateInstance<BuildSettings>();

            buildSettings.appInfo = appInfo;
            buildSettings.configurations = configurations;
            buildSettings.consoleDisplaySettings = consoleDisplaySettings;
            buildSettings.mobileDisplaySettings = mobileDisplaySettings;
            buildSettings.standaloneDisplaySettings = standaloneDisplaySettings;
            buildSettings.webDisplaySettings = webDisplaySettings;
            buildSettings.androidSettings = androidSettings;
            buildSettings.includeBuildScripts = includeBuildScripts;

            return buildSettings;
        }
    }

    /// <summary>
    /// Information about the app.
    /// </summary>
    [Serializable]
    public struct AppInfo
    {
        [Space(5)]
        public string companyName;

        [Space(5)]
        public string appName;

        [Space(5)]
        public string appVersion;

        [Space(5)]
        public Texture2D appIcon;

        [Space(5)]
        public Sprite splashScreen;

        [HideInInspector]
        public string appIdentifier;
    }

    /// <summary>
    /// App build settings.
    /// </summary>
    [Serializable]
    public struct BuildConfig
    {
        [Space(5)]
        public string[] buildScenes;

        [Space(5)]
        public BuildTarget platform;

        [Space(5)]
        public bool allowDebugging;

        [Space(5)]
        public bool developmentBuild;

        [HideInInspector]
        public string buildLocation;

        [HideInInspector]
        public string targetBuildDirectory;
    }

    #region Display Data

    [Serializable]
    public struct ConsoleDisplaySettings
    {
        public AppResolution resolution;
    }

    [Serializable]
    public struct MobileDisplaySettings
    {
        [Space(5)]
        public UIOrientation allowedOrientation;
    }

    [Serializable]
    public struct StandaloneDisplaySettings
    {
        [Space(5)]
        public bool windowed;

        [Space(5)]
        public AppResolution resolution;
    }

    [Serializable]
    public struct WebDisplaySettings
    {
        [Space(5)]
        public AppResolution resolution;
    }

    #endregion

    [Serializable]
    public struct AndroidBuildSettings
    {
        [Space(5)]
        public AndroidPreferredInstallLocation installLocation;

        [Space(5)]
        public AndroidSdkVersions sdkVersion;

        [Space(5)]
        public bool buildAppBundle;
    }

    public static class AppDataSettings
    {
        public static RuntimeOS GetRuntimeOS(BuildSettings buildSettings)
        {
            if (buildSettings.configurations.platform == BuildTarget.Android || buildSettings.configurations.platform == BuildTarget.iOS)
            {
                return RuntimeOS.Mobile;
            }

            if (buildSettings.configurations.platform == BuildTarget.StandaloneWindows ||
               buildSettings.configurations.platform == BuildTarget.StandaloneWindows64 ||
               buildSettings.configurations.platform == BuildTarget.StandaloneOSX ||
               buildSettings.configurations.platform == BuildTarget.StandaloneLinux64)
            {
                return RuntimeOS.Standalone;
            }

            if (buildSettings.configurations.platform == BuildTarget.XboxOne ||
                buildSettings.configurations.platform == BuildTarget.PS4 ||
                buildSettings.configurations.platform == BuildTarget.PS5 ||
                buildSettings.configurations.platform == BuildTarget.Switch)
            {
                return RuntimeOS.Console;
            }

            if (buildSettings.configurations.platform == BuildTarget.WebGL)
            {
                return RuntimeOS.Web;
            }

            return RuntimeOS.None;
        }
    }

    public static class PlatformSpecificData
    {
        public static BuildFileExtension GetFileExtension(BuildTarget platform)
        {
            BuildFileExtension output = BuildFileExtension.none;

            switch(platform)
            {
                case BuildTarget.Android:

                    output = BuildFileExtension.apk;

                    break;

                case BuildTarget.StandaloneWindows:

                    output = BuildFileExtension.exe;

                    break;

                case BuildTarget.StandaloneWindows64:

                    output = BuildFileExtension.exe;

                    break;
            }

            return output;
        }
    }

    [Serializable]
    public struct BatchFileData
    {
        public string batchFile;
        public string projectRootPath;
        public string fileRootPath;
        public string fullbatchFilePath;
    }


    [Serializable]
    public class BuildCompiler
    {
        #region Property Fields

        public string echoOff;
        public string echoPrepareBuild;
        public string removeDirectoryCommand;
        public string echoCopy;
        public string copyContentCommand;
        public string changeToBuildDirectory;
        public string echoCompile;
        public string compilerBuildCommand;
        public string pause;

        #endregion

        public override string ToString()
        {
            return $"{echoOff} \n " +
                   $"{echoPrepareBuild} \n " +
                   $"{removeDirectoryCommand} \n " +
                   $"{echoCopy} \n " +
                   $"{copyContentCommand} \n " +
                   $"{changeToBuildDirectory} \n " +
                   $"{echoCompile} \n " +
                   $"{compilerBuildCommand} \n " +
                   $"{pause} ";
        }
    }

    [Serializable]
    public class Compiler
    {
        #region Property Fields

        public string echoOff;
        public string echoInitializeBuild;
        public string editorLogBuildStarted;
        public string startBuildCommand;    
        public string moveBuildCommand;
        public string openBuildFolderPath;
        public string changeToProjectDirectory;
        public string editorLogBuildEnded;
        public string echoBuildCompleted;
        public string pause;

        #endregion

        public override string ToString()
        {
            return $"{echoOff} \n  " +
                   $"{echoInitializeBuild} \n  " +
                   $"{editorLogBuildStarted} \n " +
                   $"{startBuildCommand} \n " +           
                   $"{moveBuildCommand} \n " +
                   $"{openBuildFolderPath} \n " +
                   $"{changeToProjectDirectory} \n " +
                   $"{editorLogBuildEnded} \n " +
                   $"{echoBuildCompleted} \n " +
                   $"{pause}";
        }
    }

    [Serializable]
    public class BuildCleaner
    {
        #region Property Fields

        public string echoOff;
        public string echoCleaningBuild;
        public string editorLogCleanStarted;
        public string cleanBuildPathCommand;
        public string editorLogCleanEnded;

        #endregion

        public override string ToString()
        {
            return $"{echoOff} \n  " +
                   $"{echoCleaningBuild} \n  " +
                   $"{editorLogCleanStarted} \n  " +
                   $"{cleanBuildPathCommand} \n  " +
                   $"{editorLogCleanEnded}";
        }
    }

    #endregion
}
