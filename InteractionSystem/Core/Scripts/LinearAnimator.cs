﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Animator whose speed is set based on a linear mapping
//
//=============================================================================

using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class LinearAnimator : MonoBehaviour
    {
        public LinearAnimator(IntPtr value) : base(value) { }
        public LinearMapping linearMapping;
        public Animator animator;

        private float currentLinearMapping = float.NaN;
        private int framesUnchanged = 0;


        //-------------------------------------------------
        void Awake()
        {
            if ( animator == null )
            {
                animator = GetComponent<Animator>();
            }

            animator.speed = 0.0f;

            if ( linearMapping == null )
            {
                linearMapping = GetComponent<LinearMapping>();
            }
        }


        //-------------------------------------------------
        void Update()
        {
            if ( currentLinearMapping != linearMapping.value )
            {
                currentLinearMapping = linearMapping.value;
                animator.enabled = true;
                animator.Play( 0, 0, currentLinearMapping );
                framesUnchanged = 0;
            }
            else
            {
                framesUnchanged++;
                if ( framesUnchanged > 2 )
                {
                    animator.enabled = false;
                }
            }
        }
    }
}
