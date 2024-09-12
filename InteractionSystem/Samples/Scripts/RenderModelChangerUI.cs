//======= Copyright (c) Valve Corporation, All rights reserved. ===============

#if false
using UnityEngine;
using System.Collections;
using System;

namespace Valve.VR.InteractionSystem.Sample
{
    public class RenderModelChangerUI : UIElement
    {
        public RenderModelChangerUI(IntPtr va) : base(va) { }
        public GameObject leftPrefab;
        public GameObject rightPrefab;

        protected SkeletonUIOptions ui;

        protected override void Awake()
        {
            base.Awake();

            ui = this.GetComponentInParent<SkeletonUIOptions>();
        }

        protected override void OnButtonClick()
        {
            base.OnButtonClick();

            if (ui != null)
            {
                ui.SetRenderModel(this);
            }
        }
    }
}
#else
using System;
using UnityEngine;
namespace Valve.VR.InteractionSystem.Sample
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class RenderModelChangerUI : MonoBehaviour
    {
        public RenderModelChangerUI(IntPtr va) : base(va) { }
        public GameObject leftPrefab;
        public GameObject rightPrefab;
    }
}
#endif