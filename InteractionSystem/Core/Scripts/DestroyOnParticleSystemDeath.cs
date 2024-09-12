//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Destroys this object when its particle system dies
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class DestroyOnParticleSystemDeath : MonoBehaviour
    {
        public DestroyOnParticleSystemDeath(IntPtr value) : base(value) { }
        private ParticleSystem particles;

        //-------------------------------------------------
        void Awake()
        {
            particles = GetComponent<ParticleSystem>();

            InvokeRepeating( "CheckParticleSystem", 0.1f, 0.1f );
        }


        //-------------------------------------------------
        private void CheckParticleSystem()
        {
            if ( !particles.IsAlive() )
            {
                Destroy( this.gameObject );
            }
        }
    }
}
