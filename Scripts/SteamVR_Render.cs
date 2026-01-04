using MelonLoader;
using SteamVR_Melon.Scripts;
using SteamVR_Melon.Util;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mathf = HPVR.Util.Mathf;

namespace Valve.VR
{

    [RegisterTypeInIl2Cpp()]
    public class SteamVRRender : MonoBehaviour
    {
        public SteamVRRender(IntPtr value) : base(value) { }

        public SteamVRExternalCamera externalCamera;
        public string externalCameraConfigPath = "externalcamera.cfg";

        public static EVREye eye { get; private set; }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public static SteamVRRender instance { get { return SteamVRBehaviour.instance.steamvr_render; } }

        static private bool isQuitting;
        void OnApplicationQuit()
        {
            isQuitting = true;
            SteamVR.SafeDispose();
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        static public void Add(SteamVRCamera vrcam)
        {
            if (!isQuitting)
            {
                instance.AddInternal(vrcam);
            }
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        static public void Remove(SteamVRCamera vrcam)
        {
            if (!isQuitting && instance != null)
            {
                instance.RemoveInternal(vrcam);
            }
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        static public SteamVRCamera Top()
        {
            if (!isQuitting)
            {
                return instance.TopInternal();
            }

            return null;
        }

        private SteamVRCamera[] cameras = Array.Empty<SteamVRCamera>();

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        void AddInternal(SteamVRCamera vrcam)
        {
            MelonLogger.Msg("[HPVR] " + SceneManager.GetActiveScene().name + " adding " + vrcam);
            var camera = vrcam.GetComponent<Camera>();
            var length = cameras.Length;
            var sorted = new SteamVRCamera[length + 1];
            int insert = 0;
            for (int i = 0; i < length; i++)
            {
                var c = cameras[i].GetComponent<Camera>();
                if (i == insert && c.depth > camera.depth)
                {
                    sorted[insert++] = vrcam;
                }

                sorted[insert++] = cameras[i];
            }
            if (insert == length)
            {
                sorted[insert] = vrcam;
            }

            cameras = sorted;
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        void RemoveInternal(SteamVRCamera vrcam)
        {
            var length = cameras.Length;
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                var c = cameras[i];
                if (c == vrcam)
                {
                    ++count;
                }
            }
            if (count == 0)
            {
                return;
            }

            var sorted = new SteamVRCamera[length - count];
            int insert = 0;
            for (int i = 0; i < length; i++)
            {
                var c = cameras[i];
                if (c != vrcam)
                {
                    sorted[insert++] = c;
                }
            }

            cameras = sorted;
        }

        SteamVRCamera TopInternal()
        {
            if (cameras.Length > 0)
            {
                return cameras[^1];
            }

            return null;
        }

        public TrackedDevicePoseT[] poses = new TrackedDevicePoseT[OpenVR.kUnMaxTrackedDeviceCount];
        public TrackedDevicePoseT[] gamePoses = Array.Empty<TrackedDevicePoseT>();

        static private bool _pauseRendering;

        public static event Action OnPreRender = () => { };

        static public bool PauseRendering
        {
            get { return _pauseRendering; }
            set
            {
                _pauseRendering = value;

                if (!value)
                {
                    MelonLogger.Msg("Pausing rendering");
                }
                else
                {
                    MelonLogger.Msg("Unpausing rendering");
                }

                var compositor = OpenVR.Compositor;
                compositor?.SuspendRendering(value);
            }
        }

        private readonly WaitForEndOfFrame waitForEndOfFrame = new();

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private IEnumerator RenderLoop()
        {
            while (Application.isPlaying)
            {
                yield return waitForEndOfFrame;

                if (PauseRendering)
                {
                    continue;
                }

                //MelonLogger.Msg("invoking camera onprerender");
                OnPreRender.Invoke();

                var compositor = OpenVR.Compositor;
                if (compositor != null)
                {
                    if (compositor.CanRenderScene())
                    {
                        //MelonLogger.Msg("can render scene");
                        compositor.SetTrackingSpace(SteamVR.settings.trackingSpace);
                    }
                    else
                    {
                        //MelonLogger.Msg("cannot render scene");
                        continue;
                    }
                }

                var overlay = SteamVROverlay.instance;
                overlay?.UpdateOverlay();

                if (CheckExternalCamera())
                {
                    RenderExternalCamera();
                }
            }
        }

        private bool? doesPathExist = null;
        private bool CheckExternalCamera()
        {
            if (doesPathExist == false)
            {
                return false;
            }
            else if (doesPathExist == null)
            {
                doesPathExist = System.IO.File.Exists(externalCameraConfigPath);
            }

            if (externalCamera == null && doesPathExist == true)
            {
                GameObject prefab = Resources.Load<GameObject>("SteamVR_ExternalCamera");
                if (prefab == null)
                {
                    doesPathExist = false;
                    return false;
                }
                else
                {
                    if (SteamVRSettings.instance.legacyMixedRealityCamera)
                    {
                        if (SteamVRExternalCameraLegacyManager.hasCamera == false)
                        {
                            return false;
                        }

                        GameObject instance = Instantiate(prefab);
                        instance.gameObject.name = "External Camera";

                        externalCamera = instance.transform.GetChild(0).GetComponent<SteamVRExternalCamera>();
                        externalCamera.configPath = externalCameraConfigPath;
                        externalCamera.ReadConfig();
                        externalCamera.SetupDeviceIndex(SteamVRExternalCameraLegacyManager.cameraIndex);
                    }
                    else
                    {
                        SteamVRActionPose cameraPose = SteamVRSettings.instance.mixedRealityCameraPose;
                        SteamVRInputSources cameraSource = SteamVRSettings.instance.mixedRealityCameraInputSource;

                        if (cameraPose != null && SteamVRSettings.instance.mixedRealityActionSetAutoEnable)
                        {
                            if (cameraPose.actionSet != null && cameraPose.actionSet.IsActive(cameraSource) == false)
                            {
                                cameraPose.actionSet.Activate(cameraSource);
                            }
                        }

                        if (cameraPose == null)
                        {
                            doesPathExist = false;
                            return false;
                        }

                        if (cameraPose != null && cameraPose[cameraSource].active && cameraPose[cameraSource].deviceIsConnected)
                        {
                            GameObject instance = Instantiate(prefab);
                            instance.gameObject.name = "External Camera";

                            externalCamera = instance.transform.GetChild(0).GetComponent<SteamVRExternalCamera>();
                            externalCamera.configPath = externalCameraConfigPath;
                            externalCamera.ReadConfig();
                            externalCamera.SetupPose(cameraPose, cameraSource);
                        }
                    }
                }
            }

            return (externalCamera != null);
        }

        void RenderExternalCamera()
        {
            if (externalCamera == null)
            {
                return;
            }

            if (!externalCamera.gameObject.activeInHierarchy)
            {
                return;
            }

            var frameSkip = (int)Mathf.Max(externalCamera.config.frameSkip, 0.0f);
            if (Time.frameCount % (frameSkip + 1) != 0)
            {
                return;
            }

            // Keep external camera relative to the most relevant vr camera.
            externalCamera.AttachToCamera(TopInternal());

            externalCamera.RenderNear();
            externalCamera.RenderFar();
        }

        float sceneResolutionScale = 1.0f, timeScale = 1.0f;

        private void OnInputFocus(bool hasFocus)
        {
            if (SteamVR.Active == false)
            {
                return;
            }

            if (hasFocus)
            {
                if (SteamVR.settings.pauseGameWhenDashboardVisible)
                {
                    Time.timeScale = timeScale;
                }

                SteamVRCamera.sceneResolutionScale = sceneResolutionScale;
            }
            else
            {
                if (SteamVR.settings.pauseGameWhenDashboardVisible)
                {
                    timeScale = Time.timeScale;
                    Time.timeScale = 0.0f;
                }

                sceneResolutionScale = SteamVRCamera.sceneResolutionScale;
                SteamVRCamera.sceneResolutionScale = 0.5f;
            }
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private string GetScreenshotFilename(uint screenshotHandle, EVRScreenshotPropertyFilenames screenshotPropertyFilename)
        {
            var error = EVRScreenshotError.None;
            var capacity = OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, null, 0, ref error);
            if (error != EVRScreenshotError.None && error != EVRScreenshotError.BufferTooSmall)
            {
                return null;
            }

            if (capacity > 1)
            {
                var result = new System.Text.StringBuilder((int)capacity);
                OpenVR.Screenshots.GetScreenshotPropertyFilename(screenshotHandle, screenshotPropertyFilename, result, capacity, ref error);
                if (error != EVRScreenshotError.None)
                {
                    return null;
                }

                return result.ToString();
            }
            return null;
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void OnRequestScreenshot(VREventT vrEvent)
        {
            var screenshotHandle = vrEvent.data.screenshot.handle;
            var screenshotType = (EVRScreenshotType)vrEvent.data.screenshot.type;

            if (screenshotType == EVRScreenshotType.StereoPanorama)
            {
                string previewFilename = GetScreenshotFilename(screenshotHandle, EVRScreenshotPropertyFilenames.Preview);
                string VRFilename = GetScreenshotFilename(screenshotHandle, EVRScreenshotPropertyFilenames.VR);

                if (previewFilename == null || VRFilename == null)
                {
                    return;
                }

                // Do the stereo panorama screenshot
                // Figure out where the view is
                GameObject screenshotPosition = new("screenshotPosition");
                screenshotPosition.transform.position = SteamVRRender.Top().transform.position;
                screenshotPosition.transform.rotation = SteamVRRender.Top().transform.rotation;
                screenshotPosition.transform.localScale = SteamVRRender.Top().transform.lossyScale;
                SteamVRUtils.TakeStereoScreenshot(screenshotHandle, screenshotPosition, 32, 0.064f, ref previewFilename, ref VRFilename);

                // and submit it
                OpenVR.Screenshots.SubmitScreenshot(screenshotHandle, screenshotType, previewFilename, VRFilename);
            }
        }

        private readonly EVRScreenshotType[] screenshotTypes = new EVRScreenshotType[] { EVRScreenshotType.StereoPanorama };

        private void OnEnable()
        {
            MelonCoroutines.Start(RenderLoop());
            SteamVREvents.InputFocus.Listen(OnInputFocus);
            SteamVREvents.System(EVREventType.VREventRequestScreenshot).Listen(OnRequestScreenshot);

            if (SteamVRSettings.instance.legacyMixedRealityCamera)
            {
                SteamVRExternalCameraLegacyManager.SubscribeToNewPoses();
            }

            UnityHooks.PreUpdate += OnBeforeRender;

            if (SteamVR.initializedState == SteamVR.InitializedStates.InitializeSuccess)
            {
                OpenVR.Screenshots.HookScreenshot(screenshotTypes);
            }
            else
            {
                SteamVREvents.Initialized.Listen(OnSteamVRInitialized);
            }
        }

        private void OnSteamVRInitialized(bool success)
        {
            if (success)
            {
                MelonLogger.Msg("hooking screenshots");
                OpenVR.Screenshots.HookScreenshot(screenshotTypes);
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            SteamVREvents.InputFocus.Remove(OnInputFocus);
            SteamVREvents.System(EVREventType.VREventRequestScreenshot).Remove(OnRequestScreenshot);

            UnityHooks.PreUpdate -= OnBeforeRender;

            if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
            {
                SteamVREvents.Initialized.Remove(OnSteamVRInitialized);
            }
        }

        public void UpdatePoses()
        {
            var compositor = OpenVR.Compositor;
            if (compositor != null)
            {
                compositor.GetLastPoses(poses, gamePoses);
                SteamVREvents.NewPoses.Send(poses);
                SteamVREvents.NewPosesApplied.Send();
            }
        }

        void OnBeforeRender()
        {
            if (SteamVR.Active == false)
            {
                MelonLogger.Msg("steamvr not active");
                return;
            }

            if (SteamVR.settings.IsPoseUpdateMode(SteamVRUpdateModes.OnPreCull))
            {
                UpdatePoses();
                //MelonLogger.Msg("updated Poses");
            }
            else
            {

                MelonLogger.Msg("Poses were not updated");
            }
        }

        public void Update()
        {
            if (SteamVR.Active == false)
            {
                return;
            }

            // Dispatch any OpenVR events.
            var system = OpenVR.System;
            if (system == null)
            {
                return;
            }

            //MelonLogger.Msg("polling vrevents");
            UpdatePoses();

            var vrEvent = new VREventT();
            var size = (uint)Marshal.SizeOf(typeof(VREventT));
            for (int i = 0; i < 64; i++)
            {
                if (!system.PollNextEvent(ref vrEvent, size))
                {
                    break;
                }

                switch ((EVREventType)vrEvent.eventType)
                {
                    case EVREventType.VREventInputFocusCaptured: // another app has taken focus (likely dashboard)
                        if (vrEvent.data.process.oldPid == 0)
                        {
                            SteamVREvents.InputFocus.Send(false);
                        }
                        break;
                    case EVREventType.VREventInputFocusReleased: // that app has released input focus
                        if (vrEvent.data.process.pid == 0)
                        {
                            SteamVREvents.InputFocus.Send(true);
                        }
                        break;
                    case EVREventType.VREventShowRenderModels:
                        SteamVREvents.HideRenderModels.Send(false);
                        break;
                    case EVREventType.VREventHideRenderModels:
                        SteamVREvents.HideRenderModels.Send(true);
                        break;
                    default:
                        SteamVREvents.System((EVREventType)vrEvent.eventType).Send(vrEvent);
                        break;
                }
            }

            // Ensure various settings to minimize latency.
            Application.targetFrameRate = -1;
            Application.runInBackground = true; // don't require companion window focus
            QualitySettings.maxQueuedFrames = -1;
            QualitySettings.vSyncCount = 0; // this applies to the companion window

            if (SteamVR.settings.lockPhysicsUpdateRateToRenderFrequency && Time.timeScale > 0.0f)
            {
                var vr = SteamVR.Instance;
                if (vr != null && Application.isPlaying)
                {
                    //var timing = new Compositor_FrameTiming();
                    //timing.m_nSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(Compositor_FrameTiming));
                    //vr.compositor.GetFrameTiming(ref timing, 0);

                    Time.fixedDeltaTime = Time.timeScale / vr.hmdDisplayFrequency;
                }
            }
        }
    }
}
