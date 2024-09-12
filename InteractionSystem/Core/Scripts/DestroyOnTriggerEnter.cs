//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Destroys this object when it enters a trigger
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class DestroyOnTriggerEnter : MonoBehaviour
    {
        public DestroyOnTriggerEnter(IntPtr value) : base(value) { }
        public string tagFilter;

        private bool useTag;

        //-------------------------------------------------
        void Start()
        {
            if ( !string.IsNullOrEmpty( tagFilter ) )
            {
                useTag = true;
            }
        }


        //-------------------------------------------------
        void OnTriggerEnter( Collider collider )
        {
            if ( !useTag || ( useTag && collider.gameObject.tag == tagFilter ) )
            {
                Destroy( collider.gameObject.transform.root.gameObject );
            }
        }
    }
}
