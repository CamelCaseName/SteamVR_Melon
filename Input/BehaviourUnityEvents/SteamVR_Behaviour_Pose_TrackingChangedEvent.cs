//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;

namespace Valve.VR
{
    [Serializable]
    public class SteamVRBehaviourPoseTrackingChangedEvent : SteamVREvents.Event<SteamVRBehaviourPose, SteamVRInputSources, ETrackingResult> { }
}