using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System;
using System.IO;
using Bridge.Core.App.Manager;
using Bridge.Core.UnityEditor.Debugger;
using Bridge.Core.App.Data.Storage;

namespace Bridge.Core.UnityEditor.App.Manager
{
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
            appSettings.androidSettings.sdkVersion = AndroidSdkVersions.AndroidApiLevel24;
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
            }

            if (AppDataSettings.GetRuntimeOS(appSettings) == RuntimeOS.Standalone)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("standaloneDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();
            }

            if (AppDataSettings.GetRuntimeOS(appSettings) == RuntimeOS.Web)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("webDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();
            }

            if (appSettings.configurations.platform == BuildTarget.Android)
            {
                GUILayout.Space(10);

                SerializedObject androidConfigSerializedObject = new SerializedObject(appSettings);
                SerializedProperty androidConfigSerializedObjectProperty = androidConfigSerializedObject.FindProperty("androidSettings");
                EditorGUILayout.PropertyField(androidConfigSerializedObjectProperty, true);
                androidConfigSerializedObject.ApplyModifiedProperties();
            }

            #endregion

            #region App Builder

            GUILayout.Space(10);

            if (GUILayout.Button("Apply Settings", GUILayout.Height(45)))
            {
                ApplyAppSettings(appSettings);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Build & Launch App", GUILayout.Height(45)))
            {
                CreateBuildCompiler();
            }

            if (File.Exists(appSettings.configurations.buildLocation) == true)
            {
                GUILayout.Space(10);

                if (GUILayout.Button("Open Build Folder", GUILayout.Height(45)))
                {
                    OpenFileInExplorer(appSettings.configurations.buildLocation);
                }
            }

            #endregion

            #endregion

            EditorGUILayout.EndScrollView();

            #endregion

            GUILayout.EndArea();
        }

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
        private static void ApplyAppSettings(BuildSettings buildSettings)
        {
            if (string.IsNullOrEmpty(buildSettings.appInfo.companyName))
            {
                DebugConsole.Log(Debug.LogLevel.Warning, "App info's company name field is empty. Please assign company name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                return;
            }

            if (string.IsNullOrEmpty(buildSettings.appInfo.appName))
            {
                DebugConsole.Log(Debug.LogLevel.Warning, "App info's app name field is empty. Please assign app name in the <color=red>[AR Tool Kit Master]</color> inspector panel.");
                return;
            }

            PlayerSettings.companyName = (string.IsNullOrEmpty(buildSettings.appInfo.companyName)) ? PlayerSettings.companyName : buildSettings.appInfo.companyName;
            PlayerSettings.productName = (string.IsNullOrEmpty(buildSettings.appInfo.appName)) ? PlayerSettings.productName : buildSettings.appInfo.appName;
            PlayerSettings.bundleVersion = (string.IsNullOrEmpty(buildSettings.appInfo.appVersion)) ? PlayerSettings.bundleVersion : buildSettings.appInfo.appVersion;

            string companyName = buildSettings.appInfo.companyName;

            if (companyName.Contains(" "))
            {
                companyName = companyName.Replace(" ", "");
            }

            string appName = buildSettings.appInfo.appName;

            if (appName.Contains(" "))
            {
                appName = appName.Replace(" ", "");
            }

            buildSettings.appInfo.appIdentifier = $"com.{companyName}.{appName}";

            if(appSettings.configurations.platform == BuildTarget.Android || appSettings.configurations.platform == BuildTarget.iOS)
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

            if (buildSettings.appInfo.splashScreen != null)
            {
                PlayerSettings.SplashScreen.background = buildSettings.appInfo.splashScreen;
                PlayerSettings.SplashScreen.backgroundColor = Color.black;
            }

            if (buildSettings.appInfo.appIcon != null)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { buildSettings.appInfo.appIcon });
            }

            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:

                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, buildSettings.appInfo.appIdentifier);

                    GraphicsDeviceType[] graphicsDeviceType = new GraphicsDeviceType[1];
                    graphicsDeviceType[0] = GraphicsDeviceType.OpenGLES3;
                    PlayerSettings.SetGraphicsAPIs(buildSettings.configurations.platform, graphicsDeviceType);

                    PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, false);

                    PlayerSettings.Android.minSdkVersion = buildSettings.androidSettings.sdkVersion;
                    PlayerSettings.Android.androidTVCompatibility = false;
                    PlayerSettings.Android.preferredInstallLocation = buildSettings.androidSettings.installLocation;
                    PlayerSettings.Android.ARCoreEnabled = true;

                    EditorUserBuildSettings.buildAppBundle = buildSettings.androidSettings.buildAppBundle;

                    break;

                case BuildTarget.iOS:

                    PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, buildSettings.appInfo.appIdentifier);

                    break;
            }

            EditorUserBuildSettings.allowDebugging = buildSettings.configurations.allowDebugging;
            EditorUserBuildSettings.development = buildSettings.configurations.developmentBuild;
            EditorUserBuildSettings.SetBuildLocation(buildSettings.configurations.platform, buildSettings.configurations.buildLocation);

            BuildSettingsData serializedSettings = buildSettings.ToSerializable();

            Storage.JsonData.Save(GetDefaultStorageInfo(), serializedSettings, (infoData, saveResults) =>
            {
                if (saveResults.error)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, $"Save Failed With Error: { saveResults.errorValue}");
                    return;
                }

                if(saveResults.success)
                {
                    DebugConsole.Log(Debug.LogLevel.Success, $"Save Completed With Results : {saveResults.successValue}");
                }
            });
        }

        private static void CreateBuildCompiler()
        {
            try
            {
                #region Header Data

                string buildScripts = "/BuildScripts";
                string compilerFileName = "Compiler.bat";
                string buildCompilerFileName = "BuildCompiler.bat";

                #endregion

                #region Compiler

                var compilerData = new FileInfo(compilerFileName);
                string compilerDir = compilerData.DirectoryName;
                string compileScriptsDir = compilerDir + buildScripts;
                string compileScriptsCommand = compileScriptsDir + "/" + compilerData.Name;

                #endregion

                #region Build Compiler

                var buildCompilerData = new FileInfo(buildCompilerFileName);
                string buildCompilerDir = buildCompilerData.DirectoryName;
                string buildScriptsDir = buildCompilerDir + buildScripts;
                string buildScriptsCommand = buildScriptsDir + "/" + buildCompilerData.Name;

                #endregion

                UnityEngine.Debug.Log($"Creating Compiler - Directory : {buildScriptsDir} - File Path : {buildScriptsCommand}");

                if (!Directory.Exists(buildScriptsDir))
                {
                    Directory.CreateDirectory(buildScriptsDir);
                    UnityEngine.Debug.Log($"--> Creating Build Compiler Directory : <color=cyan>{buildScriptsDir}</color>");
                }

                string targetDest = Path.GetTempPath();
                string targetDestDir = Path.Combine(targetDest, appSettings.appInfo.appName.Replace(" ", string.Empty));

                if (!Directory.Exists(targetDestDir))
                {
                    UnityEngine.Debug.Log($"--> Creating Temp Build Project Directory @ : <color=cyan>{targetDestDir}</color>");
                    Directory.CreateDirectory(targetDestDir);
                }

                string excludedFolders = "/E /xd Library Temp Build Builds Obj Logs UserSettings MemoryCaptures";

                string directoryToRemove = (Directory.Exists(targetDestDir)) ? "@RD /S /Q " + targetDestDir : string.Empty;

                if (!string.IsNullOrEmpty(directoryToRemove))
                {
                    UnityEngine.Debug.Log($"--> Purging All Project Data @ : <color=orange>{targetDestDir}</color>");
                }

                string builds = "*.apk *.exe *.aab *.unitypackage";
                string autoGenerated = "*.csproj *.unityproj *.sln *.suo *.tmp *.user *.userprefs *.pidb *.booproj *.svd *.pdb *.mdb *.opendb *.VC.db";
                string excludeFiles = $"/xf {autoGenerated} {builds}";

                string projectCopyCommand = $"robocopy {buildCompilerDir} {targetDestDir} {excludedFolders} {excludeFiles}";

                // Build Command
                string rootPath = "C:/Program Files/";
                string unityVersion = Application.unityVersion;
                string path = "/Editor/Unity.exe";
                string pathCombined = "\"" + rootPath + unityVersion + path + "\"";
                string compilerDirectory = $"\"{targetDestDir.Replace("\\", "/")}{buildScripts}\"";
                string targetDirectory = $"\"{targetDestDir.Replace("\\", "/")}\"";
                string changeDirectoryCommand = "chdir /d " + compilerDirectory;
                string buildCommand = pathCombined + $" -quit -batchMode -projectPath .. -executeMethod BuildScript.Build";
                string compilerBatchFile = "./" + compilerFileName;

                UnityEngine.Debug.Log($"-->Change To Temp Compiler Directory : {changeDirectoryCommand}");

                string projectDir = "chdir /d " + targetDestDir;

                UnityEngine.Debug.Log($"-->Change To Build Compiler Directory : {projectDir}");

                Compiler compiler = new Compiler
                {
                    echoInitializeBuild = "echo Initializing Project Build...",
                    startBuildCommand = buildCommand,
                    echoEndBuild = "echo Build Completed...",
                    pause = "@pause"
                };

                BuildCompiler buildCompiler = new BuildCompiler
                {
                    echoPrepareBuild = "echo Preparing Build...",
                    removeDirectory = directoryToRemove,
                    echoCopy = "echo Copying Build Files...",
                    //buildAppCommand = buildCommand,
                    copyCommand = projectCopyCommand,
                    changeDirectory = changeDirectoryCommand,
                    echoCompile = "echo Compiling Build Scripts...",
                    compileBuildCommand = compilerBatchFile,
                    pause = "@pause"
                };

                File.WriteAllText(compileScriptsCommand, compiler.ToString());

                File.WriteAllText(buildScriptsCommand, buildCompiler.ToString());

                if (File.Exists(buildScriptsCommand))
                {
                    UnityEngine.Debug.Log($"--> <color=orange>Build Started....</color>");
                    StartBatchBuildCommand(buildScriptsCommand);
                }
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        private static void StartBatchBuildCommand(string buildCommand)
        {
            System.Diagnostics.Process.Start(buildCommand);
        }


        private static void OpenFileInExplorer(string assetPat)
        {
            //DebugConsole.Log(LogLevel.Debug, $"Path : {assetPat}");
        }

        #endregion

        #region Settings

        #endregion

        #endregion
    }
}
