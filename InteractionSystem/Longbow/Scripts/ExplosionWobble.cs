//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes the weeble wobble
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class ExplosionWobble : MonoBehaviour
    {
        public ExplosionWobble(IntPtr value) : base(value) { }
        //-------------------------------------------------
        public void ExplosionEvent(Vector3 explosionPos)
        {
            var rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddExplosionForce(2000, explosionPos, 10.0f);
            }
        }
    }
}
