using System;

namespace Bridge.Core.UnityEditor.App.Manager
{
    [Serializable]
    public struct BatchFileData
    {
        public string batchFile;
        public string projectRootPath;
        public string fileRootPath;
        public string fullbatchFilePath;
    }


    [Serializable]
    public class BuildCompiler
    {
        #region Property Fields

        public string echoOff;
        public string echoPrepareBuild;
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
            return $"{echoOff} \n " +
                   $"{echoPrepareBuild} \n " +
                   $"{removeDirectoryCommand} \n " +
                   $"{echoCopy} \n " +
                   $"{copyContentCommand} \n " +
                   $"{changeToBuildDirectory} \n " +
                   $"{echoCompile} \n " +
                   $"{compilerBuildCommand} \n " +
                   $"{pause} ";
        }
    }

    [Serializable]
    public class Compiler
    {
        #region Property Fields

        public string echoOff;
        public string echoInitializeBuild;
        public string editorLogBuildStarted;
        public string startBuildCommand;
        public string moveBuildCommand;
        public string openBuildFolderPath;
        public string changeToProjectDirectory;
        public string editorLogBuildEnded;
        public string echoBuildCompleted;
        public string pause;

        #endregion

        public override string ToString()
        {
            return $"{echoOff} \n  " +
                   $"{echoInitializeBuild} \n  " +
                   $"{editorLogBuildStarted} \n " +
                   $"{startBuildCommand} \n " +
                   $"{moveBuildCommand} \n " +
                   $"{openBuildFolderPath} \n " +
                   $"{changeToProjectDirectory} \n " +
                   $"{editorLogBuildEnded} \n " +
                   $"{echoBuildCompleted} \n " +
                   $"{pause}";
        }
    }

    [Serializable]
    public class BuildCleaner
    {
        #region Property Fields

        public string echoOff;
        public string echoCleaningBuild;
        public string editorLogCleanStarted;
        public string cleanBuildPathCommand;
        public string editorLogCleanEnded;

        #endregion

        public override string ToString()
        {
            return $"{echoOff} \n  " +
                   $"{echoCleaningBuild} \n  " +
                   $"{editorLogCleanStarted} \n  " +
                   $"{cleanBuildPathCommand} \n  " +
                   $"{editorLogCleanEnded}";
        }
    }
}
