using UnityEditor;

namespace Bridge.Core.App.Manager
{
    public class AppManagerEditor : Editor
    {
        [MenuItem("3ridge/Create/App Manager")]
        private static void CreateAppManager()
        {
            var appManager = new UnityEngine.GameObject("_3ridge App Manager");
            appManager.AddComponent<AppManager>();

            if(Selection.activeGameObject != null) appManager.transform.SetParent(Selection.activeGameObject.transform);

            UnityEngine.Debug.Log("<color=white>-->></color> <color=green> Success </color>:<color=white> An app manager has been created successfully.</color>");
        }

        [MenuItem("3ridge/Create/App Manager", true)]
        private static bool CanCreateAppManager()
        {
            return FindObjectOfType<AppManager>() == null;
        }
    }
}
