//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Valve.VR
{
    [Serializable]
    /// <summary>
    /// An analog action with two values generally from -1 to 1. Also provides a delta since the last update.
    /// </summary>
    public class SteamVRActionVector2 : SteamVRActionIn<SteamVRActionVector2SourceMap, SteamVRActionVector2Source>, ISteamVRActionVector2
    {
        public delegate void AxisHandler(SteamVRActionVector2 fromAction, SteamVRInputSources fromSource, Vector2 axis, Vector2 delta);
        public delegate void ActiveChangeHandler(SteamVRActionVector2 fromAction, SteamVRInputSources fromSource, bool active);
        public delegate void ChangeHandler(SteamVRActionVector2 fromAction, SteamVRInputSources fromSource, Vector2 axis, Vector2 delta);
        public delegate void UpdateHandler(SteamVRActionVector2 fromAction, SteamVRInputSources fromSource, Vector2 axis, Vector2 delta);

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event fires whenever the axis changes by more than the specified changeTolerance</summary>
        public event ChangeHandler onChange
        { add { sourceMap[SteamVRInputSources.Any].onChange += value; } remove { sourceMap[SteamVRInputSources.Any].onChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event fires whenever the action is updated</summary>
        public event UpdateHandler onUpdate
        { add { sourceMap[SteamVRInputSources.Any].onUpdate += value; } remove { sourceMap[SteamVRInputSources.Any].onUpdate -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event will fire whenever the Vector2 value of the action is non-zero</summary>
        public event AxisHandler onAxis
        { add { sourceMap[SteamVRInputSources.Any].onAxis += value; } remove { sourceMap[SteamVRInputSources.Any].onAxis -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the active state (ActionSet active and binding active) changes</summary>
        public event ActiveChangeHandler onActiveChange
        { add { sourceMap[SteamVRInputSources.Any].onActiveChange += value; } remove { sourceMap[SteamVRInputSources.Any].onActiveChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the active state of the binding changes</summary>
        public event ActiveChangeHandler onActiveBindingChange
        { add { sourceMap[SteamVRInputSources.Any].onActiveBindingChange += value; } remove { sourceMap[SteamVRInputSources.Any].onActiveBindingChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The current Vector2 value of the action.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 axis { get { return sourceMap[SteamVRInputSources.Any].axis; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The Vector2 value of the action from the previous update.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 lastAxis { get { return sourceMap[SteamVRInputSources.Any].lastAxis; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The Vector2 value difference between this update and the previous update.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 delta { get { return sourceMap[SteamVRInputSources.Any].delta; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> The Vector2 value difference between the previous update and update before that.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 lastDelta { get { return sourceMap[SteamVRInputSources.Any].lastDelta; } }

        public SteamVRActionVector2() { }

        /// <summary>The current Vector2 value of the action</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector2 GetAxis(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].axis;
        }

        /// <summary>The Vector2 value difference between this update and the previous update.</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector2 GetAxisDelta(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].delta;
        }

        /// <summary>The Vector2 value of the action from the previous update.</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector2 GetLastAxis(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastAxis;
        }

        /// <summary>The Vector2 value difference between the previous update and update before that. </summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public Vector2 GetLastAxisDelta(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastDelta;
        }

        /// <summary>Executes a function when the *functional* active state of this action (with the specified inputSource) changes.
        /// This happens when the action is bound or unbound, or when the ActionSet changes state.</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's active state changes and the corresponding input source</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnActiveChangeListener(ActiveChangeHandler functionToCall, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onActiveChange += functionToCall;
        }

        /// <summary>Stops executing a function when the *functional* active state of this action (with the specified inputSource) changes.
        /// This happens when the action is bound or unbound, or when the ActionSet changes state.</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnActiveChangeListener(ActiveChangeHandler functionToStopCalling, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onActiveChange -= functionToStopCalling;
        }

        /// <summary>Executes a function when the active state of this action (with the specified inputSource) changes. This happens when the action is bound or unbound</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's active state changes and the corresponding input source</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnActiveBindingChangeListener(ActiveChangeHandler functionToCall, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onActiveBindingChange += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnActiveBindingChangeListener(ActiveChangeHandler functionToStopCalling, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onActiveBindingChange -= functionToStopCalling;
        }

        /// <summary>Executes a function when the axis changes by more than the specified changeTolerance</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnChangeListener(ChangeHandler functionToCall, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onChange += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive on change events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnChangeListener(ChangeHandler functionToStopCalling, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onChange -= functionToStopCalling;
        }

        /// <summary>Executes a function when the state of this action (with the specified inputSource) is updated.</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnUpdateListener(UpdateHandler functionToCall, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onUpdate += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnUpdateListener(UpdateHandler functionToStopCalling, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onUpdate -= functionToStopCalling;
        }

        /// <summary>Executes a function when the Vector2 value of the action is non-zero.</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnAxisListener(AxisHandler functionToCall, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onAxis += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnAxisListener(AxisHandler functionToStopCalling, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onAxis -= functionToStopCalling;
        }

        /// <summary>
        /// Removes all listeners, useful for dispose pattern
        /// </summary>
        public void RemoveAllListeners(SteamVRInputSources inputSources)
        {
            sourceMap[inputSources].RemoveAllListeners();
        }
    }

    /// <summary>
    /// Boolean actions are either true or false. There is an onStateUp and onStateDown event for the rising and falling edge.
    /// </summary>
    public class SteamVRActionVector2SourceMap : SteamVRActionInSourceMap<SteamVRActionVector2Source>
    {
    }

    public class SteamVRActionVector2Source : SteamVRActionInSource, ISteamVRActionVector2
    {
        protected static uint actionData_size = 0;

        /// <summary>The amount the axis needs to change before a change is detected</summary>
        public float changeTolerance = Mathf.Epsilon;

        /// <summary>Event fires when the value of the action is non-zero</summary>
        public event SteamVRActionVector2.AxisHandler onAxis;

        /// <summary>Event fires when the active state (ActionSet active and binding active) changes</summary>
        public event SteamVRActionVector2.ActiveChangeHandler onActiveChange;

        /// <summary>Event fires when the active state of the binding changes</summary>
        public event SteamVRActionVector2.ActiveChangeHandler onActiveBindingChange;

        /// <summary>This event fires whenever the axis changes by more than the specified changeTolerance</summary>
        public event SteamVRActionVector2.ChangeHandler onChange;

        /// <summary>Event fires when the action is updated</summary>
        public event SteamVRActionVector2.UpdateHandler onUpdate;

        /// <summary>The current Vector2 value of the action.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 axis { get; protected set; }

        /// <summary>The Vector2 value of the action from the previous update.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 lastAxis { get; protected set; }

        /// <summary>The Vector2 value difference between this update and the previous update.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 delta { get; protected set; }

        /// <summary>The Vector2 value difference between the previous update and update before that.
        /// Note: Will only return non-zero if the action is also active.</summary>
        public Vector2 lastDelta { get; protected set; }

        /// <summary>If the Vector2 value of this action has changed more than the changeTolerance since the last update</summary>
        public override bool changed { get; protected set; }

        /// <summary>If the Vector2 value of this action has changed more than the changeTolerance between the previous update and the update before that</summary>
        public override bool lastChanged { get; protected set; }

        /// <summary>The handle to the origin of the component that was used to update the value for this action</summary>
        public override ulong activeOrigin
        {
            get
            {
                if (active)
                {
                    return actionData.activeOrigin;
                }

                return 0;
            }
        }

        /// <summary>The handle to the origin of the component that was used to update the value for this action (for the previous update)</summary>
        public override ulong lastActiveOrigin { get { return lastActionData.activeOrigin; } }

        /// <summary>Returns true if this action is bound and the ActionSet is active</summary>
        public override bool active { get { return activeBinding && action.actionSet.IsActive(inputSource); } }

        /// <summary>Returns true if the action is bound</summary>
        public override bool activeBinding { get { return actionData.bActive; } }

        /// <summary>Returns true if the action was bound and the ActionSet was active during the previous update</summary>
        public override bool lastActive { get; protected set; }

        /// <summary>Returns true if the action was bound during the previous update</summary>
        public override bool lastActiveBinding { get { return lastActionData.bActive; } }

        protected InputAnalogActionDataT actionData = new();
        protected InputAnalogActionDataT lastActionData = new();

        protected SteamVRActionVector2 vector2Action;

        /// <summary>
        /// <strong>[Should not be called by user code]</strong> Sets up the internals of the action source before SteamVR has been initialized.
        /// </summary>
        public override void Preinitialize(SteamVRAction wrappingAction, SteamVRInputSources forInputSource)
        {
            base.Preinitialize(wrappingAction, forInputSource);
            vector2Action = (SteamVRActionVector2)wrappingAction;
        }

        /// <summary>
        /// <strong>[Should not be called by user code]</strong>
        /// Initializes the handle for the inputSource, the action data size, and any other related SteamVR data.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            if (actionData_size == 0)
            {
                actionData_size = (uint)Marshal.SizeOf(typeof(InputAnalogActionDataT));
            }
        }

        /// <summary>
        /// Removes all listeners, useful for dispose pattern
        /// </summary>
        public void RemoveAllListeners()
        {
            Delegate[] delegates;

            if (onAxis != null)
            {
                delegates = onAxis.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onAxis -= (SteamVRActionVector2.AxisHandler)existingDelegate;
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
                        onUpdate -= (SteamVRActionVector2.UpdateHandler)existingDelegate;
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
                        onChange -= (SteamVRActionVector2.ChangeHandler)existingDelegate;
                    }
                }
            }

            if (onActiveChange != null)
            {
                delegates = onActiveChange.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onActiveChange -= (SteamVRActionVector2.ActiveChangeHandler)existingDelegate;
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
                        onActiveBindingChange -= (SteamVRActionVector2.ActiveChangeHandler)existingDelegate;
                    }
                }
            }
        }

        /// <summary><strong>[Should not be called by user code]</strong>
        /// Updates the data for this action and this input source. Sends related events.
        /// </summary>
        public override void UpdateValue()
        {
            lastActionData = actionData;
            lastActive = active;
            lastAxis = axis;
            lastDelta = delta;

            EVRInputError err = OpenVR.Input.GetAnalogActionData(handle, ref actionData, actionData_size, SteamVRInputSource.GetHandle(inputSource));
            if (err != EVRInputError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] GetAnalogActionData error (" + fullPath + "): " + err.ToString() + " handle: " + handle.ToString());
            }

            updateTime = Time.realtimeSinceStartup;
            axis = new Vector2(actionData.x, actionData.y);
            delta = new Vector2(actionData.deltaX, actionData.deltaY);

            changed = false;

            if (active)
            {
                if (delta.magnitude > changeTolerance)
                {
                    changed = true;
                    changedTime = Time.realtimeSinceStartup + actionData.fUpdateTime; //fUpdateTime is the time from the time the action was called that the action changed

                    onChange?.Invoke(vector2Action, inputSource, axis, delta);
                }

                if (axis != Vector2.zero)
                {
                    onAxis?.Invoke(vector2Action, inputSource, axis, delta);
                }

                onUpdate?.Invoke(vector2Action, inputSource, axis, delta);
            }

            if (onActiveBindingChange != null && lastActiveBinding != activeBinding)
            {
                onActiveBindingChange.Invoke(vector2Action, inputSource, activeBinding);
            }

            if (onActiveChange != null && lastActive != active)
            {
                onActiveChange.Invoke(vector2Action, inputSource, activeBinding);
            }
        }
    }

    public interface ISteamVRActionVector2 : ISteamVRActionInSource
    {
        /// <summary>The current float value of the action.
        /// Note: Will only return non-zero if the action is also active.</summary>
        Vector2 axis { get; }

        /// <summary>The float value of the action from the previous update.
        /// Note: Will only return non-zero if the action is also active.</summary>
        Vector2 lastAxis { get; }

        /// <summary>The float value difference between this update and the previous update.
        /// Note: Will only return non-zero if the action is also active.</summary>
        Vector2 delta { get; }

        /// <summary>The float value difference between the previous update and update before that.
        /// Note: Will only return non-zero if the action is also active.</summary>
        Vector2 lastDelta { get; }
    }
}