//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// Action sets are logical groupings of actions. Multiple sets can be active at one time.
    /// </summary>
    public static class SteamVRActionSetManager
    {
        public static VRActiveActionSetT[] rawActiveActionSetArray
        {
            get
            {
                if (currentArraySize <= 0)
                {
                    return null;
                }
                else
                {
                    return poolActiveActionSetArrays[currentArraySize];
                }
            }
        }

        [NonSerialized]
        private static uint activeActionSetSize;

        [NonSerialized]
        private static bool changed = false;

        [NonSerialized]
        private static int currentArraySize;
        [NonSerialized]
        private static Dictionary<int, VRActiveActionSetT[]> poolActiveActionSetArrays;

        public static void Initialize()
        {
            activeActionSetSize = (uint)(Marshal.SizeOf(typeof(VRActiveActionSetT)));
            poolActiveActionSetArrays = new Dictionary<int, VRActiveActionSetT[]>();
        }

        /// <summary>
        /// Disable all known action sets.
        /// </summary>
        public static void DisableAllActionSets()
        {
            for (int actionSetIndex = 0; actionSetIndex < SteamVRInput.actionSets.Length; actionSetIndex++)
            {
                SteamVRInput.actionSets[actionSetIndex].Deactivate(SteamVRInputSources.Any);
                SteamVRInput.actionSets[actionSetIndex].Deactivate(SteamVRInputSources.LeftHand);
                SteamVRInput.actionSets[actionSetIndex].Deactivate(SteamVRInputSources.RightHand);
            }
        }

        private static int lastFrameUpdated;
        public static void UpdateActionStates(bool force = false)
        {
            if (force || Time.frameCount != lastFrameUpdated)
            {
                lastFrameUpdated = Time.frameCount;

                if (changed)
                {
                    UpdateActionSetsArray();
                }

                if (rawActiveActionSetArray != null && rawActiveActionSetArray.Length > 0)
                {
                    if (OpenVR.Input != null)
                    {
                        EVRInputError err = OpenVR.Input.UpdateActionState(rawActiveActionSetArray, activeActionSetSize);
                        if (err != EVRInputError.None)
                        {
                            MelonLoader.MelonLogger.Error("[HPVR] UpdateActionState error: " + err.ToString());
                        }
                        //else MelonLoader.MelonLogger.Msg("Action sets activated: " + activeActionSets.Length);
                    }
                }
                else
                {
                    //MelonLoader.MelonLogger.Warning("No sets active");
                }
            }
        }

        public static void SetChanged()
        {
            changed = true;
        }

        private static int GetNewArraySize()
        {
            int size = 0;

            SteamVRInputSources[] sources = SteamVRInputSource.GetAllSources();
            for (int actionSetIndex = 0; actionSetIndex < SteamVRInput.actionSets.Length; actionSetIndex++)
            {
                SteamVRActionSet set = SteamVRInput.actionSets[actionSetIndex];

                for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
                {
                    SteamVRInputSources source = sources[sourceIndex];

                    if (set.ReadRawSetActive(source))
                    {
                        size++;
                    }
                }
            }

            return size;
        }

        private static void UpdateActionSetsArray()
        {
            int newArraySize = GetNewArraySize();
            if (poolActiveActionSetArrays.ContainsKey(newArraySize) == false)
            {
                poolActiveActionSetArrays[newArraySize] = new VRActiveActionSetT[newArraySize];
            }

            int arrayIndex = 0;
            SteamVRInputSources[] sources = SteamVRInputSource.GetAllSources();

            for (int actionSetIndex = 0; actionSetIndex < SteamVRInput.actionSets.Length; actionSetIndex++)
            {
                SteamVRActionSet set = SteamVRInput.actionSets[actionSetIndex];

                for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
                {
                    SteamVRInputSources source = sources[sourceIndex];

                    if (set.ReadRawSetActive(source))
                    {
                        poolActiveActionSetArrays[newArraySize][arrayIndex].ulActionSet = set.handle;
                        poolActiveActionSetArrays[newArraySize][arrayIndex].nPriority = set.ReadRawSetPriority(source);
                        poolActiveActionSetArrays[newArraySize][arrayIndex].ulRestrictedToDevice = SteamVRInputSource.GetHandle(source);

                        arrayIndex++;
                    }
                }
            }

            changed = false;
            currentArraySize = newArraySize;

            if (Application.isEditor || updateDebugTextInBuilds)
            {
                UpdateDebugText();
            }
        }

        public static SteamVRActionSet GetSetFromHandle(ulong handle)
        {
            for (int actionSetIndex = 0; actionSetIndex < SteamVRInput.actionSets.Length; actionSetIndex++)
            {
                SteamVRActionSet set = SteamVRInput.actionSets[actionSetIndex];
                if (set.handle == handle)
                {
                    return set;
                }
            }

            return null;
        }

        public static string debugActiveSetListText;
        public static bool updateDebugTextInBuilds = false;
        private static void UpdateDebugText()
        {
            StringBuilder stringBuilder = new();

            for (int activeIndex = 0; activeIndex < rawActiveActionSetArray.Length; activeIndex++)
            {
                VRActiveActionSetT set = rawActiveActionSetArray[activeIndex];
                stringBuilder.Append(set.nPriority);
                stringBuilder.Append("\t");
                stringBuilder.Append(SteamVRInputSource.GetSource(set.ulRestrictedToDevice));
                stringBuilder.Append("\t");
                stringBuilder.Append(GetSetFromHandle(set.ulActionSet).GetShortName());
                stringBuilder.Append("\n");
            }

            debugActiveSetListText = stringBuilder.ToString();
        }
    }
}