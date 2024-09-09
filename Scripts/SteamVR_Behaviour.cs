// Decompiled with JetBrains decompiler
// Type: Valve.VR.SteamVR_Behaviour
// Assembly: SteamVR, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: DF474E11-42EA-4738-BF41-6A2D38F0B79C
// Assembly location: S:\SteamLibrary\steamapps\common\GTFO\GTFO_Data\BrokenAssembly20012020\Managed\SteamVR.dll

using MelonLoader;
using HPVR.Util;
using System;
using UnityEngine;
using UnityEngine.XR;

namespace Valve.VR
{
    public class SteamVR_Behaviour : MonoBehaviour
    {
        public SteamVR_Behaviour(IntPtr value)
: base(value) { }

        public static bool forcingInitialization = false;
        internal static bool isPlaying = false;
        private static bool initializing = false;
        protected static int lastFrameCount = -1;
        public bool initializeSteamVROnAwake = true;
        public bool doNotDestroy = true;
        private const string openVRDeviceName = "OpenVR";
        private static SteamVR_Behaviour _instance;

        public SteamVR_Render steamvr_render;
        private bool loadedOpenVRDeviceSuccess;

        public static SteamVR_Behaviour instance
        {
            get
            {
                if (_instance == null)
                    Initialize(true);
                return _instance;
            }
        }

        public static void Initialize(bool forceUnityVRToOpenVR = false)
        {
            if (!(_instance == null) || initializing)
                return;
            initializing = true;
            GameObject gameObject1 = null;
            if (forceUnityVRToOpenVR)
                forcingInitialization = true;

            SteamVR_Render objectOfType1 = FindObjectOfType<SteamVR_Render>();
            if (objectOfType1 != null)
            {
                gameObject1 = objectOfType1.gameObject;
                MelonLogger.Msg("[HPVR] found the render");
            }

            SteamVR_Behaviour objectOfType2 = FindObjectOfType<SteamVR_Behaviour>();
            if (objectOfType2 != null)
            {
                gameObject1 = objectOfType2.gameObject;
                MelonLogger.Msg("[HPVR] found the behaviour");
            }

            if (gameObject1 == null)
            {
                MelonLogger.Msg("[HPVR] creating new behaviour and renderer");
                GameObject gameObject2 = new GameObject("[SteamVR]");
                _instance = gameObject2.AddComponent<SteamVR_Behaviour>();
                _instance.steamvr_render = gameObject2.AddComponent<SteamVR_Render>();
            }
            else
            {
                SteamVR_Behaviour steamVrBehaviour = gameObject1.GetComponent<SteamVR_Behaviour>();
                if (steamVrBehaviour == null)
                    steamVrBehaviour = gameObject1.AddComponent<SteamVR_Behaviour>();
                if (objectOfType1 != null)
                {
                    steamVrBehaviour.steamvr_render = objectOfType1;
                }
                else
                {
                    steamVrBehaviour.steamvr_render = gameObject1.GetComponent<SteamVR_Render>();
                    if (steamVrBehaviour.steamvr_render == null)
                        steamVrBehaviour.steamvr_render = gameObject1.AddComponent<SteamVR_Render>();
                }
                _instance = steamVrBehaviour;
            }

            MelonLogger.Msg("[HPVR] behaviour: " + _instance?.name);
            MelonLogger.Msg("[HPVR] renderer: " + _instance?.steamvr_render?.name);

            if (_instance != null && _instance.doNotDestroy)
                DontDestroyOnLoad(_instance.transform.root.gameObject);
            initializing = false;
        }

        protected void Awake()
        {
            isPlaying = true;
            if (!initializeSteamVROnAwake || forcingInitialization)
                return;
            InitializeSteamVR(false);
        }

        public void InitializeSteamVR(bool forceUnityVRToOpenVR = false)
        {
            if (forceUnityVRToOpenVR)
            {
                throw new System.Exception("This should not be used");
            }
            else
                SteamVR.Initialize(false);
        }

        private void XRDevice_deviceLoaded(string deviceName)
        {
            if (deviceName == "OpenVR")
            {
                loadedOpenVRDeviceSuccess = true;
            }
            else
            {
                MelonLogger.Error("[HPVR] Tried to async load: OpenVR. Loaded: " + deviceName, this);
                loadedOpenVRDeviceSuccess = true;
            }
        }

        private void EnableOpenVR()
        {
            XRSettings.enabled = true;
            SteamVR.Initialize(false);
            forcingInitialization = false;
        }

        protected void OnEnable()
        {
            SteamVR_Events.System(EVREventType.VREvent_Quit).Listen(OnQuit);
        }

        protected void OnDisable()
        {
            SteamVR_Events.System(EVREventType.VREvent_Quit).Remove(OnQuit);
        }

        protected void OnBeforeRender()
        {
            PreCull();
        }

        protected void PreCull()
        {
            if (Time.frameCount == lastFrameCount)
                return;
            lastFrameCount = Time.frameCount;
            SteamVR_Input.OnPreCull();
        }

        protected void FixedUpdate()
        {
            SteamVR_Input.FixedUpdate();
        }

        protected void LateUpdate()
        {
            SteamVR_Input.LateUpdate();
        }

        protected void Update()
        {
            SteamVR_Input.Update();
        }

        protected void OnQuit(VREvent_t vrEvent)
        {
            Application.Quit();
        }
    }
}
