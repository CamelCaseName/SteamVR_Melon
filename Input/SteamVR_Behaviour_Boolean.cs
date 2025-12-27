using System;
//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// This component simplifies using boolean actions.
    /// <para>Provides editor accessible events: OnPress, OnPressDown, OnPressUp, OnChange, and OnUpdate.</para>
    /// <para>Provides script accessible events: OnPressEvent, OnPressDownEvent, OnPressUpEvent, OnChangeEvent, and OnUpdateEvent.</para>
    /// </summary>
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRBehaviourBoolean : MonoBehaviour
    {
        public SteamVRBehaviourBoolean(IntPtr value) : base(value) { }
        /// <summary>The SteamVR boolean action that this component should use</summary>
        public SteamVRActionBoolean booleanAction;

        /// <summary>The device this action should apply to. Any if the action is not device specific.</summary>
        public SteamVRInputSources inputSource;

        /// <summary>This UnityEvent fires whenever a change happens in the action</summary>
        public SteamVRBehaviourBooleanEvent onChange;

        /// <summary>This C# event fires whenever a change happens in the action</summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event ChangeHandler onChangeEvent;

        /// <summary>This UnityEvent fires whenever the action is updated</summary>
        public SteamVRBehaviourBooleanEvent onUpdate;

        /// <summary>This C# event fires whenever the action is updated</summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event UpdateHandler onUpdateEvent;

        /// <summary>This UnityEvent will fire whenever the boolean action is true and gets updated</summary>
        public SteamVRBehaviourBooleanEvent onPress;

        /// <summary>This C# event will fire whenever the boolean action is true and gets updated</summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event StateHandler onPressEvent;

        /// <summary>This UnityEvent will fire whenever the boolean action has changed from false to true in the last update</summary>
        public SteamVRBehaviourBooleanEvent onPressDown;

        /// <summary>This C# event will fire whenever the boolean action has changed from false to true in the last update</summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event StateDownHandler onPressDownEvent;

        /// <summary>This UnityEvent will fire whenever the boolean action has changed from true to false in the last update</summary>
        public SteamVRBehaviourBooleanEvent onPressUp;

        /// <summary>This C# event will fire whenever the boolean action has changed from true to false in the last update</summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event StateUpHandler onPressUpEvent;

        /// <summary>Returns true if this action is currently bound and its action set is active</summary>
        public bool isActive { get { return booleanAction[inputSource].active; } }

        /// <summary>Returns the action set that this action is in.</summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public SteamVRActionSet actionSet { get { if (booleanAction != null) { return booleanAction.actionSet; } else { return null; } } }

        protected virtual void OnEnable()
        {
            if (booleanAction == null)
            {
                MelonLoader.MelonLogger.Error("[HPVR] Boolean action not set.", this);
                return;
            }

            AddHandlers();
        }

        protected virtual void OnDisable()
        {
            RemoveHandlers();
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        protected void AddHandlers()
        {
            booleanAction[inputSource].onUpdate += SteamVR_Behaviour_Boolean_OnUpdate;
            booleanAction[inputSource].onChange += SteamVR_Behaviour_Boolean_OnChange;
            booleanAction[inputSource].onState += SteamVR_Behaviour_Boolean_OnState;
            booleanAction[inputSource].onStateDown += SteamVR_Behaviour_Boolean_OnStateDown;
            booleanAction[inputSource].onStateUp += SteamVR_Behaviour_Boolean_OnStateUp;
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        protected void RemoveHandlers()
        {

            if (booleanAction != null)
            {
                booleanAction[inputSource].onUpdate -= SteamVR_Behaviour_Boolean_OnUpdate;
                booleanAction[inputSource].onChange -= SteamVR_Behaviour_Boolean_OnChange;
                booleanAction[inputSource].onState -= SteamVR_Behaviour_Boolean_OnState;
                booleanAction[inputSource].onStateDown -= SteamVR_Behaviour_Boolean_OnStateDown;
                booleanAction[inputSource].onStateUp -= SteamVR_Behaviour_Boolean_OnStateUp;
            }
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Boolean_OnStateUp(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource)
        {
            onPressUp?.Send(this, fromSource, false);

            onPressUpEvent?.Invoke(this, fromSource);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Boolean_OnStateDown(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource)
        {
            onPressDown?.Send(this, fromSource, true);

            onPressDownEvent?.Invoke(this, fromSource);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Boolean_OnState(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource)
        {
            onPress?.Send(this, fromSource, true);

            onPressEvent?.Invoke(this, fromSource);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Boolean_OnUpdate(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource, bool newState)
        {
            onUpdate?.Send(this, fromSource, newState);

            onUpdateEvent?.Invoke(this, fromSource, newState);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Boolean_OnChange(SteamVRActionBoolean fromAction, SteamVRInputSources fromSource, bool newState)
        {
            onChange?.Send(this, fromSource, newState);

            onChangeEvent?.Invoke(this, fromSource, newState);
        }

        /// <summary>
        /// Gets the localized name of the device that the action corresponds to.
        /// </summary>
        /// <param name="localizedParts">
        /// <list type="bullet">
        /// <item><description>VRInputString_Hand - Which hand the origin is in. E.g. "Left Hand"</description></item>
        /// <item><description>VRInputString_ControllerType - What kind of controller the user has in that hand.E.g. "Vive Controller"</description></item>
        /// <item><description>VRInputString_InputSource - What part of that controller is the origin. E.g. "Trackpad"</description></item>
        /// <item><description>VRInputString_All - All of the above. E.g. "Left Hand Vive Controller Trackpad"</description></item>
        /// </list>
        /// </param>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public string GetLocalizedName(params EVRInputStringBits[] localizedParts)
        {
            if (booleanAction != null)
            {
                return booleanAction.GetLocalizedOriginPart(inputSource, localizedParts);
            }

            return null;
        }

        public delegate void StateDownHandler(SteamVRBehaviourBoolean fromAction, SteamVRInputSources fromSource);
        public delegate void StateUpHandler(SteamVRBehaviourBoolean fromAction, SteamVRInputSources fromSource);
        public delegate void StateHandler(SteamVRBehaviourBoolean fromAction, SteamVRInputSources fromSource);
        public delegate void ActiveChangeHandler(SteamVRBehaviourBoolean fromAction, SteamVRInputSources fromSource, bool active);
        public delegate void ChangeHandler(SteamVRBehaviourBoolean fromAction, SteamVRInputSources fromSource, bool newState);
        public delegate void UpdateHandler(SteamVRBehaviourBoolean fromAction, SteamVRInputSources fromSource, bool newState);
    }
}