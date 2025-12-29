using SteamVR_Melon.Util;
using System;
using System.Threading.Tasks;
using UnityEngine;

using UnityEngine.XR;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRBehaviour : MonoBehaviour
    {
        public SteamVRBehaviour(IntPtr value) : base(value) { }
        private const string openVRDeviceName = "OpenVR";
        public static bool forcingInitialization = false;

        private static SteamVRBehaviour _instance;
        public static SteamVRBehaviour instance
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

        public SteamVRRender steamvr_render;

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

                SteamVRRender renderInstance = GameObject.FindObjectOfType<SteamVRRender>();
                if (renderInstance != null)
                {
                    steamVRObject = renderInstance.gameObject;
                }

                SteamVRBehaviour behaviourInstance = GameObject.FindObjectOfType<SteamVRBehaviour>();
                if (behaviourInstance != null)
                {
                    steamVRObject = behaviourInstance.gameObject;
                }

                if (steamVRObject == null)
                {
                    GameObject objectInstance = new("[HPVR]");
                    _instance = objectInstance.AddComponent<SteamVRBehaviour>();
                    _instance.steamvr_render = objectInstance.AddComponent<SteamVRRender>();
                }
                else
                {
                    behaviourInstance = steamVRObject.GetComponent<SteamVRBehaviour>();
                    if (behaviourInstance == null)
                    {
                        behaviourInstance = steamVRObject.AddComponent<SteamVRBehaviour>();
                    }

                    if (renderInstance != null)
                    {
                        behaviourInstance.steamvr_render = renderInstance;
                    }
                    else
                    {
                        behaviourInstance.steamvr_render = steamVRObject.GetComponent<SteamVRRender>();
                        if (behaviourInstance.steamvr_render == null)
                        {
                            behaviourInstance.steamvr_render = steamVRObject.AddComponent<SteamVRRender>();
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
                }

                if (XRSettings.loadedDeviceName == openVRDeviceName)
                {
                    EnableOpenVR();
                }
                else
                {
                    initializeCoroutine = Task.Factory.StartNew(new Action(() => DoInitializeSteamVR(forceUnityVRToOpenVR)));
                }
            }
            else
            {
                SteamVR.Initialize(false);
            }
        }

        private Task initializeCoroutine;

        private bool loadedOpenVRDeviceSuccess = false;
        private void DoInitializeSteamVR(bool forceUnityVRToOpenVR = false)
        {
            XRDevice.add_deviceLoaded(new Action<string>(XRDevice_deviceLoaded));
            XRSettings.LoadDeviceByName(openVRDeviceName);
            while (loadedOpenVRDeviceSuccess == false)
            {
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
            SteamVREvents.System(EVREventType.VREventQuit).Listen(OnQuit);
        }
        protected void OnDisable()
        {
            UnityHooks.OnBeforeRender -= OnBeforeRender;
            SteamVREvents.System(EVREventType.VREventQuit).Remove(OnQuit);
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

                    SteamVRInput.OnPreCull();
                }
            }
        }

        protected void FixedUpdate()
        {
            if (OpenVR.Input != null)
            {
                SteamVRInput.FixedUpdate();
            }
        }

        protected void LateUpdate()
        {
            if (OpenVR.Input != null)
            {
                SteamVRInput.LateUpdate();
            }
        }

        protected void Update()
        {
            if (OpenVR.Input != null)
            {
                SteamVRInput.Update();
            }
        }

        protected void OnQuit(VREventT vrEvent)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
