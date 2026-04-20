//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Access to SteamVR system (hmd) and compositor (distort) interfaces.
//
//=============================================================================

using MelonLoader;
using SteamVR_Melon.Standalone;
using SteamVR_Melon.Util;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Valve.VR
{
    public class SteamVR : System.IDisposable
    {
        // Use this to check if SteamVR is currently active without attempting
        // to activate it in the process.
        public static bool Active { get { return _instance != null; } }

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
        public static bool Enabled
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
        public static SteamVR Instance
        {
            get
            {
                if (!Enabled)
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
        }

        private static SteamVR CreateInstance()
        {
            initializedState = InitializedStates.Initializing;

            try
            {
                UnityHooks.Init();
            }
            catch (System.Exception e)
            {
                MelonLogger.Error("[HPVR] " + e);
                SteamVREvents.Initialized.Send(false);
                return null;
            }

            MelonLogger.Msg("Creating SteamVR Instance...");

            _enabled = true;
            initializedState = InitializedStates.InitializeSuccess;
            SteamVREvents.Initialized.Send(true);
            MelonLogger.Msg("returning SteamVR Object");
            return new SteamVR();
        }

        // tracking status
        static public bool Initializing { get; private set; }
        static public bool Calibrating { get; private set; }
        static public bool OutOfRange { get; private set; }

        // hmd properties
        public string hmdTrackingSystemName { get { return XRHMD.all[0].displayName; } }
        public string hmdModelNumber { get { return XRGraphics.loadedDeviceName; } }
        public float hmdDisplayFrequency { get { return XRDevice.refreshRate; } }

        #region Event callbacks

        private void OnInitializing(bool initializing)
        {
            SteamVR.Initializing = initializing;
        }

        private void OnCalibrating(bool calibrating)
        {
            SteamVR.Calibrating = calibrating;
        }

        private void OnOutOfRange(bool outOfRange)
        {
            SteamVR.OutOfRange = outOfRange;
        }

        #endregion

        private SteamVR()
        {
            MelonLogger.Msg($"Initialized SteamVR");

            SteamVREvents.Initializing.Listen(OnInitializing);
            SteamVREvents.Calibrating.Listen(OnCalibrating);
            SteamVREvents.OutOfRange.Listen(OnOutOfRange);
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

            _instance = null;
        }

        // Use this interface to avoid accidentally creating the instance in the process of attempting to dispose of it.
        public static void SafeDispose()
        {
            _instance?.Dispose();
        }
    }
}