//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Flip Object to match which hand you pick it up in
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{

    public enum WhichHand
    {
        Left,
        Right
    }

    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class Equippable : MonoBehaviour
    {
        public Equippable(IntPtr value) : base(value) { }

        /// <summary>Array of children you do not want to be mirrored. Text, logos, etc.</summary>
        public Transform[] antiFlip;

        public WhichHand defaultHand = WhichHand.Right;

        private Vector3 initialScale;
        private Interactable interactable;

        public SteamVRInputSources attachedHandType
        {
            get
            {
                if (interactable.attachedToHand)
                {
                    return interactable.attachedToHand.handType;
                }
                else
                {
                    return SteamVRInputSources.Any;
                }
            }
        }

        private void Start()
        {
            initialScale = transform.localScale;
            interactable = GetComponent<Interactable>();
        }

        private void Update()
        {
            if (interactable.attachedToHand)
            {
                Vector3 flipScale = initialScale;
                if ((attachedHandType == SteamVRInputSources.RightHand && defaultHand == WhichHand.Right) || (attachedHandType == SteamVRInputSources.LeftHand && defaultHand == WhichHand.Left))
                {
                    flipScale.x *= 1;
                    for (int transformIndex = 0; transformIndex < antiFlip.Length; transformIndex++)
                    {
                        antiFlip[transformIndex].localScale = new Vector3(1, 1, 1);
                    }
                }
                else
                {
                    flipScale.x *= -1;
                    for (int transformIndex = 0; transformIndex < antiFlip.Length; transformIndex++)
                    {
                        antiFlip[transformIndex].localScale = new Vector3(-1, 1, 1);
                    }
                }
                transform.localScale = flipScale;
            }
        }
    }
}
