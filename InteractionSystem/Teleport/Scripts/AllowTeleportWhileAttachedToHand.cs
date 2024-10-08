﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Adding this component to an object will allow the player to
//			initiate teleporting while that object is attached to their hand
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class AllowTeleportWhileAttachedToHand : MonoBehaviour
    {
        public AllowTeleportWhileAttachedToHand(IntPtr value) : base(value) { }
        public bool teleportAllowed = true;
        public bool overrideHoverLock = true;
    }
}
