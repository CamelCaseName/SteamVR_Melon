//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Keeps track of the ItemPackage this object is a part of
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class ItemPackageReference : MonoBehaviour
    {
        public ItemPackageReference(IntPtr value) : base(value) { }
        public ItemPackage itemPackage;
    }
}
