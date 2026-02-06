//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Unparents an object and keeps track of the old parent
//
//=============================================================================

using System;
using UnityEngine;

namespace SteamVR_Melon.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
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
            if (oldParent == null)
            {
                Destroy(gameObject);
            }
        }

        //-------------------------------------------------
        public Transform GetOldParent()
        {
            return oldParent;
        }
    }
}
