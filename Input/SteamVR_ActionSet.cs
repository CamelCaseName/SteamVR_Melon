//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// Action sets are logical groupings of actions. Multiple sets can be active at one time.
    /// </summary>
    [Serializable]
    public class SteamVRActionSet : IEquatable<SteamVRActionSet>, ISteamVRActionSet
    {
        public SteamVRActionSet() { }

        private string actionSetPath;

        [NonSerialized]
        protected SteamVRActionSetData setData;

        /// <summary>All actions within this set (including out actions)</summary>
        public SteamVRAction[] allActions
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.allActions;
            }
        }

        /// <summary>All IN actions within this set that are NOT pose or skeleton actions</summary>
        public ISteamVRActionIn[] nonVisualInActions
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.nonVisualInActions;
            }
        }

        /// <summary>All pose and skeleton actions within this set</summary>
        public ISteamVRActionIn[] visualActions
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.visualActions;
            }
        }

        /// <summary>All pose actions within this set</summary>
        public SteamVRActionPose[] poseActions
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.poseActions;
            }
        }

        /// <summary>All skeleton actions within this set</summary>
        public SteamVRActionSkeleton[] skeletonActions
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.skeletonActions;
            }
        }

        /// <summary>All out actions within this set</summary>
        public ISteamVRActionOut[] outActionArray
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.outActionArray;
            }
        }

        /// <summary>The full path to this action set (ex: /actions/in/default)</summary>
        public string fullPath
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.fullPath;
            }
        }
        public string usage
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.usage;
            }
        }

        public ulong handle
        {
            get
            {
                if (initialized == false)
                {
                    Initialize();
                }

                return setData.handle;
            }
        }

        [NonSerialized]
        protected bool initialized = false;

        public static CreateType Create<CreateType>(string newSetPath) where CreateType : SteamVRActionSet, new()
        {
            CreateType actionSet = new();
            actionSet.PreInitialize(newSetPath);
            return actionSet;
        }
        public static CreateType CreateFromName<CreateType>(string newSetName) where CreateType : SteamVRActionSet, new()
        {
            CreateType actionSet = new();
            actionSet.PreInitialize(SteamVRInputActionFileActionSet.GetPathFromName(newSetName));
            return actionSet;
        }

        public void PreInitialize(string newActionPath)
        {
            actionSetPath = newActionPath;

            setData = new SteamVRActionSetData
            {
                fullPath = actionSetPath
            };
            setData.PreInitialize();

            initialized = true;
        }

        public virtual void FinishPreInitialize()
        {
            setData.FinishPreInitialize();
        }

        /// <summary>
        /// Initializes the handle for the action
        /// </summary>
        public virtual void Initialize(bool createNew = false, bool throwErrors = true)
        {
            if (createNew)
            {
                setData.Initialize();
            }
            else
            {
                setData = SteamVRInput.GetActionSetDataFromPath(actionSetPath);

                if (setData == null)
                {
#if UNITY_EDITOR
                    if (throwErrors)
                    {
                        if (string.IsNullOrEmpty(actionSetPath))
                        {
                            MelonLoader.MelonLogger.Error("[HPVR] Action has not been assigned.");
                        }
                        else
                        {
                            MelonLoader.MelonLogger.Error("[HPVR] Could not find action with path: " + actionSetPath);
                        }
                    }
#endif
                }
            }

            initialized = true;
        }

        public string GetPath()
        {
            return actionSetPath;
        }

        /// <summary>
        /// Returns whether the set is currently active or not.
        /// </summary>
        /// <param name="source">The device to check. Any means all devices here (not left or right, but all)</param>
        public bool IsActive(SteamVRInputSources source = SteamVRInputSources.Any)
        {
            return setData.IsActive(source);
        }

        /// <summary>
        /// Returns the last time this action set was changed (set to active or inactive)
        /// </summary>
        /// <param name="source">The device to check. Any means all devices here (not left or right, but all)</param>
        public float GetTimeLastChanged(SteamVRInputSources source = SteamVRInputSources.Any)
        {
            return setData.GetTimeLastChanged(source);
        }

        /// <summary>
        /// Activate this set so its actions can be called
        /// </summary>
        /// <param name="disableAllOtherActionSets">Disable all other action sets at the same time</param>
        /// <param name="priority">The priority of this action set. If you have two actions bound to the same input (button) the higher priority set will override the lower priority. If they are the same priority both will execute.</param>
        /// <param name="activateForSource">Will activate this action set only for the specified source. Any if you want to activate for everything</param>
        public void Activate(SteamVRInputSources activateForSource = SteamVRInputSources.Any, int priority = 0, bool disableAllOtherActionSets = false)
        {
            setData.Activate(activateForSource, priority, disableAllOtherActionSets);
        }

        /// <summary>
        /// Deactivate the action set so its actions can no longer be called
        /// </summary>
        public void Deactivate(SteamVRInputSources forSource = SteamVRInputSources.Any)
        {
            setData.Deactivate(forSource);
        }

        /// <summary>Gets the last part of the path for this action. Removes "actions" and direction.</summary>
        public string GetShortName()
        {
            return setData.GetShortName();
        }

        /// <summary>
        /// Shows all the bindings for the actions in this set.
        /// </summary>
        /// <param name="originToHighlight">Highlights the binding of the passed in action (must be in an active set)</param>
        /// <returns></returns>
        public bool ShowBindingHints(ISteamVRActionIn originToHighlight = null)
        {
            if (originToHighlight == null)
            {
                return SteamVRInput.ShowBindingHints(this);
            }
            else
            {
                return SteamVRInput.ShowBindingHints(originToHighlight);
            }
        }

        public bool ReadRawSetActive(SteamVRInputSources inputSource)
        {
            return setData.ReadRawSetActive(inputSource);
        }

        public float ReadRawSetLastChanged(SteamVRInputSources inputSource)
        {
            return setData.ReadRawSetLastChanged(inputSource);
        }

        public int ReadRawSetPriority(SteamVRInputSources inputSource)
        {
            return setData.ReadRawSetPriority(inputSource);
        }

        public SteamVRActionSetData GetActionSetData()
        {
            return setData;
        }

        public CreateType GetCopy<CreateType>() where CreateType : SteamVRActionSet, new()
        {
            if (SteamVRInput.ShouldMakeCopy()) //no need to make copies at runtime
            {
                CreateType actionSet = new()
                {
                    actionSetPath = this.actionSetPath,
                    setData = this.setData,
                    initialized = true
                };
                return actionSet;
            }
            else
            {
                return (CreateType)this;
            }
        }

        public bool Equals(SteamVRActionSet other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            return this.actionSetPath == other.actionSetPath;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(null, other))
            {
                if (string.IsNullOrEmpty(this.actionSetPath)) //if we haven't set a path, say this action set is equal to null
                {
                    return true;
                }

                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (other is SteamVRActionSet)
            {
                return this.Equals((SteamVRActionSet)other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            if (actionSetPath == null)
            {
                return 0;
            }
            else
            {
                return actionSetPath.GetHashCode();
            }
        }

        public static bool operator !=(SteamVRActionSet set1, SteamVRActionSet set2)
        {
            return !(set1 == set2);
        }

        public static bool operator ==(SteamVRActionSet set1, SteamVRActionSet set2)
        {
            bool set1null = (ReferenceEquals(null, set1) || string.IsNullOrEmpty(set1.actionSetPath) || set1.GetActionSetData() == null);
            bool set2null = (ReferenceEquals(null, set2) || string.IsNullOrEmpty(set2.actionSetPath) || set2.GetActionSetData() == null);

            if (set1null && set2null)
            {
                return true;
            }
            else if (set1null != set2null)
            {
                return false;
            }

            return set1.Equals(set2);
        }
    }
    /// <summary>
    /// Action sets are logical groupings of actions. Multiple sets can be active at one time.
    /// </summary>
    public class SteamVRActionSetData : ISteamVRActionSet
    {
        public SteamVRActionSetData() { }

        /// <summary>All actions within this set (including out actions)</summary>
        public SteamVRAction[] allActions { get; set; }

        /// <summary>All IN actions within this set that are NOT pose or skeleton actions</summary>
        public ISteamVRActionIn[] nonVisualInActions { get; set; }

        /// <summary>All pose and skeleton actions within this set</summary>
        public ISteamVRActionIn[] visualActions { get; set; }

        /// <summary>All pose actions within this set</summary>
        public SteamVRActionPose[] poseActions { get; set; }

        /// <summary>All skeleton actions within this set</summary>
        public SteamVRActionSkeleton[] skeletonActions { get; set; }

        /// <summary>All out actions within this set</summary>
        public ISteamVRActionOut[] outActionArray { get; set; }

        /// <summary>The full path to this action set (ex: /actions/in/default)</summary>
        public string fullPath { get; set; }
        public string usage { get; set; }

        public ulong handle { get; set; }

        protected bool[] rawSetActive = new bool[SteamVRInputSource.numSources];

        protected float[] rawSetLastChanged = new float[SteamVRInputSource.numSources];

        protected int[] rawSetPriority = new int[SteamVRInputSource.numSources];

        protected bool initialized = false;

        public void PreInitialize()
        {
        }

        public void FinishPreInitialize()
        {
            List<SteamVRAction> allActionsList = new();
            List<ISteamVRActionIn> nonVisualInActionsList = new();
            List<ISteamVRActionIn> visualActionsList = new();
            List<SteamVRActionPose> poseActionsList = new();
            List<SteamVRActionSkeleton> skeletonActionsList = new();
            List<ISteamVRActionOut> outActionList = new();

            if (SteamVRInput.actions == null)
            {
                MelonLoader.MelonLogger.Error("[SteamVR Input] Actions not initialized!");
                return;
            }

            for (int actionIndex = 0; actionIndex < SteamVRInput.actions.Length; actionIndex++)
            {
                SteamVRAction action = SteamVRInput.actions[actionIndex];

                if (action.actionSet.GetActionSetData() == this)
                {
                    allActionsList.Add(action);

                    if (action is ISteamVRActionBoolean || action is ISteamVRActionSingle || action is ISteamVRActionVector2 || action is ISteamVRActionVector3)
                    {
                        nonVisualInActionsList.Add((ISteamVRActionIn)action);
                    }
                    else if (action is SteamVRActionPose)
                    {
                        visualActionsList.Add((ISteamVRActionIn)action);
                        poseActionsList.Add((SteamVRActionPose)action);
                    }
                    else if (action is SteamVRActionSkeleton)
                    {
                        visualActionsList.Add((ISteamVRActionIn)action);
                        skeletonActionsList.Add((SteamVRActionSkeleton)action);
                    }
                    else if (action is ISteamVRActionOut)
                    {
                        outActionList.Add((ISteamVRActionOut)action);
                    }
                    else
                    {
                        MelonLoader.MelonLogger.Error("[SteamVR Input] Action doesn't implement known interface: " + action.fullPath);
                    }
                }
            }

            allActions = allActionsList.ToArray();
            nonVisualInActions = nonVisualInActionsList.ToArray();
            visualActions = visualActionsList.ToArray();
            poseActions = poseActionsList.ToArray();
            skeletonActions = skeletonActionsList.ToArray();
            outActionArray = outActionList.ToArray();
        }

        public void Initialize()
        {
            ulong newHandle = 0;
            EVRInputError err = OpenVR.Input.GetActionSetHandle(fullPath.ToLower(), ref newHandle);
            handle = newHandle;

            if (err != EVRInputError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] GetActionSetHandle (" + fullPath + ") error: " + err.ToString());
            }

            initialized = true;
        }

        /// <summary>
        /// Returns whether the set is currently active or not.
        /// </summary>
        /// <param name="source">The device to check. Any means all devices here (not left or right, but all)</param>
        public bool IsActive(SteamVRInputSources source = SteamVRInputSources.Any)
        {
            int sourceIndex = (int)source;

            if (initialized)
            {
                return rawSetActive[sourceIndex] || rawSetActive[0];
            }

            return false;
        }

        /// <summary>
        /// Returns the last time this action set was changed (set to active or inactive)
        /// </summary>
        /// <param name="source">The device to check. Any means all devices here (not left or right, but all)</param>
        public float GetTimeLastChanged(SteamVRInputSources source = SteamVRInputSources.Any)
        {
            int sourceIndex = (int)source;

            if (initialized)
            {
                return rawSetLastChanged[sourceIndex];
            }

            return 0;
        }

        /// <summary>
        /// Activate this set so its actions can be called
        /// </summary>
        /// <param name="disableAllOtherActionSets">Disable all other action sets at the same time</param>
        /// <param name="priority">The priority of this action set. If you have two actions bound to the same input (button) the higher priority set will override the lower priority. If they are the same priority both will execute.</param>
        /// <param name="activateForSource">Will activate this action set only for the specified source. Any if you want to activate for everything</param>
        public void Activate(SteamVRInputSources activateForSource = SteamVRInputSources.Any, int priority = 0, bool disableAllOtherActionSets = false)
        {
            int sourceIndex = (int)activateForSource;

            if (disableAllOtherActionSets)
            {
                SteamVRActionSetManager.DisableAllActionSets();
            }

            if (rawSetActive[sourceIndex] == false)
            {
                rawSetActive[sourceIndex] = true;
                SteamVRActionSetManager.SetChanged();

                rawSetLastChanged[sourceIndex] = Time.realtimeSinceStartup;
            }

            if (rawSetPriority[sourceIndex] != priority)
            {
                rawSetPriority[sourceIndex] = priority;
                SteamVRActionSetManager.SetChanged();

                rawSetLastChanged[sourceIndex] = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Deactivate the action set so its actions can no longer be called
        /// </summary>
        public void Deactivate(SteamVRInputSources forSource = SteamVRInputSources.Any)
        {
            int sourceIndex = (int)forSource;

            if (rawSetActive[sourceIndex] != false)
            {
                rawSetLastChanged[sourceIndex] = Time.realtimeSinceStartup;
                SteamVRActionSetManager.SetChanged();
            }

            rawSetActive[sourceIndex] = false;
            rawSetPriority[sourceIndex] = 0;
        }

        private string cachedShortName;

        /// <summary>Gets the last part of the path for this action. Removes "actions" and direction.</summary>
        public string GetShortName()
        {
            if (cachedShortName == null)
            {
                cachedShortName = SteamVRInputActionFile.GetShortName(fullPath);
            }

            return cachedShortName;
        }

        public bool ReadRawSetActive(SteamVRInputSources inputSource)
        {
            int sourceIndex = (int)inputSource;
            return rawSetActive[sourceIndex];
        }

        public float ReadRawSetLastChanged(SteamVRInputSources inputSource)
        {
            int sourceIndex = (int)inputSource;
            return rawSetLastChanged[sourceIndex];
        }

        public int ReadRawSetPriority(SteamVRInputSources inputSource)
        {
            int sourceIndex = (int)inputSource;
            return rawSetPriority[sourceIndex];
        }
    }
    /// <summary>
    /// Action sets are logical groupings of actions. Multiple sets can be active at one time.
    /// </summary>
    public interface ISteamVRActionSet
    {
        /// <summary>All actions within this set (including out actions)</summary>
        SteamVRAction[] allActions { get; }

        /// <summary>All IN actions within this set that are NOT pose or skeleton actions</summary>
        ISteamVRActionIn[] nonVisualInActions { get; }

        /// <summary>All pose and skeleton actions within this set</summary>
        ISteamVRActionIn[] visualActions { get; }

        /// <summary>All pose actions within this set</summary>
        SteamVRActionPose[] poseActions { get; }

        /// <summary>All skeleton actions within this set</summary>
        SteamVRActionSkeleton[] skeletonActions { get; }

        /// <summary>All out actions within this set</summary>
        ISteamVRActionOut[] outActionArray { get; }

        /// <summary>The full path to this action set (ex: /actions/in/default)</summary>
        string fullPath { get; }

        /// <summary>How the binding UI should display this set</summary>
        string usage { get; }

        ulong handle { get; }

        bool ReadRawSetActive(SteamVRInputSources inputSource);
        float ReadRawSetLastChanged(SteamVRInputSources inputSource);
        int ReadRawSetPriority(SteamVRInputSources inputSource);

        /// <summary>
        /// Returns whether the set is currently active or not.
        /// </summary>
        /// <param name="source">The device to check. Any means all devices here (not left or right, but all)</param>
        bool IsActive(SteamVRInputSources source = SteamVRInputSources.Any);

        /// <summary>
        /// Returns the last time this action set was changed (set to active or inactive)
        /// </summary>
        /// <param name="source">The device to check. Any means all devices here (not left or right, but all)</param>
        float GetTimeLastChanged(SteamVRInputSources source = SteamVRInputSources.Any);

        /// <summary>
        /// Activate this set so its actions can be called
        /// </summary>
        /// <param name="disableAllOtherActionSets">Disable all other action sets at the same time</param>
        /// <param name="priority">The priority of this action set. If you have two actions bound to the same input (button) the higher priority set will override the lower priority. If they are the same priority both will execute.</param>
        /// <param name="activateForSource">Will activate this action set only for the specified source. Any if you want to activate for everything</param>
        void Activate(SteamVRInputSources activateForSource = SteamVRInputSources.Any, int priority = 0, bool disableAllOtherActionSets = false);

        /// <summary>Deactivate the action set so its actions can no longer be called</summary>
        void Deactivate(SteamVRInputSources forSource = SteamVRInputSources.Any);

        /// <summary>Gets the last part of the path for this action. Removes "actions" and direction.</summary>
        string GetShortName();
    }
}