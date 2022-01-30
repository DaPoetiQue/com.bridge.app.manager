using UnityEditor;
using UnityEditor.Build;
using Bridge.Core.UnityEditor.Debugger;
using Bridge.Core.App.Manager;

namespace Bridge.Core.UnityEditor.App.Manager
{
    public class BuildPlatformEditorEvents : IActiveBuildTargetChanged
    {
        #region Components

        public int callbackOrder { get { return 0; } }

        #endregion

        #region Main

        public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
        {
            BuildManager.ApplyBuildSettings(AppDataBuilder.CreateNewBuildSettingsInstance(BuildManager.GetBuildSettings(BuildManager.GetDefaultStorageInfo())), (results, data) => 
            {
                if(results.error == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Error, $"Failed to apply [Build Settings] for target platform : {newTarget}. - with results : {results.errorValue}");
                    return;
                }

                if (results.success == true)
                {
                    DebugConsole.Log(Debug.LogLevel.Success, $"[Build Settings] applied successfullly for target platform : {newTarget}. - with results : {results.successValue}");
                }
            });
            DebugConsole.Log(Debug.LogLevel.Debug, $"Build platform target switch from : {previousTarget} to : {newTarget}");
        }

        #endregion
    }
}
