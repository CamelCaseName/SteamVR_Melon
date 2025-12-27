//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Used to render an external camera of vr player (split front/back).
//
//=============================================================================

namespace Valve.VR
{
    public class SteamVRExternalCameraLegacyManager
    {
        public static bool hasCamera { get { return cameraIndex != -1; } }

        public static int cameraIndex = -1;

        private static SteamVREvents.Action newPosesAction = null;

        public static void SubscribeToNewPoses()
        {
            if (newPosesAction == null)
            {
                newPosesAction = SteamVREvents.NewPosesAction(OnNewPoses);
            }

            newPosesAction.enabled = true;
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private static void OnNewPoses(TrackedDevicePoseT[] poses)
        {
            if (cameraIndex != -1)
            {
                return;
            }

            int controllercount = 0;
            for (int index = 0; index < poses.Length; index++)
            {
                if (poses[index].bDeviceIsConnected)
                {
                    ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass((uint)index);
                    if (deviceClass == ETrackedDeviceClass.Controller || deviceClass == ETrackedDeviceClass.GenericTracker)
                    {
                        controllercount++;
                        if (controllercount >= 3)
                        {
                            cameraIndex = index;
                            break;
                        }
                    }
                }
            }
        }
    }
}