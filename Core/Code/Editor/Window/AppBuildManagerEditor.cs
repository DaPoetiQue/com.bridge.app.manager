using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System;
using System.IO;
using Bridge.Core.App.Manager;
using Bridge.Core.UnityEditor.Debugger;
using Bridge.Core.App.Data.Storage;
using Bridge.Core.App.Events;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEditorInternal;

namespace Bridge.Core.UnityEditor.App.Manager
{
    [CanEditMultipleObjects]
    public class AppBuildManagerEditor : EditorWindow
    {
        #region Components

        private static AppBuildManagerEditor window;

        #endregion

        #region Serializations

        private static string fileName = "BuildSettingsData";
        private static string folderName = "BridgeEditor";

        #endregion

        #region Open Editor Window

        [MenuItem("Window/3ridge/App Build Manager #&m")]
        public static void OpenAppBuildManagerEditor()
        {
            var windowInstance = GetWindow<AppBuildManagerEditor>("App Build Manager");
            windowInstance.minSize = new Vector2(350, 400);
            windowInstance.maxSize = new Vector2(500, 600);
            windowInstance.Show();
        }

        #endregion

        #region Window Layouts

        #region Textures

        private Texture2D iconTexture;
        private Texture2D headerSectionTexture;
        private Texture2D settingsSectionTexture;
        private Texture2D settingsSectionContentTexture;

        #endregion

        #region Colors

        private Color headerSectionColor = new Color(239.0f/ 255.0f, 124.0f / 255.0f, 24.0f / 255.0f, 1.0f);
        private Color settingsSectionColor = new Color(25.0f / 255.0f, 25.0f / 255.0f, 25.0f / 255.0f, 1.0f);
        private Color settingsSectionContentColor = new Color(25.0f / 255.0f, 25.0f / 255.0f, 25.0f / 255.0f, 1.0f);

        #endregion

        #region Rects

        private Rect iconRect;
        private Rect headerSectionRect;
        private Rect settingsSectionRect;
        private Rect settingsSectionContentRect;

        #endregion

        #region Window Styles

        private GUIStyle settingsHeaderStyle = new GUIStyle();
        private GUIStyle settingContentStyle = new GUIStyle();

        #endregion

        #region Data

        private static Vector2 scrollPosition;

        #endregion

        #endregion

        #region Window Content

        private GUIContent settingsHeaderContent = new GUIContent();

        #endregion

        #region Settings

        private static BuildSettings appSettings;
        private static bool RunAppOnCompletion;
        private static AndroidPreferredInstallLocation installLocation;

        #region Storage Data

        //private static StorageData.DirectoryInfoData appInfoStorageData = new StorageData.DirectoryInfoData()
        //{ 
        //    fileName = "artoolkit",
        //    folderName = "Editor Data",
        //    extensionType = StorageData.ExtensionType.json
        //};

        #endregion

        #endregion

        #region Unity

        private void OnEnable() => Init();

        private void OnGUI() => OnWindowUpdates();

        #endregion

        #region Initializations

        private void Init()
        {
            InitializeTextures();
            InitializeLayoutStyles();
            InitializeContentData();
        }

        private void InitializeTextures()
        {
            #region Header

            headerSectionTexture = new Texture2D(1, 1);
            headerSectionTexture.SetPixel(0, 0, headerSectionColor);
            headerSectionTexture.Apply();

            #endregion

            #region Icon

            iconTexture = Resources.Load<Texture2D>("Editor/Windows");

            #endregion

            #region Settings

            settingsSectionTexture = new Texture2D(1, 1);
            settingsSectionTexture.SetPixel(0, 0, settingsSectionColor);
            settingsSectionTexture.Apply();

            settingsSectionContentTexture = new Texture2D(1, 1);
            settingsSectionContentTexture.SetPixel(0, 0, settingsSectionContentColor);
            settingsSectionContentTexture.Apply();

            #endregion
        }

        private void InitializeLayoutStyles()
        {
            #region Settings Header

            settingsHeaderStyle.normal.textColor = Color.white;
            settingsHeaderStyle.fontSize = 15;
            settingsHeaderStyle.fontStyle = FontStyle.Bold;
            settingsHeaderStyle.padding.top = 40;
            settingsHeaderStyle.padding.left = 50;
            settingsHeaderStyle.alignment = TextAnchor.LowerCenter;
            settingsHeaderContent.text = "Build Manager";

            #endregion

            #region Settings Content

            settingContentStyle.normal.textColor = Color.white;
            settingContentStyle.fontSize = 12;
            settingContentStyle.alignment = TextAnchor.LowerLeft;
            settingContentStyle.padding.left = 25;

            #endregion
        }

        private void InitializeContentData()
        {
            appSettings = CreateInstance<BuildSettings>();

            // If config not loaded. set default settings.
            appSettings.appInfo.appVersion = "1.0";
            appSettings.configurations.platform = BuildTarget.Android; // This will be loaded from a json file called buildSettings.json
            appSettings.androidBuildSettings.sdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        }

        #endregion

        #region Main

        /// <summary>
        /// Draws window layouts.
        /// </summary>
        private void OnWindowUpdates()
        {
            if (window == null)
            {
                appSettings.appInfo = GetBuildSettings().appInfo;

                window = GetWindow<AppBuildManagerEditor>();
                DebugConsole.Log(Debug.LogLevel.Debug, this, "Window Refreshed!.");
            }

            DrawLayouts();
            OnEditorWindowUpdate();
        }

        private void DrawLayouts()
        {
            if(headerSectionTexture == null)
            {
                DebugConsole.Log(Debug.LogLevel.Warning, this, "Header Texture Missing");
            }

            #region Header Section

            headerSectionRect.x = 0;
            headerSectionRect.y = 0;
            headerSectionRect.width = Screen.width;
            headerSectionRect.height = 100;

            if(headerSectionTexture == null)
            {
                InitializeTextures();
            }

            GUI.DrawTexture(headerSectionRect, headerSectionTexture);

            #endregion

            #region Icon

            iconRect.width = 100;
            iconRect.height = 100;
            iconRect.x = 10;
            iconRect.y = 0;

            GUI.DrawTexture(iconRect, iconTexture);
            GUILayout.Label(settingsHeaderContent, settingsHeaderStyle);

            #endregion

            #region Settings Section

            settingsSectionRect.x = 0;
            settingsSectionRect.y = headerSectionRect.height;
            settingsSectionRect.width = window.position.width;
            settingsSectionRect.height = window.position.height - headerSectionRect.height;

            GUI.DrawTexture(settingsSectionRect, settingsSectionTexture, ScaleMode.ScaleToFit);

            settingsSectionContentRect.x = 0;
            settingsSectionContentRect.y = settingsSectionRect.y;
            settingsSectionContentRect.width = settingsSectionRect.width;
            settingsSectionContentRect.height = settingsSectionRect.height;

            GUI.DrawTexture(settingsSectionContentRect, settingsSectionContentTexture);

            #endregion
        }

        private void OnEditorWindowUpdate()
        {
            GUILayout.BeginArea(settingsSectionRect);

            #region Menu Content Area

            #region Menu Content and Style Update

            if (appSettings == null)
            {
                InitializeContentData();
            }

            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(10, 10, 25, 25);

            var layout = new GUILayoutOption[2];
            layout[0] = GUILayout.Width(settingsSectionRect.width);
            layout[1] = GUILayout.ExpandHeight(true);

            #endregion

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, style ,layout);

            #region Scroll Area

            #region App Info & Configurations

            GUILayout.Space(10);
            SerializedObject appInfoSerializedObject = new SerializedObject(appSettings);
            SerializedProperty appInfoSerializedObjectProperty = appInfoSerializedObject.FindProperty("appInfo");
            EditorGUILayout.PropertyField(appInfoSerializedObjectProperty, true);
            appInfoSerializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
            SerializedObject sceneDataSerializedObject = new SerializedObject(appSettings);
            sceneDataSerializedObject.CopyFromSerializedPropertyIfDifferent(appInfoSerializedObjectProperty);
            SerializedProperty sceneDataSerializedObjectProperty = sceneDataSerializedObject.FindProperty("buildScenes");
            EditorGUILayout.PropertyField(sceneDataSerializedObjectProperty, true);
            sceneDataSerializedObject.ApplyModifiedProperties();

            GUILayout.Space(10);
            SerializedObject appConfigSerializedObject = new SerializedObject(appSettings);
            SerializedProperty appConfigSerializedObjectProperty = appConfigSerializedObject.FindProperty("configurations");
            EditorGUILayout.PropertyField(appConfigSerializedObjectProperty, true);
            appConfigSerializedObject.ApplyModifiedProperties();

            if (AppDataSettings.GetRuntimeOS(appSettings) == RuntimeOS.Console)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("consoleDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();
            }

            if (AppDataSettings.GetRuntimeOS(appSettings) == RuntimeOS.Mobile)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("mobileDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();

                if (appSettings.configurations.platform == BuildTarget.Android)
                {
                    GUILayout.Space(10);

                    SerializedObject androidConfigSerializedObject = new SerializedObject(appSettings);
                    SerializedProperty androidConfigSerializedObjectProperty = androidConfigSerializedObject.FindProperty("androidBuildSettings");
                    EditorGUILayout.PropertyField(androidConfigSerializedObjectProperty, true);
                    androidConfigSerializedObject.ApplyModifiedProperties();
                }

                if (appSettings.configurations.platform == BuildTarget.iOS)
                {
                    GUILayout.Space(10);

                    SerializedObject androidConfigSerializedObject = new SerializedObject(appSettings);
                    SerializedProperty androidConfigSerializedObjectProperty = androidConfigSerializedObject.FindProperty("iOSBuildSettings");
                    EditorGUILayout.PropertyField(androidConfigSerializedObjectProperty, true);
                    androidConfigSerializedObject.ApplyModifiedProperties();
                }
            }

            if (AppDataSettings.GetRuntimeOS(appSettings) == RuntimeOS.Standalone)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("standaloneDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();

                GUILayout.Space(10);

                SerializedObject buildSettingsObject = new SerializedObject(appSettings);
                SerializedProperty buildSettingsProperty = buildSettingsObject.FindProperty("standaloneBuildSettings");
                EditorGUILayout.PropertyField(buildSettingsProperty, true);
                buildSettingsObject.ApplyModifiedProperties();

                //if(appSettings.configurations.platform == BuildTarget.StandaloneWindows || appSettings.configurations.platform == BuildTarget.StandaloneWindows64)
                //{
                //    GUILayout.Space(10);

                //    SerializedObject buildSettingsObject = new SerializedObject(appSettings);
                //    SerializedProperty buildSettingsProperty = buildSettingsObject.FindProperty("windowsBuildSettings");
                //    EditorGUILayout.PropertyField(buildSettingsProperty, true);
                //    buildSettingsObject.ApplyModifiedProperties();
                //}

                //if (appSettings.configurations.platform == BuildTarget.StandaloneOSX)
                //{
                //    GUILayout.Space(10);

                //    SerializedObject buildSettingsObject = new SerializedObject(appSettings);
                //    SerializedProperty buildSettingsProperty = buildSettingsObject.FindProperty("macBuildSettings");
                //    EditorGUILayout.PropertyField(buildSettingsProperty, true);
                //    buildSettingsObject.ApplyModifiedProperties();
                //}

                //if (appSettings.configurations.platform == BuildTarget.StandaloneLinux64)
                //{
                //    GUILayout.Space(10);

                //    SerializedObject buildSettingsObject = new SerializedObject(appSettings);
                //    SerializedProperty buildSettingsProperty = buildSettingsObject.FindProperty("linuxBuildSettings");
                //    EditorGUILayout.PropertyField(buildSettingsProperty, true);
                //    buildSettingsObject.ApplyModifiedProperties();
                //}
            }

            if (AppDataSettings.GetRuntimeOS(appSettings) == RuntimeOS.Web)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("webDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();

                if (appSettings.configurations.platform == BuildTarget.WebGL)
                {
                    GUILayout.Space(10);

                    SerializedObject buildSettingsObject = new SerializedObject(appSettings);
                    SerializedProperty buildSettingsProperty = buildSettingsObject.FindProperty("webGLBuildSettings");
                    EditorGUILayout.PropertyField(buildSettingsProperty, true);
                    buildSettingsObject.ApplyModifiedProperties();
                }
            }

            #endregion

            #region App Builder

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Settings", GUILayout.Height(45)))
            {
                ApplyAppSettings(appSettings.ToSerializable());
            }

            if (Directory.Exists(appSettings.configurations.targetBuildDirectory) == true)
            {
                GUILayout.Space(2);

                if (GUILayout.Button("Open Build Folder", GUILayout.Height(45)))
                {
                    Storage.Directory.OpenFolder(GetBuildSettings(GetDefaultStorageInfo()).configurations.targetBuildDirectory);
                }
            }

            if (Directory.Exists(Storage.Directory.GetProjectTempDirectory()))
            {
                GUILayout.Space(2);

                if (GUILayout.Button("Clear Project Cache", GUILayout.Height(45)))
                {
                    BuildCompilerScript.ClearProjectCache();
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (GUILayout.Button("Build App", GUILayout.Height(60)))
            {
                Build();
            }

            GUILayout.Space(10);

            SerializedObject buildSerializedObject = new SerializedObject(appSettings);
            SerializedProperty buildSerializedObjectProperty = buildSerializedObject.FindProperty("buildAndRun");
            EditorGUILayout.PropertyField(buildSerializedObjectProperty, true);
            buildSerializedObject.ApplyModifiedProperties();

            #endregion

            #endregion

            EditorGUILayout.EndScrollView();

            #endregion

            GUILayout.EndArea();
        }

        #region Build App

        [MenuItem("3ridge/Config/Build App #B")]
        private static void Build()
        {
            BuildSettingsData buildSettings = GetBuildSettings(GetDefaultStorageInfo());

            string fileFullPath = buildSettings.configurations.buildLocation;
            buildSettings.configurations.buildLocation = fileFullPath;

            if (string.IsNullOrEmpty(buildSettings.configurations.targetBuildDirectory) || Directory.Exists(buildSettings.configurations.targetBuildDirectory) == false)
            {
                buildSettings.configurations.targetBuildDirectory = EditorUtility.SaveFolderPanel("Choose Build Folder", "", "");
                ApplyAppSettings(buildSettings);
                DebugConsole.Log(Debug.LogLevel.Debug, $"Build Directory Set @ : {appSettings.configurations.buildLocation}");
            }

            #region Saving Editor Data

            Scene currentScene = SceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(currentScene);
            EditorApplication.ExecuteMenuItem("File/Save Project");

            #endregion

            if (!string.IsNullOrEmpty(buildSettings.configurations.targetBuildDirectory) && Directory.Exists(buildSettings.configurations.targetBuildDirectory) == true)
            {
                BuildCompilerScript.BuildCompiler(buildSettings, (results, resultsData) =>
                {
                   if(results.error)
                   {
                        DebugConsole.Log(Debug.LogLevel.Error, results.errorValue);
                        return;
                   }

                   if(results.success)
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
        public static BuildSettings GetBuildSettings()
        {
            appSettings = GetBuildSettings(GetDefaultStorageInfo()).ToInstance();
            return appSettings;
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

        /// <summary>
        /// This method create build settings for the selected platform.
        /// </summary>
        /// <param name="buildSettings"></param>
        private static void ApplyAppSettings(BuildSettingsData buildSettings)
        {
            ApplyAppInfo(buildSettings.appInfo, (callbackResults, resultsData) =>
            { 
                if(callbackResults.error)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, $"Failed to apply app info with results : {callbackResults.errorValue}.");
                    return;
                }

                if(callbackResults.success)
                {
                    buildSettings.appInfo = resultsData;

                    ApplyBuildSettings(buildSettings, (callbackResults, resultsData) =>
                    { 
                        if(callbackResults.error)
                        {
                            DebugConsole.Log(Debug.LogLevel.Error, $"Failed to apply build settings with results : {callbackResults.errorValue}.");
                            return;
                        }

                        if(callbackResults.success)
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
        /// <param name="appInfo"></param>
        /// <param name="callback"></param>
        private static void ApplyAppInfo(AppInfo appInfo, Action<AppEventsData.CallBackResults, AppInfo> callback = null)
        {
            AppEventsData.CallBackResults callBackResults = new AppEventsData.CallBackResults();

            try
            {
                if (string.IsNullOrEmpty(appInfo.companyName))
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, "App info's company name field is empty. Please assign company name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                    return;
                }

                if (string.IsNullOrEmpty(appInfo.appName))
                {
                    DebugConsole.Log(Debug.LogLevel.Warning, "App info's app name field is empty. Please assign app name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                    return;
                }

                PlayerSettings.companyName = (string.IsNullOrEmpty(appInfo.companyName)) ? PlayerSettings.companyName : appInfo.companyName;
                PlayerSettings.productName = (string.IsNullOrEmpty(appInfo.appName)) ? PlayerSettings.productName : appInfo.appName;
                PlayerSettings.bundleVersion = (string.IsNullOrEmpty(appInfo.appVersion)) ? PlayerSettings.bundleVersion : appInfo.appVersion;

                string companyName = appInfo.companyName;

                if (companyName.Contains(" "))
                {
                    companyName = companyName.Replace(" ", "");
                }

                string appName = appInfo.appName;

                if (appName.Contains(" "))
                {
                    appName = appName.Replace(" ", "");
                }

                if (appInfo.splashScreen != null)
                {
                    PlayerSettings.SplashScreen.background = appInfo.splashScreen;
                    PlayerSettings.SplashScreen.backgroundColor = Color.black;
                }

                if (appInfo.appIcon != null)
                {
                    PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { appInfo.appIcon });
                }

                appInfo.appIdentifier = $"com.{companyName}.{appName}";

                callBackResults.success = true;
                callback.Invoke(callBackResults, appInfo);
            }
            catch (Exception exception)
            {
                callBackResults.error = true;
                callBackResults.errorValue = exception.Message;
                callback.Invoke(callBackResults, appInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buildSettings"></param>
        /// <param name="callback"></param>
        private static void ApplyBuildSettings(BuildSettingsData buildSettings, Action<AppEventsData.CallBackResults, BuildSettingsData> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                SwitchBuildTarget(buildSettings.configurations, switchResults =>
                {
                    if(switchResults.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, switchResults.errorValue);
                        return;
                    }

                    if(switchResults.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, $"Applied build settings for : {buildSettings.configurations.platform}");

                        if (appSettings.configurations.platform == BuildTarget.Android || appSettings.configurations.platform == BuildTarget.iOS)
                        {
                            switch (buildSettings.mobileDisplaySettings.allowedOrientation)
                            {
                                case UIOrientation.AutoRotation:

                                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;

                                    break;

                                case UIOrientation.Portrait:

                                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

                                    break;

                                case UIOrientation.PortraitUpsideDown:

                                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.PortraitUpsideDown; ;

                                    break;

                                case UIOrientation.LandscapeLeft:

                                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

                                    break;

                                case UIOrientation.LandscapeRight:

                                    PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeRight;

                                    break;
                            }
                        }

                        switch (EditorUserBuildSettings.activeBuildTarget)
                        {
                            case BuildTarget.Android:

                                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, buildSettings.appInfo.appIdentifier);

                                GraphicsDeviceType[] graphicsDeviceType = new GraphicsDeviceType[1];
                                graphicsDeviceType[0] = GraphicsDeviceType.OpenGLES3;
                                PlayerSettings.SetGraphicsAPIs(buildSettings.configurations.platform, graphicsDeviceType);

                                PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false);

                                PlayerSettings.Android.minSdkVersion = buildSettings.androidBuildSettings.sdkVersion;
                                PlayerSettings.Android.androidTVCompatibility = false;
                                PlayerSettings.Android.preferredInstallLocation = buildSettings.androidBuildSettings.installLocation;
                                PlayerSettings.Android.ARCoreEnabled = true;

                                EditorUserBuildSettings.buildAppBundle = buildSettings.androidBuildSettings.buildAppBundle;

                                break;

                            case BuildTarget.iOS:

                                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, buildSettings.appInfo.appIdentifier);

                                break;

                            case BuildTarget.StandaloneWindows:
                       
                                PlayerSettings.SetScriptingBackend(GetBuildTargetGroup(buildSettings.configurations.platform), buildSettings.standaloneBuildSettings.otherSettings.scriptingBackend);

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
        private static void SwitchBuildTarget(BuildConfig config, Action<AppEventsData.CallBackResults> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                IsPlatformSupported(config.platform, supportResults => 
                {
                    if(supportResults.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Warning, supportResults.errorValue);
                        EditorUtility.DisplayDialog("3ridge Build Settings", supportResults.errorValue, "Cancel");
                        return;
                    }

                    if(supportResults.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, $"Config Success : {supportResults.successValue}");
                        if (config.platform == EditorUserBuildSettings.activeBuildTarget)
                        {
                            results.success = true;
                            results.successValue = $"Applied build settings for : {config.platform.ToString()}";          
                        }
                        else
                        {
                            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(GetBuildTargetGroup(config.platform), config.platform);

                            results.success = true;
                            results.successValue = $"Current build target has been successfully switched to : {config.platform}";
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
        private static void IsPlatformSupported(BuildTarget buildTarget, Action<AppEventsData.CallBackResults> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                var moduleManager = Type.GetType("UnityEditor.Modules.ModuleManager,UnityEditor.dll");
                var isPlatformSupportLoaded = moduleManager.GetMethod("IsPlatformSupportLoaded", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                var getTargetStringFromBuildTarget = moduleManager.GetMethod("GetTargetStringFromBuildTarget", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);

                results.success = (bool)isPlatformSupportLoaded.Invoke(null, new object[] { (string)getTargetStringFromBuildTarget.Invoke(null, new object[] { buildTarget }) });

                if(results.success == true)
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
            catch(Exception exception)
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
        private static BuildTargetGroup GetBuildTargetGroup(BuildTarget platform)
        {
            bool standalone = platform == BuildTarget.StandaloneWindows || platform == BuildTarget.StandaloneWindows64 || platform == BuildTarget.StandaloneOSX || platform == BuildTarget.StandaloneLinux64;
            BuildTargetGroup group = new BuildTargetGroup();

            if(standalone)
            {
                group = BuildTargetGroup.Standalone;
            }
            else
            {
                switch(platform)
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

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        #endregion
    }
}
