//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes the hand act as an input module for Unity's event system
//
//=============================================================================
using MelonLoader;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Valve.VR.InteractionSystem
{
    [MelonLoader.RegisterTypeInIl2Cpp]
    //-------------------------------------------------------------------------
    public class InputModule : BaseInputModule
    {
        public InputModule(IntPtr value) : base(value) { }

        private GameObject submitObject;

        private static readonly float deadzone = 0;
        private static readonly float pointerResScale = 0.03f;

        private Vector2 lastPointerPos = Vector2.zero;
        private Vector2 thisFramePointerPos = Vector2.zero;
        private Vector2 pressPointerPos = Vector2.zero;
        private bool isPressed = false;

        //-------------------------------------------------
        private static InputModule _instance;
        private int callDepth = 0;
        public static InputModule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<InputModule>();
                }

                return _instance;
            }
        }

        public void EndFrame()
        {
            lastPointerPos = thisFramePointerPos;
        }

        //-------------------------------------------------
        public override bool ShouldActivateModule()
        {
            callDepth++;
            if (callDepth > 5)
            {
                return submitObject != null;
            }
            if (!base.ShouldActivateModule())
            {
                return false;
            }

            return submitObject != null;
        }

        //-------------------------------------------------
        //todo check if hover events work for dropdowns and stuff
        public void HoverBegin(GameObject gameObject, Vector2 pointerPosition, bool PosIsValid)
        {
            PointerEventData pointerEventData = new(eventSystem);
            if (PosIsValid)
            {
                thisFramePointerPos = pointerPosition;
                pointerEventData.position = pointerPosition;
            }
            ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerEnterHandler);
        }

        //-------------------------------------------------
        public void HoverEnd(GameObject gameObject)
        {
            PointerEventData pointerEventData = new(eventSystem)
            {
                selectedObject = null
            };
            ExecuteEvents.Execute(gameObject, pointerEventData, ExecuteEvents.pointerExitHandler);
        }

        public void HoverUpdate(GameObject gameObject, Vector2 pointerPosition)
        {
            thisFramePointerPos = pointerPosition;

            //todo investigate the slider scaling
            AxisEventData axisData = GetAxisEventData((pointerPosition.x - lastPointerPos.x) * pointerResScale, (pointerPosition.y - lastPointerPos.y) * pointerResScale, deadzone);
            ExecuteEvents.Execute(gameObject, axisData, ExecuteEvents.moveHandler);
            lastPointerPos = thisFramePointerPos;
        }

        public void PointerPressedUpdate(GameObject gameObject, Vector2 pointerPosition)
        {
            thisFramePointerPos = pointerPosition;
            PointerEventData data = new(eventSystem)
            {
                position = pointerPosition,
                pressPosition = pressPointerPos,
                dragging = true
            };
            ExecuteEvents.Execute(gameObject, data, ExecuteEvents.dragHandler);
        }

        public void PointerBeginPress(GameObject gameObject, Vector2 pointerPosition)
        {
            thisFramePointerPos = pointerPosition;
            if (!isPressed)
            {
                isPressed = true;
                pressPointerPos = pointerPosition;
            }
            PointerEventData data = new(eventSystem)
            {
                position = pointerPosition,
                pressPosition = pressPointerPos
            };
            ExecuteEvents.Execute(gameObject, data, ExecuteEvents.pointerDownHandler);
        }

        public void PointerEndPress(GameObject gameObject, Vector2 pointerPosition)
        {
            thisFramePointerPos = pointerPosition;
            isPressed = false;

            PointerEventData data = new(eventSystem)
            {
                position = pointerPosition,
                pressPosition = pointerPosition,
                dragging = false
            };
            ExecuteEvents.Execute(gameObject, data, ExecuteEvents.dragHandler);
        }

        //-------------------------------------------------
        public void Submit(GameObject gameObject)
        {
            submitObject = gameObject;
        }

        //-------------------------------------------------
        public override void Process()
        {
            if (submitObject is not null)
            {
                try
                {
                    BaseEventData data = GetBaseEventData();
                    data.selectedObject = submitObject;
                    ExecuteEvents.Execute(submitObject, data, ExecuteEvents.submitHandler);

                    submitObject = null;
                }
                catch (Exception e)
                {
                    MelonLogger.Msg(e);
                    MelonLogger.Msg(e.InnerException?.Message ?? "none");
                }
            }
        }
    }
}