//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object's rigidbody goes to sleep when created
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SleepOnAwake : MonoBehaviour
    {
        public SleepOnAwake(IntPtr value) : base(value) { }
        //-------------------------------------------------
        void Awake()
        {
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            if (rigidbody)
            {
                rigidbody.Sleep();
            }
        }
    }
}
