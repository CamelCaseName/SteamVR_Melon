//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Access to SteamVR system (hmd) and compositor (distort) interfaces.
//
//=============================================================================

using Assets.SteamVR_Melon.Standalone;
using MelonLoader;
using SteamVR_Melon.Scripts;
using SteamVR_Melon.Standalone;
using SteamVR_Melon.Util;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace Valve.VR
{
    public class SteamVR : System.IDisposable
    {
        // Use this to check if SteamVR is currently active without attempting
        // to activate it in the process.
        public static bool active { get { return _instance != null; } }

        private static bool ___enabled = true;
        // Set this to false to keep from auto-initializing when calling SteamVR.instance.
        private static bool _enabled
        {
            get => ___enabled;
            set
            {
                //MelonLogger.Msg("setting enabled to " + value);
                ___enabled = value;
            }
        }
        public static bool enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                _enabled = value;

                if (_enabled)
                {
                    Initialize();
                }
                else
                {
                    SafeDispose();
                }
            }
        }

        private static SteamVR ___instance;
        private static SteamVR _instance
        {
            get
            {
                return ___instance;
            }
            set
            {
                //MelonLogger.Msg("setting instance to " + value);
                ___instance = value;
            }
        }
        public static SteamVR instance
        {
            get
            {
                if (!enabled)
                {
                    return null;
                }

                if (_instance == null)
                {
                    MelonLogger.Msg("Instance was null, creating new");
                    _instance = CreateInstance();

                    // If init failed, then auto-disable so scripts don't continue trying to re-initialize things.
                    if (_instance == null)
                    {
                        MelonLogger.Msg("createInstance failed");
                        _enabled = false;
                    }
                }

                return _instance;
            }
        }

        public enum InitializedStates
        {
            None,
            Initializing,
            InitializeSuccess,
            InitializeFailure,
        }

        public static InitializedStates initializedState = InitializedStates.None;

        public static void Initialize(bool forceUnityVRMode = false)
        {
            if (forceUnityVRMode)
            {
                SteamVRBehaviour.instance.InitializeSteamVR(true);
                return;
            }
            else
            {
                if (_instance == null)
                {
                    _instance = CreateInstance();
                    if (_instance == null)
                    {
                        _enabled = false;
                    }
                }
            }

            if (_enabled)
            {
                SteamVRBehaviour.Initialize(forceUnityVRMode);
            }
        }

        public static bool usingNativeSupport
        {
            get { return XRDevice.GetNativePtr() != System.IntPtr.Zero; }
        }

        public static SteamVRSettings settings { get; private set; }

        private static void ReportGeneralErrors()
        {
            string errorLog = "[HPVR] Initialization failed. ";

            if (XRSettings.enabled == false)
            {
                errorLog += "VR may be disabled in player settings. Go to player settings in the editor and check the 'Virtual Reality Supported' checkbox'. ";
            }

            if (XRSettings.supportedDevices != null && XRSettings.supportedDevices.Length > 0)
            {
                if (XRSettings.supportedDevices.Contains("OpenVR") == false)
                {
                    errorLog += "OpenVR is not in your list of supported virtual reality SDKs. Add it to the list in player settings. ";
                }
                else if (XRSettings.supportedDevices.First().Contains("OpenVR") == false)
                {
                    errorLog += "OpenVR is not first in your list of supported virtual reality SDKs. This is okay, but if you have an Oculus device plugged in, and Oculus above OpenVR in this list, it will try and use the Oculus SDK instead of OpenVR. ";
                }
            }
            else
            {
                errorLog += "You have no SDKs in your Player Settings list of supported virtual reality SDKs. Add OpenVR to it. ";
            }

            errorLog += "To attempt to force OpenVR initialization call SteamVR.Initialize(true). ";

            MelonLogger.Warning(errorLog);
        }

        private static SteamVR CreateInstance()
        {
            initializedState = InitializedStates.Initializing;

            try
            {
                var error = EVRInitError.None;

                PluginImporter.LoadPlugin(OpenVRMagic.openvrApi);
                UnityHooks.Init();
                //VRShaders.TryLoadShaders();

                OpenVR.Init(ref error, EVRApplicationType.VRApplicationScene, "");

                if (error == EVRInitError.InitHmdNotFound)
                {
                    MelonLogger.Error("#####################################################################");
                    MelonLogger.Error("###                                                               ###");
                    MelonLogger.Error("###  YOU NEED TO HAVE YOUR VR HEADSET CONNECTED BEFORE STARTING!  ###");
                    MelonLogger.Error("###                                                               ###");
                    MelonLogger.Error("#####################################################################");
                    initializedState = InitializedStates.InitializeFailure;
                    SteamVREvents.Initialized.Send(false);
                    return null;
                }

                MelonLogger.Msg("openvr init returned: " + error);
                CVRSystem system = OpenVR.System;
                string manifestFile = GetManifestFile();
                MelonLogger.Msg("manifest file: " + manifestFile);
                EVRApplicationError evrapplicationError = OpenVR.Applications.AddApplicationManifest(manifestFile, true);
                if (evrapplicationError != EVRApplicationError.None)
                {
                    if (!SteamVR.usingNativeSupport)
                    {
                        ReportGeneralErrors();
                        initializedState = InitializedStates.InitializeFailure;
                        SteamVREvents.Initialized.Send(false);
                        return null;
                    }
                }

                // Verify common interfaces are valid.

                OpenVR.GetGenericInterface(OpenVR.IVRCompositorVersion, ref error);
                if (error != EVRInitError.None)
                {
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    ReportGeneralErrors();
                    SteamVREvents.Initialized.Send(false);
                    return null;
                }

                OpenVR.GetGenericInterface(OpenVR.IVROverlayVersion, ref error);
                if (error != EVRInitError.None)
                {
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    SteamVREvents.Initialized.Send(false);
                    return null;
                }

                OpenVR.GetGenericInterface(OpenVR.IVRInputVersion, ref error);
                if (error != EVRInitError.None)
                {
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    SteamVREvents.Initialized.Send(false);
                    return null;
                }

                settings = SteamVRSettings.instance;
                if (Application.isEditor)
                {
                    IdentifyEditorApplication();
                }

                SteamVRInput.IdentifyActionsFile();
                if (SteamVRSettings.instance.inputUpdateMode != SteamVRUpdateModes.Nothing || SteamVRSettings.instance.poseUpdateMode != SteamVRUpdateModes.Nothing)
                {
                    MelonLogger.Msg("activating steamvr input");
                    SteamVRInput.Initialize();
                }
            }
            catch (System.Exception e)
            {
                MelonLogger.Error("[HPVR] " + e);
                SteamVREvents.Initialized.Send(false);
                return null;
            }

            _enabled = true;
            initializedState = InitializedStates.InitializeSuccess;
            SteamVREvents.Initialized.Send(true);
            MelonLogger.Msg("returning SteamVR Object");
            return new SteamVR();
        }

        static void ReportError(EVRInitError error)
        {
            switch (error)
            {
                case EVRInitError.None:
                    break;
                case EVRInitError.VendorSpecificUnableToConnectToOculusRuntime:
                    MelonLoader.MelonLogger.Warning("[HPVR] Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
                    break;
                case EVRInitError.InitVRClientDLLNotFound:
                    MelonLoader.MelonLogger.Warning("[HPVR] Drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
                    break;
                case EVRInitError.DriverRuntimeOutOfDate:
                    MelonLoader.MelonLogger.Warning("[HPVR] Initialization Failed!  Make sure device's runtime is up to date.");
                    break;
                default:
                    MelonLoader.MelonLogger.Warning("[HPVR] " + OpenVR.GetStringForHmdError(error));
                    break;
            }
        }

        // native interfaces
        public CVRSystem hmd { get; private set; }
        public CVRCompositor compositor { get; private set; }
        public CVROverlay overlay { get; private set; }

        // tracking status
        static public bool initializing { get; private set; }
        static public bool calibrating { get; private set; }
        static public bool outOfRange { get; private set; }

        static public bool[] connected = new bool[OpenVR.kUnMaxTrackedDeviceCount];

        // render values
        public float sceneWidth { get; private set; }
        public float sceneHeight { get; private set; }
        public float aspect { get; private set; }
        public float fieldOfView { get; private set; }
        public Vector2 tanHalfFov { get; private set; }
        public VRTextureBoundsT[] textureBounds { get; private set; }
        public SteamVRUtils.RigidTransform[] eyes { get; private set; }
        public ETextureType textureType;

        // hmd properties
        public string hmdTrackingSystemName { get { return GetStringProperty(ETrackedDeviceProperty.PropTrackingSystemNameString); } }
        public string hmdModelNumber { get { return GetStringProperty(ETrackedDeviceProperty.PropModelNumberString); } }
        public string hmdSerialNumber { get { return GetStringProperty(ETrackedDeviceProperty.PropSerialNumberString); } }
        public string hmdType { get { return GetStringProperty(ETrackedDeviceProperty.PropControllerTypeString); } }

        public float hmdSecondsFromVsyncToPhotons { get { return GetFloatProperty(ETrackedDeviceProperty.PropSecondsFromVsyncToPhotonsFloat); } }
        public float hmdDisplayFrequency { get { return GetFloatProperty(ETrackedDeviceProperty.PropDisplayFrequencyFloat); } }

        public EDeviceActivityLevel GetHeadsetActivityLevel()
        {
            return OpenVR.System.GetTrackedDeviceActivityLevel(OpenVR.kUnTrackedDeviceIndexHmd);
        }

        public string GetTrackedDeviceString(uint deviceId)
        {
            var error = ETrackedPropertyError.TrackedPropSuccess;
            var capacity = hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.PropAttachedDeviceIdString, null, 0, ref error);
            if (capacity > 1)
            {
                var result = new System.Text.StringBuilder((int)capacity);
                hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.PropAttachedDeviceIdString, result, capacity, ref error);
                return result.ToString();
            }
            return null;
        }

        public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId = OpenVR.kUnTrackedDeviceIndexHmd)
        {
            var error = ETrackedPropertyError.TrackedPropSuccess;
            var capactiy = hmd.GetStringTrackedDeviceProperty(deviceId, prop, null, 0, ref error);
            if (capactiy > 1)
            {
                var result = new System.Text.StringBuilder((int)capactiy);
                hmd.GetStringTrackedDeviceProperty(deviceId, prop, result, capactiy, ref error);
                return result.ToString();
            }
            return (error != ETrackedPropertyError.TrackedPropSuccess) ? error.ToString() : "<unknown>";
        }

        public float GetFloatProperty(ETrackedDeviceProperty prop, uint deviceId = OpenVR.kUnTrackedDeviceIndexHmd)
        {
            var error = ETrackedPropertyError.TrackedPropSuccess;
            return hmd.GetFloatTrackedDeviceProperty(deviceId, prop, ref error);
        }

        private static bool runningTemporarySession = false;
        public static bool InitializeTemporarySession(bool initInput = false)
        {
            if (Application.isEditor)
            {
                //bool needsInit = (!active && !usingNativeSupport && !runningTemporarySession);

                EVRInitError initError = EVRInitError.None;
                OpenVR.GetGenericInterface(OpenVR.IVRCompositorVersion, ref initError);
                bool needsInit = initError != EVRInitError.None;

                if (needsInit)
                {
                    EVRInitError error = EVRInitError.None;
                    OpenVR.Init(ref error, EVRApplicationType.VRApplicationOverlay);

                    if (error != EVRInitError.None)
                    {
                        MelonLoader.MelonLogger.Error("[HPVR] Error during OpenVR Init: " + error.ToString());
                        return false;
                    }

                    IdentifyEditorApplication(false);

                    SteamVRInput.IdentifyActionsFile(false);

                    runningTemporarySession = true;
                }

                if (initInput)
                {
                    SteamVRInput.Initialize(true);
                }

                return needsInit;
            }

            return false;
        }

        public static void ExitTemporarySession()
        {
            if (runningTemporarySession)
            {
                OpenVR.Shutdown();
                runningTemporarySession = false;
            }
        }

        public const string defaultUnityAppKeyTemplate = "application.generated.unity.{0}.exe";
        public const string defaultAppKeyTemplate = "application.generated.{0}";

        public static string GenerateAppKey()
        {
            string productName = GenerateCleanProductName();

            return string.Format(defaultUnityAppKeyTemplate, productName);
        }

        public static string GenerateCleanProductName()
        {
            string productName = Application.productName;
            if (string.IsNullOrEmpty(productName))
            {
                productName = "unnamed_product";
            }
            else
            {
                productName = System.Text.RegularExpressions.Regex.Replace(Application.productName, "[^\\w\\._]", "");
                productName = productName.ToLower();
            }

            return productName;
        }

        private static string GetManifestFile()
        {
            string currentPath = Application.dataPath;
            int lastIndex = currentPath.LastIndexOf('/');
            currentPath = currentPath[..lastIndex];

            string fullPath = Path.Combine(currentPath, "unityProject.vrmanifest");

            FileInfo fullManifestPath = new(SteamVRInput.GetActionsFilePath());

            if (File.Exists(fullPath))
            {
                string jsonText = File.ReadAllText(fullPath);
                SteamVRInputManifestFile existingFile = Newtonsoft.Json.JsonConvert.DeserializeObject<SteamVRInputManifestFile>(jsonText);

                if (existingFile != null && existingFile.applications != null && existingFile.applications.Count > 0 &&
                    existingFile.applications[0].app_key != SteamVRSettings.instance.editorAppKey)
                {
                    MelonLoader.MelonLogger.Msg("[HPVR] Deleting existing VRManifest because it has a different app key.");
                    FileInfo existingInfo = new(fullPath);
                    if (existingInfo.IsReadOnly)
                    {
                        existingInfo.IsReadOnly = false;
                    }

                    existingInfo.Delete();
                }

                if (existingFile != null && existingFile.applications != null && existingFile.applications.Count > 0 &&
                    existingFile.applications[0].action_manifest_path != fullManifestPath.FullName)
                {
                    MelonLoader.MelonLogger.Msg("[HPVR] Deleting existing VRManifest because it has a different action manifest path:" +
                        "\nExisting:" + existingFile.applications[0].action_manifest_path +
                        "\nNew: " + fullManifestPath.FullName);
                    FileInfo existingInfo = new(fullPath);
                    if (existingInfo.IsReadOnly)
                    {
                        existingInfo.IsReadOnly = false;
                    }

                    existingInfo.Delete();
                }
            }

            if (File.Exists(fullPath) == false)
            {
                SteamVRInputManifestFile manifestFile = new()
                {
                    source = "Unity"
                };
                SteamVRInputManifestFileApplication manifestApplication = new()
                {
                    app_key = SteamVRSettings.instance.editorAppKey,
                    action_manifest_path = fullManifestPath.FullName,
                    launch_type = "url",
                    //manifestApplication.binary_path_windows = SteamVR_Utils.ConvertToForwardSlashes(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    //manifestApplication.binary_path_linux = SteamVR_Utils.ConvertToForwardSlashes(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    //manifestApplication.binary_path_osx = SteamVR_Utils.ConvertToForwardSlashes(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    url = "steam://launch/"
                };
                manifestApplication.strings.Add("en_us", new SteamVRInputManifestFileApplicationString() { name = string.Format("{0} [Testing]", Application.productName) });

                /*
                var bindings = new System.Collections.Generic.List<SteamVR_Input_ManifestFile_Application_Binding>();

                SteamVR_Input.InitializeFile();
                if (SteamVR_Input.actionFile != null)
                {
                    string[] bindingFiles = SteamVR_Input.actionFile.GetFilesToCopy(true);
                    if (bindingFiles.Length == SteamVR_Input.actionFile.default_bindings.Count)
                    {
                        for (int bindingIndex = 0; bindingIndex < bindingFiles.Length; bindingIndex++)
                        {
                            SteamVR_Input_ManifestFile_Application_Binding binding = new SteamVR_Input_ManifestFile_Application_Binding();
                            binding.binding_url = bindingFiles[bindingIndex];
                            binding.controller_type = SteamVR_Input.actionFile.default_bindings[bindingIndex].controller_type;
                            bindings.Add(binding);
                        }
                        manifestApplication.bindings = bindings;
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Error("[HPVR] Mismatch in available binding files.");
                    }
                }
                else
                {
                    MelonLoader.MelonLogger.Error("[HPVR] Could not load actions file.");
                }
                */

                manifestFile.applications = new System.Collections.Generic.List<SteamVRInputManifestFileApplication>();
                manifestFile.applications.Add(manifestApplication);

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(manifestFile, Newtonsoft.Json.Formatting.Indented,
                    new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore });

                File.WriteAllText(fullPath, json);
            }

            return fullPath;
        }

        private static void IdentifyEditorApplication(bool showLogs = true)
        {
            //bool isInstalled = OpenVR.Applications.IsApplicationInstalled(SteamVR_Settings.instance.editorAppKey);

            if (string.IsNullOrEmpty(SteamVRSettings.instance.editorAppKey))
            {
                MelonLoader.MelonLogger.Error("[HPVR] Critical Error identifying application. EditorAppKey is null or empty. Input may not work.");
                return;
            }

            string manifestPath = GetManifestFile();

            EVRApplicationError addManifestErr = OpenVR.Applications.AddApplicationManifest(manifestPath, true);
            if (addManifestErr != EVRApplicationError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] Error adding vr manifest file: " + addManifestErr.ToString());
            }
            else
            {
                if (showLogs)
                {
                    MelonLoader.MelonLogger.Msg("[HPVR] Successfully added VR manifest to SteamVR");
                }
            }

            int processId = System.Diagnostics.Process.GetCurrentProcess().Id;
            EVRApplicationError applicationIdentifyErr = OpenVR.Applications.IdentifyApplication((uint)processId, SteamVRSettings.instance.editorAppKey);

            if (applicationIdentifyErr != EVRApplicationError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] Error identifying application: " + applicationIdentifyErr.ToString());
            }
            else
            {
                if (showLogs)
                {
                    MelonLoader.MelonLogger.Msg(string.Format("[HPVR] Successfully identified process as editor project to SteamVR ({0})", SteamVRSettings.instance.editorAppKey));
                }
            }
        }

        #region Event callbacks

        private void OnInitializing(bool initializing)
        {
            SteamVR.initializing = initializing;
        }

        private void OnCalibrating(bool calibrating)
        {
            SteamVR.calibrating = calibrating;
        }

        private void OnOutOfRange(bool outOfRange)
        {
            SteamVR.outOfRange = outOfRange;
        }

        private void OnDeviceConnected(int i, bool connected)
        {
            SteamVR.connected[i] = connected;
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void OnNewPoses(TrackedDevicePoseT[] poses)
        {
            // Update eye offsets to account for IPD changes.
            eyes[0] = new SteamVRUtils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.EyeLeft));
            eyes[1] = new SteamVRUtils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.EyeRight));

            for (int i = 0; i < poses.Length; i++)
            {
                var connected = poses[i].bDeviceIsConnected;
                if (connected != SteamVR.connected[i])
                {
                    SteamVREvents.DeviceConnected.Send(i, connected);
                }
            }

            if (poses.Length > OpenVR.kUnTrackedDeviceIndexHmd)
            {
                var result = poses[(int)OpenVR.kUnTrackedDeviceIndexHmd].eTrackingResult;

                var initializing = result == ETrackingResult.Uninitialized;
                if (initializing != SteamVR.initializing)
                {
                    SteamVREvents.Initializing.Send(initializing);
                }

                var calibrating =
                    result == ETrackingResult.CalibratingInProgress ||
                    result == ETrackingResult.CalibratingOutOfRange;
                if (calibrating != SteamVR.calibrating)
                {
                    SteamVREvents.Calibrating.Send(calibrating);
                }

                var outOfRange =
                    result == ETrackingResult.RunningOutOfRange ||
                    result == ETrackingResult.CalibratingOutOfRange;
                if (outOfRange != SteamVR.outOfRange)
                {
                    SteamVREvents.OutOfRange.Send(outOfRange);
                }
            }
        }

        #endregion

        private SteamVR()
        {
            hmd = OpenVR.System;
            MelonLoader.MelonLogger.Msg($"Initialized. Connected to {hmdTrackingSystemName} : {hmdModelNumber} : {hmdSerialNumber} :: {hmdType}");

            compositor = OpenVR.Compositor;
            overlay = OpenVR.Overlay;

            // Setup render values
            uint w = 0, h = 0;
            hmd.GetRecommendedRenderTargetSize(ref w, ref h);
            sceneWidth = (float)w;
            sceneHeight = (float)h;
            MelonLogger.Msg($"hmd res: {sceneWidth}:{sceneHeight}");

            float l_left = 0.0f, l_right = 0.0f, l_top = 0.0f, l_bottom = 0.0f;
            hmd.GetProjectionRaw(EVREye.EyeLeft, ref l_left, ref l_right, ref l_top, ref l_bottom);

            float r_left = 0.0f, r_right = 0.0f, r_top = 0.0f, r_bottom = 0.0f;
            hmd.GetProjectionRaw(EVREye.EyeRight, ref r_left, ref r_right, ref r_top, ref r_bottom);
            MelonLogger.Msg($"hmd projection: {r_left}{r_right}{r_top}{r_bottom}");

            tanHalfFov = new Vector2(
                Mathf.Max(-l_left, l_right, -r_left, r_right),
                Mathf.Max(-l_top, l_bottom, -r_top, r_bottom));

            textureBounds = new VRTextureBoundsT[2];

            textureBounds[0].uMin = 0.5f + 0.5f * l_left / tanHalfFov.x;
            textureBounds[0].uMax = 0.5f + 0.5f * l_right / tanHalfFov.x;
            textureBounds[0].vMin = 0.5f - 0.5f * l_bottom / tanHalfFov.y;
            textureBounds[0].vMax = 0.5f - 0.5f * l_top / tanHalfFov.y;

            textureBounds[1].uMin = 0.5f + 0.5f * r_left / tanHalfFov.x;
            textureBounds[1].uMax = 0.5f + 0.5f * r_right / tanHalfFov.x;
            textureBounds[1].vMin = 0.5f - 0.5f * r_bottom / tanHalfFov.y;
            textureBounds[1].vMax = 0.5f - 0.5f * r_top / tanHalfFov.y;

            // Grow the recommended size to account for the overlapping fov
            sceneWidth = sceneWidth / Mathf.Max(textureBounds[0].uMax - textureBounds[0].uMin, textureBounds[1].uMax - textureBounds[1].uMin);
            sceneHeight = sceneHeight / Mathf.Max(textureBounds[0].vMax - textureBounds[0].vMin, textureBounds[1].vMax - textureBounds[1].vMin);

            aspect = tanHalfFov.x / tanHalfFov.y;
            fieldOfView = 2.0f * Mathf.Atan(tanHalfFov.y) * Mathf.Rad2Deg;

            eyes = new SteamVRUtils.RigidTransform[] {
            new(hmd.GetEyeToHeadTransform(EVREye.EyeLeft)),
            new(hmd.GetEyeToHeadTransform(EVREye.EyeRight)) };

            textureType = ETextureType.DirectX;

            SteamVREvents.Initializing.Listen(OnInitializing);
            SteamVREvents.Calibrating.Listen(OnCalibrating);
            SteamVREvents.OutOfRange.Listen(OnOutOfRange);
            SteamVREvents.DeviceConnected.Listen(OnDeviceConnected);
            SteamVREvents.NewPoses.Listen(OnNewPoses);
        }

        ~SteamVR()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            SteamVREvents.Initializing.Remove(OnInitializing);
            SteamVREvents.Calibrating.Remove(OnCalibrating);
            SteamVREvents.OutOfRange.Remove(OnOutOfRange);
            SteamVREvents.DeviceConnected.Remove(OnDeviceConnected);
            SteamVREvents.NewPoses.Remove(OnNewPoses);

            _instance = null;
        }

        // Use this interface to avoid accidentally creating the instance in the process of attempting to dispose of it.
        public static void SafeDispose()
        {
            _instance?.Dispose();
        }
    }
}