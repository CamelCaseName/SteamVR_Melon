//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Handles aligning audio listener when using speakers.
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVREars : MonoBehaviour
    {
        public SteamVREars(IntPtr value) : base(value) { }

        public SteamVRCamera vrcam;

        bool usingSpeakers;
        Quaternion offset;

        private void OnNewPosesApplied()
        {
            var origin = vrcam.origin;
            var baseRotation = origin != null ? origin.rotation : Quaternion.identity;
            transform.rotation = baseRotation * offset;
        }

        void OnEnable()
        {
            usingSpeakers = false;

            //var settings = OpenVR.Settings;
            //if (settings != null)
            //{
            //    var error = EVRSettingsError.None;
            //    if (settings.GetBool(OpenVR.kPchSteamVRSection, OpenVR.kPchSteamVRUsingSpeakersBool, ref error))
            //    {
                    usingSpeakers = true;

            //        var yawOffset = settings.GetFloat(OpenVR.kPchSteamVRSection, OpenVR.kPchSteamVRSpeakersForwardYawOffsetDegreesFloat, ref error);
            //        offset = Quaternion.Euler(0.0f, yawOffset, 0.0f);
            //    }
            //}

            if (usingSpeakers)
            {
                SteamVREvents.NewPosesApplied.Listen(OnNewPosesApplied);
            }
        }

        void OnDisable()
        {
            if (usingSpeakers)
            {
                SteamVREvents.NewPosesApplied.Remove(OnNewPosesApplied);
            }
        }
    }
}