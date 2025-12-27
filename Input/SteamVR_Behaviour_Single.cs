//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;

using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// SteamVR_Behaviour_Single simplifies the use of single actions. It gives an event to subscribe to for when the action has changed.
    /// </summary>
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRBehaviourSingle : MonoBehaviour
    {
        public SteamVRBehaviourSingle(IntPtr value) : base(value) { }
        /// <summary>The single action to get data from.</summary>
        public SteamVRActionSingle singleAction;

        /// <summary>The device this action applies to. Any if the action is not device specific.</summary>
        /// <summary>The device this action should apply to. Any if the action is not device specific.</summary>
        public SteamVRInputSources inputSource;

        /// <summary>Unity event that Fires whenever the action's value has changed since the last update.</summary>
        /// <summary>Fires whenever the action's value has changed since the last update.</summary>
        public SteamVRBehaviourSingleEvent onChange;

        /// <summary>Unity event that Fires whenever the action's value has been updated</summary>
        /// <summary>Fires whenever the action's value has been updated.</summary>
        public SteamVRBehaviourSingleEvent onUpdate;

        /// <summary>Unity event that Fires whenever the action's value has been updated and is non-zero</summary>
        /// <summary>Fires whenever the action's value has been updated and is non-zero.</summary>
        public SteamVRBehaviourSingleEvent onAxis;

        /// <summary>C# event that fires whenever the action's value has changed since the last update.</summary>
        public ChangeHandler onChangeEvent;

        /// <summary>C# event that fires whenever the action's value has been updated</summary>
        public UpdateHandler onUpdateEvent;

        /// <summary>C# event that fires whenever the action's value has been updated and is non-zero</summary>
        public AxisHandler onAxisEvent;

        /// <summary>Returns whether this action is bound and the action set is active</summary>
        public bool isActive { get { return singleAction.GetActive(inputSource); } }

        protected virtual void OnEnable()
        {
            if (singleAction == null)
            {
                MelonLoader.MelonLogger.Error("[HPVR] Single action not set.", this);
                return;
            }

            AddHandlers();
        }

        protected virtual void OnDisable()
        {
            RemoveHandlers();
        }

        protected void AddHandlers()
        {
            singleAction[inputSource].onUpdate += SteamVR_Behaviour_Single_OnUpdate;
            singleAction[inputSource].onChange += SteamVR_Behaviour_Single_OnChange;
            singleAction[inputSource].onAxis += SteamVR_Behaviour_Single_OnAxis;
        }

        protected void RemoveHandlers()
        {
            if (singleAction != null)
            {
                singleAction[inputSource].onUpdate -= SteamVR_Behaviour_Single_OnUpdate;
                singleAction[inputSource].onChange -= SteamVR_Behaviour_Single_OnChange;
                singleAction[inputSource].onAxis -= SteamVR_Behaviour_Single_OnAxis;
            }
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Single_OnUpdate(SteamVRActionSingle fromAction, SteamVRInputSources fromSource, float newAxis, float newDelta)
        {
            onUpdate?.Send(this, fromSource, newAxis, newDelta);

            onUpdateEvent?.Invoke(this, fromSource, newAxis, newDelta);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Single_OnChange(SteamVRActionSingle fromAction, SteamVRInputSources fromSource, float newAxis, float newDelta)
        {
            onChange?.Send(this, fromSource, newAxis, newDelta);

            onChangeEvent?.Invoke(this, fromSource, newAxis, newDelta);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void SteamVR_Behaviour_Single_OnAxis(SteamVRActionSingle fromAction, SteamVRInputSources fromSource, float newAxis, float newDelta)
        {
            onAxis?.Send(this, fromSource, newAxis, newDelta);

            onAxisEvent?.Invoke(this, fromSource, newAxis, newDelta);
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
            if (singleAction != null)
            {
                return singleAction.GetLocalizedOriginPart(inputSource, localizedParts);
            }

            return null;
        }

        public delegate void AxisHandler(SteamVRBehaviourSingle fromAction, SteamVRInputSources fromSource, float newAxis, float newDelta);
        public delegate void ChangeHandler(SteamVRBehaviourSingle fromAction, SteamVRInputSources fromSource, float newAxis, float newDelta);
        public delegate void UpdateHandler(SteamVRBehaviourSingle fromAction, SteamVRInputSources fromSource, float newAxis, float newDelta);
    }
}