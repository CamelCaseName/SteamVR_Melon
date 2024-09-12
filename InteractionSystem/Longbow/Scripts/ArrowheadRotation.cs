//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Sets aUnityEngine.Random rotation for the arrow head
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class ArrowheadRotation : MonoBehaviour
    {
        public ArrowheadRotation(IntPtr value) : base(value) { }
        //-------------------------------------------------
        void Start()
        {
            float randX =UnityEngine.Random.Range( 0f, 180f );
            transform.localEulerAngles = new Vector3( randX, -90f, 90f );
        }
    }
}
