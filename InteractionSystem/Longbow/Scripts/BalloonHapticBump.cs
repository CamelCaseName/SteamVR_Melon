//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Provides a haptic bump when colliding with balloons
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class BalloonHapticBump : MonoBehaviour
    {
        public BalloonHapticBump(IntPtr value) : base(value) { }
        public GameObject physParent;

        //-------------------------------------------------
        void OnCollisionEnter( Collision other )
        {
            Balloon contactBalloon = other.collider.GetComponentInParent<Balloon>();
            if ( contactBalloon != null )
            {
                Hand hand = physParent.GetComponentInParent<Hand>();
                if ( hand != null )
                {
                    hand.TriggerHapticPulse( 500 );
                }
            }
        }
    }
}
