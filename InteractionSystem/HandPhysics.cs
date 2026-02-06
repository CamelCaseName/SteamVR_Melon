//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: handles the physics of hands colliding with the world
//
//=============================================================================

using Il2CppInterop.Runtime;
using MelonLoader;
using System;
using UnityEngine;
using Valve.VR;

namespace SteamVR_Melon.InteractionSystem
{
    [RegisterTypeInIl2Cpp()]
    public class HandPhysics : MonoBehaviour
    {
        public HandPhysics(IntPtr value) : base(value) { }

        public HandCollider handCollider;

        /// <summary>Layers to consider when checking if an area is clear</summary>
        public LayerMask clearanceCheckMask;

        public Hand hand;

        // distance at which hand will teleport back to controller
        const float handResetDistance = 0.6f;

        const float collisionReenableClearanceRadius = 0.1f;

        private bool initialized = false;

        private bool collisionsEnabled = true;

        public void Initialize(GameObject HandColliderPrefab)
        {
            try
            {
                handCollider = HandColliderPrefab.GetComponent(Il2CppType.Of<HandCollider>()).Cast<HandCollider>();
                Vector3 localPosition = handCollider.transform.localPosition;
                Quaternion localRotation = handCollider.transform.localRotation;

                handCollider.transform.parent = Player.instance.transform;
                handCollider.transform.localPosition = localPosition;
                handCollider.transform.localRotation = localRotation;
                handCollider.hand = this;
            }
            catch (Exception e)
            {
                MelonLogger.Error("CONTROLLER NOT FOUND");
                MelonLogger.Error(e);
                MelonLogger.Error(e.InnerException.ToString() ?? "");
            }
        }

        // cached transformations
        Matrix4x4 wristToRoot;
        Matrix4x4 rootToArmature;
        Matrix4x4 wristToArmature;

        Vector3 targetPosition = Vector3.zero;
        Quaternion targetRotation = Quaternion.identity;

        private void FixedUpdate()
        {
            if (handCollider is null || handCollider.transform is null)
            {
                return;
            }

            initialized = true;

            UpdateCenterPoint();

            handCollider.MoveTo(targetPosition, targetRotation);

            if ((handCollider.transform.position - targetPosition).sqrMagnitude > handResetDistance * handResetDistance)
            {
                handCollider.TeleportTo(targetPosition, targetRotation);
            }

            UpdateFingertips();
        }

        private void UpdateCenterPoint()
        {
            //Todo replace by own lookup
            //Vector3 offset = hand.skeleton.GetBonePosition(SteamVRSkeletonJointIndexes.middleProximal) - hand.skeleton.GetBonePosition(SteamVRSkeletonJointIndexes.root);
            //if (hand.HasSkeleton())
            //{
            //    handCollider.SetCenterPoint(hand.skeleton.transform.position + offset);
            //}
        }

        readonly Collider[] clearanceBuffer = new Collider[1];

        private void UpdatePositions()
        {
            // disable collisions when holding something
            if (hand.currentAttachedObject != null)
            {
                collisionsEnabled = false;
            }
            else
            {
                // wait for area to become clear before reenabling collisions
                if (!collisionsEnabled)
                {
                    clearanceBuffer[0] = null;
                    Physics.OverlapSphereNonAlloc(hand.objectAttachmentPoint.position, collisionReenableClearanceRadius, clearanceBuffer, clearanceCheckMask);
                    // if we don't find anything in the vicinity, reenable collisions!
                    if (clearanceBuffer[0] == null)
                    {
                        collisionsEnabled = true;
                    }
                }
            }

            handCollider.SetCollisionDetectionEnabled(collisionsEnabled);

            initialized = true;

            // get the desired pose of the wrist in world space. Can't get the wrist bone transform, as this is affected by the resulting physics.

            //todo replace by own lookup
            //wristToRoot = Matrix4x4.TRS(ProcessPos(wristBone, hand.skeleton.GetBone(wristBone).localPosition),
            //    ProcessRot(wristBone, hand.skeleton.GetBone(wristBone).localRotation),
            //    Vector3.one).inverse;

            //rootToArmature = Matrix4x4.TRS(ProcessPos(rootBone, hand.skeleton.GetBone(rootBone).localPosition),
            //    ProcessRot(rootBone, hand.skeleton.GetBone(rootBone).localRotation),
            //    Vector3.one).inverse;

            //wristToArmature = (wristToRoot * rootToArmature).inverse;

            //// step up through virtual transform hierarchy and into world space
            //targetPosition = transform.TransformPoint(wristToArmature.MultiplyPoint3x4(Vector3.zero));

            //targetRotation = transform.rotation * wristToArmature.GetRotation();

            //bypass physics when game paused
            if (Time.timeScale == 0)
            {
                handCollider.TeleportTo(targetPosition, targetRotation);
            }
        }

        Transform wrist;

        //const int thumbBone = SteamVRSkeletonJointIndexes.thumbDistal;
        //const int indexBone = SteamVRSkeletonJointIndexes.indexDistal;
        //const int middleBone = SteamVRSkeletonJointIndexes.middleDistal;
        //const int ringBone = SteamVRSkeletonJointIndexes.ringDistal;
        //const int pinkyBone = SteamVRSkeletonJointIndexes.pinkyDistal;

        void UpdateFingertips()
        {
            //todo replace lookup by our own
            //wrist = hand.skeleton.GetBone(SteamVRSkeletonJointIndexes.wrist);

            //// set finger tip positions in wrist space

            //for (int finger = 0; finger < 5; finger++)
            //{
            //    int tip = SteamVRSkeletonJointIndexes.GetBoneForFingerTip(finger);
            //    int bone = tip;
            //    for (int i = 0; i < handCollider.fingerColliders[finger].Length; i++)
            //    {
            //        bone = tip - 1 - i; // start at distal and go down
            //        if (handCollider.fingerColliders[finger][i] != null)
            //        {
            //            handCollider.fingerColliders[finger][i].localPosition = wrist.InverseTransformPoint(hand.skeleton.GetBone(bone).position);
            //        }
            //    }
            //}
            /*
            if(handCollider.tip_thumb != null)
                handCollider.tip_thumb.localPosition = wrist.InverseTransformPoint(hand.skeleton.GetBone(thumbBone).position);

            if(handCollider.tip_index != null)
            handCollider.tip_index.localPosition = wrist.InverseTransformPoint(hand.skeleton.GetBone(indexBone).position);

            if(handCollider.tip_middle != null)
            handCollider.tip_middle.localPosition = wrist.InverseTransformPoint(hand.skeleton.GetBone(middleBone).position);

            if(handCollider.tip_ring != null)
            handCollider.tip_ring.localPosition = wrist.InverseTransformPoint(hand.skeleton.GetBone(ringBone).position);

            if (handCollider.tip_pinky != null)
            handCollider.tip_pinky.localPosition = wrist.InverseTransformPoint(hand.skeleton.GetBone(pinkyBone).position);
            */
        }
    }
}