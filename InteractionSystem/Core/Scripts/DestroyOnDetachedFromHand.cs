//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Destroys this object when it is detached from the hand
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class DestroyOnDetachedFromHand : MonoBehaviour
    {
        public DestroyOnDetachedFromHand(IntPtr value) : base(value) { }
        //-------------------------------------------------
        private void OnDetachedFromHand( Hand hand )
        {
            Destroy( gameObject );
        }
    }
}
