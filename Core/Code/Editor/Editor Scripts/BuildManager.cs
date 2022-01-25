using Bridge.Core.App.Events;
using Bridge.Core.App.Manager;
using Bridge.Core.UnityEditor.Debugger;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using static UnityEditor.PlayerSettings;
using Bridge.Core.App.Data.Storage;

namespace Bridge.Core.UnityEditor.App.Manager
{
    public static class BuildManager
    {
        #region Serializations

        private static string fileName = "BuildSettingsData";
        private static string folderName = "BridgeEditor";

        #endregion

        #region Build App

        /// <summary>
        /// This function builds the app to a defined directory.
        /// </summary>
        [MenuItem("3ridge/Config/Build App #B")]
        public static void Build()
        {
            BuildSettingsData buildSettings = GetBuildSettings(GetDefaultStorageInfo());

            string fileFullPath = buildSettings.configurations.buildLocation;
            buildSettings.configurations.buildLocation = fileFullPath;

            if (string.IsNullOrEmpty(buildSettings.configurations.targetBuildDirectory) ||   Storage.Directory.FolderExist(buildSettings.configurations.targetBuildDirectory) == false)
            {
                buildSettings.configurations.targetBuildDirectory = EditorUtility.SaveFolderPanel("Choose Build Folder", "", "").Replace(" ", string.Empty);
                ApplyAppSettings(buildSettings);
                DebugConsole.Log(Debug.LogLevel.Debug, $"Build Directory Set @ : {buildSettings.configurations.buildLocation}");
            }

            #region Saving Editor Data

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(currentScene);
            EditorApplication.ExecuteMenuItem("File/Save Project");

            #endregion

            if (!string.IsNullOrEmpty(buildSettings.configurations.targetBuildDirectory) && Storage.Directory.FolderExist(buildSettings.configurations.targetBuildDirectory) == true)
            {
                BuildCompilerScript.BuildCompiler(buildSettings, (results, resultsData) =>
                {
                    if (results.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Error, results.errorValue);
                        return;
                    }

                    if (results.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                    }
                });
            }
        }

        #endregion

        #region App Build Settings

        /// <summary>
        /// Used by the App Build Manager To Update Build Settings Window.
        /// </summary>
        /// <returns></returns>
        public static BuildSettings GetCurrentBuildSettings()
        {
            BuildSettingsData settings = new BuildSettingsData();

            Storage.Directory.DataPathExists(GetDefaultStorageInfo(), (resultsData, results) =>
            {
                if (results.error == true)
                {
                    settings = GetDefaultBuildSettings();
                    DebugConsole.Log(Debug.LogLevel.Warning, $"Build data not found @ : {GetDefaultStorageInfo().filePath}. - Getting default build settings.");
                }

                if (results.success == true)
                {
                    settings = GetBuildSettings(GetDefaultStorageInfo());
                    DebugConsole.Log(Debug.LogLevel.Success, $"Build data loaded successfully @ : {GetDefaultStorageInfo().filePath}.");
                }

            });

            return settings.ToInstance();
        }

        private static BuildSettingsData GetDefaultBuildSettings()
        {
            BuildSettingsData buildSettings = new BuildSettingsData();

            #region App Info

            buildSettings.appInfo = GetDefaultAppInfo().ToSerializable();

            #endregion

            #region Build Scenes

            buildSettings.buildScenes = AppBuildConfig.GetBuildScenes();

            #endregion

            #region Build Config

            buildSettings.configurations = GetDefaultBuildConfigurations();

            #endregion

            #region Standalone Settings

            //buildSettings.standaloneBuildSettings = GetDefaultBuildStandaloneBuildSettings();

            #endregion

            return buildSettings;
        }

        #region Get Default Settings

        public static AppInfo GetDefaultAppInfo()
        {
            AppInfo appInfo = new AppInfo();

            #region Info

            appInfo.companyName = companyName;
            appInfo.appName = productName;
            appInfo.appVersion = bundleVersion;
            appInfo.appIdentifier = applicationIdentifier;

            #endregion

            #region App Icon

            BuildTarget platform = EditorUserBuildSettings.activeBuildTarget;
            var nameBuild = NamedBuildTarget.FromBuildTargetGroup(GetBuildTargetGroup(platform));

            if (GetIcons(nameBuild, IconKind.Application).Length > 0)
            {
                appInfo.appIcon = GetIcons(nameBuild, IconKind.Application)[0];
            }

            #endregion

            #region Splash Screen

            if (PlayerSettings.SplashScreen.logos.Length > 0)
            {
                appInfo.splashScreens.screens = GetSplashScreenLogoData(PlayerSettings.SplashScreen.logos);
            }

            if (PlayerSettings.SplashScreen.background != null)
            {
                appInfo.splashScreens.background = PlayerSettings.SplashScreen.background;
                appInfo.splashScreens.backgroundColor = PlayerSettings.SplashScreen.backgroundColor;
            }
            else
            {
                appInfo.splashScreens.backgroundColor = Color.black;
            }

            appInfo.splashScreens.unityLogoStyle = PlayerSettings.SplashScreen.unityLogoStyle;
            appInfo.splashScreens.animationMode = PlayerSettings.SplashScreen.animationMode;
            appInfo.splashScreens.logoDrawMode = PlayerSettings.SplashScreen.drawMode;
            appInfo.splashScreens.showSplashScreen = PlayerSettings.SplashScreen.show;
            appInfo.splashScreens.showUnityLogo = PlayerSettings.SplashScreen.showUnityLogo;

            #endregion

            return appInfo;
        }

        public static BuildConfig GetDefaultBuildConfigurations()
        {
            BuildConfig buildConfig = new BuildConfig();

            buildConfig.platform = EditorUserBuildSettings.activeBuildTarget;
            buildConfig.allowDebugging = EditorUserBuildSettings.allowDebugging;
            buildConfig.developmentBuild = EditorUserBuildSettings.development;

            return buildConfig;
        }

        public static StandaloneBuildSettings GetDefaultBuildStandaloneBuildSettings()
        {
            StandaloneBuildSettings settings = new StandaloneBuildSettings();

            #region Mac OSX

            settings.mac.colorSpace = colorSpace;
            settings.mac.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneOSX);

            #endregion

            #region Windows

            settings.windows.colorSpace = colorSpace;

            if(GetDefaultBuildSettings().configurations.platform == BuildTarget.StandaloneWindows)
            {
                settings.windows.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneWindows);
            }

            if (GetDefaultBuildSettings().configurations.platform == BuildTarget.StandaloneWindows64)
            {
                settings.windows.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneWindows64);
            }

            #endregion

            #region Linux

            settings.linux.colorSpace = colorSpace;
            settings.linux.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneLinux64);

            #endregion

            #region Other Settings

            settings.otherSettings.apiCompatibility = GetApiCompatibilityLevel(GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            settings.otherSettings.scriptingBackend = GetScriptingBackend(GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            #endregion

            return settings;
        }

        #endregion

        #region Get Serialized Settings

        #endregion

        /// <summary>
        /// Converts Unity splash screen logos array to bridge splash screen logo data array.
        /// </summary>
        /// <param name="splashScreenLogos"></param>
        /// <returns></returns>
        private static Core.App.Manager.SplashScreenLogo[] GetSplashScreenLogoData(PlayerSettings.SplashScreenLogo[] splashScreenLogos)
        {
            if (splashScreenLogos.Length <= 0)
            {
                DebugConsole.Log(Debug.LogLevel.Warning, "There are no splash screen logos found/assigned.");
                return null;
            }

            Core.App.Manager.SplashScreenLogo[] splashScreenLogoDataList = new Core.App.Manager.SplashScreenLogo[splashScreenLogos.Length];

            for (int i = 0; i < splashScreenLogos.Length; i++)
            {
                splashScreenLogoDataList[i].duration = splashScreenLogos[i].duration;
                splashScreenLogoDataList[i].logo = splashScreenLogos[i].logo;
            }

            return splashScreenLogoDataList;
        }

        /// <summary>
        /// Converts Bridge splash screen logo data array to Unity splash screen logos array.
        /// </summary>
        /// <param name="splashScreenLogos"></param>
        /// <returns></returns>
        private static PlayerSettings.SplashScreenLogo[] GetSplashScreenLogos(Core.App.Manager.SplashScreenLogo[] splashScreenLogoData)
        {
            if (splashScreenLogoData.Length <= 0)
            {
                DebugConsole.Log(Debug.LogLevel.Warning, "There's no splash screen logo data found/assigned.");
                return null;
            }

            PlayerSettings.SplashScreenLogo[] splashScreenLogosList = new PlayerSettings.SplashScreenLogo[splashScreenLogoData.Length];

            for (int i = 0; i < splashScreenLogoData.Length; i++)
            {
                splashScreenLogosList[i] = PlayerSettings.SplashScreenLogo.Create(splashScreenLogoData[i].duration, splashScreenLogoData[i].logo);
            }

            return splashScreenLogosList;
        }

        /// <summary>
        /// Global Build Setting Data
        /// </summary>
        /// <param name="directoryInfo">The directory info used to load the data file from storage.</param>
        /// <returns>Returns A Serializable Version Of The Build Settings Data</returns>
        public static BuildSettingsData GetBuildSettings(StorageData.DirectoryInfoData directoryInfo)
        {
            BuildSettingsData settings = new BuildSettingsData();

            Storage.JsonData.Load<BuildSettingsData>(directoryInfo, (loadedResults, loadStatus) =>
            {
                if (loadStatus.error)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, $"Load Failed With Error: {loadStatus.errorValue}");
                    return;
                }

                if (loadStatus.success)
                {
                    settings = loadedResults;

                    DebugConsole.Log(Debug.LogLevel.Success, $"Load Completed With Results : {loadStatus.successValue}");
                }
            });

            return settings;
        }

        /// <summary>
        /// The default storage director info for the App Build Manager.
        /// </summary>
        /// <returns></returns>
        public static StorageData.DirectoryInfoData GetDefaultStorageInfo()
        {
            var directoryInfo = new StorageData.DirectoryInfoData
            {
                fileName = fileName,
                folderName = folderName,
                extensionType = StorageData.ExtensionType.json
            };

            return directoryInfo;
        }

        public static Texture2D[] GetPlatformIconList(Texture2D icon)
        {
            int minIconsCount = 8;
            Texture2D[] icons = new Texture2D[minIconsCount];

            for (int i = 0; i < icons.Length; i++)
            {
                icons[i] = icon;
            }

            return icons;
        }

        /// <summary>
        /// This method create build settings for the selected platform.
        /// </summary>
        /// <param name="buildSettings"></param>
        public static void ApplyAppSettings(BuildSettingsData buildSettings)
        {
            ApplyAppInfo(buildSettings, (callbackResults, resultsData) =>
            {
                if (callbackResults.error)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, $"Failed to apply app info with results : {callbackResults.errorValue}.");
                    return;
                }

                if (callbackResults.success)
                {
                    buildSettings.appInfo = resultsData.ToSerializable();

                    ApplyBuildSettings(buildSettings, (callbackResults, resultsData) =>
                    {
                        if (callbackResults.error)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, $"Failed to apply build settings with results : {callbackResults.errorValue}.");
                            return;
                        }

                        if (callbackResults.success)
                        {
                            Storage.JsonData.Save(GetDefaultStorageInfo(), buildSettings, (infoData, saveResults) =>
                            {
                                if (saveResults.error)
                                {
                                    DebugConsole.Log(Debug.LogLevel.Error, saveResults.errorValue);
                                    return;
                                }

                                if (saveResults.success)
                                {
                                    DebugConsole.Log(Debug.LogLevel.Success, saveResults.successValue);
                                }
                            });
                        }
                    });
                }
            });
        }

        #endregion

        #region Settings

        /// <summary>
        /// Applies the app info for the current project.
        /// </summary>
        /// <param name="buildSettingsData"></param>
        /// <param name="callback"></param>
        private static void ApplyAppInfo(BuildSettingsData buildSettingsData, Action<AppEventsData.CallBackResults, AppInfo> callback = null)
        {
            AppEventsData.CallBackResults callBackResults = new AppEventsData.CallBackResults();

            try
            {
                if (string.IsNullOrEmpty(buildSettingsData.appInfo.companyName))
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, "App info's company name field is empty. Please assign company name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                    return;
                }

                if (string.IsNullOrEmpty(buildSettingsData.appInfo.appName))
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, "App info's app name field is empty. Please assign app name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                    return;
                }

                companyName = (string.IsNullOrEmpty(buildSettingsData.appInfo.companyName)) ? companyName : buildSettingsData.appInfo.companyName;
                productName = (string.IsNullOrEmpty(buildSettingsData.appInfo.appName)) ? productName : buildSettingsData.appInfo.appName;
                bundleVersion = (string.IsNullOrEmpty(buildSettingsData.appInfo.appVersion)) ? bundleVersion : buildSettingsData.appInfo.appVersion;

                string appCompanyName = buildSettingsData.appInfo.companyName;

                if (appCompanyName.Contains(" "))
                {
                    appCompanyName = appCompanyName.Replace(" ", "");
                }

                string appName = buildSettingsData.appInfo.appName;

                if (appName.Contains(" "))
                {
                    appName = appName.Replace(" ", "");
                }

                //PlayerSettings.SplashScreen.logos = GetSplashScreenLogos(buildSettingsData.appInfo.splashScreens.screens);
                //PlayerSettings.SplashScreen.background = buildSettingsData.appInfo.splashScreens.background;
                //PlayerSettings.SplashScreen.backgroundColor = buildSettingsData.appInfo.splashScreens.backgroundColor;


                PlayerSettings.SplashScreen.unityLogoStyle = buildSettingsData.appInfo.splashScreens.unityLogoStyle;
                PlayerSettings.SplashScreen.animationMode = buildSettingsData.appInfo.splashScreens.animationMode;
                PlayerSettings.SplashScreen.drawMode = buildSettingsData.appInfo.splashScreens.logoDrawMode;
                PlayerSettings.SplashScreen.showUnityLogo = buildSettingsData.appInfo.splashScreens.showUnityLogo;
                PlayerSettings.SplashScreen.show = buildSettingsData.appInfo.splashScreens.showSplashScreen;

                if (buildSettingsData.appInfo.appIcon != null)
                {
                    //SetIconsForTargetGroup(GetBuildTargetGroup(buildSettingsData.configurations.platform), GetPlatformIconList(buildSettingsData.appInfo.appIcon));
                }

                buildSettingsData.appInfo.appIdentifier = $"com.{appCompanyName}.{appName}";

                callBackResults.success = true;
                callback.Invoke(callBackResults, buildSettingsData.appInfo.ToInstance());
            }
            catch (Exception exception)
            {
                callBackResults.error = true;
                callBackResults.errorValue = exception.Message;
                callback.Invoke(callBackResults, buildSettingsData.appInfo.ToInstance());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildSettings"></param>
        /// <param name="callback"></param>
        public static void ApplyBuildSettings(BuildSettingsData buildSettings, Action<AppEventsData.CallBackResults, BuildSettingsData> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                SwitchBuildTarget(buildSettings.configurations, switchResults =>
                {
                    if (switchResults.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, switchResults.errorValue);
                        return;
                    }

                    if (switchResults.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, $"Applied build settings for : {buildSettings.configurations.platform}");

                        if (buildSettings.configurations.platform == BuildTarget.Android || buildSettings.configurations.platform == BuildTarget.iOS)
                        {
                            switch (buildSettings.mobileDisplaySettings.allowedOrientation)
                            {
                                case UIOrientation.AutoRotation:

                                    defaultInterfaceOrientation = UIOrientation.AutoRotation;

                                    break;

                                case UIOrientation.Portrait:

                                    defaultInterfaceOrientation = UIOrientation.Portrait;

                                    break;

                                case UIOrientation.PortraitUpsideDown:

                                    defaultInterfaceOrientation = UIOrientation.PortraitUpsideDown; ;

                                    break;

                                case UIOrientation.LandscapeLeft:

                                    defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

                                    break;

                                case UIOrientation.LandscapeRight:

                                    defaultInterfaceOrientation = UIOrientation.LandscapeRight;

                                    break;
                            }
                        }

                        switch (EditorUserBuildSettings.activeBuildTarget)
                        {
                            case BuildTarget.Android:

                                SetApplicationIdentifier(BuildTargetGroup.Android, buildSettings.appInfo.appIdentifier);

                                GraphicsDeviceType[] graphicsDeviceType = new GraphicsDeviceType[1];
                                graphicsDeviceType[0] = GraphicsDeviceType.OpenGLES3;
                                SetGraphicsAPIs(buildSettings.configurations.platform, graphicsDeviceType);

                                SetMobileMTRendering(BuildTargetGroup.Android, false);

                                Android.minSdkVersion = buildSettings.androidBuildSettings.sdkVersion;
                                Android.androidTVCompatibility = false;
                                Android.preferredInstallLocation = buildSettings.androidBuildSettings.installLocation;
                                Android.ARCoreEnabled = true;

                                EditorUserBuildSettings.buildAppBundle = buildSettings.androidBuildSettings.buildAppBundle;

                                break;

                            case BuildTarget.iOS:

                                SetApplicationIdentifier(BuildTargetGroup.iOS, buildSettings.appInfo.appIdentifier);

                                break;

                            case BuildTarget.StandaloneWindows:

                                SetScriptingBackend(GetBuildTargetGroup(buildSettings.configurations.platform), buildSettings.standaloneBuildSettings.otherSettings.scriptingBackend);

                                break;

                            case BuildTarget.WebGL:

                                SetApplicationIdentifier(BuildTargetGroup.WebGL, buildSettings.appInfo.appIdentifier);

                                break;
                        }

                        EditorUserBuildSettings.allowDebugging = buildSettings.configurations.allowDebugging;
                        EditorUserBuildSettings.development = buildSettings.configurations.developmentBuild;
                        EditorUserBuildSettings.SetBuildLocation(buildSettings.configurations.platform, buildSettings.configurations.buildLocation);

                        results.success = true;
                        callback.Invoke(results, buildSettings);
                    }
                });
            }
            catch (Exception exception)
            {
                results.error = true;
                results.errorValue = exception.Message;
                callback.Invoke(results, new BuildSettingsData());
            }
        }

        /// <summary>
        /// Switch build platform to the selected target platform.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="callback"></param>
        public static void SwitchBuildTarget(BuildConfig config, Action<AppEventsData.CallBackResults> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                IsPlatformSupported(config.platform, supportResults =>
                {
                    if (supportResults.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, supportResults.errorValue);
                        EditorUtility.DisplayDialog("3ridge Build Settings", supportResults.errorValue, "Cancel");
                        return;
                    }

                    if (supportResults.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, $"Config Success : {supportResults.successValue}");
                        if (config.platform == EditorUserBuildSettings.activeBuildTarget)
                        {
                            results.success = true;
                            results.successValue = $"Applied build settings for : {config.platform}";
                        }
                        else
                        {
                           if(EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetBuildTargetGroup(config.platform), config.platform))
                            {
                                results.success = true;
                                results.successValue = $"Current build target has been successfully switched to : {config.platform}";
                            }
                        }

                        callback.Invoke(results);
                    }
                });
            }
            catch (Exception exception)
            {
                results.error = true;
                results.errorValue = exception.Message;
                callback.Invoke(results);
            }
        }

        /// <summary>
        /// Checks if the selected build platform is supported in the editor.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <param name="callback"></param>
        public static void IsPlatformSupported(BuildTarget buildTarget, Action<AppEventsData.CallBackResults> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
                var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

                results.success = (bool)isPlatformSupportLoaded.Invoke(null, new object[] { (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { buildTarget }) });

                if (results.success == true)
                {
                    results.successValue = $"Build platform for {buildTarget.ToString()} is supported.";
                }
                else
                {
                    results.error = true;
                    results.errorValue = $"There is no build module installed for : {buildTarget.ToString()}.";
                }

                callback.Invoke(results);
            }
            catch (Exception exception)
            {
                results.error = true;
                results.errorValue = exception.Message;
                callback.Invoke(results);
            }
        }

        /// <summary>
        /// Gets the target group for the selected platform.
        /// </summary>
        /// <param name="platform"></param>
        /// <returns>Return a build target group for the assigned platform</returns>
        public static BuildTargetGroup GetBuildTargetGroup(BuildTarget platform)
        {
            bool standalone = platform == BuildTarget.StandaloneWindows || platform == BuildTarget.StandaloneWindows64 || platform == BuildTarget.StandaloneOSX || platform == BuildTarget.StandaloneLinux64;
            BuildTargetGroup group = new BuildTargetGroup();

            if (standalone)
            {
                group = BuildTargetGroup.Standalone;
            }
            else
            {
                switch (platform)
                {
                    case BuildTarget.Android:

                        group = BuildTargetGroup.Android;

                        break;

                    case BuildTarget.CloudRendering:

                        group = BuildTargetGroup.CloudRendering;

                        break;

                    case BuildTarget.iOS:

                        group = BuildTargetGroup.iOS;

                        break;

                    case BuildTarget.EmbeddedLinux:

                        group = BuildTargetGroup.EmbeddedLinux;

                        break;

                    case BuildTarget.GameCoreXboxOne:

                        group = BuildTargetGroup.GameCoreXboxOne;

                        break;

                    case BuildTarget.GameCoreXboxSeries:

                        group = BuildTargetGroup.GameCoreXboxSeries;

                        break;

                    case BuildTarget.Lumin:

                        group = BuildTargetGroup.Lumin;

                        break;

                    case BuildTarget.PS4:

                        group = BuildTargetGroup.PS4;

                        break;

                    case BuildTarget.PS5:

                        group = BuildTargetGroup.PS5;

                        break;

                    case BuildTarget.Stadia:

                        group = BuildTargetGroup.Stadia;

                        break;

                    case BuildTarget.Switch:

                        group = BuildTargetGroup.Switch;

                        break;

                    case BuildTarget.tvOS:

                        group = BuildTargetGroup.tvOS;

                        break;

                    case BuildTarget.WebGL:

                        group = BuildTargetGroup.WebGL;

                        break;

                    case BuildTarget.WSAPlayer:

                        group = BuildTargetGroup.WSA;

                        break;

                    case BuildTarget.XboxOne:

                        group = BuildTargetGroup.XboxOne;

                        break;
                        ;

                }
            }

            return group;
        }

        #endregion
    }
}
