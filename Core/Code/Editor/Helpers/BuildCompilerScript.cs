using Bridge.Core.App.Manager;
using System;
using System.IO;
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

                #region Constucted Batch File Data

                BuildScriptCompiler compiler = new BuildScriptCompiler
                {
                    echoOff = BatchCommands.GetBatchFileAttribute().echoOff,
                    echoInitializeBuild = BatchCommands.GetBatchFileAttribute().echoInitializeBuild,
                    editorLogBuildStartedCommand = BatchCommands.GetUnityEditorLogFile(),
                    startBuildCommand = BatchCommands.BuildProject(),
                    moveBuildCommand = BatchCommands.CopyFilesAndSubFoldersAndRemoveSource(Storage.Directory.GetUnityEditorProjectInfoData(GetBuildScriptFolderName()).projectBuildDirectory, buildSettings.configurations.targetBuildDirectory),
                    openBuildFolderPathCommand = BatchCommands.OpenFolderLocation(buildSettings.configurations.targetBuildDirectory),
                    editorLogBuildEndedCommand = BatchCommands.GetUnityEditorLogFile(),
                    echoBuildCompleted = BatchCommands.GetBatchFileAttribute().echoBuildCompleted,
                    pause = BatchCommands.GetBatchFileAttribute().pause
                };

                BuildCompiler buildCompiler = new BuildCompiler
                {
                    echoOffAttribute = BatchCommands.GetBatchFileAttribute().echoOff,
                    echoPrepareBuildAttribute = BatchCommands.GetBatchFileAttribute().prepareBuild,
                    removeDirectoryCommand = BatchCommands.RemoveDirectory(Storage.Directory.GetProjectTempDirectory()),
                    echoCopy = BatchCommands.GetBatchFileAttribute().echoCopy,
                    copyContentCommand = BatchCommands.CopyFiles(buildCompilerBatchFileData.projectRootPath, Storage.Directory.GetProjectTempDirectory(), BatchCommands.UnityExcludedFolders(), BatchCommands.UnityExcludedFiles()),
                    changeToBuildDirectory = BatchCommands.ChangeToDirectory(Storage.Directory.GetProjectTempDirectory() + GetBuildScriptFolderName()),
                    echoCompile = BatchCommands.GetBatchFileAttribute().echoBuildCompile,
                    compilerBuildCommand = compilerBatchFileData.batchFile,
                    pause = BatchCommands.GetBatchFileAttribute().pause
                };

                BuildScriptDisposer buildCleaner = new BuildScriptDisposer
                {
                    echoOffAttribute = BatchCommands.GetBatchFileAttribute().echoOff,
                    echoCleaningBuildProjectAttribute = BatchCommands.GetBatchFileAttribute().echoCleaningBuildProjectAttribute,
                    editorLogCleanStarted = BatchCommands.GetUnityEditorLogFile(),
                    cleanBuildPathCommand = BatchCommands.RemoveDirectory(Storage.Directory.GetProjectTempDirectory()),
                    editorLogCleanEnded = BatchCommands.GetUnityEditorLogFile()
                };

                #endregion

                #region Create Batch File Data

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
                    BatchCommands.RunBatchCommand(buildCompilerBatchFileData.fullbatchFilePath);
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

        /// <summary>
        /// This function creates a batch file in the given directory.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path">This function takes a path for storing the created batch file, as well as batch file data.</param>
        /// <param name="fileData">File data represents the structure of the command file.</param>
        /// <param name="callback">This function returns a callback when exectuted function completes.</param>
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
            BatchCommands.RunBatchCommand(buildCleanerBatchFileData.fullbatchFilePath);
        }

    }
}
