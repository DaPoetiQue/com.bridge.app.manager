using JetBrains.Annotations;
using System;
// using Bridge.Core.App.Data.Storage;
using UnityEngine;
using UnityEditor;

namespace Bridge.Core.App.Manager
{
    #region App Info

    public enum RuntimePlatform
    {
        Android, iOS
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

        [Space(5)]
        public AndroidBuildSettings androidSettings;
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
        public string scene;

        [Space(5)]
        public UIOrientation allowedOrientation;

        [Space(5)]
        public BuildTarget platform;

        [Space(5)]
        public bool allowDebugging;

        [Space(5)]
        public bool developmentBuild;

        [HideInInspector]
        public string buildLocation; 
    }

    [Serializable]
    public struct AndroidBuildSettings
    {
        [Space(5)]
        public AndroidPreferredInstallLocation installLocation;

        [Space(5)]
        public AndroidSdkVersions SdkVersion;

        [Space(5)]
        public bool buildAppBundle;
    }
  
    #endregion
}
