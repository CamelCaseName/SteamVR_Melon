//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: UIElement that responds to VR hands and generates UnityEvents
//
//=============================================================================

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
        public static readonly bool debugPlacements = false;
        public Canvas canvas;
        bool startedMove = false;

        //-------------------------------------------------
        protected virtual void Awake()
        {
            canvas ??= GetComponent<Canvas>();
            canvas ??= GetComponentInParent<Canvas>();
            canvas ??= GetComponentInChildren<Canvas>();

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

            //if we are in a scrollbox or scrollview or whatever only enable if the item is visible, else hide completely or rescale to bounds
            var collider = BoxGO.AddComponent<BoxCollider>();
            rect = GetComponent<RectTransform>();
            rect ??= GetComponentInChildren<RectTransform>();
            rect ??= GetComponentInParent<RectTransform>();
            BoxGO.transform.localScale = new(rect.sizeDelta.x, rect.sizeDelta.y, 0.1f);
            colliderRoot = BoxGO.transform;

            interactable = GetComponent<Interactable>();
            interactable.OnHandHoverBegin += OnHandHoverBegin;
            interactable.OnHandHoverEnd += OnHandHoverEnd;
            interactable.HandHoverUpdate += HandHoverUpdate;

            Button button = GetComponent<Button>();
            button?.onClick.AddListener(new Action(OnButtonClick));
            Toggle toggle = GetComponent<Toggle>();
            toggle?.onValueChanged.AddListener(new Action<bool>(OnToggleChange));
            Slider slider = GetComponent<Slider>();
            slider?.onValueChanged.AddListener(new Action<float>(OnSliderChange));
        }

        private void OnSliderChange(float obj)
        {
            onHandClick.Send(currentHand);
        }

        private void OnToggleChange(bool obj)
        {
            onHandClick.Send(currentHand);
        }

        //-------------------------------------------------
        //todo not sure if these get called
        private void OnHandHoverBegin(Hand hand, Vector2 position, bool poseIsValid)
        {
            currentHand = hand;
            InputModule.Instance.HoverBegin(gameObject, position, poseIsValid);
        }

        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            InputModule.Instance.HoverEnd(gameObject);
            currentHand = null;
        }

        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand, Vector2 position, bool posIsValid)
        {
            if (hand.uiInteractAction != null && hand.uiInteractAction.GetStateDown(hand.handType))
            {
                if (posIsValid)
                {
                    if (!startedMove)
                    {
                        startedMove = true;
                        InputModule.Instance.PointerBeginPress(gameObject, position);
                    }
                    InputModule.Instance.PointerPressedUpdate(gameObject, position);
                }
                else
                {
                    InputModule.Instance.Submit(gameObject);
                }
            }
            else if (posIsValid)
            {
                if (startedMove)
                {
                    startedMove = false;
                    InputModule.Instance.PointerEndPress(gameObject, position);
                }
                InputModule.Instance.HoverUpdate(gameObject, position);
            }
        }

        //-------------------------------------------------
        protected virtual void OnButtonClick()
        {
            onHandClick.Send(currentHand);
        }

        protected void Update()
        {
            if (canvas is null)
            {
                canvas ??= GetComponent<Canvas>();
                canvas ??= GetComponentInParent<Canvas>();
                canvas ??= GetComponentInChildren<Canvas>();
            }
            //update collider if needed
            if (rect.sizeDelta.x != colliderRoot.localScale.x || rect.sizeDelta.y != colliderRoot.localScale.y || colliderRoot.localScale.z != 0.1f)
            {
                colliderRoot.localScale = new(rect.sizeDelta.x, rect.sizeDelta.y, 0.1f);
            }
        }
    }
}