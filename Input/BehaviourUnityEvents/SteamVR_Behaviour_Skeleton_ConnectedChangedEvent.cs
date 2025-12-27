//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;

namespace Valve.VR
{
    [Serializable]
    public class SteamVRBehaviourSkeletonConnectedChangedEvent : SteamVREvents.Event<SteamVRBehaviourSkeleton, SteamVRInputSources, bool> { }
}