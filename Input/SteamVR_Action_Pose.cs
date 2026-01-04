//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Valve.VR
{
    [Serializable]
    /// <summary>
    /// Pose actions represent a position, rotation, and velocities inside the tracked space.
    /// SteamVR keeps a log of past poses so you can retrieve old poses with GetPoseAtTimeOffset or GetVelocitiesAtTimeOffset.
    /// You can also pass in times in the future to these methods for SteamVR's best prediction of where the pose will be at that time.
    /// </summary>
    public class SteamVRActionPose : SteamVRActionPoseBase<SteamVRActionPoseSourceMap<SteamVRActionPoseSource>, SteamVRActionPoseSource>
    {
        public delegate void ActiveChangeHandler(SteamVRActionPose fromAction, SteamVRInputSources fromSource, bool active);
        public delegate void ChangeHandler(SteamVRActionPose fromAction, SteamVRInputSources fromSource);
        public delegate void UpdateHandler(SteamVRActionPose fromAction, SteamVRInputSources fromSource);
        public delegate void TrackingChangeHandler(SteamVRActionPose fromAction, SteamVRInputSources fromSource, ETrackingResult trackingState);
        public delegate void ValidPoseChangeHandler(SteamVRActionPose fromAction, SteamVRInputSources fromSource, bool validPose);
        public delegate void DeviceConnectedChangeHandler(SteamVRActionPose fromAction, SteamVRInputSources fromSource, bool deviceConnected);

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the active state (ActionSet active and binding active) changes</summary>
        public event ActiveChangeHandler onActiveChange
        { add { sourceMap[SteamVRInputSources.Any].onActiveChange += value; } remove { sourceMap[SteamVRInputSources.Any].onActiveChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the active state of the binding changes</summary>
        public event ActiveChangeHandler onActiveBindingChange
        { add { sourceMap[SteamVRInputSources.Any].onActiveBindingChange += value; } remove { sourceMap[SteamVRInputSources.Any].onActiveBindingChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the orientation of the pose changes more than the changeTolerance</summary>
        public event ChangeHandler onChange
        { add { sourceMap[SteamVRInputSources.Any].onChange += value; } remove { sourceMap[SteamVRInputSources.Any].onChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the action is updated</summary>
        public event UpdateHandler onUpdate
        { add { sourceMap[SteamVRInputSources.Any].onUpdate += value; } remove { sourceMap[SteamVRInputSources.Any].onUpdate -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the state of the tracking has changed</summary>
        public event TrackingChangeHandler onTrackingChanged
        { add { sourceMap[SteamVRInputSources.Any].onTrackingChanged += value; } remove { sourceMap[SteamVRInputSources.Any].onTrackingChanged -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the validity of the pose has changed</summary>
        public event ValidPoseChangeHandler onValidPoseChanged
        { add { sourceMap[SteamVRInputSources.Any].onValidPoseChanged += value; } remove { sourceMap[SteamVRInputSources.Any].onValidPoseChanged -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the device bound to this pose is connected or disconnected</summary>
        public event DeviceConnectedChangeHandler onDeviceConnectedChanged
        { add { sourceMap[SteamVRInputSources.Any].onDeviceConnectedChanged += value; } remove { sourceMap[SteamVRInputSources.Any].onDeviceConnectedChanged -= value; } }

        /// <summary>Fires an event when a device is connected or disconnected.</summary>
        /// <param name="inputSource">The device you would like to add an event to. Any if the action is not device specific.</param>
        /// <param name="functionToCall">The method you would like to be called when a device is connected. Should take a SteamVR_Action_Pose as a param</param>
        public void AddOnDeviceConnectedChanged(SteamVRInputSources inputSource, DeviceConnectedChangeHandler functionToCall)
        {
            sourceMap[inputSource].onDeviceConnectedChanged += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="inputSource">The device you would like to remove an event from. Any if the action is not device specific.</param>
        /// <param name="functionToStopCalling">The method you would like to stop calling when a device is connected. Should take a SteamVR_Action_Pose as a param</param>
        public void RemoveOnDeviceConnectedChanged(SteamVRInputSources inputSource, DeviceConnectedChangeHandler functionToStopCalling)
        {
            sourceMap[inputSource].onDeviceConnectedChanged -= functionToStopCalling;
        }

        /// <summary>Fires an event when the tracking of the device has changed</summary>
        /// <param name="inputSource">The device you would like to add an event to. Any if the action is not device specific.</param>
        /// <param name="functionToCall">The method you would like to be called when tracking has changed. Should take a SteamVR_Action_Pose as a param</param>
        public void AddOnTrackingChanged(SteamVRInputSources inputSource, TrackingChangeHandler functionToCall)
        {
            sourceMap[inputSource].onTrackingChanged += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="inputSource">The device you would like to remove an event from. Any if the action is not device specific.</param>
        /// <param name="functionToStopCalling">The method you would like to stop calling when tracking has changed. Should take a SteamVR_Action_Pose as a param</param>
        public void RemoveOnTrackingChanged(SteamVRInputSources inputSource, TrackingChangeHandler functionToStopCalling)
        {
            sourceMap[inputSource].onTrackingChanged -= functionToStopCalling;
        }

        /// <summary>Fires an event when the device now has a valid pose or no longer has a valid pose</summary>
        /// <param name="inputSource">The device you would like to add an event to. Any if the action is not device specific.</param>
        /// <param name="functionToCall">The method you would like to be called when the pose has become valid or invalid. Should take a SteamVR_Action_Pose as a param</param>
        public void AddOnValidPoseChanged(SteamVRInputSources inputSource, ValidPoseChangeHandler functionToCall)
        {
            sourceMap[inputSource].onValidPoseChanged += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="inputSource">The device you would like to remove an event from. Any if the action is not device specific.</param>
        /// <param name="functionToStopCalling">The method you would like to stop calling when the pose has become valid or invalid. Should take a SteamVR_Action_Pose as a param</param>
        public void RemoveOnValidPoseChanged(SteamVRInputSources inputSource, ValidPoseChangeHandler functionToStopCalling)
        {
            sourceMap[inputSource].onValidPoseChanged -= functionToStopCalling;
        }

        /// <summary>Executes a function when this action's bound state changes</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnActiveChangeListener(SteamVRInputSources inputSource, ActiveChangeHandler functionToCall)
        {
            sourceMap[inputSource].onActiveChange += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnActiveChangeListener(SteamVRInputSources inputSource, ActiveChangeHandler functionToStopCalling)
        {
            sourceMap[inputSource].onActiveChange -= functionToStopCalling;
        }

        /// <summary>Executes a function when the state of this action (with the specified inputSource) changes</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnChangeListener(SteamVRInputSources inputSource, ChangeHandler functionToCall)
        {
            sourceMap[inputSource].onChange += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive on change events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnChangeListener(SteamVRInputSources inputSource, ChangeHandler functionToStopCalling)
        {
            sourceMap[inputSource].onChange -= functionToStopCalling;
        }

        /// <summary>Executes a function when the state of this action (with the specified inputSource) is updated.</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnUpdateListener(SteamVRInputSources inputSource, UpdateHandler functionToCall)
        {
            sourceMap[inputSource].onUpdate += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnUpdateListener(SteamVRInputSources inputSource, UpdateHandler functionToStopCalling)
        {
            sourceMap[inputSource].onUpdate -= functionToStopCalling;
        }

        /// <summary>
        /// Removes all listeners, useful for dispose pattern
        /// </summary>
        public void RemoveAllListeners(SteamVRInputSources inputSources)
        {
            sourceMap[inputSources].RemoveAllListeners();
        }

        /// <summary>
        /// Sets all pose and skeleton actions to use the specified universe origin.
        /// </summary>
        public static void SetTrackingUniverseOrigin(ETrackingUniverseOrigin newOrigin)
        {
            SetUniverseOrigin(newOrigin);
            OpenVR.Compositor.SetTrackingSpace(newOrigin);
        }
    }

    [Serializable]
    /// <summary>
    /// The base pose action (pose and skeleton inherit from this)
    /// </summary>
    public abstract class SteamVRActionPoseBase<SourceMap, SourceElement> : SteamVRActionIn<SourceMap, SourceElement>, ISteamVRActionPose
        where SourceMap : SteamVRActionPoseSourceMap<SourceElement>, new()
        where SourceElement : SteamVRActionPoseSource, new()
    {
        /// <summary>
        /// Sets all pose (and skeleton) actions to use the specified universe origin.
        /// </summary>
        protected static void SetUniverseOrigin(ETrackingUniverseOrigin newOrigin)
        {
            for (int actionIndex = 0; actionIndex < SteamVRInput.actionsPose.Length; actionIndex++)
            {
                SteamVRInput.actionsPose[actionIndex].sourceMap.SetTrackingUniverseOrigin(newOrigin);
            }

            for (int actionIndex = 0; actionIndex < SteamVRInput.actionsSkeleton.Length; actionIndex++)
            {
                SteamVRInput.actionsSkeleton[actionIndex].sourceMap.SetTrackingUniverseOrigin(newOrigin);
            }
        }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The local position of this action relative to the universe origin</summary>
        public Vector3 localPosition { get { return sourceMap[SteamVRInputSources.Any].localPosition; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The local rotation of this action relative to the universe origin</summary>
        public Quaternion localRotation { get { return sourceMap[SteamVRInputSources.Any].localRotation; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The state of the tracking system that is used to create pose data (position, rotation, etc)</summary>
        public ETrackingResult trackingState { get { return sourceMap[SteamVRInputSources.Any].trackingState; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The local velocity of this pose relative to the universe origin</summary>
        public Vector3 velocity { get { return sourceMap[SteamVRInputSources.Any].velocity; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The local angular velocity of this pose relative to the universe origin</summary>
        public Vector3 angularVelocity { get { return sourceMap[SteamVRInputSources.Any].angularVelocity; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> True if the pose retrieved for this action and input source is valid (good data from the tracking source)</summary>
        public bool poseIsValid { get { return sourceMap[SteamVRInputSources.Any].poseIsValid; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> True if the device bound to this action and input source is connected</summary>
        public bool deviceIsConnected { get { return sourceMap[SteamVRInputSources.Any].deviceIsConnected; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The local position for this pose during the previous update</summary>
        public Vector3 lastLocalPosition { get { return sourceMap[SteamVRInputSources.Any].lastLocalPosition; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The local rotation for this pose during the previous update</summary>
        public Quaternion lastLocalRotation { get { return sourceMap[SteamVRInputSources.Any].lastLocalRotation; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The tracking state for this pose during the previous update</summary>
        public ETrackingResult lastTrackingState { get { return sourceMap[SteamVRInputSources.Any].lastTrackingState; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The velocity for this pose during the previous update</summary>
        public Vector3 lastVelocity { get { return sourceMap[SteamVRInputSources.Any].lastVelocity; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The angular velocity for this pose during the previous update</summary>
        public Vector3 lastAngularVelocity { get { return sourceMap[SteamVRInputSources.Any].lastAngularVelocity; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> True if the pose was valid during the previous update</summary>
        public bool lastPoseIsValid { get { return sourceMap[SteamVRInputSources.Any].lastPoseIsValid; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> True if the device bound to this action was connected during the previous update</summary>
        public bool lastDeviceIsConnected { get { return sourceMap[SteamVRInputSources.Any].lastDeviceIsConnected; } }

        public SteamVRActionPoseBase() { }

        /// <summary>
        /// <strong>[Should not be called by user code]</strong>
        /// Updates the data for all the input sources the system has detected need to be updated.
        /// </summary>
        public virtual void UpdateValues(bool skipStateAndEventUpdates)
        {
            sourceMap.UpdateValues(skipStateAndEventUpdates);
        }

        /// <summary>
        /// SteamVR keeps a log of past poses so you can retrieve old poses or estimated poses in the future by passing in a secondsFromNow value that is negative or positive.
        /// </summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        /// <param name="secondsFromNow">The time offset in the future (estimated) or in the past (previously recorded) you want to get data from</param>
        /// <returns>true if the call succeeded</returns>
        public bool GetVelocitiesAtTimeOffset(SteamVRInputSources inputSource, float secondsFromNow, out Vector3 velocity, out Vector3 angularVelocity)
        {
            return sourceMap[inputSource].GetVelocitiesAtTimeOffset(secondsFromNow, out velocity, out angularVelocity);
        }

        /// <summary>
        /// SteamVR keeps a log of past poses so you can retrieve old poses or estimated poses in the future by passing in a secondsFromNow value that is negative or positive.
        /// </summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        /// <param name="secondsFromNow">The time offset in the future (estimated) or in the past (previously recorded) you want to get data from</param>
        /// <returns>true if the call succeeded</returns>
        public bool GetPoseAtTimeOffset(SteamVRInputSources inputSource, float secondsFromNow, out Vector3 localPosition, out Quaternion localRotation, out Vector3 velocity, out Vector3 angularVelocity)
        {
            return sourceMap[inputSource].GetPoseAtTimeOffset(secondsFromNow, out localPosition, out localRotation, out velocity, out angularVelocity);
        }

        /// <summary>
        /// Update a transform's local position and local roation to match the pose from the most recent update
        /// </summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        /// <param name="transformToUpdate">The transform of the object to be updated</param>
        public virtual void UpdateTransform(SteamVRInputSources inputSource, Transform transformToUpdate)
        {
            sourceMap[inputSource].UpdateTransform(transformToUpdate);
        }

        /// <summary>The local position of this action relative to the universe origin</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector3 GetLocalPosition(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].localPosition;
        }

        /// <summary>The local rotation of this action relative to the universe origin</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Quaternion GetLocalRotation(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].localRotation;
        }

        /// <summary>The local velocity of this pose relative to the universe origin</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector3 GetVelocity(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].velocity;
        }

        /// <summary>The local angular velocity of this pose relative to the universe origin</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector3 GetAngularVelocity(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].angularVelocity;
        }

        /// <summary>True if the device bound to this action and input source is connected</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetDeviceIsConnected(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].deviceIsConnected;
        }

        /// <summary>True if the pose retrieved for this action and input source is valid (good data from the tracking source)</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetPoseIsValid(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].poseIsValid;
        }

        /// <summary>The state of the tracking system that is used to create pose data (position, rotation, etc)</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public ETrackingResult GetTrackingResult(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].trackingState;
        }

        /// <summary>The local position for this pose during the previous update</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector3 GetLastLocalPosition(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastLocalPosition;
        }

        /// <summary>The local rotation for this pose during the previous update</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Quaternion GetLastLocalRotation(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastLocalRotation;
        }

        /// <summary>The velocity for this pose during the previous update</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector3 GetLastVelocity(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastVelocity;
        }

        /// <summary>The angular velocity for this pose during the previous update</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector3 GetLastAngularVelocity(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastAngularVelocity;
        }

        /// <summary>True if the device bound to this action was connected during the previous update</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetLastDeviceIsConnected(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastDeviceIsConnected;
        }

        /// <summary>True if the pose was valid during the previous update</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetLastPoseIsValid(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastPoseIsValid;
        }

        /// <summary>The tracking state for this pose during the previous update</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public ETrackingResult GetLastTrackingResult(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastTrackingState;
        }
    }

    /// <summary>
    /// Boolean actions are either true or false. There is an onStateUp and onStateDown event for the rising and falling edge.
    /// </summary>
    public class SteamVRActionPoseSourceMap<Source> : SteamVRActionInSourceMap<Source>
        where Source : SteamVRActionPoseSource, new()
    {
        /// <summary>
        /// Sets all pose (and skeleton) actions to use the specified universe origin without going through the sourcemap indexer
        /// </summary>
        public void SetTrackingUniverseOrigin(ETrackingUniverseOrigin newOrigin)
        {
            for (int sourceIndex = 0; sourceIndex < sources.Length; sourceIndex++)
            {
                if (sources[sourceIndex] != null)
                {
                    sources[sourceIndex].universeOrigin = newOrigin;
                }
            }
        }

        public virtual void UpdateValues(bool skipStateAndEventUpdates)
        {
            for (int sourceIndex = 0; sourceIndex < updatingSources.Count; sourceIndex++)
            {
                sources[updatingSources[sourceIndex]].UpdateValue(skipStateAndEventUpdates);
            }
        }
    }

    public class SteamVRActionPoseSource : SteamVRActionInSource, ISteamVRActionPose
    {
        public ETrackingUniverseOrigin universeOrigin = ETrackingUniverseOrigin.TrackingUniverseRawAndUncalibrated;

        protected static uint poseActionData_size = 0;

        /// <summary>The distance the pose needs to move/rotate before a change is detected</summary>
        public float changeTolerance = Mathf.Epsilon;

        /// <summary>Event fires when the active state (ActionSet active and binding active) changes</summary>
        public event SteamVRActionPose.ActiveChangeHandler onActiveChange;

        /// <summary>Event fires when the active state of the binding changes</summary>
        public event SteamVRActionPose.ActiveChangeHandler onActiveBindingChange;

        /// <summary>Event fires when the orientation of the pose changes more than the changeTolerance</summary>
        public event SteamVRActionPose.ChangeHandler onChange;

        /// <summary>Event fires when the action is updated</summary>
        public event SteamVRActionPose.UpdateHandler onUpdate;

        /// <summary>Event fires when the state of the tracking system that is used to create pose data (position, rotation, etc) changes</summary>
        public event SteamVRActionPose.TrackingChangeHandler onTrackingChanged;

        /// <summary>Event fires when the state of the pose data retrieved for this action changes validity (good/bad data from the tracking source)</summary>
        public event SteamVRActionPose.ValidPoseChangeHandler onValidPoseChanged;

        /// <summary>Event fires when the device bound to this action is connected or disconnected</summary>
        public event SteamVRActionPose.DeviceConnectedChangeHandler onDeviceConnectedChanged;

        /// <summary>True when the orientation of the pose has changhed more than changeTolerance in the last update. Note: Will only return true if the action is also active.</summary>
        public override bool changed { get; protected set; }

        /// <summary>The value of the action's 'changed' during the previous update</summary>
        public override bool lastChanged { get; protected set; }

        /// <summary>The handle to the origin of the component that was used to update this pose</summary>
        public override ulong activeOrigin
        {
            get
            {
                if (active)
                {
                    return poseActionData.activeOrigin;
                }

                return 0;
            }
        }

        /// <summary>The handle to the origin of the component that was used to update the value for this action (for the previous update)</summary>
        public override ulong lastActiveOrigin { get { return lastPoseActionData.activeOrigin; } }

        /// <summary>True if this action is bound and the ActionSet is active</summary>
        public override bool active { get { return activeBinding && action.actionSet.IsActive(inputSource); } }

        /// <summary>True if the action is bound</summary>
        public override bool activeBinding { get { return poseActionData.bActive; } }

        /// <summary>If the action was active (ActionSet active and binding active) during the last update</summary>
        public override bool lastActive { get; protected set; }

        /// <summary>If the action's binding was active during the previous update</summary>
        public override bool lastActiveBinding { get { return lastPoseActionData.bActive; } }

        /// <summary>The state of the tracking system that is used to create pose data (position, rotation, etc)</summary>
        public ETrackingResult trackingState { get { return poseActionData.pose.eTrackingResult; } }

        /// <summary>The tracking state for this pose during the previous update</summary>
        public ETrackingResult lastTrackingState { get { return lastPoseActionData.pose.eTrackingResult; } }

        /// <summary>True if the pose retrieved for this action and input source is valid (good data from the tracking source)</summary>
        public bool poseIsValid { get { return poseActionData.pose.bPoseIsValid; } }

        /// <summary>True if the pose was valid during the previous update</summary>
        public bool lastPoseIsValid { get { return lastPoseActionData.pose.bPoseIsValid; } }

        /// <summary>True if the device bound to this action and input source is connected</summary>
        public bool deviceIsConnected { get { return poseActionData.pose.bDeviceIsConnected; } }

        /// <summary>True if the device bound to this action was connected during the previous update</summary>
        public bool lastDeviceIsConnected { get { return lastPoseActionData.pose.bDeviceIsConnected; } }

        /// <summary>The local position of this action relative to the universe origin</summary>
        public Vector3 localPosition { get; protected set; }

        /// <summary>The local rotation of this action relative to the universe origin</summary>
        public Quaternion localRotation { get; protected set; }

        /// <summary>The local position for this pose during the previous update</summary>
        public Vector3 lastLocalPosition { get; protected set; }

        /// <summary>The local rotation for this pose during the previous update</summary>
        public Quaternion lastLocalRotation { get; protected set; }

        /// <summary>The local velocity of this pose relative to the universe origin</summary>
        public Vector3 velocity { get; protected set; }

        /// <summary>The velocity for this pose during the previous update</summary>
        public Vector3 lastVelocity { get; protected set; }

        /// <summary>The local angular velocity of this pose relative to the universe origin</summary>
        public Vector3 angularVelocity { get; protected set; }

        /// <summary>The angular velocity for this pose during the previous update</summary>
        public Vector3 lastAngularVelocity { get; protected set; }

        protected InputPoseActionDataT poseActionData = new();

        protected InputPoseActionDataT lastPoseActionData = new();

        protected InputPoseActionDataT tempPoseActionData = new();

        protected SteamVRActionPose poseAction;

        /// <summary>
        /// <strong>[Should not be called by user code]</strong> Sets up the internals of the action source before SteamVR has been initialized.
        /// </summary>
        public override void Preinitialize(SteamVRAction wrappingAction, SteamVRInputSources forInputSource)
        {
            base.Preinitialize(wrappingAction, forInputSource);
            poseAction = wrappingAction as SteamVRActionPose;
        }

        /// <summary>
        /// <strong>[Should not be called by user code]</strong>
        /// Initializes the handle for the inputSource, the pose action data size, and any other related SteamVR data.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (poseActionData_size == 0)
            {
                poseActionData_size = (uint)Marshal.SizeOf(typeof(InputPoseActionDataT));
            }
        }

        /// <summary>
        /// Removes all listeners, useful for dispose pattern
        /// </summary>
        public virtual void RemoveAllListeners()
        {

            Delegate[] delegates;

            if (onActiveChange != null)
            {
                delegates = onActiveChange.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onActiveChange -= (SteamVRActionPose.ActiveChangeHandler)existingDelegate;
                    }
                }
            }

            if (onActiveBindingChange != null)
            {
                delegates = onActiveBindingChange.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onActiveBindingChange -= (SteamVRActionPose.ActiveChangeHandler)existingDelegate;
                    }
                }
            }

            if (onChange != null)
            {
                delegates = onChange.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onChange -= (SteamVRActionPose.ChangeHandler)existingDelegate;
                    }
                }
            }

            if (onUpdate != null)
            {
                delegates = onUpdate.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onUpdate -= (SteamVRActionPose.UpdateHandler)existingDelegate;
                    }
                }
            }

            if (onTrackingChanged != null)
            {
                delegates = onTrackingChanged.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onTrackingChanged -= (SteamVRActionPose.TrackingChangeHandler)existingDelegate;
                    }
                }
            }

            if (onValidPoseChanged != null)
            {
                delegates = onValidPoseChanged.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onValidPoseChanged -= (SteamVRActionPose.ValidPoseChangeHandler)existingDelegate;
                    }
                }
            }

            if (onDeviceConnectedChanged != null)
            {
                delegates = onDeviceConnectedChanged.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onDeviceConnectedChanged -= (SteamVRActionPose.DeviceConnectedChangeHandler)existingDelegate;
                    }
                }
            }
        }

        /// <summary><strong>[Should not be called by user code]</strong>
        /// Updates the data for this action and this input source. Sends related events.
        /// </summary>
        public override void UpdateValue()
        {
            UpdateValue(false);
        }

        public static float framesAhead = -1;

        /// <summary><strong>[Should not be called by user code]</strong>
        /// Updates the data for this action and this input source. Sends related events.
        /// </summary>
        public virtual void UpdateValue(bool skipStateAndEventUpdates)
        {
            lastChanged = changed;
            lastPoseActionData = poseActionData;
            lastLocalPosition = localPosition;
            lastLocalRotation = localRotation;
            lastVelocity = velocity;
            lastAngularVelocity = angularVelocity;

            EVRInputError err;

            if (framesAhead == -1)
            {
                err = OpenVR.Input.GetPoseActionDataForNextFrame(handle, universeOrigin, ref poseActionData, poseActionData_size, inputSourceHandle);
            }
            else
            {
                err = OpenVR.Input.GetPoseActionDataRelativeToNow(handle, universeOrigin, framesAhead * (1 / SteamVR.Instance.hmdDisplayFrequency), ref poseActionData, poseActionData_size, inputSourceHandle);
            }

            if (err != EVRInputError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] GetPoseActionData error (" + fullPath + "): " + err.ToString() + " Handle: " + handle.ToString() + ". Input source: " + inputSource.ToString());
            }

            if (active)
            {
                SetCacheVariables();
                changed = GetChanged();
            }

            if (changed)
            {
                changedTime = updateTime;
            }

            if (skipStateAndEventUpdates == false)
            {
                CheckAndSendEvents();
            }
        }

        protected void SetCacheVariables()
        {
            localPosition = poseActionData.pose.mDeviceToAbsoluteTracking.GetPosition();
            localRotation = poseActionData.pose.mDeviceToAbsoluteTracking.GetRotation();
            velocity = GetUnityCoordinateVelocity(poseActionData.pose.vVelocity);
            angularVelocity = GetUnityCoordinateAngularVelocity(poseActionData.pose.vAngularVelocity);
            updateTime = Time.realtimeSinceStartup;
        }

        protected bool GetChanged()
        {
            if (Vector3.Distance(localPosition, lastLocalPosition) > changeTolerance)
            {
                return true;
            }
            else if (Mathf.Abs(Quaternion.Angle(localRotation, lastLocalRotation)) > changeTolerance)
            {
                return true;
            }
            else if (Vector3.Distance(velocity, lastVelocity) > changeTolerance)
            {
                return true;
            }
            else if (Vector3.Distance(angularVelocity, lastAngularVelocity) > changeTolerance)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// SteamVR keeps a log of past poses so you can retrieve old poses or estimated poses in the future by passing in a secondsFromNow value that is negative or positive.
        /// </summary>
        /// <param name="secondsFromNow">The time offset in the future (estimated) or in the past (previously recorded) you want to get data from</param>
        /// <returns>true if we successfully returned a pose</returns>
        public bool GetVelocitiesAtTimeOffset(float secondsFromNow, out Vector3 velocityAtTime, out Vector3 angularVelocityAtTime)
        {
            EVRInputError err = OpenVR.Input.GetPoseActionDataRelativeToNow(handle, universeOrigin, secondsFromNow, ref tempPoseActionData, poseActionData_size, inputSourceHandle);
            if (err != EVRInputError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] GetPoseActionData error (" + fullPath + "): " + err.ToString() + " handle: " + handle.ToString());

                velocityAtTime = Vector3.zero;
                angularVelocityAtTime = Vector3.zero;
                return false;
            }

            velocityAtTime = GetUnityCoordinateVelocity(tempPoseActionData.pose.vVelocity);
            angularVelocityAtTime = GetUnityCoordinateAngularVelocity(tempPoseActionData.pose.vAngularVelocity);

            return true;
        }

        /// <summary>
        /// SteamVR keeps a log of past poses so you can retrieve old poses or estimated poses in the future by passing in a secondsFromNow value that is negative or positive.
        /// </summary>
        /// <param name="secondsFromNow">The time offset in the future (estimated) or in the past (previously recorded) you want to get data from</param>
        /// <returns>true if we successfully returned a pose</returns>
        public bool GetPoseAtTimeOffset(float secondsFromNow, out Vector3 positionAtTime, out Quaternion rotationAtTime, out Vector3 velocityAtTime, out Vector3 angularVelocityAtTime)
        {
            EVRInputError err = OpenVR.Input.GetPoseActionDataRelativeToNow(handle, universeOrigin, secondsFromNow, ref tempPoseActionData, poseActionData_size, inputSourceHandle);
            if (err != EVRInputError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] GetPoseActionData error (" + fullPath + "): " + err.ToString() + " handle: " + handle.ToString());
                velocityAtTime = Vector3.zero;
                angularVelocityAtTime = Vector3.zero;
                positionAtTime = Vector3.zero;
                rotationAtTime = Quaternion.identity;
                return false;
            }

            velocityAtTime = GetUnityCoordinateVelocity(tempPoseActionData.pose.vVelocity);
            angularVelocityAtTime = GetUnityCoordinateAngularVelocity(tempPoseActionData.pose.vAngularVelocity);
            positionAtTime = tempPoseActionData.pose.mDeviceToAbsoluteTracking.GetPosition();
            rotationAtTime = tempPoseActionData.pose.mDeviceToAbsoluteTracking.GetRotation();

            return true;
        }

        /// <summary>
        /// Update a transform's local position and local roation to match the pose.
        /// </summary>
        /// <param name="transformToUpdate">The transform of the object to be updated</param>
        public void UpdateTransform(Transform transformToUpdate)
        {
            transformToUpdate.localPosition = localPosition;
            transformToUpdate.localRotation = localRotation;
        }

        protected virtual void CheckAndSendEvents()
        {
            if (trackingState != lastTrackingState && onTrackingChanged != null)
            {
                onTrackingChanged.Invoke(poseAction, inputSource, trackingState);
            }

            if (poseIsValid != lastPoseIsValid && onValidPoseChanged != null)
            {
                onValidPoseChanged.Invoke(poseAction, inputSource, poseIsValid);
            }

            if (deviceIsConnected != lastDeviceIsConnected && onDeviceConnectedChanged != null)
            {
                onDeviceConnectedChanged.Invoke(poseAction, inputSource, deviceIsConnected);
            }

            if (changed && onChange != null)
            {
                onChange.Invoke(poseAction, inputSource);
            }

            if (active != lastActive && onActiveChange != null)
            {
                onActiveChange.Invoke(poseAction, inputSource, active);
            }

            if (activeBinding != lastActiveBinding && onActiveBindingChange != null)
            {
                onActiveBindingChange.Invoke(poseAction, inputSource, activeBinding);
            }

            onUpdate?.Invoke(poseAction, inputSource);
        }

        protected Vector3 GetUnityCoordinateVelocity(HmdVector3T vector)
        {
            return GetUnityCoordinateVelocity(vector.v0, vector.v1, vector.v2);
        }

        protected Vector3 GetUnityCoordinateAngularVelocity(HmdVector3T vector)
        {
            return GetUnityCoordinateAngularVelocity(vector.v0, vector.v1, vector.v2);
        }

        protected Vector3 GetUnityCoordinateVelocity(float x, float y, float z)
        {
            Vector3 vector = new()
            {
                x = x,
                y = y,
                z = -z
            };
            return vector;
        }

        protected Vector3 GetUnityCoordinateAngularVelocity(float x, float y, float z)
        {
            Vector3 vector = new()
            {
                x = -x,
                y = -y,
                z = z
            };
            return vector;
        }
    }

    /// <summary>
    /// Boolean actions are either true or false. There is an onStateUp and onStateDown event for the rising and falling edge.
    /// </summary>
    public interface ISteamVRActionPose : ISteamVRActionInSource
    {
        /// <summary>The local position of this action relative to the universe origin</summary>
        Vector3 localPosition { get; }

        /// <summary>The local rotation of this action relative to the universe origin</summary>
        Quaternion localRotation { get; }

        /// <summary>The state of the tracking system that is used to create pose data (position, rotation, etc)</summary>
        ETrackingResult trackingState { get; }

        /// <summary>The local velocity of this pose relative to the universe origin</summary>
        Vector3 velocity { get; }

        /// <summary>The local angular velocity of this pose relative to the universe origin</summary>
        Vector3 angularVelocity { get; }

        /// <summary>True if the pose retrieved for this action and input source is valid (good data from the tracking source)</summary>
        bool poseIsValid { get; }

        /// <summary>True if the device bound to this action and input source is connected</summary>
        bool deviceIsConnected { get; }

        /// <summary>The local position for this pose during the previous update</summary>
        Vector3 lastLocalPosition { get; }

        /// <summary>The local rotation for this pose during the previous update</summary>
        Quaternion lastLocalRotation { get; }

        /// <summary>The tracking state for this pose during the previous update</summary>
        ETrackingResult lastTrackingState { get; }

        /// <summary>The velocity for this pose during the previous update</summary>
        Vector3 lastVelocity { get; }

        /// <summary>The angular velocity for this pose during the previous update</summary>
        Vector3 lastAngularVelocity { get; }

        /// <summary>True if the pose was valid during the previous update</summary>
        bool lastPoseIsValid { get; }

        /// <summary>True if the device bound to this action was connected during the previous update</summary>
        bool lastDeviceIsConnected { get; }
    }
}