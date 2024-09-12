//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Triggers haptic pulses based on a linear mapping
//
//=============================================================================

using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class HapticRack : MonoBehaviour
    {
        public HapticRack(IntPtr value) : base(value) { }
        ///<summary>The linear mapping driving the haptic rack</summary>
        public LinearMapping linearMapping;

        ///<summary>The number of haptic pulses evenly distributed along the mapping</summary>
        public int teethCount = 128;

        ///<summary>Minimum duration of the haptic pulse</summary>
        public int minimumPulseDuration = 500;

        ///<summary>Maximum duration of the haptic pulse</summary>
        public int maximumPulseDuration = 900;

        ///<summary>This event is triggered every time a haptic pulse is made</summary>
        public UnityEvent onPulse;

        private Hand hand;
        private int previousToothIndex = -1;

        //-------------------------------------------------
        void Awake()
        {
            if ( linearMapping == null )
            {
                linearMapping = GetComponent<LinearMapping>();
            }
        }


        //-------------------------------------------------
        private void OnHandHoverBegin( Hand hand )
        {
            this.hand = hand;
        }


        //-------------------------------------------------
        private void OnHandHoverEnd( Hand hand )
        {
            this.hand = null;
        }


        //-------------------------------------------------
        void Update()
        {
            int currentToothIndex = Mathf.RoundToInt( linearMapping.value * teethCount - 0.5f );
            if ( currentToothIndex != previousToothIndex )
            {
                Pulse();
                previousToothIndex = currentToothIndex;
            }
        }


        //-------------------------------------------------
        private void Pulse()
        {
            if ( hand && (hand.isActive) && ( hand.GetBestGrabbingType() != GrabTypes.None ) )
            {
                ushort duration = (ushort)UnityEngine.Random.Range( minimumPulseDuration, maximumPulseDuration + 1 );
                hand.TriggerHapticPulse( duration );

                onPulse.Invoke();
            }
        }
    }
}
