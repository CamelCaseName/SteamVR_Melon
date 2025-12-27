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
    [RegisterTypeInIl2Cpp()]
    public class UIElement : MonoBehaviour
    {
        public UIElement(IntPtr L) : base(L) { }
        public CustomEvents.UnityEventHand onHandClick = new();

        protected Hand currentHand;

        private Interactable interactable;
        private RectTransform rect;
        private Transform colliderRoot;
        public static readonly bool debugPlacements = true;
        public Canvas canvas;
        private BoxCollider collider;
        bool startedMove = false;
        private ScrollRect scrollRect;
        private bool partialVisible = false;
        public event Action OnSubmit;

        //-------------------------------------------------
        protected virtual void Awake()
        {
            try
            {
                canvas ??= GetComponent<Canvas>();
                canvas ??= GetComponentInParent<Canvas>();
                canvas ??= GetComponentInChildren<Canvas>();

                var BoxGO = new GameObject(name + "Collider");
                BoxGO.transform.parent = transform;
                BoxGO.layer = LayerMask.NameToLayer("UI");

                //todo scrollviews still off

                //if we are in a scrollbox or scrollview or whatever only enable if the item is visible, else hide completely or rescale to bounds
                collider = BoxGO.AddComponent<BoxCollider>();
                rect = GetComponent<RectTransform>();
                rect ??= GetComponentInChildren<RectTransform>();
                rect ??= GetComponentInParent<RectTransform>();
                BoxGO.transform.localScale = new(rect.sizeDelta.x, rect.sizeDelta.y, 0.1f);
                BoxGO.transform.localPosition = new(0, 0, -0.05f);
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

                scrollRect = GetComponentInParent<ScrollRect>();
                //todo if we are in a scrollview only show what is visible -> investigate more with explorer
                //we can surely get the world coords of the scrollview rect and then the coords of our ui element and decide based on that
                scrollRect?.onValueChanged.AddListener((Action<Vector2>)((Vector2 change) =>
                {
                    UpdateColliderForScrollRect(collider);
                }));

                if (scrollRect is not null)
                {
                    MelonLogger.Msg(gameObject.name + " found scrollrect" + scrollRect.name);
                    UpdateColliderForScrollRect(collider);
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error(e);
            }
        }

        public void SetDebugMesh(string prefabName)
        {
            //didnt work
            if (debugPlacements)
            {
                GameObject floor = GameObject.Find(prefabName);
                if (floor is not null)
                {
                    var render = floor.GetComponent<MeshRenderer>();
                    if (render is not null)
                    {
                        Material m = new(render.material);
                        var mesh = collider.gameObject.AddComponent<MeshRenderer>();
                        mesh.material = m;
                        mesh.material.color = Color.red;
                        mesh.material.color.ColorWithAlpha(0.2f);
                    }
                }
            }
        }

        private void UpdateColliderForScrollRect(BoxCollider collider)
        {
            partialVisible = false;
            var view = scrollRect.m_ViewBounds;
            var coll = collider.bounds;
            //item is fully visible
            if (view.Contains(coll.min) && view.Contains(coll.max))
            {
                colliderRoot.gameObject.SetActive(true);
            }
            //partially visible
            //todo test if the coordinates are all from the same origin/reference
            else if (view.Intersects(coll))
            {
                // The min and max points
                Vector3 min = new();
                Vector3 max = new();

                min.x = Mathf.Max(view.min.x, coll.min.x);
                min.y = Mathf.Max(view.min.y, coll.min.y);
                min.z = Mathf.Max(view.min.z, coll.min.z);

                max.x = Mathf.Min(view.max.x, coll.max.x);
                max.y = Mathf.Min(view.max.y, coll.max.y);
                max.z = Mathf.Min(view.max.z, coll.max.z);

                coll.SetMinMax(min, max);
                partialVisible = true;
                //set new collider bounds

                colliderRoot.gameObject.SetActive(true);
            }
            else
            {
                colliderRoot.gameObject.SetActive(false);
            }
        }

        private void OnSliderChange(float obj)
        {
            if (!enabled)
            {
                return;
            }
            onHandClick.Send(currentHand);
        }

        private void OnToggleChange(bool obj)
        {
            if (!enabled)
            {
                return;
            }
            onHandClick.Send(currentHand);
        }

        //-------------------------------------------------
        private void OnHandHoverBegin(Hand hand, Vector2 position, bool poseIsValid)
        {
            if (!enabled)
            {
                return;
            }
            currentHand = hand;
            InputModule.Instance.HoverBegin(gameObject, position, poseIsValid);
        }

        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
            if (!enabled)
            {
                return;
            }
            InputModule.Instance.HoverEnd(gameObject);
            currentHand = null;
        }

        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand, Vector2 position, bool posIsValid)
        {
            if (!enabled)
            {
                return;
            }
            if (hand.uiInteractAction != null && hand.uiInteractAction.stateUp)
            {
                //we get here correctly, but nothing happens. either unityexplorers fault or we need to just hook the internal bit where the action resides and call it ourselves...
                //it is unityexplorers fault because of its own input system
                MelonLogger.Msg("submitting " + gameObject.name);
                InputModule.Instance.Submit(gameObject);
                OnSubmit?.Invoke();
            }
            else if (hand.uiInteractAction != null && hand.uiInteractAction.stateUp && posIsValid)
            {
                //MelonLogger.Msg("moving pressed pointer over " + gameObject.name);
                if (!startedMove)
                {
                    startedMove = true;
                    InputModule.Instance.PointerBeginPress(gameObject, position);
                }
                else
                {
                    InputModule.Instance.PointerPressedUpdate(gameObject, position);
                }
            }
            else if (posIsValid)
            {
                if (startedMove)
                {
                    startedMove = false;
                    InputModule.Instance.PointerEndPress(gameObject, position);
                }
                //InputModule.Instance.HoverUpdate(gameObject, position);
            }
        }

        //-------------------------------------------------
        protected virtual void OnButtonClick()
        {
            if (!enabled)
            {
                return;
            }
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

            if (colliderRoot is null || colliderRoot?.gameObject is null)
            {
                return;
            }

            if (!colliderRoot.gameObject.active || partialVisible)
            {
                return;
            }

            if (rect is null)
            {
                return;
            }

            //update collider if needed
            if (rect.sizeDelta.x != colliderRoot.localScale.x || rect.sizeDelta.y != colliderRoot.localScale.y || colliderRoot.localScale.z != 0.1f)
            {
                colliderRoot.localScale = new(rect.sizeDelta.x, rect.sizeDelta.y, 0.1f);
            }

            //we dont have to adjust local position as we are already childs of the button!
            //if (rect.localPosition != rect.localPosition + new Vector3(0, 0, -0.05f))
            //{
            //    colliderRoot.localPosition = rect.localPosition + new Vector3(0, 0, -0.05f);
            //}
        }
    }
}