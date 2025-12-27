//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRSkeletonPose : MonoBehaviour
    {
        public SteamVRSkeletonPose(IntPtr p) : base(p) { }
        public SteamVRSkeletonPose() : base(ClassInjector.DerivedConstructorPointer<SteamVRSkeletonPose>()) => ClassInjector.DerivedConstructorBody(this);

        public SteamVRSkeletonPoseHand leftHand = new(SteamVRInputSources.LeftHand);
        public SteamVRSkeletonPoseHand rightHand = new(SteamVRInputSources.RightHand);

        protected const int leftHandInputSource = (int)SteamVRInputSources.LeftHand;
        protected const int rightHandInputSource = (int)SteamVRInputSources.RightHand;

        public bool applyToSkeletonRoot = true;

        [HideFromIl2Cpp]
        public SteamVRSkeletonPoseHand GetHand(int hand)
        {
            if (hand == leftHandInputSource)
            {
                return leftHand;
            }
            else if (hand == rightHandInputSource)
            {
                return rightHand;
            }

            return null;
        }

        [HideFromIl2Cpp]
        public SteamVRSkeletonPoseHand GetHand(SteamVRInputSources hand)
        {
            if (hand == SteamVRInputSources.LeftHand)
            {
                return leftHand;
            }
            else if (hand == SteamVRInputSources.RightHand)
            {
                return rightHand;
            }

            return null;
        }
    }

    [Serializable]
    public class SteamVRSkeletonPoseHand
    {
        public SteamVRSkeletonPoseHand() { }

        public SteamVRInputSources inputSource;

        public SteamVRSkeletonFingerExtensionTypes thumbFingerMovementType = SteamVRSkeletonFingerExtensionTypes.Static;
        public SteamVRSkeletonFingerExtensionTypes indexFingerMovementType = SteamVRSkeletonFingerExtensionTypes.Static;
        public SteamVRSkeletonFingerExtensionTypes middleFingerMovementType = SteamVRSkeletonFingerExtensionTypes.Static;
        public SteamVRSkeletonFingerExtensionTypes ringFingerMovementType = SteamVRSkeletonFingerExtensionTypes.Static;
        public SteamVRSkeletonFingerExtensionTypes pinkyFingerMovementType = SteamVRSkeletonFingerExtensionTypes.Static;

        /// <summary>
        /// Get extension type for a particular finger. Thumb is 0, Index is 1, etc.
        /// </summary>
        public SteamVRSkeletonFingerExtensionTypes GetFingerExtensionType(int finger)
        {
            if (finger == 0)
            {
                return thumbFingerMovementType;
            }

            if (finger == 1)
            {
                return indexFingerMovementType;
            }

            if (finger == 2)
            {
                return middleFingerMovementType;
            }

            if (finger == 3)
            {
                return ringFingerMovementType;
            }

            if (finger == 4)
            {
                return pinkyFingerMovementType;
            }

            //default to static
            MelonLoader.MelonLogger.Warning("Finger not in range!");
            return SteamVRSkeletonFingerExtensionTypes.Static;
        }

        public bool ignoreRootPoseData = true;
        public bool ignoreWristPoseData = true;

        public Vector3 position;
        public Quaternion rotation;

        public List<Vector3> bonePositions;
        public List<Quaternion> boneRotations;

        public SteamVRSkeletonPoseHand(SteamVRInputSources source)
        {
            inputSource = source;
        }

        public SteamVRSkeletonFingerExtensionTypes GetMovementTypeForBone(int boneIndex)
        {
            int fingerIndex = SteamVRSkeletonJointIndexes.GetFingerForBone(boneIndex);

            switch (fingerIndex)
            {
                case SteamVRSkeletonFingerIndexes.thumb:
                    return thumbFingerMovementType;

                case SteamVRSkeletonFingerIndexes.index:
                    return indexFingerMovementType;

                case SteamVRSkeletonFingerIndexes.middle:
                    return middleFingerMovementType;

                case SteamVRSkeletonFingerIndexes.ring:
                    return ringFingerMovementType;

                case SteamVRSkeletonFingerIndexes.pinky:
                    return pinkyFingerMovementType;
            }

            return SteamVRSkeletonFingerExtensionTypes.Static;
        }
    }

    public enum SteamVRSkeletonFingerExtensionTypes
    {
        Static,
        Free,
        Extend,
        Contract,
    }

    public class SteamVRSkeletonFingerExtensionTypeLists
    {
        private SteamVRSkeletonFingerExtensionTypes[] _enumList;
        public SteamVRSkeletonFingerExtensionTypes[] enumList
        {
            get
            {
                if (_enumList == null)
                {
                    _enumList = (SteamVRSkeletonFingerExtensionTypes[])System.Enum.GetValues(typeof(SteamVRSkeletonFingerExtensionTypes));
                }

                return _enumList;
            }
        }

        private string[] _stringList;
        public string[] stringList
        {
            get
            {
                if (_stringList == null)
                {
                    _stringList = enumList.Select(element => element.ToString()).ToArray();
                }

                return _stringList;
            }
        }
    }
}