﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Demonstrates the use of the controller hint system
//
//=============================================================================

using System;
using System.Collections;
using UnityEngine;

namespace Valve.VR.InteractionSystem.Sample
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class ControllerHintsExample : MonoBehaviour
    {
        public ControllerHintsExample(IntPtr value) : base(value) { }
        private IEnumerator buttonHintCoroutine;
        private IEnumerator textHintCoroutine;

        //-------------------------------------------------
        public void ShowButtonHints(Hand hand)
        {
            if (buttonHintCoroutine != null)
            {
                MelonLoader.MelonCoroutines.Stop(buttonHintCoroutine);
            }
            buttonHintCoroutine = TestButtonHints(hand);

            MelonLoader.MelonCoroutines.Start(buttonHintCoroutine);
        }


        //-------------------------------------------------
        public void ShowTextHints(Hand hand)
        {
            if (textHintCoroutine != null)
            {
                MelonLoader.MelonCoroutines.Stop(textHintCoroutine);
            }
            textHintCoroutine = TestTextHints(hand);

            MelonLoader.MelonCoroutines.Start(textHintCoroutine);
        }


        //-------------------------------------------------
        public void DisableHints()
        {
            if (buttonHintCoroutine != null)
            {
                MelonLoader.MelonCoroutines.Stop(buttonHintCoroutine);
                buttonHintCoroutine = null;
            }

            if (textHintCoroutine != null)
            {
                MelonLoader.MelonCoroutines.Stop(textHintCoroutine);
                textHintCoroutine = null;
            }

            foreach (Hand hand in Player.instance.hands)
            {
                ControllerButtonHints.HideAllButtonHints(hand);
                ControllerButtonHints.HideAllTextHints(hand);
            }
        }


        //-------------------------------------------------
        // Cycles through all the button hints on the controller
        //-------------------------------------------------
        private IEnumerator TestButtonHints(Hand hand)
        {
            ControllerButtonHints.HideAllButtonHints(hand);

            while (true)
            {
                for (int actionIndex = 0; actionIndex < SteamVR_Input.actionsIn.Length; actionIndex++)
                {
                    ISteamVR_Action_In action = SteamVR_Input.actionsIn[actionIndex];
                    if (action.GetActive(hand.handType))
                    {
                        ControllerButtonHints.ShowButtonHint(hand, action);
                        yield return new WaitForSeconds(1.0f);
                        ControllerButtonHints.HideButtonHint(hand, action);
                        yield return new WaitForSeconds(0.5f);
                    }
                    yield return null;
                }

                ControllerButtonHints.HideAllButtonHints(hand);
                yield return new WaitForSeconds(1.0f);
            }
        }


        //-------------------------------------------------
        // Cycles through all the text hints on the controller
        //-------------------------------------------------
        private IEnumerator TestTextHints(Hand hand)
        {
            ControllerButtonHints.HideAllTextHints(hand);

            while (true)
            {
                for (int actionIndex = 0; actionIndex < SteamVR_Input.actionsIn.Length; actionIndex++)
                {
                    ISteamVR_Action_In action = SteamVR_Input.actionsIn[actionIndex];
                    if (action.GetActive(hand.handType))
                    {
                        ControllerButtonHints.ShowTextHint(hand, action, action.GetShortName());
                        yield return new WaitForSeconds(3.0f);
                        ControllerButtonHints.HideTextHint(hand, action);
                        yield return new WaitForSeconds(0.5f);
                    }
                    yield return null;
                }

                ControllerButtonHints.HideAllTextHints(hand);
                yield return new WaitForSeconds(3.0f);
            }
        }
    }
}
