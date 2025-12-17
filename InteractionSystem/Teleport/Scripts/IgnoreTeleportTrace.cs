//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Allows the teleport arc trace to pass through any colliders on this
//			object
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------

    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class IgnoreTeleportTrace : MonoBehaviour
    {
        public IgnoreTeleportTrace(IntPtr value) : base(value) { }
    }
}
