//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Custom Unity Events that take in additional parameters
//
//=============================================================================

using UnityEngine.Events;
using System;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public static class CustomEvents
	{
		//-------------------------------------------------
		[System.Serializable]
		public class UnityEventSingleFloat : SteamVR_Events.Event<float>
		{
		}


		//-------------------------------------------------
		[System.Serializable]
		public class UnityEventHand : SteamVR_Events.Event<Hand>
		{
		}
	}
}
