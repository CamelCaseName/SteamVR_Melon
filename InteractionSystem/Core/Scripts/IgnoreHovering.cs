//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes this object ignore any hovering by the hands
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class IgnoreHovering : MonoBehaviour
    {
        public IgnoreHovering(IntPtr value) : base(value) { }
        public Hand onlyIgnoreHand = null;
    }
}
