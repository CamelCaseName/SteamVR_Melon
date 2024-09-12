//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object won't be destroyed when a new scene is loaded
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class DontDestroyOnLoad : MonoBehaviour
	{
		public DontDestroyOnLoad(IntPtr value) : base(value) { }
		//-------------------------------------------------
		void Awake()
		{
			DontDestroyOnLoad( this );
		}
	}
}
