//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Generates a mesh based on field of view.
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRFrustum : MonoBehaviour
    {
        public SteamVRFrustum(IntPtr value) : base(value) { }

        public SteamVRTrackedObject.EIndex index;

        public float fovLeft = 45, fovRight = 45, fovTop = 45, fovBottom = 45, nearZ = 0.5f, farZ = 2.5f;

        public void UpdateModel()
        {
            fovLeft = Mathf.Clamp(fovLeft, 1, 89);
            fovRight = Mathf.Clamp(fovRight, 1, 89);
            fovTop = Mathf.Clamp(fovTop, 1, 89);
            fovBottom = Mathf.Clamp(fovBottom, 1, 89);
            farZ = Mathf.Max(farZ, nearZ + 0.01f);
            nearZ = Mathf.Clamp(nearZ, 0.01f, farZ - 0.01f);

            var lsin = Mathf.Sin(-fovLeft * Mathf.Deg2Rad);
            var rsin = Mathf.Sin(fovRight * Mathf.Deg2Rad);
            var tsin = Mathf.Sin(fovTop * Mathf.Deg2Rad);
            var bsin = Mathf.Sin(-fovBottom * Mathf.Deg2Rad);

            var lcos = Mathf.Cos(-fovLeft * Mathf.Deg2Rad);
            var rcos = Mathf.Cos(fovRight * Mathf.Deg2Rad);
            var tcos = Mathf.Cos(fovTop * Mathf.Deg2Rad);
            var bcos = Mathf.Cos(-fovBottom * Mathf.Deg2Rad);

            var corners = new Vector3[] {
            new(lsin * nearZ / lcos, tsin * nearZ / tcos, nearZ), //tln
            new(rsin * nearZ / rcos, tsin * nearZ / tcos, nearZ), //trn
            new(rsin * nearZ / rcos, bsin * nearZ / bcos, nearZ), //brn
            new(lsin * nearZ / lcos, bsin * nearZ / bcos, nearZ), //bln
            new(lsin * farZ  / lcos, tsin * farZ  / tcos, farZ ), //tlf
            new(rsin * farZ  / rcos, tsin * farZ  / tcos, farZ ), //trf
            new(rsin * farZ  / rcos, bsin * farZ  / bcos, farZ ), //brf
            new(lsin * farZ  / lcos, bsin * farZ  / bcos, farZ ), //blf
        };

            var triangles = new int[] {
        //	0, 1, 2, 0, 2, 3, // near
        //	0, 2, 1, 0, 3, 2, // near
        //	4, 5, 6, 4, 6, 7, // far
        //	4, 6, 5, 4, 7, 6, // far
            0, 4, 7, 0, 7, 3, // left
            0, 7, 4, 0, 3, 7, // left
            1, 5, 6, 1, 6, 2, // right
            1, 6, 5, 1, 2, 6, // right
            0, 4, 5, 0, 5, 1, // top
            0, 5, 4, 0, 1, 5, // top
            2, 3, 7, 2, 7, 6, // bottom
            2, 7, 3, 2, 6, 7, // bottom
        };

            int j = 0;
            var vertices = new Vector3[triangles.Length];
            var normals = new Vector3[triangles.Length];
            for (int i = 0; i < triangles.Length / 3; i++)
            {
                var a = corners[triangles[i * 3 + 0]];
                var b = corners[triangles[i * 3 + 1]];
                var c = corners[triangles[i * 3 + 2]];
                var n = Vector3.Cross(b - a, c - a).normalized;
                normals[i * 3 + 0] = n;
                normals[i * 3 + 1] = n;
                normals[i * 3 + 2] = n;
                vertices[i * 3 + 0] = a;
                vertices[i * 3 + 1] = b;
                vertices[i * 3 + 2] = c;
                triangles[i * 3 + 0] = j++;
                triangles[i * 3 + 1] = j++;
                triangles[i * 3 + 2] = j++;
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                normals = normals,
                triangles = triangles
            };

            GetComponent<MeshFilter>().mesh = mesh;
        }

        private void OnDeviceConnected(int i, bool connected)
        {
            if (i != (int)index)
            {
                return;
            }

            GetComponent<MeshFilter>().mesh = null;

            if (connected)
            {
                var system = OpenVR.System;
                if (system != null && system.GetTrackedDeviceClass((uint)i) == ETrackedDeviceClass.TrackingReference)
                {
                    var error = ETrackedPropertyError.TrackedPropSuccess;
                    var result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.PropFieldOfViewLeftDegreesFloat, ref error);
                    if (error == ETrackedPropertyError.TrackedPropSuccess)
                    {
                        fovLeft = result;
                    }

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.PropFieldOfViewRightDegreesFloat, ref error);
                    if (error == ETrackedPropertyError.TrackedPropSuccess)
                    {
                        fovRight = result;
                    }

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.PropFieldOfViewTopDegreesFloat, ref error);
                    if (error == ETrackedPropertyError.TrackedPropSuccess)
                    {
                        fovTop = result;
                    }

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.PropFieldOfViewBottomDegreesFloat, ref error);
                    if (error == ETrackedPropertyError.TrackedPropSuccess)
                    {
                        fovBottom = result;
                    }

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.PropTrackingRangeMinimumMetersFloat, ref error);
                    if (error == ETrackedPropertyError.TrackedPropSuccess)
                    {
                        nearZ = result;
                    }

                    result = system.GetFloatTrackedDeviceProperty((uint)i, ETrackedDeviceProperty.PropTrackingRangeMaximumMetersFloat, ref error);
                    if (error == ETrackedPropertyError.TrackedPropSuccess)
                    {
                        farZ = result;
                    }

                    UpdateModel();
                }
            }
        }

        void OnEnable()
        {
            GetComponent<MeshFilter>().mesh = null;
            SteamVREvents.DeviceConnected.Listen(OnDeviceConnected);
        }

        void OnDisable()
        {
            SteamVREvents.DeviceConnected.Remove(OnDeviceConnected);
            GetComponent<MeshFilter>().mesh = null;
        }

#if UNITY_EDITOR
        void Update()
        {
            if (!Application.isPlaying)
                UpdateModel();
        }
#endif
    }
}