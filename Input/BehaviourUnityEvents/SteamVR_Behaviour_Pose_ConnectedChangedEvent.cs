﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using UnityEngine.Events;

namespace Valve.VR
{
    [Serializable]
    public class SteamVR_Behaviour_Pose_ConnectedChangedEvent : SteamVR_Events.Event<SteamVR_Behaviour_Pose, SteamVR_Input_Sources, bool> { }
}