//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SkeletonUIOptions : MonoBehaviour
    {
        public SkeletonUIOptions(IntPtr value) : base(value) { }
        public void AnimateHandWithController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                hand?.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithController);
            }
        }

        public void AnimateHandWithoutController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                hand?.SetSkeletonRangeOfMotion(Valve.VR.EVRSkeletalMotionRange.WithoutController);
            }
        }

        public void ShowController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                hand?.ShowController(true);
            }
        }

        public void SetRenderModel(RenderModelChangerUI prefabs)
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                if (hand != null)
                {
                    if (hand.handType == SteamVRInputSources.RightHand)
                    {
                        hand.SetRenderModel(prefabs.rightPrefab);
                    }

                    if (hand.handType == SteamVRInputSources.LeftHand)
                    {
                        hand.SetRenderModel(prefabs.leftPrefab);
                    }
                }
            }
        }

        public void HideController()
        {
            for (int handIndex = 0; handIndex < Player.instance.hands.Length; handIndex++)
            {
                Hand hand = Player.instance.hands[handIndex];
                hand?.HideController(true);
            }
        }
    }
}