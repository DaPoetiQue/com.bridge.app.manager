using System;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;
using UnityEngine.Rendering;
using Bridge.Core.App.Data.Storage;
using Bridge.Core.UnityCustomEditor.Debugger;
using System.Linq;
using Bridge.Core.UnityCustomEditor.App.Manager;

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
        public string displayName;

        [Space(5)]
        public string companyName;

        [Space(5)]
        public string version;

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
                displayName = this.displayName,
                companyName = this.companyName,
                version = this.version,
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
    public struct AppInfoDataObject : IEquatable<AppInfoDataObject>
    {
        #region Components

        [Space(5)]
        public string displayName;

        [Space(5)]
        public string companyName;

        [Space(5)]
        public string version;

        [Space(5)]
        public SplashScreenData splashScreens;

        #endregion

        #region Data Conversion

        public AppInfo ToInstance()
        {
            AppInfo appInfo = new AppInfo
            {
                displayName = this.displayName,
                companyName = this.companyName,
                version = this.version,
                splashScreens = this.splashScreens.ToInstance()
            };

            return appInfo;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(AppInfoDataObject other)
        {

            return this.displayName.Equals(other.displayName) &&
                (
                    object.ReferenceEquals(this.companyName, other.companyName) ||
                    this.companyName != null &&
                    this.companyName.Equals(other.companyName)
                ) &&
                (
                    object.ReferenceEquals(this.version, other.version) ||
                    this.version != null &&
                    this.version.Equals(other.version)
                );
        }

        #endregion
    }

    #endregion

    #region Platform Specific App Icons Data

    public static class SerializableInstanceDataConverter
    {
        #region 

        public static SceneAsset[] GetBuildScenes(string[] buildScenes)
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

        public static string[] GetBuildScenesDirectories(SceneAsset[] buildScenes)
        {
            try
            {
                if (buildScenes == null)
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
            catch (Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, $"Failed With Exception : {exception.Message}");
                throw exception;
            }
        }

        public static Texture2D GetIconFromPath(string path)
        {
            try
            {
                Texture2D icon = null;

                if (string.IsNullOrEmpty(path) == false)
                {
                    Storage.AssetData.LoadAsset<Texture2D>(path, (loadedAssetData, callbackResults) =>
                    {
                        if (callbackResults.error == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, callbackResults.errorValue);
                            return;
                        }

                        if (callbackResults.success == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                            icon = loadedAssetData;
                        }
                    });
                }

                return icon;
            }
            catch (Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, exception.Message);
                throw exception;
            }
        }

        public static string GetIconPath(Texture2D icon)
        {
            try
            {
                string iconPath = string.Empty;

                if (icon != null)
                {
                    Storage.Directory.GetAssetPath(icon, (loadedAssetData, callbackResults) =>
                    {
                        if (callbackResults.error == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, callbackResults.errorValue);
                            return;
                        }

                        if (callbackResults.success == true)
                        {
                            DebugConsole.Log(Debug.LogLevel.Success, callbackResults.successValue);
                            iconPath = loadedAssetData;
                        }
                    });
                }

                return iconPath;
            }
            catch (Exception exception)
            {
                DebugConsole.Log(Debug.LogLevel.Error, exception.Message);
                throw exception;
            }
        }

        #endregion
    }

    #region Android Icon Data

    [Serializable]
    public struct AndroidAppIconKind
    {
        [Space(5)]
        public AndroidIconKind appIconKind;
    }

    [Serializable]
    public struct AdaptiveAppIcon
    {
        #region Components

        [Space(5)]
        public Texture2D foreground;

        [Space(5)]
        public Texture2D background;

        #endregion

        #region Converted Data

        public AdaptiveAppIconData ToSerializable()
        {
            return new AdaptiveAppIconData
            {
                foregroundDirectory = SerializableInstanceDataConverter.GetIconPath(foreground),
                backgroundDirectory = SerializableInstanceDataConverter.GetIconPath(background)
            };
        }

        #endregion
    }

    [Serializable]
    public struct AdaptiveAppIconData
    {
        #region Components

        public string foregroundDirectory;

        public string backgroundDirectory;

        #endregion

        #region Converted Data

        public AdaptiveAppIcon ToInstance()
        {
            return new AdaptiveAppIcon
            {
                foreground = SerializableInstanceDataConverter.GetIconFromPath(foregroundDirectory),
                background = SerializableInstanceDataConverter.GetIconFromPath(backgroundDirectory)
            };
        }

        #endregion
    }

    [Serializable]
    public struct DefaultAppIcon
    {
        #region Components

        [Space(5)]
        public Texture2D defaultIcon;

        #endregion

        #region Converted Data

        public DefaultAppIconData ToSerializable()
        {
            return new DefaultAppIconData
            {
                defaultIconDirectory = SerializableInstanceDataConverter.GetIconPath(defaultIcon)
            };
        }

        #endregion

    }

    [Serializable]
    public struct DefaultAppIconData : IEquatable<DefaultAppIconData>
    {
        #region Components

        public string defaultIconDirectory;

        #endregion

        #region Converted Data

        public DefaultAppIcon ToInstance()
        {
            return new DefaultAppIcon
            {
                defaultIcon = SerializableInstanceDataConverter.GetIconFromPath(defaultIconDirectory)
            };
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(DefaultAppIconData other)
        {
            return this.defaultIconDirectory.Equals(other.defaultIconDirectory);

        }

        #endregion
    }

    #endregion

    public enum AndroidIconKind : int
    {
        Adaptive = 0,
        Round = 1,
        Legacy = 2
    }

    public struct AppPlatformIconData
    {
        #region Platform Specific Data

        public static PlatformIconKind GetAndroindPlatformIconKind(AndroidIconKind iconKind)
        {
            PlatformIconKind platformIcon = AndroidPlatformIconKind.Adaptive;

            switch(iconKind)
            {
                case AndroidIconKind.Adaptive:

                    platformIcon = AndroidPlatformIconKind.Adaptive;

                    break;

                case AndroidIconKind.Legacy:

                    platformIcon = AndroidPlatformIconKind.Legacy;

                    break;

                case AndroidIconKind.Round:

                    platformIcon = AndroidPlatformIconKind.Round;

                    break;
            }

            return platformIcon;
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

            if (this.screens.Length > 0)
            {
                splashScreen.screens = new SplashScreenLogoData[this.screens.Length];

                for (int i = 0; i < this.screens.Length; i++)
                {
                    splashScreen.screens[i] = this.screens[i].ToSerializable();
                }
            }

            Storage.Directory.GetAssetPath<Sprite>(background, (backgroundPath, results) =>
            {
                if (results.error == true)
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
            if (screens == null)
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
                if (results.error == true)
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
                if (results.error == true)
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
                if (results.error == true)
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

    #region Build Settings

    [Serializable]
    public class BuildProfile : ScriptableObject
    {
        [Space(5)]
        public BuildSettings profile;
    }

    /// <summary>
    /// This class contains the app build settings.
    /// </summary>
    [CreateAssetMenu(fileName = "New Profile", menuName = "3ridge/Build Manager/Profile")]
    [Serializable]
    [CustomEditor(typeof(BuildSettings))]
    public class BuildSettings : ScriptableObject
    {
        [Space(5)]
        public AppInfo appInfo;

        #region App Icons

        [Space(5)]
        public bool showIconSettings;

        #region Standalone

        [Space(5)]
        public IconKind appIconKind;

        [Space(5)]
        public DefaultAppIcon standaloneAppIcon;

        #endregion

        #region Android

        [Space(5)]
        public AndroidAppIconKind androidIconKind;

        [Space(5)]
        public AdaptiveAppIcon androidAdaptiveAppIcon;

        [Space(5)]
        public DefaultAppIcon androidRoundAppIcon;

        [Space(5)]
        public DefaultAppIcon androidLegacyAppIcon;

        #endregion

        #region iOS & tvOS

        [Space(5)]
        public DefaultAppIcon iOSAppIcon;

        [Space(5)]
        public DefaultAppIcon tvOSAppIcon;

        #endregion

        #endregion

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
                showIconSettings = this.showIconSettings,
                appIconKind = this.appIconKind,
                standaloneAppIconData = standaloneAppIcon.ToSerializable(),
                androidIconKind = this.androidIconKind,
                androidAdaptiveAppIconData = this.androidAdaptiveAppIcon.ToSerializable(),
                androidRoundAppIconData = this.androidRoundAppIcon.ToSerializable(),
                androidLegacyAppIconData = this.androidLegacyAppIcon.ToSerializable(),
                iOSAppIconData = this.iOSAppIcon.ToSerializable(),
                tvOSAppIconData = this.tvOSAppIcon.ToSerializable(),
                buildScenes = SerializableInstanceDataConverter.GetBuildScenesDirectories(buildScenes),
                configurations = this.configurations,
                consoleDisplaySettings = this.consoleDisplaySettings,
                mobileDisplaySettings = this.mobileDisplaySettings,
                standaloneDisplaySettings = this.standaloneDisplaySettings,
                webDisplaySettings = this.webDisplaySettings,
                androidBuildSettings = this.androidBuildSettings,
                iOSBuildSettings = this.iOSBuildSettings,
                standaloneBuildSettings = this.standaloneBuildSettings,
                webGLBuildSettings = this.webGLBuildSettings,
                buildAndRun = this.buildAndRun
            };
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
    public class BuildSettingsData : IEquatable<BuildSettingsData>
    {
        [Space(5)]
        public AppInfoDataObject appInfo;

        #region App Icons

        [Space(5)]
        public bool showIconSettings;

        #region Standalone

        [Space(5)]
        public IconKind appIconKind;

        [Space(5)]
        public DefaultAppIconData standaloneAppIconData;

        #endregion

        #region Android

        [Space(5)]
        public AndroidAppIconKind androidIconKind;

        [Space(5)]
        public AdaptiveAppIconData androidAdaptiveAppIconData;

        [Space(5)]
        public DefaultAppIconData androidRoundAppIconData;

        [Space(5)]
        public DefaultAppIconData androidLegacyAppIconData;

        #endregion

        #region iOS & tvOS

        [Space(5)]
        public DefaultAppIconData iOSAppIconData;

        [Space(5)]
        public DefaultAppIconData tvOSAppIconData;

        #endregion

        #endregion

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
            return new BuildSettings
            {
                appInfo = this.appInfo.ToInstance(),
                showIconSettings = this.showIconSettings,
                appIconKind = this.appIconKind,
                standaloneAppIcon = this.standaloneAppIconData.ToInstance(),
                androidIconKind = this.androidIconKind,
                androidAdaptiveAppIcon = this.androidAdaptiveAppIconData.ToInstance(),
                androidRoundAppIcon = this.androidRoundAppIconData.ToInstance(),
                androidLegacyAppIcon = this.androidLegacyAppIconData.ToInstance(),
                iOSAppIcon = this.iOSAppIconData.ToInstance(),
                tvOSAppIcon = this.iOSAppIconData.ToInstance(),
                buildScenes = SerializableInstanceDataConverter.GetBuildScenes(buildScenes),
                configurations = this.configurations,
                consoleDisplaySettings = this.consoleDisplaySettings,
                mobileDisplaySettings = this.mobileDisplaySettings,
                standaloneDisplaySettings = this.standaloneDisplaySettings,
                webDisplaySettings = this.webDisplaySettings,
                androidBuildSettings = this.androidBuildSettings,
                iOSBuildSettings = this.iOSBuildSettings,
                standaloneBuildSettings = this.standaloneBuildSettings,
                webGLBuildSettings = this.webGLBuildSettings,
                buildAndRun = this.buildAndRun
            };
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj as BuildSettingsData);
        }

        public bool Equals(BuildSettingsData other)
        {
            if (other == null)
                return false;

            return this.appInfo.Equals(other.appInfo) && this.standaloneAppIconData.Equals(other.standaloneAppIconData) && this.showIconSettings.Equals(other.showIconSettings);

        }
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

    [Serializable]
    public struct CompressionData
    {
        [Space(5)]
        public TextureFormat textureCompressionFormat;

        [Space(5)]
        public bool hasMipMap;
    }

    #endregion

    #endregion
}
