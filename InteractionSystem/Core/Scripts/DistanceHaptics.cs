//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Triggers haptic pulses based on distance between 2 positions
//
//=============================================================================

using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class DistanceHaptics : MonoBehaviour
    {
        public DistanceHaptics(IntPtr value) : base(value) { }
        public Transform firstTransform;
        public Transform secondTransform;

        public AnimationCurve distanceIntensityCurve = AnimationCurve.Linear(0.0f, 800.0f, 1.0f, 800.0f);
        public AnimationCurve pulseIntervalCurve = AnimationCurve.Linear(0.0f, 0.01f, 1.0f, 0.0f);

        //-------------------------------------------------
        [HideFromIl2Cpp]
        IEnumerator Start()
        {
            while (true)
            {
                float distance = Vector3.Distance(firstTransform.position, secondTransform.position);

                Hand hand = GetComponentInParent<Hand>();
                if (hand != null)
                {
                    float pulse = distanceIntensityCurve.Evaluate(distance);
                    hand.TriggerHapticPulse((ushort)pulse);

                    //SteamVR_Controller.Input( (int)trackedObject.index ).TriggerHapticPulse( (ushort)pulse );
                }

                float nextPulse = pulseIntervalCurve.Evaluate(distance);

                yield return new WaitForSeconds(nextPulse);
            }

        }
    }
}
