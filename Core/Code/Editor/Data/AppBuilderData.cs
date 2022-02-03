using System;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using UnityEngine.Rendering;
using Bridge.Core.App.Data.Storage;
using Bridge.Core.UnityEditor.Debugger;
using System.Linq;

namespace Bridge.Core.App.Manager
{
    #region Data

    #region Options

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

    #endregion

    #region Info Data

    /// <summary>
    /// Information about the app.
    /// </summary>
    [Serializable]
    public struct AppInfo
    {
        #region Components

        [Space(5)]
        public string companyName;

        [Space(5)]
        public string appName;

        [Space(5)]
        public string appVersion;

        [Space(5)]
        public AppIconInfo appIconInfo;

        [Space(5)]
        public SplashScreen splashScreens;

        #endregion

        #region Data Conversion

        /// <summary>
        /// This function converts the App Info to a serializable data struct.
        /// </summary>
        /// <returns>Serializable version of the App Info Struct.</returns>
        public AppInfoDataObject ToSerializable()
        {
            AppInfoDataObject appInfoData = new AppInfoDataObject
            {
                appName = this.appName,
                companyName = this.companyName,
                appVersion = this.appVersion,
                appIconInfoData = appIconInfo.ToSerializable(),
                splashScreens = this.splashScreens.ToSerializable()
            };

            return appInfoData;
        }

        #endregion
    }

    /// <summary>
    /// Serializable information about the app.
    /// </summary>
    [Serializable]
    public struct AppInfoDataObject
    {
        #region Components

        [Space(5)]
        public string companyName;

        [Space(5)]
        public string appName;

        [Space(5)]
        public string appVersion;

        [Space(5)]
        public AppIconInfoData appIconInfoData;

        [Space(5)]
        public SplashScreenData splashScreens;

        #endregion

        #region Data Conversion

        public AppInfo ToInstance()
        {
            AppInfo appInfo = new AppInfo
            {
                appName = this.appName,
                companyName = this.companyName,
                appVersion = this.appVersion,
                appIconInfo = appIconInfoData.ToInstance(),
                splashScreens = this.splashScreens.ToInstance()
            };

            appInfo.appIconInfo = new AppIconInfo
            {
                iconList = appIconInfoData.ToInstance().iconList
            };

            return appInfo;
        }

        #endregion
    }

    #endregion

    #region Build Settings

    /// <summary>
    /// This class contains the app build settings.
    /// </summary>
    public class BuildSettings : ScriptableObject
    {
        [Space(5)]
        public AppInfo appInfo;

        [Space(5)]
        [NonReorderable]
        public SceneAsset[] buildScenes;

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

        #region Platform Build Settings

        [Space(5)]
        public AndroidBuildSettings androidBuildSettings;

        [Space(5)]
        public iOSBuildSettings iOSBuildSettings;

        [Space(5)]
        public StandaloneBuildSettings standaloneBuildSettings;

        [Space(5)]
        public WebGLBuildSettings webGLBuildSettings;

        #endregion

        [Space(5)]
        public bool buildAndRun;

        #region Converted Settings

       public BuildSettingsData ToSerializable()
        {
            return new BuildSettingsData
            {
                appInfo = this.appInfo.ToSerializable(),
                buildScenes = GetBuildScenes(),
                configurations = this.configurations,
                consoleDisplaySettings = this.consoleDisplaySettings,
                mobileDisplaySettings = this.mobileDisplaySettings,
                standaloneDisplaySettings = this.standaloneDisplaySettings,
                webDisplaySettings = this.webDisplaySettings,
                androidBuildSettings = this.androidBuildSettings,
                iOSBuildSettings = this.iOSBuildSettings,
                standaloneBuildSettings = this.standaloneBuildSettings,
                webGLBuildSettings = this.webGLBuildSettings,
                buildAndRun = buildAndRun
            };
        }

        public string[] GetBuildScenes()
        {
            try
            {
                if(buildScenes == null)
                {
                    return new string[0];
                }

                string[] buildScenesToStringArray = new string[buildScenes.Length];

                if (buildScenesToStringArray.Length > 0)
                {
                    Storage.Directory.GetAssetsPaths(buildScenes, (callBackData, callBackResults) =>
                    {
                        if (callBackResults.error == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, callBackResults.errorValue);
                        }

                        if (callBackResults.success == true)
                        {
                            buildScenesToStringArray = callBackData;
                            DebugConsole.Log(Debug.LogLevel.Success, callBackResults.successValue);
                        }

                    });
                }

                return buildScenesToStringArray;
            }
            catch(Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed With Exception : {exception.Message}");
                throw exception;
            }
        }

        #endregion
    }

    #region Remove this 

    public static class AppDataBuilder
    {
        public static void CreateNewBuildSettingsInstance(out BuildSettings settings)
        {
            BuildSettings buildSettings = ScriptableObject.CreateInstance<BuildSettings>();

            settings = buildSettings;
        }

        //public static BuildSettings CreateNewBuildSettingsInstance()
        //{
        //    BuildSettings buildSettings = ScriptableObject.CreateInstance<BuildSettings>();

        //    return buildSettings;
        //}

        //public static BuildSettings CreateNewBuildSettingsInstance(BuildSettingsData buildSettingsData)
        //{
        //    BuildSettings buildSettings = ScriptableObject.CreateInstance<BuildSettings>();

        //    buildSettings.appInfo = new AppInfo
        //    {
        //        appName = buildSettingsData.appInfo.appName,
        //        companyName = buildSettings.appInfo.companyName,
        //        appVersion = buildSettings.appInfo.appVersion,
        //        appIdentifier = buildSettings.appInfo.appIdentifier,
        //        appIcons = buildSettings.appInfo.appIcons
        //    };

        //    buildSettings.configurations = buildSettingsData.configurations;
        //    buildSettings.buildScenes = buildSettingsData.GetBuildScenes();
        //    buildSettings.buildAndRun = buildSettingsData.buildAndRun;

        //    buildSettings.androidBuildSettings = buildSettingsData.androidBuildSettings;
        //    buildSettings.standaloneBuildSettings = buildSettingsData.standaloneBuildSettings;


        //    return buildSettings;
        // }
    }

    #endregion

    /// <summary>
    /// This class contains the serializable app build settings.
    /// </summary>
    [Serializable]
    public class BuildSettingsData
    {
        [Space(5)]
        public AppInfoDataObject appInfo;

        [Space(5)]
        [NonReorderable]
        public string[] buildScenes;

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

        #region Platform Build Settings

        [Space(5)]
        public AndroidBuildSettings androidBuildSettings;

        [Space(5)]
        public iOSBuildSettings iOSBuildSettings;

        [Space(5)]
        public StandaloneBuildSettings standaloneBuildSettings;

        [Space(5)]
        public WebGLBuildSettings webGLBuildSettings;

        #endregion

        [Space(5)]
        public bool buildAndRun;

        public void CreateInstance(out BuildSettings settings)
        {
            BuildSettings buildSettings = ScriptableObject.CreateInstance<BuildSettings>();

            settings = buildSettings;
        }

        public BuildSettings ToInstance()
        {
            BuildSettings buildSettings = ScriptableObject.CreateInstance<BuildSettings>();

            buildSettings.appInfo = this.appInfo.ToInstance();

            if(buildScenes != null && buildScenes.Length > 0)
            {
                Storage.AssetData.LoadAssets<SceneAsset>(buildScenes, (loadedScenesResults, callbackResults) =>
                {
                    if (callbackResults.error == true)
                    {
                        DebugConsole.Log(Debug.LogLevel.Error, callbackResults.errorValue);
                        return;
                    }

                    if (callbackResults.success == true)
                    {
                        buildSettings.buildScenes = loadedScenesResults;
                        DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                    }
                });
            }

            buildSettings.configurations = this.configurations;
            buildSettings.consoleDisplaySettings = this.consoleDisplaySettings;
            buildSettings.mobileDisplaySettings = this.mobileDisplaySettings;
            buildSettings.standaloneDisplaySettings = this.standaloneDisplaySettings;
            buildSettings.webDisplaySettings = this.webDisplaySettings;
            buildSettings.androidBuildSettings = this.androidBuildSettings;
            buildSettings.iOSBuildSettings = this.iOSBuildSettings;
            buildSettings.standaloneBuildSettings = this.standaloneBuildSettings;
            buildSettings.webGLBuildSettings = this.webGLBuildSettings;
            buildSettings.buildAndRun = this.buildAndRun;

            return buildSettings;
        }

        public SceneAsset[] GetBuildScenes()
        {
            try
            {
                SceneAsset[] buildScenesToAssetArray = new SceneAsset[buildScenes.Length];

                if (buildScenesToAssetArray.Length > 0)
                {
                    Storage.AssetData.LoadAssets<SceneAsset>(buildScenes, (callBackData, callBackResults) =>
                    {
                        if (callBackResults.error == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, callBackResults.errorValue);
                        }

                        if (callBackResults.success == true)
                        {
                            buildScenesToAssetArray = callBackData;
                            DebugConsole.Log(Debug.LogLevel.Success, callBackResults.successValue);
                        }

                    });
                }

                return buildScenesToAssetArray;
            }
            catch (Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed With Exception : {exception.Message}");
                throw exception;
            }
        }
    }

    #endregion

    #region Icons Data

    #region Platform Options

    public enum AppIconKind
    {
        None,
        Adaptive,
        Legacy,
        Round
    }

    #endregion

    [Serializable]
    public struct AppIconInfo
    {
        #region Components

        [Space(5)]
        public bool overideStandalone;

        [Space(5)]
        [NonReorderable]
        public AppIcon[] iconList;

        #endregion

        #region Data Conversion

        public AppIconInfoData ToSerializable()
        {
            AppIconInfoData infoData = new AppIconInfoData
            {
                overideStandalone = overideStandalone,
                iconDataList = GetAppIconsDirectories()
            };

            return infoData;
        }

        public AppIconData[] GetAppIconsDirectories()
        {
            try
            {
                if (iconList != null && iconList.Length > 0)
                {
                    AppIconData[] iconData = new AppIconData[iconList.Length];

                    for (int i = 0; i < iconList.Length; i++)
                    {
                        iconData[i] = iconList[i].ToSerializable();
                    }

                    return iconData;
                }
                else
                {
                    return new AppIconData[0];
                }
            }
            catch(Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed To Load App Icons List With Exception : {exception.Message}");
                throw exception;
            }
        }

        #endregion
    }

    [Serializable]
    public struct AppIcon
    {
        #region Components

        public string name;

        [Space(5)]
        public Texture2D icon;

        [Tooltip("Android Support Only")]
        [Space(5)]
        public AppIconKind iconKind;

        [Space(5)]
        public IconKind type;

        #endregion

        #region Data Conversion

        public AppIconData ToSerializable()
        {
            try
            {
                AppIconData iconData = new AppIconData
                {
                    name = name,
                    iconAssetDirectory = GetAppIconAssetDirectory(),
                    iconKind = iconKind,
                    type = type

                };

                return iconData;
            }
            catch(Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed To Load App Icons With Exception : {exception.Message}");
                throw exception;
            }
        }

        public string GetAppIconAssetDirectory()
        {
            string appIconDirectory = string.Empty;

            Storage.Directory.GetAssetPath(icon, (resultsData, callbackResults) =>
            {
                if (callbackResults.error == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, callbackResults.errorValue);
                    return;
                }

                if (callbackResults.success == true)
                {
                    appIconDirectory = resultsData;
                    DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                }
            });

            return appIconDirectory;
        }

        #endregion
    }

    /// <summary>
    /// This struct contains a serializable version of the App Icon Info Object struct.
    /// </summary>
    [Serializable]
    public struct AppIconInfoData
    {
        #region Components

        public bool overideStandalone;
        public AppIconData[] iconDataList;

        #endregion

        #region Data Conversion

        public AppIconInfo ToInstance()
        {
            if(iconDataList == null)
            {
                return new AppIconInfo
                {
                    iconList = new AppIcon[0]
                };
            }

            AppIconInfo iconInfo = new AppIconInfo
            {
                overideStandalone = overideStandalone,
                iconList = GetAppIconsList()
            };

            return iconInfo;
        }

        public AppIcon[] GetAppIconsList()
        {
            try
            {
                if (iconDataList != null)
                {
                    AppIcon[] iconsDataList = new AppIcon[iconDataList.Length];

                    if (iconDataList != null)
                    {
                        for (int i = 0; i < iconsDataList.Length; i++)
                        {
                            iconsDataList[i] = iconDataList[i].ToInstance();
                        }
                    }

                    return iconsDataList.ToArray();
                }
                else
                {
                    DebugConsole.Log(Debug.LogLevel.Error, "Failed To Load App Icons List.");
                    return new AppIcon[0];
                }
            }
            catch(Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed To Load App Icons List With Exception : {exception.Message}");
                throw exception;
            }
        }

        #endregion
    }

    /// <summary>
    /// This struct contains a serializable version of App Icon.
    /// </summary>
    [Serializable]
    public struct AppIconData
    {
        #region Components

        public string name;
        public string iconAssetDirectory;
        public AppIconKind iconKind; // Android Support Only.
        public IconKind type;

        #endregion

        #region Data Conversion

        public AppIcon ToInstance()
        {
            try
            {
                AppIcon appIcons = new AppIcon
                { 
                    name = name,
                    icon = GetAppIconAsset(),
                    iconKind = iconKind,
                    type = type
                };

                return appIcons;
            }
            catch(Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed To Load App Icons Data With Exception : {exception.Message}");
                throw exception;
            }
        }

        public Texture2D GetAppIconAsset()
        {
            Texture2D appIconAsset = new Texture2D(1,1);

            Storage.AssetData.LoadAsset<Texture2D>(iconAssetDirectory, (resultsData, callbackResults) =>
            {
                if (callbackResults.error == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, callbackResults.errorValue);
                    return;
                }

                if (callbackResults.success == true)
                {
                    appIconAsset = resultsData;
                    DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                }
            });

            return appIconAsset;
        }

        #endregion
    }

    /// <summary>
    /// This struct contains data for plaform specific Icons.
    /// </summary>
    public struct PlatformAppIcons
    {
        #region Components

        public Texture2D[] icons;
        public IconKind type;

        #endregion

        #region Platform Icons

        public static PlatformIconKind GetPlatformIconKind(AppIconKind iconType)
        {
            PlatformIconKind iconKind = AndroidPlatformIconKind.Adaptive;

            switch(iconType)
            {
                case AppIconKind.Adaptive:

                    iconKind = AndroidPlatformIconKind.Adaptive;

                    break;

                case AppIconKind.Legacy:

                    iconKind = AndroidPlatformIconKind.Adaptive;

                    break;

                case AppIconKind.Round:

                    iconKind = AndroidPlatformIconKind.Adaptive;

                    break;
            }

            return iconKind;
        }

        #endregion
    }

    #endregion

    #region Splash Screen Data

    /// <summary>
    /// Contains splash screen data.
    /// </summary>
    [Serializable]
    public struct SplashScreen
    {
        #region Components

        [Space(5)]
        [NonReorderable]
        public SplashScreenLogo[] screens;

        [Space(5)]
        public Sprite background;

        [Space(5)]
        public Color backgroundColor;

        [Space(5)]
        public PlayerSettings.SplashScreen.UnityLogoStyle unityLogoStyle;

        [Space(5)]
        public PlayerSettings.SplashScreen.AnimationMode animationMode;

        [Space(5)]
        public PlayerSettings.SplashScreen.DrawMode logoDrawMode;

        [Space(5)]
        public bool showUnityLogo;

        [Space(5)]
        public bool showSplashScreen;

        #endregion

        #region Data Conversions

        public SplashScreenData ToSerializable()
        {
            SplashScreenData splashScreen = new SplashScreenData();

            if(this.screens.Length > 0)
            {
                splashScreen.screens = new SplashScreenLogoData[this.screens.Length];

                for (int i = 0; i < this.screens.Length; i++)
                {
                    splashScreen.screens[i] = this.screens[i].ToSerializable();
                }
            }

            Storage.Directory.GetAssetPath<Sprite>(background, (backgroundPath, results) =>
            { 
                if(results.error == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, results.errorValue);
                    return;
                }

                if (results.success == true)
                {
                    splashScreen.background = backgroundPath;
                    DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                }
            });

            splashScreen.backgroundColor = this.backgroundColor;
            splashScreen.unityLogoStyle = this.unityLogoStyle;
            splashScreen.animationMode = this.animationMode;
            splashScreen.logoDrawMode = this.logoDrawMode;
            splashScreen.showUnityLogo = this.showUnityLogo;
            splashScreen.showSplashScreen = this.showSplashScreen;

            return splashScreen;
        }

        #endregion
    }

    /// <summary>
    /// Contains serializable splash screen data.
    /// </summary>
    [Serializable]
    public struct SplashScreenData
    {
        #region Components

        [Space(5)]
        [NonReorderable]
        public SplashScreenLogoData[] screens;

        [Space(5)]
        public string background;

        [Space(5)]
        public Color backgroundColor;

        [Space(5)]
        public PlayerSettings.SplashScreen.UnityLogoStyle unityLogoStyle;

        [Space(5)]
        public PlayerSettings.SplashScreen.AnimationMode animationMode;

        [Space(5)]
        public PlayerSettings.SplashScreen.DrawMode logoDrawMode;

        [Space(5)]
        public bool showUnityLogo;

        [Space(5)]
        public bool showSplashScreen;

        #endregion

        #region Data Conversions

        public SplashScreen ToInstance()
        {
            if(screens == null)
            {
                return new SplashScreen();
            }

            SplashScreen splashScreen = new SplashScreen();

            if (screens.Length > 0)
            {
                splashScreen.screens = new SplashScreenLogo[this.screens.Length];

                for (int i = 0; i < this.screens.Length; i++)
                {
                    splashScreen.screens[i] = this.screens[i].ToInstance();
                }
            }

            Storage.AssetData.LoadAsset<Sprite>(background, (backgroundImage, results) => 
            {
                if(results.error == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, results.errorValue);
                    return;
                }

                if (results.success == true)
                {
                    splashScreen.background = backgroundImage;
                    DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                }
            });

            splashScreen.backgroundColor = this.backgroundColor;
            splashScreen.unityLogoStyle = this.unityLogoStyle;
            splashScreen.animationMode = this.animationMode;
            splashScreen.logoDrawMode = this.logoDrawMode;
            splashScreen.showUnityLogo = this.showUnityLogo;
            splashScreen.showSplashScreen = this.showSplashScreen;

            return splashScreen;
        }

        #endregion
    }

    [Serializable]
    public struct SplashScreenLogo
    {
        #region Components

        [Space(5)]
        public string name;

        [Space(5)]
        public Sprite logo;

        [Space(5)]
        public float duration;

        #endregion

        #region Data Conversions

        public SplashScreenLogoData ToSerializable()
        {
            SplashScreenLogoData splashScreenLogo = new SplashScreenLogoData();
            splashScreenLogo.name = this.name;
            Storage.Directory.GetAssetPath(logo, (logoPath, results) => 
            {
                if(results.error == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, results.errorValue);
                    return;
                }

                if (results.success == true)
                {
                    splashScreenLogo.logo = logoPath;
                    DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                }
            });       
            splashScreenLogo.duration = this.duration;

            return splashScreenLogo;
        }

        #endregion 
    }

    [Serializable]
    public struct SplashScreenLogoData
    {
        #region Components

        [Space(5)]
        public string name;

        [Space(5)]
        public string logo;

        [Space(5)]
        public float duration;

        #endregion

        #region Data Conversions

        public SplashScreenLogo ToInstance()
        {
            SplashScreenLogo splashScreenLogo = new SplashScreenLogo();
            splashScreenLogo.name = this.name;
            Storage.AssetData.LoadAsset<Sprite>(this.logo, (logo, results) => 
            {
                if(results.error == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, results.errorValue);
                    return;
                }

                if (results.success == true)
                {
                    splashScreenLogo.logo = logo;
                    DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                }
            });

            splashScreenLogo.duration = this.duration;

            return splashScreenLogo;
        }

        #endregion
    }

    #endregion

    #region Other Data

    [Serializable]
    public partial class SceneData
    {
        [Space(5)]
        [NonReorderable]
        public SceneAsset[] sceneList;
    }

    public partial class SceneData
    {
        [HideInInspector]
        public string[] sceneListGUID;
    }

    #endregion

    #region Display Data

    [Serializable]
    public struct AppResolution
    {
        public int width;

        [Space(5)]
        public int height;
    }

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
        public FullScreenMode fullScreenMode;

        [Space(5)]
        public bool defaultIsNativeResolution;

        [Space(5)]
        public AppResolution resolution;

        [Space(5)]
        public bool resizableWindow;

        [Space(5)]
        public bool allowFullScreenSwitch;
    }

    [Serializable]
    public struct WebDisplaySettings
    {
        [Space(5)]
        public AppResolution resolution;
    }

    #endregion

    #region Platform Specific Data

    /// <summary>
    /// This class contains platform related data.
    /// </summary>
    public static class PlatformSpecificData
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

    #endregion

    #region Platform Build Settings

    [Serializable]
    public struct AndroidBuildSettings
    {
        [Space(5)]
        public string packageName;

        [Space(5)]
        public AndroidPreferredInstallLocation installLocation;

        [Space(5)]
        public AndroidSdkVersions sdkVersion;

        [Space(5)]
        public bool buildAppBundle;
    }

    [Serializable]
    public struct iOSBuildSettings
    {
        [Space(5)]
        public string bundleIdentifier;

        [Space(5)]
        public string cameraUsageDescription;

        [Space(5)]
        public string microPhoneUsageDescription;

        [Space(5)]
        public string blueToothUsageDescription;
    }

    [Serializable]
    public class StandaloneBuildSettings
    {
        [Space(5)]
        public WindowsBuildSettings windows;

        [Space(5)]
        public OSXBuildSettings mac;

        [Space(5)]
        public LinuxBuildSettings linux;

        [Space(5)]
        public OtherSettings otherSettings;
    }


    [Serializable]
    public struct OSXBuildSettings
    {
        [Space(5)]
        public ColorSpace colorSpace;

        [Space(5)]
        [NonReorderable]
        public GraphicsDeviceType[] graphicsApi;

        [Space(5)]
        public string bundleIdentifier;

        [Space(5)]
        public int build;

        [Space(5)]
        public string category;

        [Space(5)]
        public bool storeValidation;

        [Space(5)]
        public UsageDescriptionData UsageDescription;
    }

    [Serializable]
    public struct OtherSettings
    {
        [Space(5)]
        public string appIdentifier;

        [Space(5)]
        public ScriptingImplementation scriptingBackend;

        [Space(5)]
        public ApiCompatibilityLevel apiCompatibility;
    }

    [Serializable]
    public struct UsageDescriptionData
    {
        [Space(5)]
        public string camera;

        [Space(5)]
        public string microPhone;

        [Space(5)]
        public string blueTooth;
    }

    [Serializable]
    public struct WindowsBuildSettings
    {
        [Space(5)]
        public ColorSpace colorSpace;

        [Space(5)]
        [NonReorderable]
        public GraphicsDeviceType[] graphicsApi;
    }

    [Serializable]
    public struct LinuxBuildSettings
    {
        [Space(5)]
        public ColorSpace colorSpace;

        [Space(5)]
        [NonReorderable]
        public GraphicsDeviceType[] graphicsApi;
    }

    [Serializable]
    public struct WebGLBuildSettings
    {

    }

    #endregion

    #region Build Config Data

    /// <summary>
    /// App build settings.
    /// </summary>
    [Serializable]
    public struct BuildConfig
    {
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

    #endregion

    #endregion
}
