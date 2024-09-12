//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Unparents an object and keeps track of the old parent
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class Unparent : MonoBehaviour
    {
        public Unparent(IntPtr value) : base(value) { }
        Transform oldParent;

        //-------------------------------------------------
        void Start()
        {
            oldParent = transform.parent;
            transform.parent = null;
            gameObject.name = oldParent.gameObject.name + "." + gameObject.name;
        }


        //-------------------------------------------------
        void Update()
        {
            if ( oldParent == null )
                GameObject.Destroy( gameObject );
        }


        //-------------------------------------------------
        public Transform GetOldParent()
        {
            return oldParent;
        }
    }
}
