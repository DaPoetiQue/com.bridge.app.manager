using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.IO;
using Bridge.Core.App.Manager;
using Bridge.Core.UnityCustomEditor.Debugger;
using Bridge.Core.App.Data.Storage;
using System;

namespace Bridge.Core.UnityCustomEditor.App.Manager
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
            windowInstance.minSize = new Vector2(400, 400);
            windowInstance.maxSize = new Vector2(600, 600);
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

        private static BuildProfile profile;
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

        #region Splash Screen Data

        SerializedObject splashScreenSerializedObject;
        SerializedProperty splashScreenSerializedObjectProperty;

        #endregion

        #region Standalone Icon Data

        SerializedObject appIconStandaloneSerializedObject;
        SerializedProperty appIconStandaloneSerializedObjectProperty;

        #endregion

        #region Build Settings Data

        ReorderableList reordarableSceneList;

        #endregion

        #region Splash Screen Data

        ReorderableList reordarableSplashScreenList;
        SerializedObject splashScreenBackgroundSerializedObject;
        SerializedProperty splashScreenBackgroundSerializedObjectProperty;

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
            settingsHeaderStyle.fontSize = 10;
            settingsHeaderStyle.fontStyle = FontStyle.Bold;
            settingsHeaderStyle.padding.top = 20;
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
            profile = CreateInstance<BuildProfile>();
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
        public void OnWindowUpdates()
        {
            if (window == null)
            {
                appBuildSettings = BuildManager.GetCurrentBuildSettings();

                window = GetWindow<BuildManagerWindow>();
                window.wantsMouseEnterLeaveWindow = false;
                window.Repaint();
                // DebugConsole.Log(Debug.LogLevel.Debug, this, "Window Refreshed!.");
            }

            #region App Icons Settings Update

            var buildTargetGroup = BuildManager.GetBuildTargetGroup(appBuildSettings.configurations.platform);

            #region Standalone Icon

            if (buildTargetGroup == BuildTargetGroup.Standalone)
            { 
                if(appIconStandaloneSerializedObject != null)
                {
                    appIconStandaloneSerializedObject.ApplyModifiedProperties();
                    appIconStandaloneSerializedObject.Update();
                }
            }

            #endregion

            #region Android Icons

            if (buildTargetGroup == BuildTargetGroup.Android)
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

            if (buildTargetGroup == BuildTargetGroup.iOS)
            {
                if(appIconIOSSerializedObject != null)
                {
                    appIconIOSSerializedObject.ApplyModifiedProperties();
                    appIconIOSSerializedObject.Update();
                }
            }

            if(buildTargetGroup == BuildTargetGroup.tvOS)
            {
                if (appIconTVOSSerializedObject != null)
                {
                    appIconTVOSSerializedObject.ApplyModifiedProperties();
                    appIconTVOSSerializedObject.Update();
                }
            }

            #endregion

            #endregion

            #region Splash Screens

            if(splashScreenSerializedObject != null)
            {
                splashScreenSerializedObject.ApplyModifiedProperties();
                splashScreenSerializedObject.Update();
            }

            if(splashScreenBackgroundSerializedObject != null)
            {
                splashScreenBackgroundSerializedObject.ApplyModifiedProperties();
                splashScreenBackgroundSerializedObject.Update();
            }

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
            headerSectionRect.height = 50;

            if(headerSectionTexture == null)
            {
                InitializeTextures();
            }

            GUI.DrawTexture(headerSectionRect, headerSectionTexture);

            #endregion

            #region Icon

            iconRect.width = 50;
            iconRect.height = 50;
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
            #region Menu Content Area

            GUILayout.BeginArea(settingsSectionRect);

            #region Menu Content and Style Update

            var defaultGUIColor = GUI.backgroundColor;

            if (appBuildSettings == null)
            {
                InitializeContentData();
            }

            GUIStyle style = new GUIStyle();
            style.padding = new RectOffset(10, 10, 25, 25);

            var layout = new GUILayoutOption[2];
            layout[0] = GUILayout.Width(settingsSectionRect.width);
            layout[1] = GUILayout.ExpandHeight(true);


            #region Text Formating

            GUIStyleState headerTextState = new GUIStyleState
            {
                textColor = Color.white
            };

            GUIStyle styleHeaderText = new GUIStyle
            {
                normal = headerTextState,
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            var infoTextFieldsLayout = new GUILayoutOption[3];
            infoTextFieldsLayout[0] = GUILayout.ExpandWidth(true);
            infoTextFieldsLayout[1] = GUILayout.MaxWidth(settingsSectionRect.width);
            infoTextFieldsLayout[2] = GUILayout.Height(25);

            GUILayout.Space(15);

            #endregion

            #endregion

            #region Scroll Area

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, style, layout);

            #region Build Profile

            GUILayout.Label("Custom Build Profile", styleHeaderText);

            GUILayout.Space(10);

            SerializedObject buildProfileSerializedObject = new SerializedObject(profile);
            SerializedProperty buildProfileSerializedObjectProperty = buildProfileSerializedObject.FindProperty("profile");
            EditorGUILayout.PropertyField(buildProfileSerializedObjectProperty, true);
            buildProfileSerializedObject.ApplyModifiedProperties();

            #endregion

            #region App Info Section

            GUILayout.Space(20);
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

            if(BuildManager.AppIconsSupported(appBuildSettings.configurations.platform))
            {
                #region Icons Settings Header

                GUILayout.Space(25);

                GUILayout.Label("Application Icon", styleHeaderText);

                GUILayout.Space(10);

                SerializedObject showIconsSettingsSerializedObject = new SerializedObject(appBuildSettings);
                SerializedProperty showIconsSettingsSerializedObjectProperty = showIconsSettingsSerializedObject.FindProperty("overideIconSettings");
                EditorGUILayout.PropertyField(showIconsSettingsSerializedObjectProperty, true);
                showIconsSettingsSerializedObject.ApplyModifiedProperties();

                #endregion

                var buildTargetGroup = BuildManager.GetBuildTargetGroup(appBuildSettings.configurations.platform);

                if (appBuildSettings.overideIconSettings == true && BuildManager.AppIconsSupported(buildTargetGroup))
                {
                    var iconLayout = new GUILayoutOption[2];
                    iconLayout[0] = GUILayout.Width(256);
                    iconLayout[1] = GUILayout.Height(100);

                    var iconKindLayout = new GUILayoutOption[1];
                    iconKindLayout[0] = GUILayout.Width(256);

                    #region Standalone

                    if (buildTargetGroup == BuildTargetGroup.Standalone)
                    {
                        GUILayout.Space(10);

                        appIconStandaloneSerializedObject = new SerializedObject(appBuildSettings);
                        appIconStandaloneSerializedObjectProperty = appIconStandaloneSerializedObject.FindProperty("standaloneAppIcon");
                        appIconStandaloneSerializedObject.ApplyModifiedProperties();
                        EditorGUILayout.ObjectField(appIconStandaloneSerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);

                        GUILayout.Space(10);

                        SerializedObject appIconKindStandaloneSerializedObject = new SerializedObject(appBuildSettings);
                        SerializedProperty appIconKindStandaloneSerializedObjectProperty = appIconKindStandaloneSerializedObject.FindProperty("appIconKind");
                        EditorGUILayout.PropertyField(appIconKindStandaloneSerializedObjectProperty, true);
                        appIconKindStandaloneSerializedObject.ApplyModifiedProperties();
                    }

                    #endregion

                    #region Android

                    if (buildTargetGroup == BuildTargetGroup.Android)
                    {
                        GUILayout.Space(10);

                        androidIconsInfoSerializedObject = new SerializedObject(appBuildSettings);
                        SerializedProperty androidIconsInfoSerializedObjectProperty = androidIconsInfoSerializedObject.FindProperty("androidIconKind");
                        androidIconsInfoSerializedObject.ApplyModifiedProperties();
                        EditorGUILayout.PropertyField(androidIconsInfoSerializedObjectProperty.FindPropertyRelative("appIconKind"), iconKindLayout);

                        GUILayout.Space(10);

                        switch (appBuildSettings.androidIconKind.appIconKind)
                        {
                            case AndroidIconKind.Adaptive:

                                appIconBackgroundSerializedObject = new SerializedObject(appBuildSettings);
                                appIconBackgrounSerializedObjectProperty = appIconBackgroundSerializedObject.FindProperty("androidAdaptiveAppIcon");
                                EditorGUILayout.ObjectField(appIconBackgrounSerializedObjectProperty.FindPropertyRelative("background"), typeof(Texture2D), iconLayout);

                                EditorGUILayout.Separator();

                                appIconForegroundSerializedObject = new SerializedObject(appBuildSettings);
                                appIconForegroundSerializedObjectProperty = appIconForegroundSerializedObject.FindProperty("androidAdaptiveAppIcon");
                                EditorGUILayout.ObjectField(appIconForegroundSerializedObjectProperty.FindPropertyRelative("foreground"), typeof(Texture2D), iconLayout);

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

                    if(buildTargetGroup == BuildTargetGroup.iOS)
                    {
                        GUILayout.Space(10);

                        appIconIOSSerializedObject = new SerializedObject(appBuildSettings);
                        appIconIOSSerializedObjectProperty = appIconIOSSerializedObject.FindProperty("iOSAppIcon");
                        EditorGUILayout.ObjectField(appIconIOSSerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);
                    }

                    if (EditorUserBuildSettings.selectedBuildTargetGroup == BuildTargetGroup.tvOS)
                    {
                        GUILayout.Space(10);

                        appIconTVOSSerializedObject = new SerializedObject(appBuildSettings);
                        appIconTVOSSerializedObjectProperty = appIconTVOSSerializedObject.FindProperty("tvOSAppIcon");
                        EditorGUILayout.ObjectField(appIconTVOSSerializedObjectProperty.FindPropertyRelative("defaultIcon"), typeof(Texture2D), iconLayout);
                    }

                    #endregion
                }
            }

            #endregion

            #region Splash Screens

            GUILayout.Space(35);

            GUILayout.Label("Splash Screen Settings", styleHeaderText);


            GUILayout.Space(15);

            SerializedObject showSplashScreenSettingsSerializedObject = new SerializedObject(appBuildSettings);
            SerializedProperty showSplashScreenSettingsSerializedObjectProperty = showSplashScreenSettingsSerializedObject.FindProperty("overideSplashScreen");
            EditorGUILayout.PropertyField(showSplashScreenSettingsSerializedObjectProperty, true);
            showSplashScreenSettingsSerializedObject.ApplyModifiedProperties();

            if (appBuildSettings.overideSplashScreen == true)
            {

                GUILayout.Space(15);

                var splashScreenPreviewLayout = new GUILayoutOption[2];
                splashScreenPreviewLayout[0] = GUILayout.Width(256);
                splashScreenPreviewLayout[1] = GUILayout.Height(100);

                splashScreenSerializedObject = new SerializedObject(appBuildSettings);
                splashScreenSerializedObjectProperty = splashScreenSerializedObject.FindProperty("splashScreenSettings").FindPropertyRelative("mainSplashScreen");
                splashScreenSerializedObject.ApplyModifiedProperties();
                splashScreenSerializedObject.Update();
                EditorGUILayout.ObjectField(splashScreenSerializedObjectProperty.FindPropertyRelative("screen"), typeof(Sprite), splashScreenPreviewLayout);

                GUILayout.Space(15);

                GUILayoutOption[] layoutOpt = new GUILayoutOption[2];
                layoutOpt[0] = GUILayout.Width(385);
                layoutOpt[1] = GUILayout.Height(256);

                GUIContent content = new GUIContent();
                content.text = "Splash Screen Preview";
                content.image = new Texture2D(1, 1);

                GUIStyle splashPreviewStyle = new GUIStyle();

                splashPreviewStyle.alignment = TextAnchor.UpperCenter;
                splashPreviewStyle.fontStyle = FontStyle.Bold;
                splashPreviewStyle.fontSize = 15;
                splashPreviewStyle.normal.textColor = Color.white;
                splashPreviewStyle.padding = new RectOffset(0, 0, 25, 25);

                SerializedObject splashScreenDataSerializedObject = new SerializedObject(appBuildSettings);
                SerializedProperty splashScreeDataSerializedObjectProperty = splashScreenDataSerializedObject.FindProperty("splashScreenSettings").FindPropertyRelative("mainSplashScreen");
                splashScreenDataSerializedObject.ApplyModifiedProperties();

                EditorGUILayout.PropertyField(splashScreeDataSerializedObjectProperty.FindPropertyRelative("duration"), infoTextFieldsLayout);

                GUILayout.Space(10);

                SerializedObject splashScreenListSerializedObject = new SerializedObject(appBuildSettings);
                splashScreenListSerializedObject.CopyFromSerializedPropertyIfDifferent(appInfoSerializedObjectProperty);
                SerializedProperty splashScreensSerializedObjectProperty = splashScreenListSerializedObject.FindProperty("splashScreenSettings").FindPropertyRelative("screens");

                if(appBuildSettings != null)
                {
                    GetSplashScreenItemReorderableList(splashScreensSerializedObjectProperty).DoLayoutList();
                    splashScreenListSerializedObject.ApplyModifiedProperties();
                    splashScreenListSerializedObject.Update();
                }


                GUILayout.Space(15);

                splashScreenBackgroundSerializedObject = new SerializedObject(appBuildSettings);
                splashScreenBackgroundSerializedObjectProperty = splashScreenBackgroundSerializedObject.FindProperty("splashScreenSettings");
                EditorGUILayout.ObjectField(splashScreenBackgroundSerializedObjectProperty.FindPropertyRelative("background"), typeof(Sprite), splashScreenPreviewLayout);
                splashScreenBackgroundSerializedObject.ApplyModifiedProperties();
                splashScreenBackgroundSerializedObject.Update();

                GUILayout.Space(20);

                GUIContent colorField = new GUIContent();
                colorField.text = "Background Color";

                appBuildSettings.splashScreenSettings.backgroundColor = EditorGUILayout.ColorField(colorField, appBuildSettings.splashScreenSettings.backgroundColor, true, true, false);

                GUILayout.Space(10);

                SerializedObject showUnityLogoSerializedObject = new SerializedObject(appBuildSettings);
                SerializedProperty showUnityLogoSerializedObjectProperty = showUnityLogoSerializedObject.FindProperty("splashScreenSettings").FindPropertyRelative("showUnityLogo");
                EditorGUILayout.PropertyField(showUnityLogoSerializedObjectProperty, true);
                showUnityLogoSerializedObject.ApplyModifiedProperties();
                showUnityLogoSerializedObject.Update();

                GUILayout.Space(5);

                if (appBuildSettings != null)
                {
                    appBuildSettings.splashScreenSettings.logoDrawMode = (PlayerSettings.SplashScreen.DrawMode)EditorGUILayout.EnumPopup("Logo Draw Mode", appBuildSettings.splashScreenSettings.logoDrawMode);
                    GUILayout.Space(5);

                    appBuildSettings.splashScreenSettings.animationMode = (PlayerSettings.SplashScreen.AnimationMode)EditorGUILayout.EnumPopup("Animation Mode", appBuildSettings.splashScreenSettings.animationMode);
                    GUILayout.Space(5);

                    appBuildSettings.splashScreenSettings.unityLogoStyle = (PlayerSettings.SplashScreen.UnityLogoStyle)EditorGUILayout.EnumPopup("Unity Logo Style", appBuildSettings.splashScreenSettings.unityLogoStyle);
                }

                GUILayout.Space(10);

                SerializedObject showSplashScreenSerializedObject = new SerializedObject(appBuildSettings);
                SerializedProperty showSplashScreenSerializedObjectProperty = showSplashScreenSerializedObject.FindProperty("splashScreenSettings").FindPropertyRelative("showSplashScreen");
                EditorGUILayout.PropertyField(showSplashScreenSerializedObjectProperty, true);
                showSplashScreenSerializedObject.ApplyModifiedProperties();
                showSplashScreenSerializedObject.Update();
            }

            #endregion

            #region Build Scenes Section

            GUILayout.Space(20);

            GUILayout.Label("Included Scenes", styleHeaderText);

            GUILayout.Space(15);

            SerializedObject sceneDataSerializedObject = new SerializedObject(appBuildSettings);
            sceneDataSerializedObject.CopyFromSerializedPropertyIfDifferent(appInfoSerializedObjectProperty);
            SerializedProperty sceneDataSerializedObjectProperty = sceneDataSerializedObject.FindProperty("buildScenes");

            GetBuildSceneItemReorderableList(sceneDataSerializedObjectProperty).DoLayoutList();
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

            GUILayout.Space(15);

            #endregion


            EditorGUILayout.EndScrollView();

            #endregion

            #region Footer Menu

            // Buttons For Applying/Revrting Changes.
            #region Apply & Revert

            GUILayout.Space(15);

            BuildSettingsData currentBuildSettingsState = appBuildSettings.ToSerializable();
            BuildSettingsData defaultBuildSettingsState = BuildManager.GetBuildSettings(BuildManager.GetDefaultStorageInfo());

            bool settingsNotChanged = currentBuildSettingsState.Equals(defaultBuildSettingsState);


            if (settingsNotChanged == false)
            {
                EditorGUILayout.BeginHorizontal();

                GUI.backgroundColor = Color.grey;

                if (GUILayout.Button("Apply Changes", GUILayout.Height(30)))
                {
                    window.SaveChanges();

                    BuildManager.ApplyAppSettings(appBuildSettings.ToSerializable());
                }

                GUI.backgroundColor = Color.gray;

                if (GUILayout.Button("Revert Changes", GUILayout.Height(30)))
                {
                    window.SaveChanges();

                    appBuildSettings = defaultBuildSettingsState.ToInstance();
                }

                EditorGUILayout.EndHorizontal();
            }

            #endregion

            // Buttons For Opening Build Settings / Folder.
            #region Open Folders

            // GUILayout.Label("Build Settings", styleHeaderText);

            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = Color.grey;

            if (GUILayout.Button("Open Builds Settings", GUILayout.Height(30)))
            {
                GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"));
            }

            if (GUILayout.Button("Open Player Settings", GUILayout.Height(30)))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(5);

            if (Directory.Exists(appBuildSettings.configurations.targetBuildDirectory) == true)
            {

                if (GUILayout.Button("Open Build Folder", GUILayout.Height(30)))
                {
                    Storage.Directory.OpenFolder(BuildManager.GetBuildSettings(BuildManager.GetDefaultStorageInfo()).configurations.targetBuildDirectory);
                }
            }

            GUILayout.Space(5);

            #endregion

            // Button For Building Application.
            #region Build App

            GUI.backgroundColor = Color.green;
            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.fontSize = 15;
            buttonStyle.fontStyle = FontStyle.Bold;

            if (GUILayout.Button("Build App", buttonStyle, GUILayout.Height(80)))
            {
                BuildManager.Build();
            }

            GUI.backgroundColor = defaultGUIColor;

            GUILayout.Space(10);

            SerializedObject buildSerializedObject = new SerializedObject(appBuildSettings);
            SerializedProperty buildSerializedObjectProperty = buildSerializedObject.FindProperty("buildAndRun");
            EditorGUILayout.PropertyField(buildSerializedObjectProperty, true);
            buildSerializedObject.ApplyModifiedProperties();

            GUILayout.Space(15);

            #endregion

            #endregion

            GUILayout.EndArea();

            #endregion
        }

        /// <summary>
        /// This function returns a reoderable list for splash screen items.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private ReorderableList GetSplashScreenItemReorderableList(SerializedProperty property)
        {
            if (reordarableSplashScreenList == null)
            {
                reordarableSplashScreenList = new ReorderableList(property.serializedObject, property, true, true, true, true);

                reordarableSplashScreenList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
                {
                    GUIContent content = new GUIContent();
                    content.tooltip = "Assign Image To Use As Slash Screen.";
                    content.text = "Duration";

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y - 3, rect.width - 185, 25), property.GetArrayElementAtIndex(index).FindPropertyRelative("screen"), GUIContent.none);
                    EditorGUI.LabelField(new Rect(rect.width - 110, rect.y, 150, EditorGUIUtility.singleLineHeight), content);
                    EditorGUI.PropertyField(new Rect(rect.width - 8, rect.y - 3, 25, 23), property.GetArrayElementAtIndex(index).FindPropertyRelative("duration"), GUIContent.none);

                    property.serializedObject.ApplyModifiedProperties();
                };

                reordarableSplashScreenList.drawHeaderCallback = (Rect headerRect) =>
                {
                    string name = "Additional Screen List";
                    EditorGUI.LabelField(headerRect, name);
                };
            }

            return reordarableSplashScreenList;
        }

        /// <summary>
        /// This function returns a reoderable list for build scene items.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private ReorderableList GetBuildSceneItemReorderableList(SerializedProperty property)
        {
            if(reordarableSceneList == null)
            {
                reordarableSceneList = new ReorderableList(property.serializedObject, property, true, true, true, true);

                reordarableSceneList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => 
                {
                    GUIContent content = new GUIContent();
                    content.tooltip = "Assign Scenes To Include In Build.";
                    content.text = (string.IsNullOrEmpty(property.GetArrayElementAtIndex(index).FindPropertyRelative("scene").objectReferenceValue?.name))? "New Scene" : property.GetArrayElementAtIndex(index).FindPropertyRelative("scene").objectReferenceValue?.name;

                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 150, EditorGUIUtility.singleLineHeight), content);
                    EditorGUI.PropertyField(new Rect(rect.x + 125, rect.y - 3, rect.width - 185, 25), property.GetArrayElementAtIndex(index).FindPropertyRelative("scene"), GUIContent.none);
                    EditorGUI.PropertyField(new Rect(rect.width - 5, rect.y, 100, EditorGUIUtility.singleLineHeight), property.GetArrayElementAtIndex(index).FindPropertyRelative("isActive"), GUIContent.none);

                    property.serializedObject.ApplyModifiedProperties();
                };

                reordarableSceneList.drawHeaderCallback = (Rect headerRect) => 
                {
                    string name = "Build Scenes List";
                    EditorGUI.LabelField(headerRect, name);
                };
            }

            return reordarableSceneList;
        }

        #region Unity Editor Event

        public static void OnBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            // 

            //if (window == null)
            //{
            //    appBuildSettings = BuildManager.GetCurrentBuildSettings();

            //    window = GetWindow<BuildManagerWindow>();

            //    EditorGUI.BeginChangeCheck();
            //    window.Repaint();
            //    EditorGUI.EndChangeCheck();
            //    DebugConsole.Log(Debug.LogLevel.Debug, "Window Refreshed!.");
            //    window.Focus();
            //}
            //else
            //{
            //    appBuildSettings = BuildManager.GetCurrentBuildSettings();

            //    window.Focus();
            //    window.Repaint();
            //    DebugConsole.Log(Debug.LogLevel.Debug, "Window Refreshed!.");
            //}


        }

        #endregion

        #endregion
    }
}
