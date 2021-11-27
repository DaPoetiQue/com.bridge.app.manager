using Bridge.Core.App.Manager;
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Bridge.Core.App.Data.Storage;
using Bridge.Core.App.Events;
using Bridge.Core.UnityEditor.Debugger;

namespace Bridge.Core.UnityEditor.App.Manager
{
    public static class BuildCompilerScript
    {
        public static void BuildCompiler(BuildSettingsData buildSettings, Action<AppEventsData.CallBackResults, BuildSettingsData> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            try
            {
                #region Defining Build Batch File Data

                BatchFileData buildCompilerBatchFileData = GetBatchFileData(GetBuildCompilerBatchFileName(), GetBuildScriptFolderName());
                BatchFileData compilerBatchFileData = GetBatchFileData(GetCompilerBatchFileName(), GetBuildScriptFolderName());
                BatchFileData buildCleanerBatchFileData = GetBatchFileData(GetBuildCleanerBatchFileName(), GetBuildScriptFolderName());

                #endregion

                if (!Directory.Exists(buildCompilerBatchFileData.fileRootPath))
                {
                    Directory.CreateDirectory(buildCompilerBatchFileData.fileRootPath);
                    UnityEngine.Debug.Log($"--> Creating Build Compiler Directory : <color=cyan>{buildCompilerBatchFileData.fileRootPath}</color>");
                }

                if (!Directory.Exists(Storage.Directory.GetProjectTempDirectory()))
                {
                    UnityEngine.Debug.Log($"--> Creating Temp Build Project Directory @ : <color=cyan>{Storage.Directory.GetProjectTempDirectory()}</color>");
                    Directory.CreateDirectory(Storage.Directory.GetProjectTempDirectory());
                }        

                // Build Command
                string rootPath = "C:/Program Files/";
                string unityVersion = Application.unityVersion;
                string path = "/Editor/Unity.exe";
                string pathCombined = "\"" + rootPath + unityVersion + path + "\"";
                string compilerDirectory = $"\"{Storage.Directory.GetProjectTempDirectory().Replace("\\", "/")}{GetBuildScriptFolderName()}\"";
                string targetDirectory = $"\"{Storage.Directory.GetProjectTempDirectory().Replace("\\", "/")}\"";
                string changeToBuildDirectoryCommand = "chdir /d " + compilerDirectory;
                string changeToProjectDirectoryCommand = "chdir /d " + buildCompilerBatchFileData.fileRootPath + GetBuildScriptFolderName();
                string buildMethodName = "AppBuildConfig.BuildApp";
                string buildCommand = pathCombined + $" -quit -batchMode -projectPath .. -executeMethod {buildMethodName}";

                UnityEngine.Debug.Log($"-->Change To Temp Compiler Directory : {changeToBuildDirectoryCommand}");

                string projectDir = "chdir /d " + Storage.Directory.GetProjectTempDirectory();

                string openBuildFolderPath = $"Start \"\" \"{buildSettings.configurations.targetBuildDirectory}\"";

                string buildDir = $"{Storage.Directory.GetProjectTempDirectory()}\\Builds";

                UnityEngine.Debug.Log($"-->Change To Build Compiler Directory : {projectDir}");

                #region Remove Content Command

                #endregion

                #region Editor Log

                string logDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string editorLogDir = $"{logDir}\\Unity\\Editor\\Editor.log";
                string logEditorCommand = $"Type {editorLogDir}";
                UnityEngine.Debug.Log($"-->Data Directory : {editorLogDir}");

                #endregion

                #region Constucted Batch File Data

                Compiler compiler = new Compiler
                {
                    echoOff = "@echo off",
                    echoInitializeBuild = "echo Initializing Project Build...",
                    editorLogBuildStarted = logEditorCommand,
                    startBuildCommand = buildCommand,
                    moveBuildCommand = Storage.BatchCommands.Move(buildDir, buildSettings.configurations.targetBuildDirectory),
                    openBuildFolderPath = openBuildFolderPath,
                    editorLogBuildEnded = logEditorCommand,
                    echoBuildCompleted = "echo Build Completed...",
                    pause = "@pause"
                };

                BuildCompiler buildCompiler = new BuildCompiler
                {
                    echoOff = "@echo off",
                    echoPrepareBuild = "echo Preparing Build...",
                    removeDirectoryCommand = Storage.BatchCommands.RemoveDirectory(Storage.Directory.GetProjectTempDirectory()),
                    echoCopy = "echo Copying Build Files...",
                    copyContentCommand = Storage.BatchCommands.Copy(buildCompilerBatchFileData.projectRootPath, Storage.Directory.GetProjectTempDirectory(), Storage.BatchCommands.UnityExcludedFolders(), Storage.BatchCommands.UnityExcludedFiles()),
                    changeToBuildDirectory = changeToBuildDirectoryCommand,
                    echoCompile = "echo Compiling Build Scripts...",
                    compilerBuildCommand = compilerBatchFileData.batchFile,
                    pause = "@pause"
                };

                BuildCleaner buildCleaner = new BuildCleaner
                {
                    echoOff = "@echo off",
                    echoCleaningBuild = "echo Cleaning Build Project...",
                    editorLogCleanStarted = logEditorCommand,
                    cleanBuildPathCommand = Storage.BatchCommands.RemoveDirectory(Storage.Directory.GetProjectTempDirectory()),
                    editorLogCleanEnded = logEditorCommand,
                };

                #endregion

                #region Create Batch File Data

                // Batch Files
                CreateBatchFile(compilerBatchFileData.fullbatchFilePath, compiler, results =>
                {
                    if(results.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Error, results.errorValue);
                    }

                    if (results.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                    }
                });

                CreateBatchFile(buildCompilerBatchFileData.fullbatchFilePath, buildCompiler, results =>
                {
                    if (results.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Error, results.errorValue);
                    }

                    if (results.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                    }
                });

                CreateBatchFile(buildCleanerBatchFileData.fullbatchFilePath, buildCleaner, results =>
                {
                    if (results.error)
                    {
                        DebugConsole.Log(Debug.LogLevel.Error, results.errorValue);
                    }

                    if (results.success)
                    {
                        DebugConsole.Log(Debug.LogLevel.Success, results.successValue);
                    }
                });

                #endregion

                if (File.Exists(buildCompilerBatchFileData.fullbatchFilePath))
                {
                    UnityEngine.Debug.Log($"--> <color=orange>Build Started....</color>");
                    Storage.BatchCommands.RunBatchCommand(buildCompilerBatchFileData.fullbatchFilePath);
                }

                results.success = true;
                results.successValue = "Build Started...";

                callback.Invoke(results, buildSettings);
            }
            catch (Exception exception)
            {
                results.error = true;
                results.errorValue = exception.Message;
                callback.Invoke(results, null);
            }
        }

        private static void CreateBatchFile<T>(string path, T fileData, Action<AppEventsData.CallBackResults> callback = null)
        {
            AppEventsData.CallBackResults results = new AppEventsData.CallBackResults();

            if(string.IsNullOrEmpty(path) == false)
            {
                File.WriteAllText(path, fileData.ToString());

                results.success = true;
                results.successValue = $"A new Batch file has been created successfully @ : {path}.";
                
                callback.Invoke(results);
            }
            else
            {
                callback.Invoke(results);
            }
        }

        #region File Names

        private static string GetCompilerBatchFileName()
        {
            string fileName = "Compiler.bat";
            return fileName;
        }

        private static string GetBuildCompilerBatchFileName()
        {
            string fileName = "BuildCompiler.bat";
            return fileName;
        }

        private static string GetBuildCleanerBatchFileName()
        {
            string fileName = "CleanBuild.bat";
            return fileName;
        }

        #endregion

        private static BatchFileData GetBatchFileData(string fileName = null, string folderName = null)
        {
            var fileInfoData = new FileInfo(fileName);
            string batchFile = $"./{fileName}";
            batchFile.Replace("/", "\\");
            string projectRootPath = fileInfoData.DirectoryName;
            string fileRootPath = projectRootPath + folderName;
            string fullbatchFilePath = $"{fileRootPath}/{fileInfoData.Name}";

            #region Batch File

            BatchFileData data = new BatchFileData
            {
                batchFile = batchFile,
                projectRootPath = projectRootPath,
                fileRootPath = fileRootPath,
                fullbatchFilePath = fullbatchFilePath
            };

            #endregion

            return data;
        }

        private static string GetBuildScriptFolderName()
        {
            string buildScriptsPath = "\\BuildScripts";
            return buildScriptsPath;
        }

        public static void ClearProjectCache()
        {
            BatchFileData buildCleanerBatchFileData = GetBatchFileData(GetBuildCleanerBatchFileName(), GetBuildScriptFolderName());
            Storage.BatchCommands.RunBatchCommand(buildCleanerBatchFileData.fullbatchFilePath);
        }

    }
}
