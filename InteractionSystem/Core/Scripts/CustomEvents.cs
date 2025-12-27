//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Custom Unity Events that take in additional parameters
//
//=============================================================================

using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    public static class CustomEvents
    {
        //-------------------------------------------------
        [Serializable]
        public class UnityEventSingleFloat : SteamVREvents.Event<float>
        {
        }

        //-------------------------------------------------
        [Serializable]
        public class UnityEventHand : SteamVREvents.Event<Hand>
        {
        }
    }
}
