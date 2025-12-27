//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using Il2CppInterop.Runtime.Attributes;
using System;
using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// This component simplifies the use of Pose actions. Adding it to a gameobject will auto set that transform's position and rotation every update to match the pose.
    /// Advanced velocity estimation is handled through a buffer of the last 30 updates.
    /// </summary>
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRBehaviourPose : MonoBehaviour
    {
        public SteamVRBehaviourPose(IntPtr value) : base(value) { }
        public SteamVRActionPose poseAction = SteamVRInput.GetAction<SteamVRActionPose>("Pose");

        /// <summary>The device this action should apply to. Any if the action is not device specific.</summary>
        public SteamVRInputSources inputSource;

        /// <summary>If not set, relative to parent</summary>
        public Transform origin;
        public new Transform transform;

        /// <summary>Returns whether or not the current pose is in a valid state</summary>
        public bool isValid { get { return poseAction[inputSource].poseIsValid; } }

        /// <summary>Returns whether or not the pose action is bound and able to be updated</summary>
        public bool isActive { get { return poseAction[inputSource].active; } }

        /// <summary>This Unity event will fire whenever the position or rotation of this transform is updated.</summary>
        public SteamVRBehaviourPoseEvent onTransformUpdated;

        /// <summary>This Unity event will fire whenever the position or rotation of this transform is changed.</summary>
        public SteamVRBehaviourPoseEvent onTransformChanged;

        /// <summary>This Unity event will fire whenever the device is connected or disconnected</summary>
        public SteamVRBehaviourPoseConnectedChangedEvent onConnectedChanged;

        /// <summary>This Unity event will fire whenever the device's tracking state changes</summary>
        public SteamVRBehaviourPoseTrackingChangedEvent onTrackingChanged;

        /// <summary>This Unity event will fire whenever the device's deviceIndex changes</summary>
        public SteamVRBehaviourPoseDeviceIndexChangedEvent onDeviceIndexChanged;

        /// <summary>This C# event will fire whenever the position or rotation of this transform is updated.</summary>
        public UpdateHandler onTransformUpdatedEvent;

        /// <summary>This C# event will fire whenever the position or rotation of this transform is changed.</summary>
        public ChangeHandler onTransformChangedEvent;

        /// <summary>This C# event will fire whenever the device is connected or disconnected</summary>
        public DeviceConnectedChangeHandler onConnectedChangedEvent;

        /// <summary>This C# event will fire whenever the device's tracking state changes</summary>
        public TrackingChangeHandler onTrackingChangedEvent;

        /// <summary>This C# event will fire whenever the device's deviceIndex changes</summary>
        public DeviceIndexChangedHandler onDeviceIndexChangedEvent;

        /// <summary>Can be disabled to stop broadcasting bound device status changes</summary>
        public bool broadcastDeviceChanges = true;

        [HideFromIl2Cpp]
        public event InputSourceHandler OnInputSource = new((SteamVRInputSources s) => { });
        [HideFromIl2Cpp]
        public event DeviceIndexHandler OnDeviceIndex = new((int i) => { });

        protected int deviceIndex = -1;

        protected SteamVRHistoryBuffer historyBuffer = new(30);

        public virtual void Init()
        {
            if (poseAction == null)
            {
                MelonLoader.MelonLogger.Error("[HPVR] No pose action set for this component", this);
                return;
            }

            onTrackingChanged = new();
            onTransformChanged = new();
            onTransformUpdated = new();
            onDeviceIndexChanged = new();
            onConnectedChanged = new();

            CheckDeviceIndex();

            if (origin == null)
            {
                origin = this.transform.parent;
            }
        }

        public virtual void FinishInit()
        {
            SteamVR.Initialize();

            if (poseAction != null)
            {
                poseAction[inputSource].onUpdate += SteamVR_Behaviour_Pose_OnUpdate;
                poseAction[inputSource].onDeviceConnectedChanged += OnDeviceConnectedChanged;
                poseAction[inputSource].onTrackingChanged += OnTrackingChanged;
                poseAction[inputSource].onChange += SteamVR_Behaviour_Pose_OnChange;
            }
        }

        protected virtual void OnDisable()
        {
            if (poseAction != null)
            {
                poseAction[inputSource].onUpdate -= SteamVR_Behaviour_Pose_OnUpdate;
                poseAction[inputSource].onDeviceConnectedChanged -= OnDeviceConnectedChanged;
                poseAction[inputSource].onTrackingChanged -= OnTrackingChanged;
                poseAction[inputSource].onChange -= SteamVR_Behaviour_Pose_OnChange;
            }

            historyBuffer.Clear();
        }

        [HideFromIl2Cpp]
        private void SteamVR_Behaviour_Pose_OnUpdate(SteamVRActionPose fromAction, SteamVRInputSources fromSource)
        {
            UpdateHistoryBuffer();

            UpdateTransform();

            onTransformUpdated?.Send(this, inputSource);
            onTransformUpdatedEvent?.Invoke(this, inputSource);
        }
        protected virtual void UpdateTransform()
        {
            CheckDeviceIndex();

            if (transform is not null)
            {
                if (origin == null)
                {
                    origin = this.transform.parent;
                }
                if (origin != null && origin.transform is not null)
                {
                    transform.position = origin.transform.TransformPoint(poseAction[inputSource].localPosition);
                    transform.rotation = origin.rotation * poseAction[inputSource].localRotation;
                }
                else
                {
                    transform.localPosition = poseAction[inputSource].localPosition;
                    transform.localRotation = poseAction[inputSource].localRotation;
                }
            }
        }

        [HideFromIl2Cpp]
        private void SteamVR_Behaviour_Pose_OnChange(SteamVRActionPose fromAction, SteamVRInputSources fromSource)
        {
            onTransformChanged?.Send(this, fromSource);
            onTransformChangedEvent?.Invoke(this, fromSource);
        }

        [HideFromIl2Cpp]
        protected virtual void OnDeviceConnectedChanged(SteamVRActionPose changedAction, SteamVRInputSources changedSource, bool connected)
        {
            CheckDeviceIndex();

            onConnectedChanged?.Send(this, inputSource, connected);
            onConnectedChangedEvent?.Invoke(this, inputSource, connected);
        }

        [HideFromIl2Cpp]
        protected virtual void OnTrackingChanged(SteamVRActionPose changedAction, SteamVRInputSources changedSource, ETrackingResult trackingChanged)
        {
            onTrackingChanged?.Send(this, inputSource, trackingChanged);
            onTrackingChangedEvent?.Invoke(this, inputSource, trackingChanged);
        }

        protected virtual void CheckDeviceIndex()
        {
            if (poseAction[inputSource].active && poseAction[inputSource].deviceIsConnected)
            {
                int currentDeviceIndex = (int)poseAction[inputSource].trackedDeviceIndex;

                if (deviceIndex != currentDeviceIndex)
                {
                    deviceIndex = currentDeviceIndex;

                    if (broadcastDeviceChanges)
                    {
                        OnInputSource.Invoke(inputSource);
                        OnDeviceIndex.Invoke(deviceIndex);
                    }

                    onDeviceIndexChanged?.Send(this, inputSource, deviceIndex);
                    onDeviceIndexChangedEvent?.Invoke(this, inputSource, deviceIndex);
                }
            }
        }

        /// <summary>
        /// Returns the device index for the device bound to the pose.
        /// </summary>
        public int GetDeviceIndex()
        {
            if (deviceIndex == -1)
            {
                CheckDeviceIndex();
            }

            return deviceIndex;
        }

        /// <summary>Returns the current velocity of the pose (as of the last update)</summary>
        public Vector3 GetVelocity()
        {
            return poseAction[inputSource].velocity;
        }

        /// <summary>Returns the current angular velocity of the pose (as of the last update)</summary>
        public Vector3 GetAngularVelocity()
        {
            return poseAction[inputSource].angularVelocity;
        }

        /// <summary>Returns the velocities of the pose at the time specified. Can predict in the future or return past values.</summary>
        public bool GetVelocitiesAtTimeOffset(float secondsFromNow, out Vector3 velocity, out Vector3 angularVelocity)
        {
            return poseAction[inputSource].GetVelocitiesAtTimeOffset(secondsFromNow, out velocity, out angularVelocity);
        }

        /// <summary>Uses previously recorded values to find the peak speed of the pose and returns the corresponding velocity and angular velocity</summary>
        public void GetEstimatedPeakVelocities(out Vector3 velocity, out Vector3 angularVelocity)
        {
            int top = historyBuffer.GetTopVelocity(10, 1);

            historyBuffer.GetAverageVelocities(out velocity, out angularVelocity, 2, top);
        }

        protected int lastFrameUpdated;
        protected void UpdateHistoryBuffer()
        {
            int currentFrame = Time.frameCount;
            if (lastFrameUpdated != currentFrame)
            {
                historyBuffer.Update(poseAction[inputSource].localPosition, poseAction[inputSource].localRotation, poseAction[inputSource].velocity, poseAction[inputSource].angularVelocity);
                lastFrameUpdated = currentFrame;
            }
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
        [HideFromIl2Cpp]
        public string GetLocalizedName(params EVRInputStringBits[] localizedParts)
        {
            if (poseAction != null)
            {
                return poseAction.GetLocalizedOriginPart(inputSource, localizedParts);
            }

            return null;
        }

        public delegate void ActiveChangeHandler(SteamVRBehaviourPose fromAction, SteamVRInputSources fromSource, bool active);
        public delegate void ChangeHandler(SteamVRBehaviourPose fromAction, SteamVRInputSources fromSource);
        public delegate void UpdateHandler(SteamVRBehaviourPose fromAction, SteamVRInputSources fromSource);
        public delegate void InputSourceHandler(SteamVRInputSources s);
        public delegate void DeviceIndexHandler(int i);
        public delegate void TrackingChangeHandler(SteamVRBehaviourPose fromAction, SteamVRInputSources fromSource, ETrackingResult trackingState);
        public delegate void ValidPoseChangeHandler(SteamVRBehaviourPose fromAction, SteamVRInputSources fromSource, bool validPose);
        public delegate void DeviceConnectedChangeHandler(SteamVRBehaviourPose fromAction, SteamVRInputSources fromSource, bool deviceConnected);
        public delegate void DeviceIndexChangedHandler(SteamVRBehaviourPose fromAction, SteamVRInputSources fromSource, int newDeviceIndex);
    }
}