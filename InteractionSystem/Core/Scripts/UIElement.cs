﻿//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: UIElement that responds to VR hands and generates UnityEvents
//
//=============================================================================

#if UNITY_UGUI_UI || !UNITY_2019_2_OR_NEWER
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class UIElement : MonoBehaviour
    {
        public UIElement(IntPtr L) : base(L) { }
        public CustomEvents.UnityEventHand onHandClick;

        protected Hand currentHand;

        //-------------------------------------------------
        protected virtual void Awake()
        {
            Button button = GetComponent<Button>();
            if (button)
            {
                button.onClick.AddListener(new Action(OnButtonClick));
            }
        }


        //-------------------------------------------------
        protected virtual void OnHandHoverBegin(Hand hand)
        {
            currentHand = hand;
            InputModule.instance.HoverBegin(gameObject);
            ControllerButtonHints.ShowButtonHint(hand, hand.uiInteractAction);
        }


        //-------------------------------------------------
        protected virtual void OnHandHoverEnd(Hand hand)
        {
            InputModule.instance.HoverEnd(gameObject);
            ControllerButtonHints.HideButtonHint(hand, hand.uiInteractAction);
            currentHand = null;
        }


        //-------------------------------------------------
        protected virtual void HandHoverUpdate(Hand hand)
        {
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

#if UNITY_EDITOR
	//-------------------------------------------------------------------------
	[UnityEditor.CustomEditor( typeof( UIElement ) )]
	public class UIElementEditor : UnityEditor.Editor
	{
		//-------------------------------------------------
		// Custom Inspector GUI allows us to click from within the UI
		//-------------------------------------------------
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			UIElement uiElement = (UIElement)target;
			if ( GUILayout.Button( "Click" ) )
			{
				InputModule.instance.Submit( uiElement.gameObject );
			}
		}
	}
#endif
}
#else
using UnityEngine;
namespace Valve.VR.InteractionSystem { public class UIElement : MonoBehaviour {} }
#endif