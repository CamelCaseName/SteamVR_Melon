//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Allows the teleport arc trace to pass through any colliders on this
//			object
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------

    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class IgnoreTeleportTrace : MonoBehaviour
    {
        public IgnoreTeleportTrace(IntPtr value) : base(value) { }
    }
}
