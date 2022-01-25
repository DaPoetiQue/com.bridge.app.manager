using Bridge.Core.App.Data.Storage;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Bridge.Core.UnityEditor.App.Manager
{
    #region Enum Data Types

    /// <summary>
    /// This enum contains a list of command types.
    /// </summary>
    public enum CommandType
    {
        None,
        Build,
        Compile,
        Dispose,
        Log,
        ChangeToBuildDirectory,
        ToProjectDirectory,
        ToTemporaryDirectory,
        OpenProjectBuildFolder
    }

    #endregion

    #region Struct Data Types

    /// <summary>
    /// This struct contains a pre-defined batch commands data.
    /// </summary>
    public struct CommandData
    {   
        public string changeToBuildDirectoryCommand;
        public string changeToProjectDirectoryCommand;
        public string changeToTemporaryProjectDirectoryCommand;
        public string buildCommand;
        public string logEditorCommand;
        public string openProjectBuildFolder;
    }

    /// <summary>
    /// This struct contains batch compiler command window logs.
    /// </summary>
    public struct BatchCompilerCommandPromtLogs
    {

    }

    #endregion

    #region Class Data

    /// <summary>
    /// This struct contains information for creating a batch file
    /// </summary>
    [Serializable]
    public class BatchFileData
    {
        // File Data
        public string batchFile;

        // Path Data
        public string projectRootPath;
        public string fileRootPath;
        public string fullbatchFilePath;
    }

    /// <summary>
    /// This class contains information for creating a build compiler command for a batch file.
    /// </summary>
    [Serializable]
    public class BuildCompiler
    {
        #region Property Fields

        public string echoOffAttribute;
        public string echoPrepareBuildAttribute;
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
            return $"{echoOffAttribute} \n " +
                   $"{echoPrepareBuildAttribute} \n " +
                   $"{removeDirectoryCommand} \n " +
                   $"{echoCopy} \n " +
                   $"{copyContentCommand} \n " +
                   $"{changeToBuildDirectory} \n " +
                   $"{echoCompile} \n " +
                   $"{compilerBuildCommand} \n " +
                   $"{pause} ";
        }
    }

    /// <summary>
    /// This class contains information for creating a build script compiler command for a batch file.
    /// </summary>
    [Serializable]
    public class BuildScriptCompiler
    {
        #region Property Fields

        public string echoOff;
        public string echoInitializeBuild;
        public string editorLogBuildStartedCommand;
        public string startBuildCommand;
        public string moveBuildCommand;
        public string openBuildFolderPathCommand;
        public string changeToProjectDirectory;
        public string editorLogBuildEndedCommand;
        public string echoBuildCompleted;
        public string pause;

        #endregion

        public override string ToString()
        {
            return $"{echoOff} \n  " +
                   $"{echoInitializeBuild} \n  " +
                   $"{editorLogBuildStartedCommand} \n " +
                   $"{startBuildCommand} \n " +
                   $"{moveBuildCommand} \n " +
                   $"{openBuildFolderPathCommand} \n " +
                   $"{changeToProjectDirectory} \n " +
                   $"{editorLogBuildEndedCommand} \n " +
                   $"{echoBuildCompleted} \n " +
                   $"{pause}";
        }
    }

    /// <summary>
    /// This class contains information for creating a build script disposer command for a batch file.
    /// </summary>
    [Serializable]
    public class BuildScriptDisposer
    {
        #region Property Fields

        public string echoOffAttribute;
        public string echoCleaningBuildProjectAttribute;
        public string editorLogCleanStarted;
        public string cleanBuildPathCommand;
        public string editorLogCleanEnded;

        #endregion

        public override string ToString()
        {
            return $"{echoOffAttribute} \n  " +
                   $"{echoCleaningBuildProjectAttribute} \n  " +
                   $"{editorLogCleanStarted} \n  " +
                   $"{cleanBuildPathCommand} \n  " +
                   $"{editorLogCleanEnded}";
        }
    }

    /// <summary>
    /// This static class contains project info data.
    /// </summary>
    [Serializable]
    public static class ProjectInfoData
    {
        public static string buildMethodName = "AppBuildConfig.BuildApp";
        public static string unityVersion = Application.unityVersion;
    }

    /// <summary>
    /// Executable batch commands.
    /// </summary>
    public static class BatchCommands
    {
        /// <summary>
        /// 
        /// </summary>
        public struct BatchFileAttribute        
        {
            public string echoOff;
            public string pause;

            public string echoCopy;
            public string echoBuildCompile;
            public string echoInitializeBuild;
            public string prepareBuild;
            public string echoBuildCompleted;

            public string echoCleaningBuildProjectAttribute;
        }

        public static BatchFileAttribute GetBatchFileAttribute()
        {
            BatchFileAttribute fileAttribute = new BatchFileAttribute
            {
                echoOff = "@echo off",
                prepareBuild = "echo Preparing Build...",
                echoInitializeBuild = "echo Initializing Project Build...",
                echoBuildCompleted = "echo Build Completed...",
                echoCopy = "echo Copying Build Files...",
                echoBuildCompile = "",
                echoCleaningBuildProjectAttribute = "echo Cleaning Build Project...",
                pause = "@pause"
            };

            return fileAttribute;
        }

        public static string GetUnityEditorLogFile()
        {
            return $"Type {Storage.Directory.GetUnityEditorProjectInfoData().unityEditorLogFilePath}";
        }

        public static string BuildProject()
        {
            return Storage.Directory.GetUnityEditorProjectInfoData().editorApplicationRelativeDirectory + $" -quit -batchMode -projectPath .. -executeMethod {ProjectInfoData.buildMethodName}";
        }

        public static string OpenFolderLocation(string directory)
        {
            return $"Start \"\" \"{directory}\"";
        }

        public static string CopyFiles(string fromDirectory, string toDirectory, string excludeFiles = null, string excludeFolders = null)
        {
            return $"robocopy {fromDirectory} {toDirectory} {excludeFolders} {excludeFiles}";
        }

        public static string CopyFilesAndSubFolders(string fromDirectory, string toDirectory, string excludeFiles = null, string excludeFolders = null)
        {
            return $"robocopy /S {fromDirectory} {toDirectory} {excludeFolders} {excludeFiles}";
        }

        public static string CopyFilesAndSubFoldersAndRemoveSource(string fromDirectory, string toDirectory, string excludeFiles = null, string excludeFolders = null)
        {
            return $"robocopy /MOVE /S /E {fromDirectory} {toDirectory} {excludeFolders} {excludeFiles}";
        }

        public static string Move(string fromDirectory, string toDirectory)
        {
            return $"move {fromDirectory}\\*.* {toDirectory.Replace("/", "\\")}";
        }

        public static string RemoveDirectory(string directory)
        {
            return $"rmdir /s/q {directory.Replace("/", "\\")}";
        }

        public static string ChangeToDirectory(string targetDirectory)
        {
            return $"chdir /d {targetDirectory.Replace(" ", string.Empty).Replace("/", "\\")}";
        }

        public static string UnityExcludedFiles()
        {
            string builds = "*.apk *.exe *.aab *.unitypackage";
            string autoGenerated = "*.csproj *.unityproj *.sln *.suo *.tmp *.user *.userprefs *.pidb *.booproj *.svd *.pdb *.mdb *.opendb *.VC.db";
            string excludeFiles = $"/xf {autoGenerated} {builds}";

            return excludeFiles;
        }

        public static string UnityExcludedFolders()
        {
            return "/E /xd Library Temp Build Builds Obj Logs UserSettings MemoryCaptures";
        }

        public static void RunBatchCommand(string command)
        {
            Process.Start(command);
        }
    }

    #endregion
}
