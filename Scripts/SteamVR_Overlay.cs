//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Displays 2d content on a large virtual screen.
//
//=============================================================================

using Il2CppInterop.Runtime.Attributes;
using SteamVR_Melon.Scripts;
using System;
using UnityEngine;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVROverlay : MonoBehaviour
    {
        public SteamVROverlay(IntPtr value) : base(value) { }

        public Texture texture;

        public float scale = 3.0f;

        public float distance = 1.25f;

        public float alpha = 1.0f;

        public Vector4 uvOffset = new(0, 0, 1, 1);
        public Vector2 mouseScale = new(1, 1);

        public VROverlayInputMethod inputMethod = VROverlayInputMethod.None;

        [HideFromIl2Cpp]
        static public SteamVROverlay instance { get; private set; }

        static public string key { get { return "unity:" + Application.companyName + "." + Application.productName; } }

        private ulong handle = OpenVR.kUlOverlayHandleInvalid;

        void Init()
        {
            var overlay = OpenVR.Overlay;
            if (overlay != null)
            {
                var error = overlay.CreateOverlay(key, gameObject.name, ref handle);
                if (error != EVROverlayError.None)
                {
                    MelonLoader.MelonLogger.Msg("[HPVR] " + overlay.GetOverlayErrorNameFromEnum(error));
                    enabled = false;
                    return;
                }
            }

            instance = this;
        }

        void OnDisable()
        {
            if (handle != OpenVR.kUlOverlayHandleInvalid)
            {
                var overlay = OpenVR.Overlay;
                overlay?.DestroyOverlay(handle);

                handle = OpenVR.kUlOverlayHandleInvalid;
            }

            instance = null;
        }

        public void UpdateOverlay()
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
            {
                return;
            }

            if (texture != null)
            {
                var error = overlay.ShowOverlay(handle);
                if (error == EVROverlayError.InvalidHandle || error == EVROverlayError.UnknownOverlay)
                {
                    if (overlay.FindOverlay(key, ref handle) != EVROverlayError.None)
                    {
                        return;
                    }
                }

                var tex = new TextureT
                {
                    handle = texture.GetNativeTexturePtr(),
                    eType = SteamVR.instance.textureType,
                    eColorSpace = EColorSpace.Auto
                };
                overlay.SetOverlayTexture(handle, ref tex);

                overlay.SetOverlayAlpha(handle, alpha);
                overlay.SetOverlayWidthInMeters(handle, scale);

                var textureBounds = new VRTextureBoundsT
                {
                    uMin = (0 + uvOffset.x) * uvOffset.z,
                    vMin = (1 + uvOffset.y) * uvOffset.w,
                    uMax = (1 + uvOffset.x) * uvOffset.z,
                    vMax = (0 + uvOffset.y) * uvOffset.w
                };
                overlay.SetOverlayTextureBounds(handle, ref textureBounds);

                var vecMouseScale = new HmdVector2T
                {
                    v0 = mouseScale.x,
                    v1 = mouseScale.y
                };
                overlay.SetOverlayMouseScale(handle, ref vecMouseScale);

                var vrcam = SteamVRRender.Top();
                if (vrcam != null && vrcam.origin != null)
                {
                    var offset = new SteamVRUtils.RigidTransform(vrcam.origin, transform);
                    offset.pos.x /= vrcam.origin.localScale.x;
                    offset.pos.y /= vrcam.origin.localScale.y;
                    offset.pos.z /= vrcam.origin.localScale.z;

                    offset.pos.z += distance;

                    var t = offset.ToHmdMatrix34();
                    overlay.SetOverlayTransformAbsolute(handle, SteamVR.settings.trackingSpace, ref t);
                }

                overlay.SetOverlayInputMethod(handle, inputMethod);
            }
            else
            {
                overlay.HideOverlay(handle);
            }
        }

        [HideFromIl2Cpp]
        public bool PollNextEvent(ref VREventT pEvent)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
            {
                return false;
            }

            var size = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(VREventT));
            return overlay.PollNextOverlayEvent(handle, ref pEvent, size);
        }

        public struct IntersectionResults
        {
            public Vector3 point;
            public Vector3 normal;
            public Vector2 UVs;
            public float distance;
        }

        [HideFromIl2Cpp]
        public bool ComputeIntersection(Vector3 source, Vector3 direction, ref IntersectionResults results)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
            {
                return false;
            }

            var input = new VROverlayIntersectionParamsT
            {
                eOrigin = SteamVR.settings.trackingSpace
            };
            input.vSource.v0 = source.x;
            input.vSource.v1 = source.y;
            input.vSource.v2 = -source.z;
            input.vDirection.v0 = direction.x;
            input.vDirection.v1 = direction.y;
            input.vDirection.v2 = -direction.z;

            var output = new VROverlayIntersectionResultsT();
            if (!overlay.ComputeOverlayIntersection(handle, ref input, ref output))
            {
                return false;
            }

            results.point = new Vector3(output.vPoint.v0, output.vPoint.v1, -output.vPoint.v2);
            results.normal = new Vector3(output.vNormal.v0, output.vNormal.v1, -output.vNormal.v2);
            results.UVs = new Vector2(output.vUVs.v0, output.vUVs.v1);
            results.distance = output.fDistance;
            return true;
        }
    }
}