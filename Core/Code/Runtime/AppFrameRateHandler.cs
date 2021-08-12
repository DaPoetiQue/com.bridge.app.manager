using UnityEngine;
using Bridge.Core.Debug;

namespace Bridge.Core.App.Manager
{
    public class AppFrameRateHandler : MonoDebug
    {
        #region Components

        [SerializeField]
        private int targetFrameRate = 60;

        public int TargetFrameRate
        {
            get { return targetFrameRate; }
            set
            {
                targetFrameRate = value;
                SetFrameRate();
            }
        }

        #endregion

        #region Unity

        void Start() => SetFrameRate();

        #endregion

        #region Main

        void SetFrameRate()
        {
            Application.targetFrameRate = targetFrameRate;
        }

        #endregion
    }
}