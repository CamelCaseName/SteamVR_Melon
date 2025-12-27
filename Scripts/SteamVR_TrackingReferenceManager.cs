using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class SteamVRTrackingReferenceManager : MonoBehaviour
    {
        public SteamVRTrackingReferenceManager(IntPtr value) : base(value) { }

        private readonly Dictionary<uint, TrackingReferenceObject> trackingReferences = new();

        private void OnEnable()
        {
            SteamVREvents.NewPoses.Listen(OnNewPoses);
        }

        private void OnDisable()
        {
            SteamVREvents.NewPoses.Remove(OnNewPoses);
        }

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        private void OnNewPoses(TrackedDevicePoseT[] poses)
        {
            if (poses == null)
            {
                return;
            }

            for (uint deviceIndex = 0; deviceIndex < poses.Length; deviceIndex++)
            {
                if (trackingReferences.ContainsKey(deviceIndex) == false)
                {
                    ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass(deviceIndex);

                    if (deviceClass == ETrackedDeviceClass.TrackingReference)
                    {
                        TrackingReferenceObject trackingReference = new()
                        {
                            trackedDeviceClass = deviceClass,
                            gameObject = new GameObject("Tracking Reference " + deviceIndex.ToString())
                        };
                        trackingReference.gameObject.transform.parent = transform;
                        trackingReference.trackedObject = trackingReference.gameObject.AddComponent<SteamVRTrackedObject>();
                        trackingReference.renderModel = trackingReference.gameObject.AddComponent<SteamVRRenderModel>();
                        trackingReference.renderModel.createComponents = false;
                        trackingReference.renderModel.updateDynamically = false;

                        trackingReferences.Add(deviceIndex, trackingReference);

                        trackingReference.gameObject.SendMessage("SetDeviceIndex", (int)deviceIndex, SendMessageOptions.DontRequireReceiver);
                    }
                    else
                    {
                        trackingReferences.Add(deviceIndex, new TrackingReferenceObject() { trackedDeviceClass = deviceClass });
                    }
                }
            }
        }

        private class TrackingReferenceObject
        {
            public ETrackedDeviceClass trackedDeviceClass;
            public GameObject gameObject;
            public SteamVRRenderModel renderModel;
            public SteamVRTrackedObject trackedObject;
        }
    }
}