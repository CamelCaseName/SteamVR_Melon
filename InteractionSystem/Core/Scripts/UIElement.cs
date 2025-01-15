//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: UIElement that responds to VR hands and generates UnityEvents
//
//=============================================================================

using MelonLoader;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class UIElement : MonoBehaviour
    {
        public UIElement(IntPtr L) : base(L) { }
        public CustomEvents.UnityEventHand onHandClick = new();

        protected Hand currentHand;

        private Interactable interactable;

        //-------------------------------------------------
        protected virtual void Awake()
        {
            interactable = GetComponent<Interactable>();
            interactable.OnHandHoverBegin += OnHandHoverBegin;
            interactable.OnHandHoverEnd += OnHandHoverEnd;
            interactable.HandHoverUpdate += HandHoverUpdate;
            Button button = GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(new Action(OnButtonClick));
            }
        }

        //-------------------------------------------------
        //todo not sure if these get called
        private void OnHandHoverBegin(Hand hand)
        {
            currentHand = hand;
            InputModule.instance.HoverBegin(gameObject);
            ControllerButtonHints.ShowButtonHint(hand, hand.uiInteractAction);
        }

        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            InputModule.instance.HoverEnd(gameObject);
            ControllerButtonHints.HideButtonHint(hand, hand.uiInteractAction);
            currentHand = null;
        }

        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            MelonLogger.Msg(hand?.name + " " + name);
            if (hand.uiInteractAction != null && hand.uiInteractAction.GetStateDown(hand.handType))
            {
                InputModule.instance.Submit(gameObject);
                ControllerButtonHints.HideButtonHint(hand, hand.uiInteractAction);
            }
        }

        //-------------------------------------------------
        protected virtual void OnButtonClick()
        {
            onHandClick.Send(currentHand);
        }
    }
}