//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Collider dangling from the player's head
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------

    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class BodyCollider : MonoBehaviour
    {
        public BodyCollider(IntPtr value) : base(value) { }
        public Transform head;

        private CapsuleCollider capsuleCollider;

        //-------------------------------------------------
        void Awake()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }


        //-------------------------------------------------
        void FixedUpdate()
        {
            float distanceFromFloor = Vector3.Dot( head.localPosition, Vector3.up );
            capsuleCollider.height = Mathf.Max( capsuleCollider.radius, distanceFromFloor );
            transform.localPosition = head.localPosition - 0.5f * distanceFromFloor * Vector3.up;
        }
    }
}
