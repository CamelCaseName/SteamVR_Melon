//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR
{
    /// <summary>
    /// Automatically activates an action set on Start() and deactivates the set on OnDestroy(). Optionally deactivating all other sets as well.
    /// </summary>
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class SteamVR_ActivateActionSetOnLoad : MonoBehaviour
    {
        public SteamVR_ActivateActionSetOnLoad(IntPtr value) : base(value) { }
        public SteamVR_ActionSet actionSet = SteamVR_Input.GetActionSet("default");

        public SteamVR_Input_Sources forSources = SteamVR_Input_Sources.Any;

        public bool disableAllOtherActionSets = false;

        public bool activateOnStart = true;
        public bool deactivateOnDestroy = true;

        public int initialPriority = 0;

        private void Start()
        {
            if (actionSet != null && activateOnStart)
            {
                //MelonLoader.MelonLogger.Msg(string.Format("[HPVR] Activating {0} action set.", actionSet.fullPath));
                actionSet.Activate(forSources, initialPriority, disableAllOtherActionSets);
            }
        }

        private void OnDestroy()
        {
            if (actionSet != null && deactivateOnDestroy)
            {
                //MelonLoader.MelonLogger.Msg(string.Format("[HPVR] Deactivating {0} action set.", actionSet.fullPath));
                actionSet.Deactivate(forSources);
            }
        }
    }
}