//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;

namespace Valve.VR
{
    [Serializable]
    public class SteamVRBehaviourPoseDeviceIndexChangedEvent : SteamVREvents.Event<SteamVRBehaviourPose, SteamVRInputSources, int> { }
}