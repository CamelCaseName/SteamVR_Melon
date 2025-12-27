//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using UnityEngine;

namespace Valve.VR
{
    /// <summary>
    /// The major difference between this component and the standard SteamVR_Behaviour_Skeleton is this one lets you
    /// only use the joints you care about. You can set the transforms you're concerned with and ignore the ones you're not.
    /// </summary>
    public class SteamVRBehaviourSkeletonCustom : SteamVRBehaviourSkeleton
    {
        public SteamVRBehaviourSkeletonCustom(IntPtr value) : base(value) { }
        protected Transform _wrist;

        protected Transform _thumbMetacarpal;

        protected Transform _thumbProximal;

        protected Transform _thumbMiddle;

        protected Transform _thumbDistal;

        protected Transform _thumbTip;

        protected Transform _thumbAux;

        protected Transform _indexMetacarpal;

        protected Transform _indexProximal;

        protected Transform _indexMiddle;

        protected Transform _indexDistal;

        protected Transform _indexTip;

        protected Transform _indexAux;

        protected Transform _middleMetacarpal;

        protected Transform _middleProximal;

        protected Transform _middleMiddle;

        protected Transform _middleDistal;

        protected Transform _middleTip;

        protected Transform _middleAux;

        protected Transform _ringMetacarpal;

        protected Transform _ringProximal;

        protected Transform _ringMiddle;

        protected Transform _ringDistal;

        protected Transform _ringTip;

        protected Transform _ringAux;

        protected Transform _pinkyMetacarpal;

        protected Transform _pinkyProximal;

        protected Transform _pinkyMiddle;

        protected Transform _pinkyDistal;

        protected Transform _pinkyTip;

        protected Transform _pinkyAux;

        protected override void AssignBonesArray()
        {
            bones[SteamVRSkeletonJointIndexes.wrist] = _wrist;
            bones[SteamVRSkeletonJointIndexes.thumbProximal] = _thumbProximal;
            bones[SteamVRSkeletonJointIndexes.thumbMiddle] = _thumbMiddle;
            bones[SteamVRSkeletonJointIndexes.thumbDistal] = _thumbDistal;
            bones[SteamVRSkeletonJointIndexes.thumbTip] = _thumbTip;
            bones[SteamVRSkeletonJointIndexes.thumbAux] = _thumbAux;
            bones[SteamVRSkeletonJointIndexes.indexProximal] = _indexProximal;
            bones[SteamVRSkeletonJointIndexes.indexMiddle] = _indexMiddle;
            bones[SteamVRSkeletonJointIndexes.indexDistal] = _indexDistal;
            bones[SteamVRSkeletonJointIndexes.indexTip] = _indexTip;
            bones[SteamVRSkeletonJointIndexes.indexAux] = _indexAux;
            bones[SteamVRSkeletonJointIndexes.middleProximal] = _middleProximal;
            bones[SteamVRSkeletonJointIndexes.middleMiddle] = _middleMiddle;
            bones[SteamVRSkeletonJointIndexes.middleDistal] = _middleDistal;
            bones[SteamVRSkeletonJointIndexes.middleTip] = _middleTip;
            bones[SteamVRSkeletonJointIndexes.middleAux] = _middleAux;
            bones[SteamVRSkeletonJointIndexes.ringProximal] = _ringProximal;
            bones[SteamVRSkeletonJointIndexes.ringMiddle] = _ringMiddle;
            bones[SteamVRSkeletonJointIndexes.ringDistal] = _ringDistal;
            bones[SteamVRSkeletonJointIndexes.ringTip] = _ringTip;
            bones[SteamVRSkeletonJointIndexes.ringAux] = _ringAux;
            bones[SteamVRSkeletonJointIndexes.pinkyProximal] = _pinkyProximal;
            bones[SteamVRSkeletonJointIndexes.pinkyMiddle] = _pinkyMiddle;
            bones[SteamVRSkeletonJointIndexes.pinkyDistal] = _pinkyDistal;
            bones[SteamVRSkeletonJointIndexes.pinkyTip] = _pinkyTip;
            bones[SteamVRSkeletonJointIndexes.pinkyAux] = _pinkyAux;
        }
    }
}