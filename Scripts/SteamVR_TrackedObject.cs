//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: For controlling in-game objects with tracked devices.
//
//=============================================================================

using MelonLoader;
using SteamVR_Melon.Scripts;
using System;
using UnityEngine;

namespace Valve.VR
{
    [RegisterTypeInIl2Cpp()]
    public class SteamVRTrackedObject : MonoBehaviour
    {
        public SteamVRTrackedObject(IntPtr value) : base(value) { }

        public enum EIndex
        {
            None = -1,
            Hmd = (int)OpenVR.kUnTrackedDeviceIndexHmd,
            Device1,
            Device2,
            Device3,
            Device4,
            Device5,
            Device6,
            Device7,
            Device8,
            Device9,
            Device10,
            Device11,
            Device12,
            Device13,
            Device14,
            Device15,
            Device16
        }

        public EIndex index;

        public Transform origin;

        public bool isValid { get; private set; }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void OnNewPoses(TrackedDevicePoseT[] poses)
        {
            if (index == EIndex.None)
            {
                return;
            }

            var i = (int)index;

            isValid = false;
            if (poses.Length <= i)
            {
                return;
            }

            if (!poses[i].bDeviceIsConnected)
            {
                return;
            }

            if (!poses[i].bPoseIsValid)
            {
                return;
            }

            isValid = true;

            var pose = new SteamVRUtils.RigidTransform(poses[i].mDeviceToAbsoluteTracking);

            if (origin != null)
            {
                transform.position = origin.transform.TransformPoint(pose.pos);
                transform.rotation = origin.rotation * pose.rot;
            }
            else
            {
                transform.localPosition = pose.pos;
                transform.localRotation = pose.rot;
            }
        }

        readonly SteamVREvents.Action newPosesAction;

        SteamVRTrackedObject()
        {
            newPosesAction = SteamVREvents.NewPosesAction(OnNewPoses);
            MelonLogger.Msg("[HPVR] newposes action is null: " + (newPosesAction == null));
            var t = transform.GetComponent<SteamVRBehaviourPose>();
            if (t is not null)
            {
                t.OnDeviceIndex += SetDeviceIndex;
            }
        }

        private void Awake()
        {
            OnEnable();
        }

        void OnEnable()
        {
            var render = SteamVRRender.instance;
            if (render == null)
            {
                enabled = false;
                return;
            }

            if (newPosesAction is not null)
            {
                newPosesAction.enabled = true;
            }
        }

        void OnDisable()
        {
            if (newPosesAction is not null)
            {
                newPosesAction.enabled = false;
            }

            isValid = false;
        }

        public void SetDeviceIndex(int index)
        {
            if (Enum.IsDefined(typeof(EIndex), index))
            {
                this.index = (EIndex)index;
            }
        }
    }
}