﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Creates an object and attaches it to the hand
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class SpawnAndAttachToHand : MonoBehaviour
    {
        public SpawnAndAttachToHand(IntPtr value) : base(value) { }
        public Hand hand;
        public GameObject prefab;


        //-------------------------------------------------
        public void SpawnAndAttach( Hand passedInhand )
        {
            Hand handToUse = passedInhand;
            if ( passedInhand == null )
            {
                handToUse = hand;
            }

            if ( handToUse == null )
            {
                return;
            }

            GameObject prefabObject = Instantiate( prefab ) as GameObject;
            handToUse.AttachObject( prefabObject, GrabTypes.Scripted );
        }
    }
}
