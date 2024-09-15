using SteamVR_Melon.Util;
using System;
using System.Collections;
using UnityEngine;

using UnityEngine.XR;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class SteamVR_Behaviour : MonoBehaviour
    {
        public SteamVR_Behaviour(IntPtr value) : base(value) { }
        private const string openVRDeviceName = "OpenVR";
        public static bool forcingInitialization = false;

        private static SteamVR_Behaviour _instance;
        public static SteamVR_Behaviour instance
        {
            get
            {
                if (_instance == null)
                {
                    Initialize(false);
                }

                return _instance;
            }
        }

        public bool initializeSteamVROnAwake = true;

        public bool doNotDestroy = true;

        public SteamVR_Render steamvr_render;

        internal static bool isPlaying = false;

        private static bool initializing = false;
        public static void Initialize(bool forceUnityVRToOpenVR = false)
        {
            if (_instance == null && initializing == false)
            {
                initializing = true;
                GameObject steamVRObject = null;

                if (forceUnityVRToOpenVR)
                {
                    forcingInitialization = true;
                }

                SteamVR_Render renderInstance = GameObject.FindObjectOfType<SteamVR_Render>();
                if (renderInstance != null)
                {
                    steamVRObject = renderInstance.gameObject;
                }

                SteamVR_Behaviour behaviourInstance = GameObject.FindObjectOfType<SteamVR_Behaviour>();
                if (behaviourInstance != null)
                {
                    steamVRObject = behaviourInstance.gameObject;
                }

                if (steamVRObject == null)
                {
                    GameObject objectInstance = new GameObject("[HPVR]");
                    _instance = objectInstance.AddComponent<SteamVR_Behaviour>();
                    _instance.steamvr_render = objectInstance.AddComponent<SteamVR_Render>();
                }
                else
                {
                    behaviourInstance = steamVRObject.GetComponent<SteamVR_Behaviour>();
                    if (behaviourInstance == null)
                    {
                        behaviourInstance = steamVRObject.AddComponent<SteamVR_Behaviour>();
                    }

                    if (renderInstance != null)
                    {
                        behaviourInstance.steamvr_render = renderInstance;
                    }
                    else
                    {
                        behaviourInstance.steamvr_render = steamVRObject.GetComponent<SteamVR_Render>();
                        if (behaviourInstance.steamvr_render == null)
                        {
                            behaviourInstance.steamvr_render = steamVRObject.AddComponent<SteamVR_Render>();
                        }
                    }

                    _instance = behaviourInstance;
                }

                if (_instance != null && _instance.doNotDestroy)
                {
                    GameObject.DontDestroyOnLoad(_instance.transform.root.gameObject);
                }

                initializing = false;
            }
        }

        protected void Awake()
        {
            isPlaying = true;

            if (initializeSteamVROnAwake && forcingInitialization == false)
            {
                InitializeSteamVR();
            }
        }

        public void InitializeSteamVR(bool forceUnityVRToOpenVR = false)
        {
            if (forceUnityVRToOpenVR)
            {
                forcingInitialization = true;

                if (initializeCoroutine != null)
                {
                    MelonLoader.MelonCoroutines.Stop(initializeCoroutine);
                }

                if (XRSettings.loadedDeviceName == openVRDeviceName)
                {
                    EnableOpenVR();
                }
                else
                {
                    initializeCoroutine = DoInitializeSteamVR(forceUnityVRToOpenVR);
                }

                MelonLoader.MelonCoroutines.Start(initializeCoroutine);
            }
            else
            {
                SteamVR.Initialize(false);
            }
        }

        private IEnumerator initializeCoroutine;

        private bool loadedOpenVRDeviceSuccess = false;
        private IEnumerator DoInitializeSteamVR(bool forceUnityVRToOpenVR = false)
        {
            XRDevice.add_deviceLoaded(new Action<string>(XRDevice_deviceLoaded));
            XRSettings.LoadDeviceByName(openVRDeviceName);
            while (loadedOpenVRDeviceSuccess == false)
            {
                yield return null;
            }
            XRDevice.remove_deviceLoaded(new Action<string>(XRDevice_deviceLoaded));
            EnableOpenVR();
        }

        private void XRDevice_deviceLoaded(string deviceName)
        {
            if (deviceName == openVRDeviceName)
            {
                loadedOpenVRDeviceSuccess = true;
            }
            else
            {
                MelonLoader.MelonLogger.Error("[HPVR] Tried to async load: " + openVRDeviceName + ". Loaded: " + deviceName, this);
                loadedOpenVRDeviceSuccess = true; //try anyway
            }
        }

        private void EnableOpenVR()
        {
            XRSettings.enabled = true;
            SteamVR.Initialize(false);
            initializeCoroutine = null;
            forcingInitialization = false;
        }

        protected void OnEnable()
        {
            UnityHooks.OnBeforeRender += OnBeforeRender;
            SteamVR_Events.System(EVREventType.VREvent_Quit).Listen(OnQuit);
        }
        protected void OnDisable()
        {
            UnityHooks.OnBeforeRender -= OnBeforeRender;
            SteamVR_Events.System(EVREventType.VREvent_Quit).Remove(OnQuit);
        }
        protected void OnBeforeRender()
        {
            PreCull();
        }

        protected static int lastFrameCount = -1;
        protected void PreCull()
        {
            if (OpenVR.Input != null)
            {
                // Only update poses on the first camera per frame.
                if (Time.frameCount != lastFrameCount)
                {
                    lastFrameCount = Time.frameCount;

                    SteamVR_Input.OnPreCull();
                }
            }
        }

        protected void FixedUpdate()
        {
            if (OpenVR.Input != null)
            {
                SteamVR_Input.FixedUpdate();
            }
        }

        protected void LateUpdate()
        {
            if (OpenVR.Input != null)
            {
                SteamVR_Input.LateUpdate();
            }
        }

        protected void Update()
        {
            if (OpenVR.Input != null)
            {
                SteamVR_Input.Update();
            }
        }

        protected void OnQuit(VREvent_t vrEvent)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
