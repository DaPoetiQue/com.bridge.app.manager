using Bridge.Core.App.Data.Storage;
using System;
using System.Diagnostics;
using UnityEngine;

namespace Bridge.Core.UnityCustomEditor.App.Manager
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
        public string echoCopy;
        public string copyContentCommand;
        public string changeToBuildDirectory;
        public string echoCompile;
        public string compilerBuildCommand;

        #endregion

        public override string ToString()
        {
            return $"{echoOffAttribute}\n" +
                   $"{echoCopy}\n" +
                   $"{copyContentCommand}\n" +
                   $"{changeToBuildDirectory}\n" +
                   $"{echoCompile}\n" +
                   $"{compilerBuildCommand}";
        }
    }

    /// <summary>
    /// This class contains information for creating a build script compiler command for a batch file.
    /// </summary>
    [Serializable]
    public class BuildScriptCompiler
    {
        #region Property Fields

        public string echoOffAttribute;
        public string echoInitializeBuild;
        public string editorLogBuildStartedCommand;
        public string startBuildCommand;
        public string moveBuildCommand;
        public string changeToProjectDirectory;
        public string editorLogBuildEndedCommand;
        public string cleanBuildCommand;

        #endregion

        public override string ToString()
        {
            return $"{echoOffAttribute}\n" +
                   $"{echoInitializeBuild}\n" +
                   $"{editorLogBuildStartedCommand}\n" +
                   $"{startBuildCommand}\n" +
                   $"{moveBuildCommand}\n" +
                   $"{changeToProjectDirectory}\n" +
                   $"{editorLogBuildEndedCommand}\n" +
                   $"{cleanBuildCommand}";
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
        public string echoBuildCompleted;
        public string openBuildFolderPathCommand;
        public string pause;

        #endregion

        public override string ToString()
        {
            return $"{echoOffAttribute}\n" +
                   $"{echoCleaningBuildProjectAttribute}\n" +
                   $"{editorLogCleanStarted}\n" +
                   $"{cleanBuildPathCommand}\n" +
                   $"{editorLogCleanEnded}\n" +
                   $"{echoBuildCompleted}\n" +
                   $"{openBuildFolderPathCommand}\n" +
                   $"{pause}";
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
        #region Batch File Data 

        /// <summary>
        /// A struct data list of pre-defined attributes used to compile a batch file.
        /// </summary>
        public struct BatchFileAttribute        
        {
            public string echoOff;
            public string pause;
            public string echoCopy;
            public string echoBuildCompile;
            public string echoInitializeBuild;
            public string echoBuildCompleted;
            public string echoCleaningBuildProjectAttribute;
        }

        /// <summary>
        /// This function returns a list of pre-defined attributes used to compile a batch file.
        /// </summary>
        /// <returns>A struct data list of pre-defined attributes</returns>
        public static BatchFileAttribute GetBatchFileAttribute()
        {
            BatchFileAttribute fileAttribute = new BatchFileAttribute
            {
                echoOff = "@echo off",
                echoInitializeBuild = "echo Initializing Project Build...",
                echoBuildCompleted = "echo Build Completed...",
                echoCopy = "echo Copying Build Files...",
                echoBuildCompile = "echo Build Started...",
                echoCleaningBuildProjectAttribute = "echo Cleaning Build Project...",
                pause = "@pause"
            };

            return fileAttribute;
        }

        #endregion

        #region Batch File Commands 

        /// <summary>
        /// This batch command function is used for executing a headless Unity build.
        /// </summary>
        /// <returns>Batch command for building a Unity project.</returns>
        public static string BuildProject()
        {
            return Storage.Directory.GetUnityEditorProjectInfoData().editorApplicationRelativeDirectory + $" -quit -batchMode -projectPath .. -executeMethod {ProjectInfoData.buildMethodName}";
        }

        /// <summary>
        /// This batch command function is used for accessing Unity's Editor log file.
        /// </summary>
        /// <returns>Batch command for accesing/getting the Unity editor log file.</returns>
        public static string GetFileFromDirectory(string directory)
        {
            return $"Type {directory}";
        }

        /// <summary>
        /// This batch command function is used for opening a folder from a given directory/location.
        /// </summary>
        /// <param name="directory">Target directory of the folder to be opened.</param>
        /// <returns>Batch command for opening folder from a source directory.</returns>
        public static string OpenFolderLocation(string directory)
        {
            return $"Start \"\" {directory}";
        }

        /// <summary>
        /// This batch command function is used for copying files from a given directory to a target directory.
        /// Optional parameters are used to set a list of excluded files and folders to be copied.
        /// </summary>
        /// <param name="fromDirectory">A source directory to copy the files/folders from.</param>
        /// <param name="toDirectory">A target directory to copy the files/folders to.</param>
        /// <param name="excludedFileExtensions">A list of file extensions to be excluded during copying.</param>
        /// <param name="excludedFolders">List of folders to be excluded during copying.</param>
        /// <returns>Batch command for copying files from a source directory to the target directory with options to exclude files and folders</returns>
        public static string CopyFiles(string fromDirectory, string toDirectory, string excludedFileExtensions = null, string excludedFolders = null)
        {
            return $"robocopy {fromDirectory} {toDirectory} {excludedFolders} {excludedFileExtensions}";
        }

        /// <summary>
        /// This batch command function is used for copying files and sub-folders from a given directory to a target directory.
        /// Optional parameters are used to set a list of excluded files and folders to be copied.
        /// </summary>
        /// <param name="fromDirectory">A source directory to copy the files/folders from.</param>
        /// <param name="toDirectory">A target directory to copy the files/folders to.</param>
        /// <param name="excludedFileExtensions">A list of file extensions to be excluded during copying.</param>
        /// <param name="excludedFolders">List of folders to be excluded during copying.</param>
        /// <returns>Batch command for copying files and sub-folders from a source directory to the target directory with an options to exclude files and folders.</returns>
        public static string CopyFilesAndSubFolders(string fromDirectory, string toDirectory, string excludedFileExtensions = null, string excludedFolders = null)
        {
            return $"robocopy /S {fromDirectory} {toDirectory} {excludedFolders} {excludedFileExtensions}";
        }

        /// <summary>
        /// This batch command function is used for copying files and sub-folders from a given directory to a target directory and remove the source files/folders on completion.
        /// Optional parameters are used to set a list of excluded files and folders to be copied.
        /// </summary>
        /// <param name="fromDirectory">A source directory to copy the files/folders from.</param>
        /// <param name="toDirectory">A target directory to copy the files/folders to.</param>
        /// <param name="excludedFileExtensions">A list of file extensions to be excluded during copying.</param>
        /// <param name="excludedFolders">List of folders to be excluded during copying.</param>
        /// <returns>Batch command for copying files and sub-folders from a source directory to the target directory and remove source files/folders on completion with an options to exclude files and folders.</returns>
        public static string CopyFilesAndSubFoldersAndRemoveSource(string fromDirectory, string toDirectory, string excludedFileExtensions = null, string excludedFolders = null)
        {
            return $"robocopy /MOVE /S /E {fromDirectory} {toDirectory} {excludedFolders} {excludedFileExtensions}";
        }

        /// <summary>
        /// This batch command function is used for moving files/folder and sub-folders from a given directory to a target directory.
        /// </summary>
        /// <param name="fromDirectory">A source directory to copy the files/folders from.</param>
        /// <param name="toDirectory">Target directory to copy the files/folders to.</param>
        /// <returns>Batch command for moving files/folder and sub-folders from a given directory to a target directory.</returns>
        public static string Move(string fromDirectory, string toDirectory)
        {
            return $"move {fromDirectory}\\*.* {toDirectory}";
        }

        /// <summary>
        /// This batch command function is used for moving files/folder and sub-folders from a given directory.
        /// </summary>
        /// <param name="directoryToRemove">Target directory to remove the files/folders from.</param>
        /// <returns>Batch command for removing files/folder and sub-folders from a given target directory.</returns>
        public static string RemoveDirectory(string directoryToRemove)
        {
            return $"rmdir /s/q {directoryToRemove}";
        }

        /// <summary>
        /// This batch command function is used for changing from a source directory to a target directory.
        /// </summary>
        /// <param name="targetDirectory">Target directory to change to.</param>
        /// <returns>Batch command for changing directory.</returns>
        public static string ChangeToDirectory(string targetDirectory)
        {
            return $"chdir /d {targetDirectory}";
        }

        /// <summary>
        /// This batch command function is used to get a list of pre-defined Unity file extensions to exclude.
        /// </summary>
        /// <returns>Batch command for excluding pre-defined Unity file extension.</returns>
        public static string UnityExcludedProjectFiles()
        {
            string builds = "*.apk *.exe *.aab *.unitypackage";
            string autoGenerated = "*.csproj *.unityproj *.sln *.suo *.tmp *.user *.userprefs *.pidb *.booproj *.svd *.pdb *.mdb *.opendb *.VC.db";
            string excludeFiles = $"/xf {autoGenerated} {builds}";

            return excludeFiles;
        }

        /// <summary>
        /// This batch command function is used to get a list of Unity project folders to exclude.
        /// </summary>
        /// <returns>Batch command for excluding pre-defined Unity project folders.</returns>
        public static string UnityExcludedProjectFolders()
        {
            return "/E /xd Library Temp Build Builds Obj Logs UserSettings MemoryCaptures";
        }

        #endregion

        #region Main

        /// <summary>
        /// This function is used to run a batch file with a given command.
        /// </summary>
        /// <param name="command">Command to run.</param>
        public static void RunBatchCommand(string command)
        {
            Process.Start(command);
        }

        #endregion
    }

    #endregion
}
