//========= Copyright 2016, Valve Corporation, All rights reserved. ===========
//
// Purpose: Helper to update poses when using native OpenVR integration.
//
//=============================================================================

using MelonLoader;
using SteamVR_Melon.Scripts;
using System;
using UnityEngine;

namespace Valve.VR
{
    [RegisterTypeInIl2Cpp()]
    public class SteamVRUpdatePoses : MonoBehaviour
    {
        public SteamVRUpdatePoses(IntPtr value) : base(value) { }

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
                var render = SteamVRRender.instance;
                compositor.GetLastPoses(render.poses, render.gamePoses);
                SteamVRUtils.Event.Send("new_poses", render.poses);
                SteamVRUtils.Event.Send("new_poses_applied");
            }
        }
    }
}