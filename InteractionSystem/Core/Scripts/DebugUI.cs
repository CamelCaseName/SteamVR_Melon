﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Debug UI shown for the player
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class DebugUI : MonoBehaviour
	{
		public DebugUI(IntPtr value) : base(value) { }
		private Player player;

		//-------------------------------------------------
		static private DebugUI _instance;
		static public DebugUI instance
		{
			get
			{
				if ( _instance == null )
				{
					_instance = GameObject.FindObjectOfType<DebugUI>();
				}
				return _instance;
			}
		}


		//-------------------------------------------------
		void Start()
		{
			player = Player.instance;
		}


#if !HIDE_DEBUG_UI
        //-------------------------------------------------
        private void OnGUI()
		{
            if (Debug.isDebugBuild)
            {
                player.Draw2DDebug();
            }
        }
#endif
    }
}
