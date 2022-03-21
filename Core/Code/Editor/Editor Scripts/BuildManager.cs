using Bridge.Core.App.Events;
using Bridge.Core.App.Manager;
using Bridge.Core.UnityCustomEditor.Debugger;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditor.Build;
using static UnityEditor.PlayerSettings;
using Bridge.Core.App.Data.Storage;
using System.Collections.Generic;

namespace Bridge.Core.UnityCustomEditor.App.Manager
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
                buildSettings.configurations.targetBuildDirectory = EditorUtility.SaveFolderPanel("Choose Build Folder", "", "");
                ApplyAppSettings(buildSettings);
                DebugConsole.Log(Debug.LogLevel.Debug, $"Build Directory Set @ : {buildSettings.configurations.targetBuildDirectory}");
            }

            #region Saving Editor Data

            SaveProjectScene();

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
            BuildSettingsData buildSettingsData = new BuildSettingsData();

            Storage.Directory.DataPathExists(GetDefaultStorageInfo(), (resultsData, results) =>
            {
                if (results.error == true)
                {
                    CreateNewBuildSettings((newBuildSettingsData, newBuildSettingsResults) => 
                    {
                        if(newBuildSettingsResults.error == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, newBuildSettingsResults.errorValue);
                        }

                        if (newBuildSettingsResults.success == true)
                        {
                            buildSettingsData = newBuildSettingsData;
                            DebugConsole.Log(Debug.LogLevel.Success, newBuildSettingsResults.successValue);
                        }

                    });
                }

                if (results.success == true)
                {
                    buildSettingsData = GetBuildSettings(GetDefaultStorageInfo());
                    DebugConsole.Log(Debug.LogLevel.Success, $"Build data loaded successfully @ : {GetDefaultStorageInfo().filePath}.");
                }

            });

            return buildSettingsData.ToInstance();
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

        /// <summary>
        /// This function creates a new Build Settings data using current project settings.
        /// </summary>
        /// <param name="callBack"></param>
        private static void CreateNewBuildSettings(Action<BuildSettingsData, AppEventsData.CallBackResults> callBack)
        {
            try
            {
                BuildSettings newBuildSettings = BuildSettings.CreateInstance<BuildSettings>();

                #region App Info

                newBuildSettings.appInfo = GetDefaultAppInfo();

                #endregion

                #region App Icon

                BuildTarget platform = EditorUserBuildSettings.activeBuildTarget;
                var platformGroup = GetBuildTargetGroup(platform);
                var nameBuild = NamedBuildTarget.FromBuildTargetGroup(platformGroup);          

                if (AppIconsSupported(GetBuildTargetGroup(platform)))
                {
                    #region Standalone

                    if(platformGroup == BuildTargetGroup.Standalone)
                    {
                        var platformIcons = GetIcons(nameBuild, IconKind.Any);

                        if(platformIcons.Length > 0)
                        {
                            for (int i = 0; i < platformIcons.Length; i++)
                            {
                                if(platformIcons[i] != null)
                                {
                                    newBuildSettings.standaloneAppIcon.defaultIcon = platformIcons[i];
                                    break;
                                }
                            }
                        }
                        else
                        {
                            DebugConsole.Log(Debug.LogLevel.Warning, $"Platform icons for target group : {platform}. missing/not found.");
                        }
                    }

                    #endregion

                    #region Mobile

                    #region Android

                    if(platformGroup == BuildTargetGroup.Android)
                    {
                        var platformKind = GetSupportedIconKinds(nameBuild);

                        DebugConsole.Log(Debug.LogLevel.Debug, $"Ikon Kinds Count : {platformKind.Length}");

                        if(platformKind.Length > 0)
                        {
                            List<PlatformIcon[]> platformIcons = new List<PlatformIcon[]>();

                            for (int i = 0; i < platformKind.Length; i++)
                            {
                                var icons = GetPlatformIcons(platformGroup, platformKind[i]);
                                DebugConsole.Log(Debug.LogLevel.Debug, $"{icons.Length} : Icons found.");
                                platformIcons.Add(icons);
                            }
                        }
                    }

                    #endregion

                    #endregion
                }

                #endregion

                #region Configurations Info

                newBuildSettings.configurations = new BuildConfig
                {
                    platform = platform
                };

                #endregion

                #region Build Scene Info

                string[] buildScenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes);

                if (buildScenes.Length > 0)
                {
                    Storage.AssetData.LoadAssets<SceneAsset>(buildScenes, (loadedScenesResults, callbackResults) =>
                    {
                        if (callbackResults.error == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, callbackResults.errorValue);
                        }

                        if (callbackResults.success == true)
                        {
                            newBuildSettings.buildScenes = loadedScenesResults;
                            DebugConsole.Log(Debug.LogLevel.Error, callbackResults.successValue);
                        }
                    });
                }

                #endregion

                #region Platform Specific Settings

                #region Android

                IsPlatformSupported(BuildTarget.Android, callbackResults => 
                {
                    if(callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, callbackResults.errorValue);
                    }

                    if (callbackResults.success == true)
                    {
                        newBuildSettings.androidBuildSettings = GetDefaultAndroidBuildSettings();
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });

                #endregion

                #region iOS

                IsPlatformSupported(BuildTarget.iOS, callbackResults =>
                {
                    if (callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, callbackResults.errorValue);
                    }

                    if (callbackResults.success == true)
                    {
                        newBuildSettings.iOSBuildSettings = GetDefaultIOSBuildSettings();
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });

                #endregion

                #region Standalone

                newBuildSettings.standaloneDisplaySettings = GetDefaultStandaloneDisplaySettings();

                IsPlatformSupported(BuildTarget.StandaloneWindows, callbackResults =>
                {
                    if (callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, callbackResults.errorValue);
                    }

                    if (callbackResults.success == true)
                    {
                        newBuildSettings.standaloneBuildSettings = GetDefaultStandaloneBuildSettings(BuildTarget.StandaloneWindows);
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });

                IsPlatformSupported(BuildTarget.StandaloneWindows64, callbackResults =>
                {
                    if (callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, callbackResults.errorValue);
                    }

                    if (callbackResults.success == true)
                    {
                        newBuildSettings.standaloneBuildSettings = GetDefaultStandaloneBuildSettings(BuildTarget.StandaloneWindows64);
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });

                IsPlatformSupported(BuildTarget.StandaloneLinux64, callbackResults =>
                {
                    if (callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, callbackResults.errorValue);
                    }

                    if (callbackResults.success == true)
                    {
                        newBuildSettings.standaloneBuildSettings = GetDefaultStandaloneBuildSettings(BuildTarget.StandaloneLinux64);
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });

                IsPlatformSupported(BuildTarget.StandaloneOSX, callbackResults =>
                {
                    if (callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, callbackResults.errorValue);
                    }

                    if (callbackResults.success == true)
                    {
                        newBuildSettings.standaloneBuildSettings = GetDefaultStandaloneBuildSettings(BuildTarget.StandaloneOSX);
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });

                #endregion

                #region Web

                IsPlatformSupported(BuildTarget.WebGL, callbackResults =>
                {
                    if (callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, callbackResults.errorValue);
                    }

                    if (callbackResults.success == true)
                    {
                        newBuildSettings.webDisplaySettings = GetDefaultWebDisplaySettings();
                        newBuildSettings.webGLBuildSettings = GetDefaultWebGLBuildSettings();
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });

                #endregion

                #endregion

                #region Data Serialization

                AppEventsData.CallBackResults callBackResults = new AppEventsData.CallBackResults();

                Storage.JsonData.Save(GetDefaultStorageInfo(), newBuildSettings, (infoData, saveResults) =>
                {
                    if (saveResults.error)
                    {
                        callBackResults.error = true;
                        callBackResults.errorValue = $"Failed To Create New Build Settings With Results : {saveResults.errorValue}";
                    }

                    if (saveResults.success)
                    {
                        callBackResults.success = true;
                        callBackResults.successValue = $"New Build Settings Created And Saved Successfully With Results : {saveResults.successValue}";
                    }
                });

                #endregion

                ApplyAppSettings(newBuildSettings.ToSerializable());

                callBack.Invoke(newBuildSettings.ToSerializable(), callBackResults);
            }
            catch(Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed To Create New Build Settings With Exception : {exception}");
                throw exception;
            }
        }

        #region Default Settings

        #region App Info

        /// <summary>
        /// This function is used for retrieving the defaut project's app info.
        /// </summary>
        /// <returns>App Information</returns>
        public static AppInfo GetDefaultAppInfo()
        {
            #region Info

            AppInfo appInfo = new AppInfo
            {
                displayName = productName,
                companyName = companyName,
                version = bundleVersion
            };

            #endregion

            #region Splash Screen

            //if (PlayerSettings.SplashScreen.logos.Length > 0)
            //{
            //    appInfo.splashScreens.screens = GetSplashScreenLogoData(PlayerSettings.SplashScreen.logos);
            //}

            //if (PlayerSettings.SplashScreen.background != null)
            //{
            //    appInfo.splashScreens.background = PlayerSettings.SplashScreen.background;
            //    appInfo.splashScreens.backgroundColor = PlayerSettings.SplashScreen.backgroundColor;
            //}
            //else
            //{
            //    appInfo.splashScreens.backgroundColor = Color.black;
            //}

            //appInfo.splashScreens.unityLogoStyle = PlayerSettings.SplashScreen.unityLogoStyle;
            //appInfo.splashScreens.animationMode = PlayerSettings.SplashScreen.animationMode;
            //appInfo.splashScreens.logoDrawMode = PlayerSettings.SplashScreen.drawMode;
            //appInfo.splashScreens.showSplashScreen = PlayerSettings.SplashScreen.show;
            //appInfo.splashScreens.showUnityLogo = PlayerSettings.SplashScreen.showUnityLogo;

            #endregion

            return appInfo;
        }

        #endregion

        #region Configurations

        public static BuildConfig GetDefaultBuildConfigurations()
        {
            BuildConfig buildConfig = new BuildConfig();

            buildConfig.platform = EditorUserBuildSettings.activeBuildTarget;
            buildConfig.allowDebugging = EditorUserBuildSettings.allowDebugging;
            buildConfig.developmentBuild = EditorUserBuildSettings.development;

            return buildConfig;
        }

        #endregion

        #region Mobile Build Settings

        /// <summary>
        /// This function retrieves the default build settings for android.
        /// </summary>
        /// <returns></returns>
        public static AndroidBuildSettings GetDefaultAndroidBuildSettings()
        {
            AndroidBuildSettings settings = new AndroidBuildSettings
            {
                packageName = GetDefaultAppIdentifier(GetDefaultAppInfo()) ?? applicationIdentifier,
                sdkVersion = Android.targetSdkVersion,
                installLocation = Android.preferredInstallLocation
            };

            return settings;
        }

        /// <summary>
        /// This function retrieves the default build settings for iOS.
        /// </summary>
        /// <returns></returns>
        public static iOSBuildSettings GetDefaultIOSBuildSettings()
        {
            iOSBuildSettings settings = new iOSBuildSettings
            {
                bundleIdentifier = GetDefaultAppIdentifier(GetDefaultAppInfo()) ?? applicationIdentifier
            };

            return settings;
        }

        #endregion

        #region Display Settings

        private static StandaloneDisplaySettings GetDefaultStandaloneDisplaySettings()
        {
            StandaloneDisplaySettings displaySettings = new StandaloneDisplaySettings
            {
                
            };

            return displaySettings;
        }

        private static WebDisplaySettings GetDefaultWebDisplaySettings()
        {
            WebDisplaySettings displaySettings = new WebDisplaySettings
            {
                resolution = new AppResolution
                {
                    width = 1920,
                    height = 1080
                }
            };

            return displaySettings;
        }

        #endregion

        #region Standalone Build Settings

        /// <summary>
        /// This function retrieves the default build settings for standalone.
        /// </summary>
        /// <returns></returns>
        public static StandaloneBuildSettings GetDefaultStandaloneBuildSettings(BuildTarget buildTarget)
        {
            StandaloneBuildSettings settings = new StandaloneBuildSettings();

            switch(buildTarget)
            {
                case BuildTarget.StandaloneWindows:

                    settings.windows.colorSpace = colorSpace;
                    settings.windows.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneWindows);

                    break;

                case BuildTarget.StandaloneWindows64:

                    settings.windows.colorSpace = colorSpace;
                    settings.windows.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneWindows64);

                    break;

                case BuildTarget.StandaloneLinux64:

                    settings.linux.colorSpace = colorSpace;
                    settings.linux.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneLinux64);

                    break;

                case BuildTarget.StandaloneOSX:

                    settings.mac.colorSpace = colorSpace;
                    settings.mac.graphicsApi = GetGraphicsAPIs(BuildTarget.StandaloneOSX);
                    settings.mac.bundleIdentifier = GetDefaultAppIdentifier(GetDefaultAppInfo())?? applicationIdentifier;

                    break;
            }

            #region Other Settings

            settings.otherSettings.apiCompatibility = GetApiCompatibilityLevel(GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            settings.otherSettings.scriptingBackend = GetScriptingBackend(GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            #endregion

            return settings;
        }

        #endregion

        #region Web Build Settings

        private static WebGLBuildSettings GetDefaultWebGLBuildSettings()
        {
            WebGLBuildSettings settings = new WebGLBuildSettings
            {
               
            };

            return settings;
        }

        #endregion

        #region Other Settings

        /// <summary>
        /// This function is used to get a valid app identifier from the current app info.
        /// </summary>
        /// <param name="info"></param>
        /// <returns>App identifier</returns>
        private static string GetDefaultAppIdentifier(AppInfo info)
        {
            string appID = string.Empty;

            if(string.IsNullOrEmpty(info.companyName) == false && string.IsNullOrEmpty(info.displayName) == false)
            {
                appID = $"com.{info.companyName.Replace(" ", string.Empty).ToLower()}.{info.displayName.Replace(" ", string.Empty).ToLower()}";
            }
            else
            {
                DebugConsole.Log(Debug.LogLevel.Warning, "App Identifier Missing");
            }

            if(string.IsNullOrEmpty(appID) == true)
            {
                appID = $"com.{Application.companyName.Replace(" ", string.Empty).ToLower()}.{Application.productName.Replace(" ", string.Empty).ToLower()}";
            }

            return appID;
        }

        #endregion

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
            BuildSettingsData buildSettingsData = new BuildSettingsData();

            Storage.JsonData.Load<BuildSettingsData>(directoryInfo, (loadedResults, loadStatus) =>
            {
                if (loadStatus.error)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, $"Load Failed With Error: {loadStatus.errorValue}");
                    return;
                }

                if (loadStatus.success)
                {
                    buildSettingsData = loadedResults;

                    DebugConsole.Log(Debug.LogLevel.Success, $"Load Completed With Results : {loadStatus.successValue}");
                }
            });

            return buildSettingsData;
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

        #endregion

        #region Applied Settings

        public static bool BuildSettingsChanged()
        {

            return true;
        }

        /// <summary>
        /// This method create build settings for the selected platform.
        /// </summary>
        /// <param name="buildSettingsData"></param>
        public static void ApplyAppSettings(BuildSettingsData buildSettingsData)
        {
            ApplyAppInfo(buildSettingsData, (callbackResults, resultsData) =>
            {
                if (callbackResults.error)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, $"Failed to apply app info with results : {callbackResults.errorValue}.");
                    return;
                }

                if (callbackResults.success)
                {
                    buildSettingsData.appInfo = resultsData;

                    ApplyBuildSettings(buildSettingsData, (callbackResults, resultsData) =>
                    {
                        if (callbackResults.error)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, $"Failed to apply build settings with results : {callbackResults.errorValue}.");
                            return;
                        }

                        if (callbackResults.success)
                        {
                            Storage.JsonData.Save(GetDefaultStorageInfo(), buildSettingsData, (infoData, saveResults) =>
                            {
                                if (saveResults.error)
                                {
                                    DebugConsole.Log(Debug.LogLevel.Error, saveResults.errorValue);
                                    return;
                                }

                                if (saveResults.success)
                                {
                                    SaveProjectScene();
                                    DebugConsole.Log(Debug.LogLevel.Success, saveResults.successValue);
                                }
                            });
                        }
                    });
                }
            });
        }

        /// <summary>
        /// Applies the app info for the current project.
        /// </summary>
        /// <param name="buildSettingsData"></param>
        /// <param name="callback"></param>
        private static void ApplyAppInfo(BuildSettingsData buildSettingsData, Action<AppEventsData.CallBackResults, AppInfoDataObject> callback = null)
        {
            AppEventsData.CallBackResults callBackResults = new AppEventsData.CallBackResults();

            try
            {
                #region App Info

                if (string.IsNullOrEmpty(buildSettingsData.appInfo.displayName))
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, "App info's app name field is empty. Please assign app name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                    return;
                }

                if (string.IsNullOrEmpty(buildSettingsData.appInfo.companyName))
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, "App info's company name field is empty. Please assign company name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                    return;
                }

                companyName = (string.IsNullOrEmpty(buildSettingsData.appInfo.companyName)) ? companyName : buildSettingsData.appInfo.companyName;
                productName = (string.IsNullOrEmpty(buildSettingsData.appInfo.displayName)) ? productName : buildSettingsData.appInfo.displayName;
                bundleVersion = (string.IsNullOrEmpty(buildSettingsData.appInfo.version)) ? bundleVersion : buildSettingsData.appInfo.version;

                string appCompanyName = buildSettingsData.appInfo.companyName;

                if (appCompanyName.Contains(" "))
                {
                    appCompanyName = appCompanyName.Replace(" ", "");
                }

                string appName = buildSettingsData.appInfo.displayName;

                if (appName.Contains(" "))
                {
                    appName = appName.Replace(" ", "");
                }

                #endregion

                #region App Icons

                // BuildSettings buildSettings = buildSettingsData.ToInstance();
                BuildSettings buildSettings = BuildSettings.CreateInstance<BuildSettings>();

                buildSettings.configurations = buildSettingsData.configurations;

                buildSettings.standaloneAppIcon = buildSettingsData.standaloneAppIconData.ToInstance();

                buildSettings.androidAdaptiveAppIcon = buildSettingsData.androidAdaptiveAppIconData.ToInstance();
                buildSettings.androidRoundAppIcon = buildSettingsData.androidRoundAppIconData.ToInstance();
                buildSettings.androidLegacyAppIcon = buildSettingsData.androidLegacyAppIconData.ToInstance();
                buildSettings.androidIconKind = buildSettingsData.androidIconKind;

                if (AppIconsSupported(GetBuildTargetGroup(buildSettings.configurations.platform)))
                {
                    var targetGroup = GetBuildTargetGroup(buildSettingsData.configurations.platform);

                    #region Standalone Icons

                    if (targetGroup == BuildTargetGroup.Standalone && buildSettings.standaloneAppIcon.defaultIcon != null)
                    {
                        var buildTarget = GetBuildTargetGroup(buildSettingsData.configurations.platform);
                        var nameBuild = NamedBuildTarget.FromBuildTargetGroup(buildTarget);
                        var icons = GetIcons(nameBuild, IconKind.Any);

                        for (int i = 0; i < icons.Length; i++)
                        {
                            icons[i] = buildSettings.standaloneAppIcon.defaultIcon;
                        }

                        SetIcons(nameBuild, icons, IconKind.Any);
                    }

                    #endregion

                    #region Mobile Icons

                    #region Android

                    if (targetGroup == BuildTargetGroup.Android)
                    {

                        switch(buildSettings.androidIconKind.appIconKind)
                        {
                            case AndroidIconKind.Adaptive:

                                bool hasAllIconsAssigned = true;

                                if(buildSettings.androidAdaptiveAppIcon.foreground != null && buildSettings.androidAdaptiveAppIcon.background == null)
                                {
                                    hasAllIconsAssigned = false;
                                    DebugConsole.Log(Debug.LogLevel.Warning, $"Android Adaptive App Icon.background is not assigned");
                                }


                                if (buildSettings.androidAdaptiveAppIcon.foreground == null && buildSettings.androidAdaptiveAppIcon.background != null)
                                {
                                    hasAllIconsAssigned = false;
                                    DebugConsole.Log(Debug.LogLevel.Warning, $"Android Adaptive App Icon.foreground is not assigned");
                                }

                                if (hasAllIconsAssigned)
                                {
                                    PlatformIconKind platformIconKind = AppPlatformIconData.GetAndroindPlatformIconKind(AndroidIconKind.Adaptive);
                                    var platformIcons = GetPlatformIcons(GetBuildTargetGroup(buildSettingsData.configurations.platform), platformIconKind);
                                    var icons = GetPlatformIcons(GetBuildTargetGroup(buildSettingsData.configurations.platform), platformIconKind);
                                    var getIcons = new Texture2D[] { buildSettings.androidAdaptiveAppIcon.background, buildSettings.androidAdaptiveAppIcon.foreground};
                                   
                                    Texture2D[][] textures = new Texture2D[icons.Length][];

                                    for (int i = 0; i < textures.Length; i++)
                                    {
                                        textures[i] = getIcons;
                                    }

                                    if (icons.Length > 0)
                                    {
                                        for (int i = 0; i < textures.Length; i++)
                                        {
                                            icons[i].SetTextures(textures[i]);
                                        }

                                        SetPlatformIcons(targetGroup, platformIconKind, icons);
                                    }
                                    else
                                    {
                                        DebugConsole.Log(Debug.LogLevel.Error, $"Failed to get platform icons for [{GetBuildTargetGroup(buildSettingsData.configurations.platform).ToString()}]");
                                    }
                                }
                                else
                                {
                                    DebugConsole.Log(Debug.LogLevel.Error, $"Failed to set platform icons for [{GetBuildTargetGroup(buildSettingsData.configurations.platform).ToString()}]. There's no App Icon asigned.");
                                    return;
                                }

                                break;

                            case AndroidIconKind.Round:

                                if (buildSettings.androidRoundAppIcon.defaultIcon != null)
                                {
                                    PlatformIconKind platformIconKind = AppPlatformIconData.GetAndroindPlatformIconKind(AndroidIconKind.Adaptive);
                                    var platformIcons = GetPlatformIcons(GetBuildTargetGroup(buildSettingsData.configurations.platform), platformIconKind);
                                    var icons = GetPlatformIcons(GetBuildTargetGroup(buildSettingsData.configurations.platform), platformIconKind);
                                    var getIcons = GetAppIcons(buildSettings.androidRoundAppIcon.defaultIcon, 2);
                                    Texture2D[][] textures = new Texture2D[icons.Length][];

                                    for (int i = 0; i < textures.Length; i++)
                                    {
                                        textures[i] = getIcons;
                                    }

                                    if (icons.Length > 0)
                                    {
                                        for (int i = 0; i < textures.Length; i++)
                                        {
                                            icons[i].SetTextures(textures[i]);
                                        }

                                        SetPlatformIcons(targetGroup, platformIconKind, icons);
                                    }
                                    else
                                    {
                                        DebugConsole.Log(Debug.LogLevel.Error, $"Failed to get platform icons for [{GetBuildTargetGroup(buildSettingsData.configurations.platform).ToString()}]");
                                    }
                                }
                                else
                                {
                                    DebugConsole.Log(Debug.LogLevel.Error, $"Failed to set platform icons for [{GetBuildTargetGroup(buildSettingsData.configurations.platform).ToString()}]. There's no App Icon asigned.");
                                }

                                break;

                            case AndroidIconKind.Legacy:


                                if (buildSettings.androidLegacyAppIcon.defaultIcon != null)
                                {
                                    PlatformIconKind platformIconKind = AppPlatformIconData.GetAndroindPlatformIconKind(AndroidIconKind.Adaptive);
                                    var platformIcons = GetPlatformIcons(GetBuildTargetGroup(buildSettingsData.configurations.platform), platformIconKind);
                                    var icons = GetPlatformIcons(GetBuildTargetGroup(buildSettingsData.configurations.platform), platformIconKind);
                                    var getIcons = GetAppIcons(buildSettings.androidLegacyAppIcon.defaultIcon, 2);
                                    Texture2D[][] textures = new Texture2D[icons.Length][];

                                    for (int i = 0; i < textures.Length; i++)
                                    {
                                        textures[i] = getIcons;
                                    }

                                    if (icons.Length > 0)
                                    {
                                        for (int i = 0; i < textures.Length; i++)
                                        {
                                            icons[i].SetTextures(textures[i]);
                                        }

                                        SetPlatformIcons(targetGroup, platformIconKind, icons);
                                    }
                                    else
                                    {
                                        DebugConsole.Log(Debug.LogLevel.Error, $"Failed to get platform icons for [{GetBuildTargetGroup(buildSettingsData.configurations.platform).ToString()}]");
                                    }
                                }
                                else
                                {
                                    DebugConsole.Log(Debug.LogLevel.Error, $"Failed to set platform icons for [{GetBuildTargetGroup(buildSettingsData.configurations.platform).ToString()}]. There's no App Icon asigned.");
                                }

                                break;
                        }
                    }

                    #endregion

                    #endregion
                }

                #endregion

                #region Splash Screens



                //PlayerSettings.SplashScreen.logos = GetSplashScreenLogos(buildSettingsData.appInfo.splashScreens.screens);
                //PlayerSettings.SplashScreen.background = buildSettingsData.appInfo.splashScreens.background;
                //PlayerSettings.SplashScreen.backgroundColor = buildSettingsData.appInfo.splashScreens.backgroundColor;


                //PlayerSettings.SplashScreen.unityLogoStyle = buildSettingsData.appInfo.splashScreens.unityLogoStyle;
                //PlayerSettings.SplashScreen.animationMode = buildSettingsData.appInfo.splashScreens.animationMode;
                //PlayerSettings.SplashScreen.drawMode = buildSettingsData.appInfo.splashScreens.logoDrawMode;
                //PlayerSettings.SplashScreen.showUnityLogo = buildSettingsData.appInfo.splashScreens.showUnityLogo;
                //PlayerSettings.SplashScreen.show = buildSettingsData.appInfo.splashScreens.showSplashScreen;

                //if (buildSettings.appInfo.appIcons.Length > 0)
                //{
                //    DebugConsole.Log(Debug.LogLevel.Debug, $"Loaded {buildSettings.appInfo.appIcons.Length} Icons Data");

                //    for (int i = 0; i < buildSettings.appInfo.appIcons.Length; i++)
                //    {
                //        if (buildSettings.appInfo.appIcons[i].icons.Length > 0)
                //        {
                //            for (int j = 0; j < buildSettings.appInfo.appIcons[i].icons.Length; j++)
                //            {
                //                var nameBuild = NamedBuildTarget.FromBuildTargetGroup(GetBuildTargetGroup(buildSettings.configurations.platform));
                //                SetIcons(nameBuild, GetAppIcons(buildSettings.appInfo.appIcons[i]), buildSettings.appInfo.appIcons[i].type);
                //            }
                //        }
                //    }
                //}

                #endregion

                #region Results

                callBackResults.success = true;
                callback.Invoke(callBackResults, buildSettingsData.appInfo);

                #endregion
            }
            catch (Exception exception)
            {
                callBackResults.error = true;
                callBackResults.errorValue = exception.Message;
                callback.Invoke(callBackResults, buildSettingsData.appInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildSettingsData"></param>
        /// <param name="callback"></param>
        public static void ApplyBuildSettings(BuildSettingsData buildSettingsData, Action<AppEventsData.CallBackResults, BuildSettingsData> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                SwitchBuildTarget(buildSettingsData.configurations, switchResults =>
                {
                    if (switchResults.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, switchResults.errorValue);
                        return;
                    }

                    if (switchResults.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, $"Applied build settings for : {buildSettingsData.configurations.platform}");

                        if (buildSettingsData.configurations.platform == BuildTarget.Android || buildSettingsData.configurations.platform == BuildTarget.iOS)
                        {
                            switch (buildSettingsData.mobileDisplaySettings.allowedOrientation)
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

                                SetApplicationIdentifier(BuildTargetGroup.Android, buildSettingsData.androidBuildSettings.packageName);

                                GraphicsDeviceType[] graphicsDeviceType = new GraphicsDeviceType[1];
                                graphicsDeviceType[0] = GraphicsDeviceType.OpenGLES3;
                                SetGraphicsAPIs(buildSettingsData.configurations.platform, graphicsDeviceType);

                                SetMobileMTRendering(BuildTargetGroup.Android, false);

                                Android.minSdkVersion = buildSettingsData.androidBuildSettings.sdkVersion;
                                Android.androidTVCompatibility = false;
                                Android.preferredInstallLocation = buildSettingsData.androidBuildSettings.installLocation;
                                Android.ARCoreEnabled = true;

                                EditorUserBuildSettings.buildAppBundle = buildSettingsData.androidBuildSettings.buildAppBundle;

                                break;

                            case BuildTarget.iOS:

                                SetApplicationIdentifier(BuildTargetGroup.iOS, buildSettingsData.iOSBuildSettings.bundleIdentifier);

                                break;

                            case BuildTarget.StandaloneWindows:

                                SetScriptingBackend(GetBuildTargetGroup(buildSettingsData.configurations.platform), buildSettingsData.standaloneBuildSettings.otherSettings.scriptingBackend);

                                break;

                            case BuildTarget.StandaloneOSX:

                                SetApplicationIdentifier(BuildTargetGroup.Standalone, buildSettingsData.standaloneBuildSettings.mac.bundleIdentifier);

                                break;
                        }

                        EditorUserBuildSettings.allowDebugging = buildSettingsData.configurations.allowDebugging;
                        EditorUserBuildSettings.development = buildSettingsData.configurations.developmentBuild;
                        EditorUserBuildSettings.SetBuildLocation(buildSettingsData.configurations.platform, buildSettingsData.configurations.buildLocation);

                        results.success = true;
                        callback.Invoke(results, buildSettingsData);
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

        #endregion

        #region App Icons Data

        /// <summary>
        /// This function checks if the current build target supports app icons.
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        public static bool AppIconsSupported(BuildTarget buildTarget)
        {
            bool isSupported = buildTarget == BuildTarget.StandaloneWindows ||
                               buildTarget == BuildTarget.StandaloneWindows64 ||
                               buildTarget == BuildTarget.StandaloneOSX ||
                               buildTarget == BuildTarget.StandaloneLinux64 ||
                               buildTarget == BuildTarget.Android ||
                               buildTarget == BuildTarget.iOS ||
                               buildTarget == BuildTarget.tvOS;

            return isSupported;
        }

        /// <summary>
        /// This function checks if the current build target supports app icons.
        /// </summary>
        /// <param name="buildTargetGroup"></param>
        /// <returns></returns>
        public static bool AppIconsSupported(BuildTargetGroup buildTargetGroup)
        {
            bool isSupported = buildTargetGroup == BuildTargetGroup.Standalone ||
                               buildTargetGroup == BuildTargetGroup.Android ||
                               buildTargetGroup == BuildTargetGroup.iOS ||
                               buildTargetGroup == BuildTargetGroup.tvOS;

            return isSupported;
        }

        /// <summary>
        /// This function creates app icon format.
        /// </summary>
        /// <param name="appIconInfo"></param>
        /// <param name="iconsResolutions"></param>
        /// <returns></returns>
        public static Texture2D[] GetAppIcons(Texture2D appIcon, int iconCount)
        {
            Texture2D[] appIcons = new Texture2D[iconCount];

            if (iconCount > 0)
            {
                for (int i = 0; i < iconCount; i++)
                {
                    appIcons[i] = appIcon;
                }
            }

            return appIcons;
        }

        #endregion

        #region Platform Data

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

        #region Editor Settings

        public static void SaveProjectScene()
        {
            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(currentScene);
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

        #endregion
    }
}
