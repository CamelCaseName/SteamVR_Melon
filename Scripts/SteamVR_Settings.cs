//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

namespace Valve.VR
{
    public class SteamVRSettings
    {
        private static SteamVRSettings _instance;
        public static SteamVRSettings instance
        {
            get
            {
                LoadInstance();

                return _instance;
            }
        }

        public bool pauseGameWhenDashboardVisible = true;
        public bool lockPhysicsUpdateRateToRenderFrequency = true;
        public ETrackingUniverseOrigin trackingSpace
        {
            get
            {
                return trackingSpaceOrigin;
            }
            set
            {
                trackingSpaceOrigin = value;
                if (SteamVRBehaviour.isPlaying)
                {
                    SteamVRActionPose.SetTrackingUniverseOrigin(trackingSpaceOrigin);
                }
            }
        }

        private ETrackingUniverseOrigin trackingSpaceOrigin = ETrackingUniverseOrigin.TrackingUniverseStanding;

        public string actionsFilePath = "actions.json";

        public string steamVRInputPath = "SteamVR_Input";

        public SteamVRUpdateModes inputUpdateMode = SteamVRUpdateModes.OnUpdate;
        public SteamVRUpdateModes poseUpdateMode = SteamVRUpdateModes.OnPreCull;

        public bool activateFirstActionSetOnStart = true;

        public string editorAppKey;
        public bool autoEnableVR = true;

        public bool legacyMixedRealityCamera = true;

        public SteamVRActionPose mixedRealityCameraPose = SteamVRInput.GetPoseAction("ExternalCamera");

        public SteamVRInputSources mixedRealityCameraInputSource = SteamVRInputSources.Camera;

        public bool mixedRealityActionSetAutoEnable = true;

        public GameObject previewHandLeft;

        public GameObject previewHandRight;

        private const string previewLeftDefaultAssetName = "vr_glove_left_model_slim";
        private const string previewRightDefaultAssetName = "vr_glove_right_model_slim";

        public bool IsInputUpdateMode(SteamVRUpdateModes tocheck)
        {
            return (inputUpdateMode & tocheck) == tocheck;
        }
        public bool IsPoseUpdateMode(SteamVRUpdateModes tocheck)
        {
            return (poseUpdateMode & tocheck) == tocheck;
        }

        public static void VerifyScriptableObject()
        {
            LoadInstance();
        }

        private static void LoadInstance()
        {
            if (_instance == null)
            {
                //_instance = Resources.Load<SteamVR_Settings>("SteamVR_Settings");

                if (_instance == null)
                {
                    _instance = new SteamVRSettings();
                }

                SetDefaultsIfNeeded();
            }
        }

        public static void Save()
        {
        }

        private const string defaultSettingsAssetName = "SteamVR_Settings";

        private static void SetDefaultsIfNeeded()
        {
            if (string.IsNullOrEmpty(_instance.editorAppKey))
            {
                _instance.editorAppKey = SteamVR.GenerateAppKey();
                MelonLoader.MelonLogger.Msg("[HPVR] Generated you an editor app key of: " + _instance.editorAppKey + ". This lets the editor tell SteamVR what project this is. Has no effect on builds. This can be changed in Assets/SteamVR/Resources/SteamVR_Settings");
            }
        }

        private static GameObject FindDefaultPreviewHand(string assetName)
        {

            return null;

        }
    }
}