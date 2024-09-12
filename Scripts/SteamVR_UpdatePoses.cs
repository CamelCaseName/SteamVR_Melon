//========= Copyright 2016, Valve Corporation, All rights reserved. ===========
//
// Purpose: Helper to update poses when using native OpenVR integration.
//
//=============================================================================

using MelonLoader;
using System;
using UnityEngine;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class SteamVR_UpdatePoses : MonoBehaviour
    {
        public SteamVR_UpdatePoses(IntPtr value) : base(value) { }

        void Awake()
        {
            var camera = Camera.main;
            MelonLogger.Msg("[HPVR] update poses camera is null: " + (camera is null));
            if (camera is not null)
            {
                camera.stereoTargetEye = StereoTargetEyeMask.None;
                camera.clearFlags = CameraClearFlags.Nothing;
                camera.useOcclusionCulling = false;
                camera.cullingMask = 0;
                camera.depth = -9999;
            }
        }

        void OnPreCull()
        {
            var compositor = OpenVR.Compositor;
            if (compositor != null)
            {
                var render = SteamVR_Render.instance;
                compositor.GetLastPoses(render.poses, render.gamePoses);
                SteamVR_Utils.Event.Send("new_poses", render.poses);
                SteamVR_Utils.Event.Send("new_poses_applied");
            }
        }
    }
}