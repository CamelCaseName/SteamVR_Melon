//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Makes the hand act as an input module for Unity's event system
//
//=============================================================================
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

        //-------------------------------------------------
        private static InputModule _instance;
        private int callDepth = 0;
        public static InputModule instance
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
        public void HoverBegin(GameObject gameObject)
        {
            PointerEventData pointerEventData = new(eventSystem);
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

        //-------------------------------------------------
        public void Submit(GameObject gameObject)
        {
            submitObject = gameObject;
        }

        //-------------------------------------------------
        public override void Process()
        {
            if (submitObject)
            {
                BaseEventData data = GetBaseEventData();
                data.selectedObject = submitObject;
                ExecuteEvents.Execute(submitObject, data, ExecuteEvents.submitHandler);

                submitObject = null;
            }
        }
    }
}