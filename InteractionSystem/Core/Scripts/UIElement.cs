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
        private RectTransform rect;
        private Transform colliderRoot;
        public static bool debugPlacements = false;

        //-------------------------------------------------
        protected virtual void Awake()
        {
            var BoxGO = new GameObject(name + "Collider");
            BoxGO.transform.parent = transform;
            BoxGO.transform.localPosition = new(0, 0, -0.05f);
            BoxGO.layer = LayerMask.NameToLayer("UI");

            if (debugPlacements)
            {
                Material m = new(GameObject.Find("Floor").GetComponent<MeshRenderer>().material);
                var mesh = BoxGO.AddComponent<MeshRenderer>();
                mesh.material = m;
                mesh.material.color = Color.white;
            }

            var collider = BoxGO.AddComponent<BoxCollider>();
            rect = GetComponent<RectTransform>();
            BoxGO.transform.localScale = new(rect.sizeDelta.x, rect.sizeDelta.y, 0.1f);
            colliderRoot = BoxGO.transform;

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
        }

        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            InputModule.instance.HoverEnd(gameObject);
            currentHand = null;
        }

        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            MelonLogger.Msg(hand?.name + " " + name);
            if (hand.uiInteractAction != null && hand.uiInteractAction.GetStateDown(hand.handType))
            {
                InputModule.instance.Submit(gameObject);
            }
        }

        //-------------------------------------------------
        protected virtual void OnButtonClick()
        {
            onHandClick.Send(currentHand);
        }

        protected void Update()
        {
            //update collider if needed
            if (rect.sizeDelta.x != colliderRoot.localScale.x || rect.sizeDelta.y != colliderRoot.localScale.y || colliderRoot.localScale.z != 0.1f)
            {
                colliderRoot.localScale = new(rect.sizeDelta.x, rect.sizeDelta.y, 0.1f);
            }
        }
    }
}