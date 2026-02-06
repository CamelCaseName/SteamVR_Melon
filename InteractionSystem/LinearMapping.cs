//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: A linear mapping value that is used by other components
//
//=============================================================================

using System;
using UnityEngine;

namespace SteamVR_Melon.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class LinearMapping : MonoBehaviour
    {
        public LinearMapping(IntPtr value) : base(value) { }
        public float value;
    }
}
