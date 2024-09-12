using System;
using UnityEngine;

namespace SteamVR_Melon.Util
{
    /// <summary>
    /// Hook into BeforeRenderHelper in your plugin to make these events work!
    /// </summary>
    public static class UnityHooks
    {
        public static event Action OnBeforeRender;

        public static void InvokeOnBeforeRender()
        {
            OnBeforeRender?.Invoke();
        }

        //todo change hook to application.onprerender
        public static void Init()
        {
            Camera.onPreRender = (
                (Camera.onPreRender == null)
                ? new Action<Camera>(OnPreRender)
                : Il2CppSystem.Delegate.Combine(Camera.onPreRender, (Camera.CameraCallback)new Action<Camera>(OnPreRender)).Cast<Camera.CameraCallback>()
                );
        }

        private static void OnPreRender(Camera cam)
        {
            if (OnPreRenderCam == null || !OnPreRenderCam.enabled)
                OnPreRenderCam = cam;
            if (OnPreRenderCam == cam)
                InvokeOnBeforeRender();
        }

        private static Camera OnPreRenderCam = null;
    }
}