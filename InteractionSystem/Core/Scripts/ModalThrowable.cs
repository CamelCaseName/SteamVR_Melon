//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Basic throwable object
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    public class ModalThrowable : Throwable
    {
        public ModalThrowable(IntPtr L) : base(L) { }   
        /// <summary>The local point which acts as a positional and rotational offset to use while held with a grip type grab</summary>
        public Transform gripOffset;

        /// <summary>The local point which acts as a positional and rotational offset to use while held with a pinch type grab</summary>
        public Transform pinchOffset;

        protected override void HandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();

            if (startingGrabType != GrabTypes.None)
            {
                if (startingGrabType == GrabTypes.Pinch)
                {
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags, pinchOffset);
                }
                else if (startingGrabType == GrabTypes.Grip)
                {
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags, gripOffset);
                }
                else
                {
                    hand.AttachObject(gameObject, startingGrabType, attachmentFlags, attachmentOffset);
                }

                hand.HideGrabHint();
            }
        }
        protected override void HandAttachedUpdate(Hand hand)
        {
            if (interactable.skeletonPoser != null)
            {
                interactable.skeletonPoser.SetBlendingBehaviourEnabled("PinchPose", hand.currentAttachedObjectInfo.Value.grabbedWithType == GrabTypes.Pinch);
            }

            base.HandAttachedUpdate(hand);
        }
    }
}