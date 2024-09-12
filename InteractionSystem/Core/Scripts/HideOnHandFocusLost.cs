//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Sets this GameObject as inactive when it loses focus from the hand
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class HideOnHandFocusLost : MonoBehaviour
    {
        public HideOnHandFocusLost(IntPtr value) : base(value) { }
        //-------------------------------------------------
        private void OnHandFocusLost( Hand hand )
        {
            gameObject.SetActive( false );
        }
    }
}
