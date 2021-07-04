using UnityEngine;
using Bridge.Core.Debug;
using Bridge.Core.Events;

namespace Bridge.Core.AppManager
{
    public class AppManager : MonoDebug
    {
        #region Instances
        private static AppManager instance;

        public static AppManager Instance
        {
            get
            {
                if(instance == null) instance = FindObjectOfType<AppManager>();

                return instance;
            }
        }

        #endregion

        #region Components

        [SerializeField]
        private AppEventsData.AppViewState appViewState;

        #endregion

        #region Unity Defaults

        private void Start()
        {
            Init();
        }

        #endregion

        #region  Main

        private void Init()
        {
            EventsManager.Instance.OnAppViewChangedEvent.AddListener(OnAppViewStateChanged);
            EventsManager.Instance.OnAppInitializedEvent.Invoke();
        }

        public void OnAppViewStateChanged(AppEventsData.AppViewState appViewState)
        {
            this.appViewState = appViewState;

             Log(LogData.LogLevel.Debug, this, $"App view changed to {appViewState.ToString()}.");
        }

        #endregion
    }
}
