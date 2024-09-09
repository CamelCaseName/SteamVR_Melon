using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using Newtonsoft.Json;
using Standalone;
using SteamVR_Melon.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Valve.VR.InteractionSystem;
using Mathf = HPVR.Util.Mathf;

namespace Valve.VR
{

    public class SteamVR : IDisposable
    {

        public static bool active
        {
            get
            {
                return _instance != null;
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
                    Initialize(false);
                    return;
                }
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
                    _instance = CreateInstance();
                    if (_instance == null)
                    {
                        _enabled = false;
                    }
                }
                return _instance;
            }
        }

        public static void Initialize(bool forceUnityVRMode = false)
        {
            if (forceUnityVRMode)
            {
                SteamVR_Behaviour.instance.InitializeSteamVR(true);
                return;
            }
            if (_instance == null)
            {
                _instance = CreateInstance();
                if (_instance == null)
                {
                    _enabled = false;
                }
            }
            if (_enabled)
            {
                SteamVR_Behaviour.Initialize(forceUnityVRMode);
            }
        }

        public static bool usingNativeSupport
        {
            get
            {
                return XRDevice.GetNativePtr() != IntPtr.Zero;
            }
        }

        public static SteamVR_Settings settings { get; private set; }

        private static void ReportGeneralErrors()
        {
            string text = "[HPVR] Initialization failed. ";
            if (!XRSettings.enabled)
            {
                text += "VR may be disabled in player settings. Go to player settings in the editor and check the 'Virtual Reality Supported' checkbox'. ";
            }
            if (XRSettings.supportedDevices != null && XRSettings.supportedDevices.Length != 0)
            {
                if (!XRSettings.supportedDevices.Contains("OpenVR"))
                {
                    text += "OpenVR is not in your list of supported virtual reality SDKs. Add it to the list in player settings. ";
                }
                else if (!XRSettings.supportedDevices.First<string>().Contains("OpenVR"))
                {
                    text += "OpenVR is not first in your list of supported virtual reality SDKs. This is okay, but if you have an Oculus device plugged in, and Oculus above OpenVR in this list, it will try and use the Oculus SDK instead of OpenVR. ";
                }
            }
            else
            {
                text += "You have no SDKs in your Player Settings list of supported virtual reality SDKs. Add OpenVR to it. ";
            }
            text += "To force OpenVR initialization call SteamVR.Initialize(true). ";
            MelonLogger.Warning(text);
        }

        private static SteamVR CreateInstance()
        {
            initializedState = InitializedStates.Initializing;

            try
            {
                var error = EVRInitError.None;

                // Verify common interfaces are valid.

                OpenVR.Init(ref error, EVRApplicationType.VRApplication_Scene, "");
                MelonLogger.Msg("openvr init returned: " + error);
                CVRSystem system = OpenVR.System;
                string manifestFile = GetManifestFile();
                MelonLogger.Msg("manifest file: " + manifestFile);
                EVRApplicationError evrapplicationError = OpenVR.Applications.AddApplicationManifest(manifestFile, true);
                if (evrapplicationError != EVRApplicationError.None)
                {
                    UnityEngine.Debug.LogError("[HPVR] Error adding vr manifest file: " + evrapplicationError.ToString());
                }
                int id = Process.GetCurrentProcess().Id;
                OpenVR.Applications.IdentifyApplication((uint)id, SteamVR_Settings.instance.editorAppKey);
                UnityEngine.Debug.Log("Is HMD here? " + OpenVR.IsHmdPresent().ToString());

                OpenVR.GetGenericInterface(OpenVR.IVRCompositor_Version, ref error);
                if (error != EVRInitError.None)
                {
                    MelonLogger.Msg("IVRCompositor Version returned: " + error);
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    ReportGeneralErrors();
                    SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                OpenVR.GetGenericInterface(OpenVR.IVROverlay_Version, ref error);
                if (error != EVRInitError.None)
                {
                    MelonLogger.Msg(error);
                    MelonLogger.Msg("IVROverlay Version returned: " + error);
                    ReportError(error);
                    SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                OpenVR.GetGenericInterface(OpenVR.IVRInput_Version, ref error);
                if (error != EVRInitError.None)
                {
                    MelonLogger.Msg("IVRInput Version returned: " + error);
                    initializedState = InitializedStates.InitializeFailure;
                    ReportError(error);
                    SteamVR_Events.Initialized.Send(false);
                    return null;
                }

                settings = SteamVR_Settings.instance;

                if (Application.isEditor)
                    IdentifyEditorApplication();

                SteamVR_Input.IdentifyActionsFile();

                if (SteamVR_Settings.instance.inputUpdateMode != SteamVR_UpdateModes.Nothing || SteamVR_Settings.instance.poseUpdateMode != SteamVR_UpdateModes.Nothing)
                {
                    SteamVR_Input.Initialize();

                }
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
                SteamVR_Events.Initialized.Send(false);
                return null;
            }

            _enabled = true;
            initializedState = InitializedStates.InitializeSuccess;
            SteamVR_Events.Initialized.Send(true);
            return new SteamVR();
        }

        private static void ReportError(EVRInitError error)
        {
            if (error <= EVRInitError.Init_VRClientDLLNotFound)
            {
                if (error == EVRInitError.None)
                {
                    return;
                }
                if (error == EVRInitError.Init_VRClientDLLNotFound)
                {
                    MelonLogger.Warning("[HPVR] Drivers not found!  They can be installed via Steam under Library > Tools.  Visit http://steampowered.com to install Steam.");
                    return;
                }
            }
            else
            {
                if (error == EVRInitError.Driver_RuntimeOutOfDate)
                {
                    MelonLogger.Warning("[HPVR] Initialization Failed!  Make sure device's runtime is up to date.");
                    return;
                }
                if (error == EVRInitError.VendorSpecific_UnableToConnectToOculusRuntime)
                {
                    MelonLogger.Warning("[HPVR] Initialization Failed!  Make sure device is on, Oculus runtime is installed, and OVRService_*.exe is running.");
                    return;
                }
            }
            MelonLogger.Warning("[HPVR] " + OpenVR.GetStringForHmdError(error));
        }

        public CVRSystem hmd { get; private set; }

        public CVRCompositor compositor { get; private set; }

        public CVROverlay overlay { get; private set; }

        public static bool initializing { get; private set; }

        public static bool calibrating { get; private set; }

        public static bool outOfRange { get; private set; }

        public float sceneWidth { get; private set; }

        public float sceneHeight { get; private set; }

        public float aspect { get; private set; }

        public float fieldOfView { get; private set; }

        public Vector2 tanHalfFov { get; private set; }

        public VRTextureBounds_t[] textureBounds { get; private set; }

        public SteamVR_Utils.RigidTransform[] eyes { get; private set; }

        public string hmd_TrackingSystemName
        {
            get
            {
                return GetStringProperty(ETrackedDeviceProperty.Prop_TrackingSystemName_String, 0u);
            }
        }

        public string hmd_ModelNumber
        {
            get
            {
                return GetStringProperty(ETrackedDeviceProperty.Prop_ModelNumber_String, 0u);
            }
        }

        public string hmd_SerialNumber
        {
            get
            {
                return GetStringProperty(ETrackedDeviceProperty.Prop_SerialNumber_String, 0u);
            }
        }

        public float hmd_SecondsFromVsyncToPhotons
        {
            get
            {
                return GetFloatProperty(ETrackedDeviceProperty.Prop_SecondsFromVsyncToPhotons_Float, 0u);
            }
        }

        public float hmd_DisplayFrequency
        {
            get
            {
                return GetFloatProperty(ETrackedDeviceProperty.Prop_DisplayFrequency_Float, 0u);
            }
        }

        public EDeviceActivityLevel GetHeadsetActivityLevel()
        {
            return OpenVR.System.GetTrackedDeviceActivityLevel(0u);
        }

        public string GetTrackedDeviceString(uint deviceId)
        {
            ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
            uint stringTrackedDeviceProperty = hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, null, 0u, ref etrackedPropertyError);
            if (stringTrackedDeviceProperty > 1u)
            {
                StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
                hmd.GetStringTrackedDeviceProperty(deviceId, ETrackedDeviceProperty.Prop_AttachedDeviceId_String, stringBuilder, stringTrackedDeviceProperty, ref etrackedPropertyError);
                return stringBuilder.ToString();
            }
            return null;
        }

        public string GetStringProperty(ETrackedDeviceProperty prop, uint deviceId = 0u)
        {
            ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
            uint stringTrackedDeviceProperty = hmd.GetStringTrackedDeviceProperty(deviceId, prop, null, 0u, ref etrackedPropertyError);
            if (stringTrackedDeviceProperty > 1u)
            {
                StringBuilder stringBuilder = new StringBuilder((int)stringTrackedDeviceProperty);
                hmd.GetStringTrackedDeviceProperty(deviceId, prop, stringBuilder, stringTrackedDeviceProperty, ref etrackedPropertyError);
                return stringBuilder.ToString();
            }
            if (etrackedPropertyError == ETrackedPropertyError.TrackedProp_Success)
            {
                return "<unknown>";
            }
            return etrackedPropertyError.ToString();
        }

        public float GetFloatProperty(ETrackedDeviceProperty prop, uint deviceId = 0u)
        {
            ETrackedPropertyError etrackedPropertyError = ETrackedPropertyError.TrackedProp_Success;
            return hmd.GetFloatTrackedDeviceProperty(deviceId, prop, ref etrackedPropertyError);
        }

        public static bool InitializeTemporarySession(bool initInput = false)
        {
            if (Application.isEditor)
            {
                EVRInitError evrinitError = EVRInitError.None;
                OpenVR.GetGenericInterface("IVRCompositor_022", ref evrinitError);
                bool flag = evrinitError > EVRInitError.None;
                if (flag)
                {
                    EVRInitError evrinitError2 = EVRInitError.None;
                    OpenVR.Init(ref evrinitError2, EVRApplicationType.VRApplication_Overlay, "");
                    if (evrinitError2 != EVRInitError.None)
                    {
                        MelonLogger.Error("[HPVR] Error during OpenVR Init: " + evrinitError2.ToString());
                        return false;
                    }
                    IdentifyEditorApplication(false);
                    SteamVR_Input.IdentifyActionsFile(false);
                    runningTemporarySession = true;
                }
                if (initInput)
                {
                    SteamVR_Input.Initialize(true);
                }
                return flag;
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

        public static string GenerateAppKey()
        {
            string arg = GenerateCleanProductName();
            return string.Format("application.generated.unity.{0}.exe", arg);
        }

        public static string GenerateCleanProductName()
        {
            string text = Application.productName;
            if (string.IsNullOrEmpty(text))
            {
                text = "unnamed_product";
            }
            else
            {
                text = Regex.Replace(Application.productName, "[^\\w\\._]", "");
                text = text.ToLower();
            }
            return text;
        }

        private static string GetManifestFile()
        {
            string text = Application.dataPath;
            int num = text.LastIndexOf('/');
            text = text.Remove(num, text.Length - num);
            string text2 = Path.Combine(text, "unityProject.vrmanifest");
            FileInfo fileInfo = new FileInfo(SteamVR_Input.GetActionsFilePath(true));
            if (File.Exists(text2))
            {
                SteamVR_Input_ManifestFile steamVR_Input_ManifestFile = JsonConvert.DeserializeObject<SteamVR_Input_ManifestFile>(File.ReadAllText(text2));
                if (steamVR_Input_ManifestFile != null && steamVR_Input_ManifestFile.applications != null && steamVR_Input_ManifestFile.applications.Count > 0 && steamVR_Input_ManifestFile.applications[0].app_key != SteamVR_Settings.instance.editorAppKey)
                {
                    MelonLogger.Msg("[HPVR] Deleting existing VRManifest because it has a different app key.");
                    FileInfo fileInfo2 = new FileInfo(text2);
                    if (fileInfo2.IsReadOnly)
                    {
                        fileInfo2.IsReadOnly = false;
                    }
                    fileInfo2.Delete();
                }
                if (steamVR_Input_ManifestFile != null && steamVR_Input_ManifestFile.applications != null && steamVR_Input_ManifestFile.applications.Count > 0 && steamVR_Input_ManifestFile.applications[0].action_manifest_path != fileInfo.FullName)
                {
                    MelonLogger.Msg("[HPVR] Deleting existing VRManifest because it has a different action manifest path:\nExisting:" + steamVR_Input_ManifestFile.applications[0].action_manifest_path + "\nNew: " + fileInfo.FullName);
                    FileInfo fileInfo3 = new FileInfo(text2);
                    if (fileInfo3.IsReadOnly)
                    {
                        fileInfo3.IsReadOnly = false;
                    }
                    fileInfo3.Delete();
                }
            }
            if (!File.Exists(text2))
            {
                SteamVR_Input_ManifestFile steamVR_Input_ManifestFile2 = new SteamVR_Input_ManifestFile();
                steamVR_Input_ManifestFile2.source = "Unity";
                SteamVR_Input_ManifestFile_Application steamVR_Input_ManifestFile_Application = new SteamVR_Input_ManifestFile_Application();
                steamVR_Input_ManifestFile_Application.app_key = SteamVR_Settings.instance.editorAppKey;
                steamVR_Input_ManifestFile_Application.action_manifest_path = fileInfo.FullName;
                steamVR_Input_ManifestFile_Application.launch_type = "url";
                steamVR_Input_ManifestFile_Application.url = "steam://launch/";
                steamVR_Input_ManifestFile_Application.strings.Add("en_us", new SteamVR_Input_ManifestFile_ApplicationString
                {
                    name = string.Format("{0} VR", Application.productName)
                });
                steamVR_Input_ManifestFile2.applications = new List<SteamVR_Input_ManifestFile_Application>();
                steamVR_Input_ManifestFile2.applications.Add(steamVR_Input_ManifestFile_Application);
                string contents = JsonConvert.SerializeObject(steamVR_Input_ManifestFile2, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                File.WriteAllText(text2, contents);
            }
            return text2;
        }

        private static void IdentifyEditorApplication(bool showLogs = true)
        {
            if (string.IsNullOrEmpty(SteamVR_Settings.instance.editorAppKey))
            {
                MelonLogger.Error("[HPVR] Critical Error identifying application. EditorAppKey is null or empty. Input may not work.");
                return;
            }
            string manifestFile = GetManifestFile();
            EVRApplicationError evrapplicationError = OpenVR.Applications.AddApplicationManifest(manifestFile, true);
            if (evrapplicationError != EVRApplicationError.None)
            {
                MelonLogger.Error("[HPVR] Error adding vr manifest file: " + evrapplicationError.ToString());
            }
            else if (showLogs)
            {
                MelonLogger.Msg("[HPVR] Successfully added VR manifest to HPVR");
            }
            int id = Process.GetCurrentProcess().Id;
            EVRApplicationError evrapplicationError2 = OpenVR.Applications.IdentifyApplication((uint)id, SteamVR_Settings.instance.editorAppKey);
            if (evrapplicationError2 != EVRApplicationError.None)
            {
                MelonLogger.Error("[HPVR] Error identifying application: " + evrapplicationError2.ToString());
                return;
            }
            if (showLogs)
            {
                MelonLogger.Msg(string.Format("[HPVR] Successfully identified process as editor project to HPVR ({0})", SteamVR_Settings.instance.editorAppKey));
            }
        }

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

        private void OnNewPoses(TrackedDevicePose_t[] poses)
        {
            eyes[0] = new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Left));
            eyes[1] = new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Right));
            for (int i = 0; i < poses.Length; i++)
            {
                bool bDeviceIsConnected = poses[i].bDeviceIsConnected;
                if (bDeviceIsConnected != connected[i])
                {
                    SteamVR_Events.DeviceConnected.Send(i, bDeviceIsConnected);
                }
            }
            if ((long)poses.Length > 0L)
            {
                ETrackingResult eTrackingResult = poses[0].eTrackingResult;
                bool flag = eTrackingResult == ETrackingResult.Uninitialized;
                if (flag != initializing)
                {
                    SteamVR_Events.Initializing.Send(flag);
                }
                bool flag2 = eTrackingResult == ETrackingResult.Calibrating_InProgress || eTrackingResult == ETrackingResult.Calibrating_OutOfRange;
                if (flag2 != calibrating)
                {
                    SteamVR_Events.Calibrating.Send(flag2);
                }
                bool flag3 = eTrackingResult == ETrackingResult.Running_OutOfRange || eTrackingResult == ETrackingResult.Calibrating_OutOfRange;
                if (flag3 != outOfRange)
                {
                    SteamVR_Events.OutOfRange.Send(flag3);
                }
            }
        }

        static bool classesRegistered = false;

        /// <summary>
        /// Inject types into il2cpp pre-emptively to prevent errors using this method
        /// </summary>
        public static void PreRegisterIL2CPPClasses()
        {
            if (classesRegistered)
            {
                return;
            }
            classesRegistered = true;
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_ActivateActionSetOnLoad>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Boolean>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Single>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Skeleton>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Vector2>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Vector3>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Behaviour_Pose>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Camera>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_CameraFlip>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_CameraMask>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Ears>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_ExternalCamera>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Frustum>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Fade>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_GameView>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_IK>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_LoadLevel>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Menu>();
            //todo fix
            //ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Overlay>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_PlayArea>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_RenderModel>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Render>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Skeleton_Poser>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_SphericalProjection>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_Skybox>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_TrackingReferenceManager>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_TrackedObject>();
            ClassInjector.RegisterTypeInIl2Cpp<SteamVR_UpdatePoses>();
            ClassInjector.RegisterTypeInIl2Cpp<VelocityEstimator>();
        }

        static bool m_setup;
        private SteamVR()
        {
            // Sometimes during game exit SteamVR will attempt to re-create itself, for some reason.
            if (m_setup)
            {
                return;
            }
            m_setup = true;
            ExternalPluginFunctionExtractor.GetLoadPluginFunction();

            PreRegisterIL2CPPClasses();
            UnityHooks.Init();

            hmd = OpenVR.System;
            MelonLogger.Msg("Initialized. Connected to " + hmd_TrackingSystemName + ":" + hmd_SerialNumber);
            compositor = OpenVR.Compositor;
            overlay = OpenVR.Overlay;
            uint num = 0u;
            uint num2 = 0u;
            hmd.GetRecommendedRenderTargetSize(ref num, ref num2);
            sceneWidth = num;
            sceneHeight = num2;
            float num3 = 0f;
            float num4 = 0f;
            float num5 = 0f;
            float num6 = 0f;
            hmd.GetProjectionRaw(EVREye.Eye_Left, ref num3, ref num4, ref num5, ref num6);
            float num7 = 0f;
            float num8 = 0f;
            float num9 = 0f;
            float num10 = 0f;
            hmd.GetProjectionRaw(EVREye.Eye_Right, ref num7, ref num8, ref num9, ref num10);
            tanHalfFov = new Vector2(Mathf.Max(new float[]
            {
                -num3,
                num4,
                -num7,
                num8
            }), Mathf.Max(new float[]
            {
                -num5,
                num6,
                -num9,
                num10
            }));
            textureBounds = new VRTextureBounds_t[2];
            textureBounds[0].uMin = 0.5f + 0.5f * num3 / tanHalfFov.x;
            textureBounds[0].uMax = 0.5f + 0.5f * num4 / tanHalfFov.x;
            textureBounds[0].vMin = 0.5f - 0.5f * num6 / tanHalfFov.y;
            textureBounds[0].vMax = 0.5f - 0.5f * num5 / tanHalfFov.y;
            textureBounds[1].uMin = 0.5f + 0.5f * num7 / tanHalfFov.x;
            textureBounds[1].uMax = 0.5f + 0.5f * num8 / tanHalfFov.x;
            textureBounds[1].vMin = 0.5f - 0.5f * num10 / tanHalfFov.y;
            textureBounds[1].vMax = 0.5f - 0.5f * num9 / tanHalfFov.y;
            sceneWidth /= Mathf.Max(textureBounds[0].uMax - textureBounds[0].uMin, textureBounds[1].uMax - textureBounds[1].uMin);
            sceneHeight /= Mathf.Max(textureBounds[0].vMax - textureBounds[0].vMin, textureBounds[1].vMax - textureBounds[1].vMin);
            aspect = tanHalfFov.x / tanHalfFov.y;
            fieldOfView = 2f * Mathf.Atan(tanHalfFov.y) * 57.29578f;
            eyes = new SteamVR_Utils.RigidTransform[]
            {
                new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Left)),
                new SteamVR_Utils.RigidTransform(hmd.GetEyeToHeadTransform(EVREye.Eye_Right))
            };
            GraphicsDeviceType graphicsDeviceType = SystemInfo.graphicsDeviceType;
            if (graphicsDeviceType <= GraphicsDeviceType.OpenGLES3)
            {
                if (graphicsDeviceType != GraphicsDeviceType.OpenGLES2 && graphicsDeviceType != GraphicsDeviceType.OpenGLES3)
                {
                    goto IL_3F8;
                }
            }
            else if (graphicsDeviceType != GraphicsDeviceType.OpenGLCore)
            {
                if (graphicsDeviceType == GraphicsDeviceType.Vulkan)
                {
                    textureType = ETextureType.Vulkan;
                    goto IL_3FF;
                }
                goto IL_3F8;
            }
            textureType = ETextureType.OpenGL;
            goto IL_3FF;
        IL_3F8:
            textureType = ETextureType.DirectX;
        IL_3FF:

            SteamVR_Events.Initializing.Listen(OnInitializing);
            SteamVR_Events.Calibrating.Listen(OnCalibrating);
            SteamVR_Events.OutOfRange.Listen(OnOutOfRange);
            SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected);
            SteamVR_Events.NewPoses.Listen(OnNewPoses);
        }

        ~SteamVR()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            SteamVR_Events.Initializing.Remove(OnInitializing);
            SteamVR_Events.Calibrating.Remove(OnCalibrating);
            SteamVR_Events.OutOfRange.Remove(OnOutOfRange);
            SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected);
            SteamVR_Events.NewPoses.Remove(OnNewPoses);
            _instance = null;
        }

        public static void SafeDispose()
        {
            if (_instance != null)
            {
                _instance.Dispose();
            }
        }

        private static bool _enabled = true;

        private static SteamVR _instance;

        public static SteamVR.InitializedStates initializedState = InitializedStates.None;

        public static bool[] connected = new bool[64];

        public ETextureType textureType;

        private static bool runningTemporarySession = false;

        public const string defaultUnityAppKeyTemplate = "application.generated.unity.{0}.exe";

        public const string defaultAppKeyTemplate = "application.generated.{0}";

        public enum InitializedStates
        {

            None,

            Initializing,

            InitializeSuccess,

            InitializeFailure
        }

        public class OpenVRMagic
        {

            public static string DLLName = "openvr_api";

            public const int k_nRenderEventID_WaitGetPoses = 201510020;

            public const int k_nRenderEventID_SubmitL = 201510021;

            public const int k_nRenderEventID_SubmitR = 201510022;

            public const int k_nRenderEventID_Flush = 201510023;

            public const int k_nRenderEventID_PostPresentHandoff = 201510024;
        }

        /// <summary>
        /// Most of this code by @Knah https://github.com/knah/VRCMods/blob/master/TrueShaderAntiCrash/TrueShaderAntiCrashMod.cs
        /// Thank him for supporting il2cpp modding ^^
        /// </summary>

        public static class ExternalPluginFunctionExtractor
        {
            // 2019.4.1f1  : 0x786D00
            // 2019.4.21f1 : 0x792350
            // 2020.3.16f1 : 0x5b71b0 cdecl FindAndLoadPlugin
            // 2020.3.16f1 : 0x76e350 fastcall FindAndLoadPluginIl2CppWrapper

            /// <summary>
            /// Use this if you're using a different engine version. Decompile UnityPlayer.dll with IDA PRO and get the pdb files for it 
            /// </summary>
            public static void SetFindAndLoadPluginFunctionOffset(int offset)
            {
                FindAndLoadUnityPluginOffset = offset;
            }

            public static int FindAndLoadUnityPluginOffset = 0x5b71b0;

            public static void GetLoadPluginFunction()
            {
                MelonLogger.Msg("[HPVR] Loading external plugin load function");
                var process = Process.GetCurrentProcess();
                foreach (ProcessModule module in process.Modules)
                {
                    MelonLogger.Msg("[HPVR] " + module.FileName);
                    if (!module.FileName.Contains("UnityPlayer"))
                        continue;
                    MelonLogger.Msg("[HPVR] Found the unityplayer module");

                    var loadLibraryAddress = module.BaseAddress + FindAndLoadUnityPluginOffset;
                    MelonLogger.Msg($"[HPVR] loadLibrary Address: {loadLibraryAddress:x} (offset {FindAndLoadUnityPluginOffset:x})");

                    var findAndLoadPlugin = Marshal.GetDelegateForFunctionPointer<FindAndLoadUnityPlugin>(loadLibraryAddress);
                    MelonLogger.Msg("[HPVR] got the delegate");

                    var strPtr = Marshal.StringToHGlobalAnsi(OpenVRMagic.DLLName);
                    MelonLogger.Msg($"[HPVR] callind the delegate with {strPtr:x} ({OpenVRMagic.DLLName})");

                    var retName = findAndLoadPlugin(strPtr, out var loaded, 1);

                    MelonLogger.Msg("[HPVR] unity loaded the plugin from: " + Marshal.PtrToStringAnsi(retName));

                    if (loaded == IntPtr.Zero)
                    {
                        MelonLogger.Error("[HPVR] Module load failed");
                        return;
                    }
                    MelonLogger.Msg("[HPVR] module loaded");

                    Marshal.FreeHGlobal(strPtr);

                    break;
                }
            }


            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr FindAndLoadUnityPlugin(IntPtr name, out IntPtr loadedModule, byte param3);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            private delegate IntPtr CallbackPointer();

            private static CallbackPointer ourGetRenderEventFunc;

            public static IntPtr GetRenderEventFunc() => ourGetRenderEventFunc();

        }
    }
}
