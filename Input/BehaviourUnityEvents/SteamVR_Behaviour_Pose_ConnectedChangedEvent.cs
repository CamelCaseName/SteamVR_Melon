//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;

namespace Valve.VR
{
    [Serializable]
    public class SteamVRBehaviourPoseConnectedChangedEvent : SteamVREvents.Event<SteamVRBehaviourPose, SteamVRInputSources, bool> { }
}