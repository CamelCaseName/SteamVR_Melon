//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Custom Unity Events that take in additional parameters
//
//=============================================================================


//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Custom Unity Events that take in additional parameters
//
//=============================================================================

using System;
using Valve.VR;

namespace SteamVR_Melon.InteractionSystem
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
