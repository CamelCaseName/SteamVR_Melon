//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// Automatically activates an action set on Start() and deactivates the set on OnDestroy(). Optionally deactivating all other sets as well.
    /// </summary>
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRActivateActionSetOnLoad : MonoBehaviour
    {
        public SteamVRActivateActionSetOnLoad(IntPtr value) : base(value) { }
        public SteamVRActionSet actionSet = SteamVRInput.GetActionSet("default");

        public SteamVRInputSources forSources = SteamVRInputSources.Any;

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