//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;

namespace Valve.VR
{
    [Serializable]
    public class SteamVRBehaviourSkeletonTrackingChangedEvent : SteamVREvents.Event<SteamVRBehaviourSkeleton, SteamVRInputSources, ETrackingResult> { }
}