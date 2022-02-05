using UnityEngine;
using UnityEditor;
using System.IO;
using Bridge.Core.App.Manager;
using Bridge.Core.UnityEditor.Debugger;
using Bridge.Core.App.Data.Storage;

namespace Bridge.Core.UnityEditor.App.Manager
{
    [CanEditMultipleObjects]
    public class BuildManagerWindow : EditorWindow
    {
        #region Components

        private static BuildManagerWindow window;

        #endregion

        #region Open Editor Window

        [MenuItem("Window/3ridge/App Build Manager #&m")]
        public static void OpenAppBuildManagerEditor()
        {
            var windowInstance = GetWindow<BuildManagerWindow>("App Build Manager");
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

        private static BuildSettings appBuildSettings;
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

        #region Updated Objects Settings

        #region Standalone Icon Data

        SerializedObject appIconStandaloneSerializedObject;
        SerializedProperty appIconStandaloneSerializedObjectProperty;

        #endregion

        #region Android Icons Data

        #region Adaptive Icon Kind

        SerializedObject appIconForegroundSerializedObject;
        SerializedProperty appIconForegroundSerializedObjectProperty;

        SerializedObject appIconBackgroundSerializedObject;
        SerializedProperty appIconBackgrounSerializedObjectProperty;

        #endregion

        #region Round Icon Kind

        SerializedObject appIconRoundSerializedObject;
        SerializedProperty appIconRoundSerializedObjectProperty;

        #endregion

        #region Legacy Icon Kind

        SerializedObject appIconLegacySerializedObject;
        SerializedProperty appIconLegacySerializedObjectProperty;

        #endregion

        SerializedObject androidIconsInfoSerializedObject;

        #endregion

        #region iOS & tvOS Icons Data

        #region iOS Icon

        SerializedObject appIconIOSSerializedObject;
        SerializedProperty appIconIOSSerializedObjectProperty;

        #endregion

        #region tvOS Icon

        SerializedObject appIconTVOSSerializedObject;
        SerializedProperty appIconTVOSSerializedObjectProperty;

        #endregion

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
            appBuildSettings = CreateInstance<BuildSettings>();

            // If config not loaded. set default settings.
            appBuildSettings.appInfo.version = "1.0";
            appBuildSettings.configurations.platform = BuildTarget.Android; // This will be loaded from a json file called buildSettings.json
            appBuildSettings.androidBuildSettings.sdkVersion = AndroidSdkVersions.AndroidApiLevel24;
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
                appBuildSettings = BuildManager.GetCurrentBuildSettings();

                window = GetWindow<BuildManagerWindow>();
                DebugConsole.Log(Debug.LogLevel.Debug, this, "Window Refreshed!.");
            }

            #region App Icons Settings Update

            #region Standalone Icon

            if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone)
            { 
                if(appIconStandaloneSerializedObject != null)
                {
                    appIconStandaloneSerializedObject.ApplyModifiedProperties();
                    appIconStandaloneSerializedObject.Update();
                }
            }

            #endregion

            #region Android Icons

            if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
            {
                if (androidIconsInfoSerializedObject != null)
                {
                    androidIconsInfoSerializedObject.ApplyModifiedProperties();
                    androidIconsInfoSerializedObject.Update();
                }

                #region Adaptive Icons

                if (appIconForegroundSerializedObject != null)
                {
                    appIconForegroundSerializedObject.ApplyModifiedProperties();
                    appIconForegroundSerializedObject.Update();
                }

                if (appIconBackgroundSerializedObject != null)
                {
                    appIconBackgroundSerializedObject.ApplyModifiedProperties();
                    appIconBackgroundSerializedObject.Update();
                }

                #endregion

                #region Round Icon

                if(appIconRoundSerializedObject != null)
                {
                    appIconRoundSerializedObject.ApplyModifiedProperties();
                    appIconRoundSerializedObject.Update();
                }

                #endregion

                #region Legacy Icon

                if(appIconLegacySerializedObject != null)
                {
                    appIconLegacySerializedObject.ApplyModifiedProperties();
                    appIconLegacySerializedObject.Update();
                }

                #endregion
            }

            #endregion

            #region iOS & tvOS Icons

            if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS)
            {
                if(appIconIOSSerializedObject != null)
                {
                    appIconIOSSerializedObject.ApplyModifiedProperties();
                    appIconIOSSerializedObject.Update();
                }
            }

            if(EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.tvOS)
            {
                if (appIconTVOSSerializedObject != null)
                {
                    appIconTVOSSerializedObject.ApplyModifiedProperties();
                    appIconTVOSSerializedObject.Update();
                }
            }

            #endregion

            #endregion

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

            if (appBuildSettings == null)
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

            #region Settings Area

            #region App Info Section

            #region Text Formating

            GUIStyleState headerTextState = new GUIStyleState
            {
                textColor = Color.white
            };

            GUIStyle styleHeaderText = new GUIStyle
            {
                normal = headerTextState,
               fontSize = 15,
               fontStyle = FontStyle.Bold
            };

            var infoTextFieldsLayout = new GUILayoutOption[3];
            infoTextFieldsLayout[0] = GUILayout.ExpandWidth(true);
            infoTextFieldsLayout[1] = GUILayout.MaxWidth(settingsSectionRect.width);
            infoTextFieldsLayout[2] = GUILayout.Height(25);

            #endregion

            GUILayout.Label("App Information", styleHeaderText);

            GUILayout.Space(10);
            SerializedObject appInfoSerializedObject = new SerializedObject(appBuildSettings);
            SerializedProperty appInfoSerializedObjectProperty = appInfoSerializedObject.FindProperty("appInfo");
            appInfoSerializedObject.ApplyModifiedProperties();

            EditorGUILayout.PropertyField(appInfoSerializedObjectProperty.FindPropertyRelative("displayName"), infoTextFieldsLayout);
            EditorGUILayout.PropertyField(appInfoSerializedObjectProperty.FindPropertyRelative("companyName"), infoTextFieldsLayout);
            EditorGUILayout.PropertyField(appInfoSerializedObjectProperty.FindPropertyRelative("version"), infoTextFieldsLayout);

            #endregion

            #region App Icons

            if(BuildManager.AppIconsSupported(EditorUserBuildSettings.selectedBuildTargetGroup))
            {
                #region Icons Settings Header

                GUILayout.Space(15);

                GUILayout.Label("Application Icon", styleHeaderText);

                GUILayout.Space(10);

                SerializedObject showIconsSettingsSerializedObject = new SerializedObject(appBuildSettings);
                SerializedProperty showIconsSettingsSerializedObjectProperty = showIconsSettingsSerializedObject.FindProperty("showIconSettings");
                EditorGUILayout.PropertyField(showIconsSettingsSerializedObjectProperty, true);
                showIconsSettingsSerializedObject.ApplyModifiedProperties();

                #endregion

                if(appBuildSettings.showIconSettings == true && BuildManager.AppIconsSupported(BuildManager.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)))
                {
                    var iconLayout = new GUILayoutOption[2];
                    iconLayout[0] = GUILayout.Width(256);
                    iconLayout[1] = GUILayout.Height(100);

                    #region Standalone

                    if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Standalone)
                    {
                        appIconStandaloneSerializedObject = new SerializedObject(appBuildSettings);
                        appIconStandaloneSerializedObjectProperty = appIconStandaloneSerializedObject.FindProperty("standaloneAppIcon");
                        appIconStandaloneSerializedObject.ApplyModifiedProperties();
                        EditorGUILayout.ObjectField(appIconStandaloneSerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);
                    }

                    #endregion

                    #region Android

                    GUILayout.Space(10);

                    if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.Android)
                    {
                        var androidIconKindLayout = new GUILayoutOption[1];
                        androidIconKindLayout[0] = GUILayout.Width(256);

                        androidIconsInfoSerializedObject = new SerializedObject(appBuildSettings);
                        SerializedProperty androidIconsInfoSerializedObjectProperty = androidIconsInfoSerializedObject.FindProperty("appIconType");
                        androidIconsInfoSerializedObject.ApplyModifiedProperties();
                        EditorGUILayout.PropertyField(androidIconsInfoSerializedObjectProperty.FindPropertyRelative("androidIconKind"), androidIconKindLayout);

                        GUILayout.Space(10);

                        switch (appBuildSettings.appIconType.androidIconKind)
                        {
                            case AndroidIconKind.Adaptive:

                                appIconForegroundSerializedObject = new SerializedObject(appBuildSettings);
                                appIconForegroundSerializedObjectProperty = appIconForegroundSerializedObject.FindProperty("androidAdaptiveAppIcon");
                                EditorGUILayout.ObjectField(appIconForegroundSerializedObjectProperty.FindPropertyRelative("foreground"), typeof(Texture2D), iconLayout);

                                EditorGUILayout.Separator();

                                appIconBackgroundSerializedObject = new SerializedObject(appBuildSettings);
                                appIconBackgrounSerializedObjectProperty = appIconBackgroundSerializedObject.FindProperty("androidAdaptiveAppIcon");
                                EditorGUILayout.ObjectField(appIconBackgrounSerializedObjectProperty.FindPropertyRelative("background"), typeof(Texture2D), iconLayout);

                                break;

                            case AndroidIconKind.Round:

                                appIconRoundSerializedObject = new SerializedObject(appBuildSettings);
                                appIconRoundSerializedObjectProperty = appIconRoundSerializedObject.FindProperty("androidRoundAppIcon");
                                EditorGUILayout.ObjectField(appIconRoundSerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);

                                break;

                            case AndroidIconKind.Legacy:

                                appIconLegacySerializedObject = new SerializedObject(appBuildSettings);
                                appIconLegacySerializedObjectProperty = appIconLegacySerializedObject.FindProperty("androidLegacyAppIcon");
                                EditorGUILayout.ObjectField(appIconLegacySerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);

                                break;
                        }

                    }

                    #endregion

                    #region iOS & tvOS

                    if(EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.iOS)
                    {
                        appIconIOSSerializedObject = new SerializedObject(appBuildSettings);
                        appIconIOSSerializedObjectProperty = appIconIOSSerializedObject.FindProperty("iOSAppIcon");
                        EditorGUILayout.ObjectField(appIconIOSSerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);
                    }

                    if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.tvOS)
                    {
                        appIconTVOSSerializedObject = new SerializedObject(appBuildSettings);
                        appIconTVOSSerializedObjectProperty = appIconTVOSSerializedObject.FindProperty("tvOSAppIcon");
                        EditorGUILayout.ObjectField(appIconTVOSSerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);
                    }

                    #endregion
                }
            }

            #endregion

            #region Build Scenes Section

            GUILayout.Space(15);

            GUILayout.Label("Included Scenes", styleHeaderText);

            GUILayout.Space(10);
            SerializedObject sceneDataSerializedObject = new SerializedObject(appBuildSettings);
            sceneDataSerializedObject.CopyFromSerializedPropertyIfDifferent(appInfoSerializedObjectProperty);
            SerializedProperty sceneDataSerializedObjectProperty = sceneDataSerializedObject.FindProperty("buildScenes");
            EditorGUILayout.PropertyField(sceneDataSerializedObjectProperty, true);
            sceneDataSerializedObject.ApplyModifiedProperties();

            #endregion

            #region Configurations Section

            GUILayout.Space(15);

            GUILayout.Label("Configurations", styleHeaderText);

            GUILayout.Space(10);
            SerializedObject appConfigSerializedObject = new SerializedObject(appBuildSettings);
            SerializedProperty appConfigSerializedObjectProperty = appConfigSerializedObject.FindProperty("configurations");
            EditorGUILayout.PropertyField(appConfigSerializedObjectProperty, true);
            appConfigSerializedObject.ApplyModifiedProperties();

            #endregion

            #region Platform Specific Settings Section

            GUILayout.Space(15);

            GUILayout.Label("Platform Settings", styleHeaderText);

            if (PlatformSpecificData.GetRuntimeOS(appBuildSettings) == RuntimeOS.Console)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appBuildSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("consoleDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();
            }

            if (PlatformSpecificData.GetRuntimeOS(appBuildSettings) == RuntimeOS.Mobile)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appBuildSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("mobileDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();

                if (appBuildSettings.configurations.platform == BuildTarget.Android)
                {
                    GUILayout.Space(10);

                    SerializedObject androidConfigSerializedObject = new SerializedObject(appBuildSettings);
                    SerializedProperty androidConfigSerializedObjectProperty = androidConfigSerializedObject.FindProperty("androidBuildSettings");
                    EditorGUILayout.PropertyField(androidConfigSerializedObjectProperty, true);
                    androidConfigSerializedObject.ApplyModifiedProperties();
                }

                if (appBuildSettings.configurations.platform == BuildTarget.iOS)
                {
                    GUILayout.Space(10);

                    SerializedObject androidConfigSerializedObject = new SerializedObject(appBuildSettings);
                    SerializedProperty androidConfigSerializedObjectProperty = androidConfigSerializedObject.FindProperty("iOSBuildSettings");
                    EditorGUILayout.PropertyField(androidConfigSerializedObjectProperty, true);
                    androidConfigSerializedObject.ApplyModifiedProperties();
                }
            }

            if (PlatformSpecificData.GetRuntimeOS(appBuildSettings) == RuntimeOS.Standalone)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appBuildSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("standaloneDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();

                GUILayout.Space(10);

                SerializedObject buildSettingsObject = new SerializedObject(appBuildSettings);
                SerializedProperty buildSettingsProperty = buildSettingsObject.FindProperty("standaloneBuildSettings");
                EditorGUILayout.PropertyField(buildSettingsProperty, true);
                buildSettingsObject.ApplyModifiedProperties();    
            }

            if (PlatformSpecificData.GetRuntimeOS(appBuildSettings) == RuntimeOS.Web)
            {
                GUILayout.Space(10);

                SerializedObject appDisplayObject = new SerializedObject(appBuildSettings);
                SerializedProperty appDisplayObjectProperty = appDisplayObject.FindProperty("webDisplaySettings");
                EditorGUILayout.PropertyField(appDisplayObjectProperty, true);
                appDisplayObject.ApplyModifiedProperties();

                if (appBuildSettings.configurations.platform == BuildTarget.WebGL)
                {
                    GUILayout.Space(10);

                    SerializedObject buildSettingsObject = new SerializedObject(appBuildSettings);
                    SerializedProperty buildSettingsProperty = buildSettingsObject.FindProperty("webGLBuildSettings");
                    EditorGUILayout.PropertyField(buildSettingsProperty, true);
                    buildSettingsObject.ApplyModifiedProperties();
                }
            }

            #endregion

            #region App Builder

            GUILayout.Space(15);

            GUILayout.Label("Build Settings", styleHeaderText);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Apply Settings", GUILayout.Height(45)))
            {
                BuildManager.ApplyAppSettings(appBuildSettings.ToSerializable());
            }

            if (Directory.Exists(appBuildSettings.configurations.targetBuildDirectory) == true)
            {
                GUILayout.Space(2);

                if (GUILayout.Button("Open Build Folder", GUILayout.Height(45)))
                {
                    Storage.Directory.OpenFolder(BuildManager.GetBuildSettings(BuildManager.GetDefaultStorageInfo()).configurations.targetBuildDirectory);
                }
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (GUILayout.Button("Build App", GUILayout.Height(60)))
            {
                BuildManager.Build();
            }

            GUILayout.Space(10);

            SerializedObject buildSerializedObject = new SerializedObject(appBuildSettings);
            SerializedProperty buildSerializedObjectProperty = buildSerializedObject.FindProperty("buildAndRun");
            EditorGUILayout.PropertyField(buildSerializedObjectProperty, true);
            buildSerializedObject.ApplyModifiedProperties();

            #endregion

            #endregion

            EditorGUILayout.EndScrollView();

            #endregion

            GUILayout.EndArea();

            #endregion
        }

        #endregion
    }
}
