//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using Il2CppInterop.Runtime.InteropTypes.Arrays;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRSkeletonPoser : MonoBehaviour
    {
        public SteamVRSkeletonPoser(IntPtr value) : base(value) { }
        #region Editor Storage
        public bool poseEditorExpanded = true;
        public bool blendEditorExpanded = true;
        public string[] poseNames;
        #endregion

        public GameObject overridePreviewLeftHandPrefab;
        public GameObject overridePreviewRightHandPrefab;

        public SteamVRSkeletonPose skeletonMainPose;
        public List<SteamVRSkeletonPose> skeletonAdditionalPoses = new();

        public bool showLeftPreview = false;

        public bool showRightPreview = true; //show the right hand by default

        public GameObject previewLeftInstance;

        public GameObject previewRightInstance;

        public int previewPoseSelection = 0;

        public int blendPoseCount { get { return blendPoses.Length; } }

        public List<PoseBlendingBehaviour> blendingBehaviours = new();

        public SteamVRSkeletonPoseSnapshot blendedSnapshotL;
        public SteamVRSkeletonPoseSnapshot blendedSnapshotR;

        public SkeletonBlendablePose[] blendPoses;

        public int boneCount;

        public bool poseUpdatedThisFrame;

        public float scale;

        public void Initialize()
        {
            if (previewLeftInstance != null)
            {
                DestroyImmediate(previewLeftInstance);
            }

            if (previewRightInstance != null)
            {
                DestroyImmediate(previewRightInstance);
            }

            blendPoses = new SkeletonBlendablePose[skeletonAdditionalPoses.Count + 1];
            if (blendPoseCount > 0)
            {
                for (int i = 0; i < blendPoseCount; i++)
                {
                    blendPoses[i] = new SkeletonBlendablePose(GetPoseByIndex(i));
                    blendPoses[i].PoseToSnapshots();
                }
            }
            boneCount = skeletonMainPose?.leftHand?.bonePositions?.Count ?? 0;
            // NOTE: Is there a better way to get the bone count? idk
            blendedSnapshotL = new SteamVRSkeletonPoseSnapshot(boneCount, SteamVRInputSources.LeftHand);
            blendedSnapshotR = new SteamVRSkeletonPoseSnapshot(boneCount, SteamVRInputSources.RightHand);
        }

        /// <summary>
        /// Set the blending value of a blendingBehaviour. Works best on Manual type behaviours.
        /// </summary>
        public void SetBlendingBehaviourValue(string behaviourName, float value)
        {
            PoseBlendingBehaviour behaviour = FindBlendingBehaviour(behaviourName);
            if (behaviour != null)
            {
                behaviour.value = value;

                if (behaviour.type != PoseBlendingBehaviour.BlenderTypes.Manual)
                {
                    MelonLoader.MelonLogger.Warning("[HPVR] Blending Behaviour: " + behaviourName + " is not a manual behaviour. Its value will likely be overriden.", this);
                }
            }
        }
        /// <summary>
        /// Get the blending value of a blendingBehaviour.
        /// </summary>
        public float GetBlendingBehaviourValue(string behaviourName)
        {
            PoseBlendingBehaviour behaviour = FindBlendingBehaviour(behaviourName);
            if (behaviour != null)
            {
                return behaviour.value;
            }
            return 0;
        }

        /// <summary>
        /// Enable or disable a blending behaviour.
        /// </summary>
        public void SetBlendingBehaviourEnabled(string behaviourName, bool value)
        {
            PoseBlendingBehaviour behaviour = FindBlendingBehaviour(behaviourName);
            if (behaviour != null)
            {
                behaviour.enabled = value;
            }
        }
        /// <summary>
        /// Check if a blending behaviour is enabled.
        /// </summary>
        /// <param name="behaviourName"></param>
        /// <returns></returns>
        public bool GetBlendingBehaviourEnabled(string behaviourName)
        {
            PoseBlendingBehaviour behaviour = FindBlendingBehaviour(behaviourName);
            if (behaviour != null)
            {
                return behaviour.enabled;
            }

            return false;
        }
        /// <summary>
        /// Get a blending behaviour by name.
        /// </summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public PoseBlendingBehaviour GetBlendingBehaviour(string behaviourName)
        {
            return FindBlendingBehaviour(behaviourName);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        protected PoseBlendingBehaviour FindBlendingBehaviour(string behaviourName, bool throwErrors = true)
        {
            PoseBlendingBehaviour behaviour = blendingBehaviours.Find(b => b.name == behaviourName);

            if (behaviour == null)
            {
                if (throwErrors)
                {
                    MelonLoader.MelonLogger.Error("[HPVR] Blending Behaviour: " + behaviourName + " not found on Skeleton Poser: " + gameObject.name, this);
                }

                return null;
            }

            return behaviour;
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public SteamVRSkeletonPose GetPoseByIndex(int index)
        {
            if (index == 0)
            { return skeletonMainPose; }
            else
            { return skeletonAdditionalPoses[index - 1]; }
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private SteamVRSkeletonPoseSnapshot GetHandSnapshot(SteamVRInputSources inputSource)
        {
            if (inputSource == SteamVRInputSources.LeftHand)
            {
                return blendedSnapshotL;
            }
            else
            {
                return blendedSnapshotR;
            }
        }

        /// <summary>
        /// Retrieve the final animated pose, to be applied to a hand skeleton
        /// </summary>
        /// <param name="forAction">The skeleton action you want to blend between</param>
        /// <param name="handType">If this is for the left or right hand</param>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public SteamVRSkeletonPoseSnapshot GetBlendedPose(SteamVRActionSkeleton skeletonAction, SteamVRInputSources handType)
        {
            UpdatePose(skeletonAction, handType);
            return GetHandSnapshot(handType);
        }

        /// <summary>
        /// Retrieve the final animated pose, to be applied to a hand skeleton
        /// </summary>
        /// <param name="skeletonBehaviour">The skeleton behaviour you want to get the action/input source from to blend between</param>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public SteamVRSkeletonPoseSnapshot GetBlendedPose(SteamVRBehaviourSkeleton skeletonBehaviour)
        {
            return GetBlendedPose(skeletonBehaviour.skeletonAction, skeletonBehaviour.inputSource);
        }

        /// <summary>
        /// Updates all pose animation and blending. Can be called from different places without performance concerns, as it will only let itself run once per frame.
        /// </summary>
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public void UpdatePose(SteamVRActionSkeleton skeletonAction, SteamVRInputSources inputSource)
        {
            // only allow this function to run once per frame
            if (poseUpdatedThisFrame)
            {
                return;
            }

            poseUpdatedThisFrame = true;

            if (skeletonAction.activeBinding)
            {
                // always do additive animation on main pose
                blendPoses[0].UpdateAdditiveAnimation(skeletonAction, inputSource);
            }

            //copy from main pose as a base
            SteamVRSkeletonPoseSnapshot snap = GetHandSnapshot(inputSource);
            snap.CopyFrom(blendPoses[0].GetHandSnapshot(inputSource));

            ApplyBlenderBehaviours(skeletonAction, inputSource, snap);

            if (inputSource == SteamVRInputSources.RightHand)
            {
                blendedSnapshotR = snap;
            }

            if (inputSource == SteamVRInputSources.LeftHand)
            {
                blendedSnapshotL = snap;
            }
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        protected void ApplyBlenderBehaviours(SteamVRActionSkeleton skeletonAction, SteamVRInputSources inputSource, SteamVRSkeletonPoseSnapshot snapshot)
        {

            // apply blending for each behaviour
            for (int behaviourIndex = 0; behaviourIndex < blendingBehaviours.Count; behaviourIndex++)
            {
                blendingBehaviours[behaviourIndex].Update(Time.deltaTime, inputSource);
                // if disabled or very low influence, skip for perf
                if (blendingBehaviours[behaviourIndex].enabled && blendingBehaviours[behaviourIndex].influence * blendingBehaviours[behaviourIndex].value > 0.01f)
                {
                    if (blendingBehaviours[behaviourIndex].pose != 0 && skeletonAction.activeBinding)
                    {
                        // update additive animation only as needed
                        blendPoses[blendingBehaviours[behaviourIndex].pose].UpdateAdditiveAnimation(skeletonAction, inputSource);
                    }

                    blendingBehaviours[behaviourIndex].ApplyBlending(snapshot, blendPoses, inputSource);
                }
            }

        }

        protected void LateUpdate()
        {
            // let the pose be updated again the next frame
            poseUpdatedThisFrame = false;
        }

        /// <summary>Weighted average of n vector3s</summary>
        protected Vector3 BlendVectors(Il2CppStructArray<Vector3> vectors, Il2CppStructArray<float> weights)
        {
            Vector3 blendedVector = Vector3.zero;
            for (int i = 0; i < vectors.Length; i++)
            {
                blendedVector += vectors[i] * weights[i];
            }
            return blendedVector;
        }

        /// <summary>Weighted average of n quaternions</summary>
        protected Quaternion BlendQuaternions(Il2CppStructArray<Quaternion> quaternions, Il2CppStructArray<float> weights)
        {
            Quaternion outquat = Quaternion.identity;
            for (int i = 0; i < quaternions.Length; i++)
            {
                outquat *= Quaternion.Slerp(Quaternion.identity, quaternions[i], weights[i]);
            }
            return outquat;
        }

        /// <summary>
        /// A SkeletonBlendablePose holds a reference to a Skeleton_Pose scriptableObject, and also contains some helper functions.
        /// Also handles pose-specific animation like additive finger motion.
        /// </summary>
        public class SkeletonBlendablePose
        {
            public SteamVRSkeletonPose pose;
            public SteamVRSkeletonPoseSnapshot snapshotR;
            public SteamVRSkeletonPoseSnapshot snapshotL;

            /// <summary>
            /// Get the snapshot of this pose with effects such as additive finger animation applied.
            /// </summary>
            public SteamVRSkeletonPoseSnapshot GetHandSnapshot(SteamVRInputSources inputSource)
            {
                if (inputSource == SteamVRInputSources.LeftHand)
                {
                    return snapshotL;
                }
                else
                {
                    return snapshotR;
                }
            }

            public void UpdateAdditiveAnimation(SteamVRActionSkeleton skeletonAction, SteamVRInputSources inputSource)
            {
                if (skeletonAction.GetSkeletalTrackingLevel() == EVRSkeletalTrackingLevel.VRSkeletalTrackingEstimated)
                {
                    //do not apply additive animation on low fidelity controllers, eg. Vive Wands and Touch
                    return;
                }

                SteamVRSkeletonPoseSnapshot snapshot = GetHandSnapshot(inputSource);
                SteamVRSkeletonPoseHand poseHand = pose.GetHand(inputSource);

                for (int boneIndex = 0; boneIndex < snapshotL.bonePositions.Length; boneIndex++)
                {
                    int fingerIndex = SteamVRSkeletonJointIndexes.GetFingerForBone(boneIndex);
                    SteamVRSkeletonFingerExtensionTypes extensionType = poseHand.GetMovementTypeForBone(boneIndex);

                    if (extensionType == SteamVRSkeletonFingerExtensionTypes.Free)
                    {
                        snapshot.bonePositions[boneIndex] = skeletonAction.bonePositions[boneIndex];
                        snapshot.boneRotations[boneIndex] = skeletonAction.boneRotations[boneIndex];
                    }
                    if (extensionType == SteamVRSkeletonFingerExtensionTypes.Extend)
                    {
                        // lerp to open pose by fingercurl
                        snapshot.bonePositions[boneIndex] = Vector3.Lerp(poseHand.bonePositions[boneIndex], skeletonAction.bonePositions[boneIndex], 1 - skeletonAction.fingerCurls[fingerIndex]);
                        snapshot.boneRotations[boneIndex] = Quaternion.Lerp(poseHand.boneRotations[boneIndex], skeletonAction.boneRotations[boneIndex], 1 - skeletonAction.fingerCurls[fingerIndex]);
                    }
                    if (extensionType == SteamVRSkeletonFingerExtensionTypes.Contract)
                    {
                        // lerp to closed pose by fingercurl
                        snapshot.bonePositions[boneIndex] = Vector3.Lerp(poseHand.bonePositions[boneIndex], skeletonAction.bonePositions[boneIndex], skeletonAction.fingerCurls[fingerIndex]);
                        snapshot.boneRotations[boneIndex] = Quaternion.Lerp(poseHand.boneRotations[boneIndex], skeletonAction.boneRotations[boneIndex], skeletonAction.fingerCurls[fingerIndex]);
                    }
                }
            }

            /// <summary>
            /// Init based on an existing Skeleton_Pose
            /// </summary>
            public SkeletonBlendablePose(SteamVRSkeletonPose p)
            {
                pose = p;
                snapshotR = new SteamVRSkeletonPoseSnapshot(p.rightHand.bonePositions.Count, SteamVRInputSources.RightHand);
                snapshotL = new SteamVRSkeletonPoseSnapshot(p.leftHand.bonePositions.Count, SteamVRInputSources.LeftHand);
            }

            /// <summary>
            /// Copy the base pose into the snapshots.
            /// </summary>
            public void PoseToSnapshots()
            {
                snapshotR.position = pose.rightHand.position;
                snapshotR.rotation = pose.rightHand.rotation;
                pose.rightHand.bonePositions.CopyTo(snapshotR.bonePositions, 0);
                pose.rightHand.boneRotations.CopyTo(snapshotR.boneRotations, 0);

                snapshotL.position = pose.leftHand.position;
                snapshotL.rotation = pose.leftHand.rotation;
                pose.leftHand.bonePositions.CopyTo(snapshotL.bonePositions, 0);
                pose.leftHand.boneRotations.CopyTo(snapshotL.boneRotations, 0);
            }

            public SkeletonBlendablePose() { }
        }

        /// <summary>
        /// A filter applied to the base pose. Blends to a secondary pose by a certain weight. Can be masked per-finger
        /// </summary>
        [Serializable]
        public class PoseBlendingBehaviour
        {
            public string name;
            public bool enabled = true;
            public float influence = 1;
            public int pose = 1;
            public float value = 0;
            public SteamVRActionSingle action_single;
            public SteamVRActionBoolean action_bool;
            public float smoothingSpeed = 0;
            public BlenderTypes type;
            public bool useMask;
            public SteamVRSkeletonHandMask mask = new();

            public bool previewEnabled;

            /// <summary>
            /// Performs smoothing based on deltaTime parameter.
            /// </summary>
            public void Update(float deltaTime, SteamVRInputSources inputSource)
            {
                if (type == BlenderTypes.AnalogAction)
                {
                    if (smoothingSpeed == 0)
                    {
                        value = action_single.GetAxis(inputSource);
                    }
                    else
                    {
                        value = Mathf.Lerp(value, action_single.GetAxis(inputSource), deltaTime * smoothingSpeed);
                    }
                }
                if (type == BlenderTypes.BooleanAction)
                {
                    if (smoothingSpeed == 0)
                    {
                        value = action_bool.GetState(inputSource) ? 1 : 0;
                    }
                    else
                    {
                        value = Mathf.Lerp(value, action_bool.GetState(inputSource) ? 1 : 0, deltaTime * smoothingSpeed);
                    }
                }
            }

            /// <summary>
            /// Apply blending to this behaviour's pose to an existing snapshot.
            /// </summary>
            /// <param name="snapshot">Snapshot to modify</param>
            /// <param name="blendPoses">List of blend poses to get the target pose</param>
            /// <param name="inputSource">Which hand to receive input from</param>
            public void ApplyBlending(SteamVRSkeletonPoseSnapshot snapshot, SkeletonBlendablePose[] blendPoses, SteamVRInputSources inputSource)
            {
                SteamVRSkeletonPoseSnapshot targetSnapshot = blendPoses[pose].GetHandSnapshot(inputSource);
                if (mask.GetFinger(0) || useMask == false)
                {
                    snapshot.position = Vector3.Lerp(snapshot.position, targetSnapshot.position, influence * value);
                    snapshot.rotation = Quaternion.Slerp(snapshot.rotation, targetSnapshot.rotation, influence * value);
                }

                for (int boneIndex = 0; boneIndex < snapshot.bonePositions.Length; boneIndex++)
                {
                    // verify the current finger is enabled in the mask, or if no mask is used.
                    if (mask.GetFinger(SteamVRSkeletonJointIndexes.GetFingerForBone(boneIndex) + 1) || useMask == false)
                    {
                        snapshot.bonePositions[boneIndex] = Vector3.Lerp(snapshot.bonePositions[boneIndex], targetSnapshot.bonePositions[boneIndex], influence * value);
                        snapshot.boneRotations[boneIndex] = Quaternion.Slerp(snapshot.boneRotations[boneIndex], targetSnapshot.boneRotations[boneIndex], influence * value);
                    }
                }
            }

            public PoseBlendingBehaviour()
            {
                enabled = true;
                influence = 1;
            }

            public enum BlenderTypes
            {
                Manual, AnalogAction, BooleanAction
            }
        }

        //this is broken
        public Vector3 GetTargetHandPosition(SteamVRBehaviourSkeleton hand, Transform origin)
        {
            Vector3 oldOrigin = origin.position;
            Quaternion oldHand = hand.transform.rotation;
            hand.transform.rotation = GetBlendedPose(hand).rotation;
            origin.position = hand.transform.TransformPoint(GetBlendedPose(hand).position);
            Vector3 offset = origin.InverseTransformPoint(hand.transform.position);
            origin.position = oldOrigin;
            hand.transform.rotation = oldHand;
            return origin.TransformPoint(offset);
        }

        public Quaternion GetTargetHandRotation(SteamVRBehaviourSkeleton hand, Transform origin)
        {
            Quaternion oldOrigin = origin.rotation;
            origin.rotation = hand.transform.rotation * GetBlendedPose(hand).rotation;
            Quaternion offsetRot = Quaternion.Inverse(origin.rotation) * hand.transform.rotation;
            origin.rotation = oldOrigin;
            return origin.rotation * offsetRot;
        }
    }

    /// <summary>
    /// PoseSnapshots hold a skeleton pose for one hand, as well as storing which hand they contain.
    /// They have several functions for combining BlendablePoses.
    /// </summary>
    public class SteamVRSkeletonPoseSnapshot
    {
        public SteamVRInputSources inputSource;

        public Vector3 position;
        public Quaternion rotation;

        public Vector3[] bonePositions;
        public Quaternion[] boneRotations;

        public SteamVRSkeletonPoseSnapshot(int boneCount, SteamVRInputSources source)
        {
            inputSource = source;
            bonePositions = new Vector3[boneCount];
            boneRotations = new Quaternion[boneCount];
            position = Vector3.zero;
            rotation = Quaternion.identity;
        }

        /// <summary>
        /// Perform a deep copy from one poseSnapshot to another.
        /// </summary>
        public void CopyFrom(SteamVRSkeletonPoseSnapshot source)
        {
            inputSource = source.inputSource;
            position = source.position;
            rotation = source.rotation;
            for (int i = 0; i < bonePositions.Length; i++)
            {
                bonePositions[i] = source.bonePositions[i];
                boneRotations[i] = source.boneRotations[i];
            }
        }

    }

    /// <summary>
    /// Simple mask for fingers
    /// </summary>
    [Serializable]
    public class SteamVRSkeletonHandMask
    {
        public bool palm;
        public bool thumb;
        public bool index;
        public bool middle;
        public bool ring;
        public bool pinky;
        public bool[] values = new bool[6];

        public void SetFinger(int i, bool value)
        {
            values[i] = value;
            Apply();
        }

        public bool GetFinger(int i)
        {
            return values[i];
        }

        public SteamVRSkeletonHandMask()
        {
            values = new bool[6];
            Reset();
        }

        /// <summary>
        /// All elements on
        /// </summary>
        public void Reset()
        {
            values = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                values[i] = true;
            }
            Apply();
        }

        protected void Apply()
        {
            palm = values[0];
            thumb = values[1];
            index = values[2];
            middle = values[3];
            ring = values[4];
            pinky = values[5];
        }

        public static readonly SteamVRSkeletonHandMask fullMask = new();
    };
}
