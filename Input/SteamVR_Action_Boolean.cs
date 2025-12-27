//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Valve.VR
{
    [Serializable]
    /// <summary>
    /// Boolean actions are either true or false. There are a variety of helper events included that will fire for the given input source. They're prefixed with "on".
    /// </summary>
    public class SteamVRActionBoolean : SteamVRActionIn<SteamVRActionBooleanSourceMap, SteamVRActionBooleanSource>, ISteamVRActionBoolean
    {
        public delegate void StateDownHandler(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource);
        public delegate void StateUpHandler(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource);
        public delegate void StateHandler(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource);
        public delegate void ActiveChangeHandler(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource, bool active);
        public delegate void ChangeHandler(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource, bool newState);
        public delegate void UpdateHandler(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource, bool newState);

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event fires whenever a state changes from false to true or true to false</summary>
        public event ChangeHandler onChange
        { add { sourceMap[SteamVRInputSources.Any].onChange += value; } remove { sourceMap[SteamVRInputSources.Any].onChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event fires whenever the action is updated</summary>
        public event UpdateHandler onUpdate
        { add { sourceMap[SteamVRInputSources.Any].onUpdate += value; } remove { sourceMap[SteamVRInputSources.Any].onUpdate -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event fires whenever the boolean action is true and gets updated</summary>
        public event StateHandler onState
        { add { sourceMap[SteamVRInputSources.Any].onState += value; } remove { sourceMap[SteamVRInputSources.Any].onState -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event fires whenever the state of the boolean action has changed from false to true in the most recent update</summary>
        public event StateDownHandler onStateDown
        { add { sourceMap[SteamVRInputSources.Any].onStateDown += value; } remove { sourceMap[SteamVRInputSources.Any].onStateDown -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> This event fires whenever the state of the boolean action has changed from true to false in the most recent update</summary>
        public event StateUpHandler onStateUp
        { add { sourceMap[SteamVRInputSources.Any].onStateUp += value; } remove { sourceMap[SteamVRInputSources.Any].onStateUp -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the active state (ActionSet active and binding active) changes</summary>
        public event ActiveChangeHandler onActiveChange
        { add { sourceMap[SteamVRInputSources.Any].onActiveChange += value; } remove { sourceMap[SteamVRInputSources.Any].onActiveChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> Event fires when the bound state of the binding changes</summary>
        public event ActiveChangeHandler onActiveBindingChange
        { add { sourceMap[SteamVRInputSources.Any].onActiveBindingChange += value; } remove { sourceMap[SteamVRInputSources.Any].onActiveBindingChange -= value; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> True when the boolean action is true</summary>
        public bool state { get { return sourceMap[SteamVRInputSources.Any].state; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> True when the boolean action is true and the last state was false</summary>
        public bool stateDown { get { return sourceMap[SteamVRInputSources.Any].stateDown; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> True when the boolean action is false and the last state was true</summary>
        public bool stateUp { get { return sourceMap[SteamVRInputSources.Any].stateUp; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> (previous update) True when the boolean action is true</summary>
        public bool lastState { get { return sourceMap[SteamVRInputSources.Any].lastState; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> (previous update) True when the boolean action is true and the last state was false</summary>
        public bool lastStateDown { get { return sourceMap[SteamVRInputSources.Any].lastStateDown; } }

        /// <summary><strong>[Shortcut to: SteamVR_Input_Sources.Any]</strong> (previous update) True when the boolean action is false and the last state was true</summary>
        public bool lastStateUp { get { return sourceMap[SteamVRInputSources.Any].lastStateUp; } }

        public SteamVRActionBoolean() { }

        /// <summary>Returns true if the value of the action has been changed to true (from false) in the most recent update.</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetStateDown(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].stateDown;
        }

        /// <summary>Returns true if the value of the action has been changed to false (from true) in the most recent update.</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetStateUp(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].stateUp;
        }

        /// <summary>Returns true if the value of the action (state) is currently true</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetState(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].state;
        }

        /// <summary>[For the previous update] Returns true if the value of the action has been set to true (from false).</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetLastStateDown(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastStateDown;
        }

        /// <summary>[For the previous update] Returns true if the value of the action has been set to false (from true).</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetLastStateUp(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastStateUp;
        }

        /// <summary>[For the previous update] Returns true if the value of the action was true.</summary>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public bool GetLastState(SteamVRInputSources inputSource)
        {
            return sourceMap[inputSource].lastState;
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

        /// <summary>Executes a function when the state of this action (with the specified inputSource) changes</summary>
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

        /// <summary>Executes a function when the state of this action (with the specified inputSource) changes to true (from false).</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnStateDownListener(StateDownHandler functionToCall, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onStateDown += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive update events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnStateDownListener(StateDownHandler functionToStopCalling, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onStateDown -= functionToStopCalling;
        }

        /// <summary>Executes a function when the state of this action (with the specified inputSource) changes to false (from true).</summary>
        /// <param name="functionToCall">A local function that receives the boolean action who's state has changed, the corresponding input source, and the new value</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void AddOnStateUpListener(StateUpHandler functionToCall, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onStateUp += functionToCall;
        }

        /// <summary>Stops executing the function setup by the corresponding AddListener</summary>
        /// <param name="functionToStopCalling">The local function that you've setup to receive events</param>
        /// <param name="inputSource">The device you would like to get data from. Any if the action is not device specific.</param>
        public void RemoveOnStateUpListener(StateUpHandler functionToStopCalling, SteamVRInputSources inputSource)
        {
            sourceMap[inputSource].onStateUp -= functionToStopCalling;
        }

        /// <summary>
        /// Remove all listeners registered in the source, useful for Dispose pattern
        /// </summary>
        public void RemoveAllListeners(SteamVRInputSources inputSources)
        {
            sourceMap[inputSources].RemoveAllListeners();
        }
    }

    public class SteamVRActionBooleanSourceMap : SteamVRActionInSourceMap<SteamVRActionBooleanSource>
    {
    }

    public class SteamVRActionBooleanSource : SteamVRActionInSource, ISteamVRActionBoolean
    {
        protected static uint actionData_size = 0;

        /// <summary>Event fires when the state of the action changes from false to true</summary>
        public event SteamVRActionBoolean.StateDownHandler onStateDown;

        /// <summary>Event fires when the state of the action changes from true to false</summary>
        public event SteamVRActionBoolean.StateUpHandler onStateUp;

        /// <summary>Event fires when the state of the action is true and the action gets updated</summary>
        public event SteamVRActionBoolean.StateHandler onState;

        /// <summary>Event fires when the active state (ActionSet active and binding active) changes</summary>
        public event SteamVRActionBoolean.ActiveChangeHandler onActiveChange;

        /// <summary>Event fires when the active state of the binding changes</summary>
        public event SteamVRActionBoolean.ActiveChangeHandler onActiveBindingChange;

        /// <summary>Event fires when the state of the action changes from false to true or true to false</summary>
        public event SteamVRActionBoolean.ChangeHandler onChange;

        /// <summary>Event fires when the action is updated</summary>
        public event SteamVRActionBoolean.UpdateHandler onUpdate;

        /// <summary>The current value of the boolean action. Note: Will only return true if the action is also active.</summary>
        public bool state { get { return active && actionData.bState; } }

        /// <summary>True when the action's state changes from false to true. Note: Will only return true if the action is also active.</summary>
        /// <remarks>Will only return true if the action is also active.</remarks>
        public bool stateDown { get { return active && actionData.bState && actionData.bChanged; } }

        /// <summary>True when the action's state changes from true to false. Note: Will only return true if the action is also active.</summary>
        /// <remarks>Will only return true if the action is also active.</remarks>
        public bool stateUp { get { return active && actionData.bState == false && actionData.bChanged; } }

        /// <summary>True when the action's state changed during the most recent update. Note: Will only return true if the action is also active.</summary>
        /// <remarks>ActionSet is ignored since get is coming from the native struct.</remarks>
        public override bool changed { get { return active && actionData.bChanged; } protected set { } }

        /// <summary>The value of the action's 'state' during the previous update</summary>
        /// <remarks>Always returns the previous update state</remarks>
        public bool lastState { get { return lastActionData.bState; } }

        /// <summary>The value of the action's 'stateDown' during the previous update</summary>
        /// <remarks>Always returns the previous update state</remarks>
        public bool lastStateDown { get { return lastActionData.bState && lastActionData.bChanged; } }

        /// <summary>The value of the action's 'stateUp' during the previous update</summary>
        /// <remarks>Always returns the previous update state</remarks>
        public bool lastStateUp { get { return lastActionData.bState == false && lastActionData.bChanged; } }

        /// <summary>The value of the action's 'changed' during the previous update</summary>
        /// <remarks>Always returns the previous update state. Set is ignored since get is coming from the native struct.</remarks>
        public override bool lastChanged { get { return lastActionData.bChanged; } protected set { } }

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

        protected InputDigitalActionDataT actionData = new();
        protected InputDigitalActionDataT lastActionData = new();

        protected SteamVRActionBoolean booleanAction;

        /// <summary>
        /// <strong>[Should not be called by user code]</strong> Sets up the internals of the action source before SteamVR has been initialized.
        /// </summary>
        public override void Preinitialize(SteamVRAction wrappingAction, SteamVRInputSources forInputSource)
        {
            base.Preinitialize(wrappingAction, forInputSource);
            booleanAction = (SteamVRActionBoolean)wrappingAction;
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
                actionData_size = (uint)Marshal.SizeOf(typeof(InputDigitalActionDataT));
            }
        }

        /// <summary>
        /// Remove all listeners, useful for Dispose pattern
        /// </summary>
        public void RemoveAllListeners()
        {
            Delegate[] delegates;

            if (onStateDown != null)
            {
                delegates = onStateDown.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onStateDown -= (SteamVRActionBoolean.StateDownHandler)existingDelegate;
                    }
                }
            }

            if (onStateUp != null)
            {
                delegates = onStateUp.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onStateUp -= (SteamVRActionBoolean.StateUpHandler)existingDelegate;
                    }
                }
            }

            if (onState != null)
            {
                delegates = onState.GetInvocationList();
                if (delegates != null)
                {
                    foreach (Delegate existingDelegate in delegates)
                    {
                        onState -= (SteamVRActionBoolean.StateHandler)existingDelegate;
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
                        onChange -= (SteamVRActionBoolean.ChangeHandler)existingDelegate;
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
                        onUpdate -= (SteamVRActionBoolean.UpdateHandler)existingDelegate;
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
                        onActiveChange -= (SteamVRActionBoolean.ActiveChangeHandler)existingDelegate;
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

            EVRInputError err = OpenVR.Input.GetDigitalActionData(action.handle, ref actionData, actionData_size, inputSourceHandle);
            if (err != EVRInputError.None)
            {
                MelonLoader.MelonLogger.Error("[HPVR] GetDigitalActionData error (" + action.fullPath + "): " + err.ToString() + " handle: " + action.handle.ToString());
            }

            if (changed)
            {
                changedTime = Time.realtimeSinceStartup + actionData.fUpdateTime;
            }

            updateTime = Time.realtimeSinceStartup;

            if (active)
            {
                if (onStateDown != null && stateDown)
                {
                    onStateDown.Invoke(booleanAction, inputSource);
                }

                if (onStateUp != null && stateUp)
                {
                    onStateUp.Invoke(booleanAction, inputSource);
                }

                if (onState != null && state)
                {
                    onState.Invoke(booleanAction, inputSource);
                }

                if (onChange != null && changed)
                {
                    onChange.Invoke(booleanAction, inputSource, state);
                }

                onUpdate?.Invoke(booleanAction, inputSource, state);
            }

            if (onActiveBindingChange != null && lastActiveBinding != activeBinding)
            {
                onActiveBindingChange.Invoke(booleanAction, inputSource, activeBinding);
            }

            if (onActiveChange != null && lastActive != active)
            {
                onActiveChange.Invoke(booleanAction, inputSource, activeBinding);
            }
        }
    }

    public interface ISteamVRActionBoolean : ISteamVRActionInSource
    {
        /// <summary>The current value of the boolean action. Note: Will only return true if the action is also active.</summary>
        bool state { get; }

        /// <summary>True when the action's state changes from false to true. Note: Will only return true if the action is also active.</summary>
        bool stateDown { get; }

        /// <summary>True when the action's state changes from true to false. Note: Will only return true if the action is also active.</summary>
        bool stateUp { get; }

        /// <summary>The value of the action's 'state' during the previous update</summary>
        /// <remarks>Always returns the previous update state</remarks>
        bool lastState { get; }

        /// <summary>The value of the action's 'stateDown' during the previous update</summary>
        /// <remarks>Always returns the previous update state</remarks>
        bool lastStateDown { get; }

        /// <summary>The value of the action's 'stateUp' during the previous update</summary>
        /// <remarks>Always returns the previous update state</remarks>
        bool lastStateUp { get; }
    }
}