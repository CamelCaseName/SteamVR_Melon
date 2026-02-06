//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Player interface used to query HMD transforms and VR hands
//
//=============================================================================

using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections;
using UnityEngine;
using Valve.VR;

namespace SteamVR_Melon.InteractionSystem
{
    //-------------------------------------------------------------------------
    // Singleton representing the local VR player/user, with methods for getting
    // the player's hands, head, tracking origin, and guesses for various properties.
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class Player : MonoBehaviour
    {
        public Player(IntPtr value) : base(value) { }
        ///<summary>Virtual transform corresponding to the meatspace tracking origin. Devices are tracked relative to this.</summary>
        public Transform trackingOriginTransform;

        ///<summary>List of possible transforms for the head/HMD, including the no-SteamVR fallback camera.</summary>
        public Transform[] hmdTransforms;

        ///<summary>List of possible Hands, including no-SteamVR fallback Hands.</summary>
        public Hand[] hands;

        ///<summary>Reference to the physics collider that follows the player's HMD position.</summary>
        public Collider headCollider;

        ///<summary>These objects are enabled when SteamVR is available</summary>
        public GameObject rigSteamVR;

        ///<summary>The audio listener for this player</summary>
        public Transform audioListener;

        public bool allowToggleTo2D = true;

        //-------------------------------------------------
        // Singleton instance of the Player. Only one can exist at a time.
        //-------------------------------------------------
        private static Player _instance;
        public static Player instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<Player>();
                }
                return _instance;
            }
        }

        //-------------------------------------------------
        // Get the number of active Hands.
        //-------------------------------------------------
        public int handCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < hands.Length; i++)
                {
                    if (hands[i].gameObject.activeInHierarchy)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        //-------------------------------------------------
        // Get the i-th active Hand.
        //
        // i - Zero-based index of the active Hand to get
        //-------------------------------------------------
        public Hand GetHand(int i)
        {
            for (int j = 0; j < hands.Length; j++)
            {
                if (!hands[j].gameObject.activeInHierarchy)
                {
                    continue;
                }

                if (i > 0)
                {
                    i--;
                    continue;
                }

                return hands[j];
            }

            return null;
        }

        //-------------------------------------------------
        public Hand leftHand
        {
            get
            {
                for (int j = 0; j < hands.Length; j++)
                {
                    if (!hands[j].gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    return hands[j];
                }

                return null;
            }
        }

        //-------------------------------------------------
        public Hand rightHand
        {
            get
            {
                for (int j = 0; j < hands.Length; j++)
                {
                    if (!hands[j].gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    return hands[j];
                }

                return null;
            }
        }

        //-------------------------------------------------
        // Get Player scale. Assumes it is scaled equally on all axes.
        //-------------------------------------------------

        public float scale
        {
            get
            {
                return transform.lossyScale.x;
            }
        }

        //-------------------------------------------------
        // Get the HMD transform. This might return the fallback camera transform if SteamVR is unavailable or disabled.
        //-------------------------------------------------
        public Transform hmdTransform
        {
            get
            {
                if (hmdTransforms != null)
                {
                    for (int i = 0; i < hmdTransforms.Length; i++)
                    {
                        if (hmdTransforms[i].gameObject.activeInHierarchy)
                        {
                            return hmdTransforms[i];
                        }
                    }
                }
                return null;
            }
        }

        //-------------------------------------------------
        // Height of the eyes above the ground - useful for estimating player height.
        //-------------------------------------------------
        public float eyeHeight
        {
            get
            {
                Transform hmd = hmdTransform;
                if (hmd)
                {
                    Vector3 eyeOffset = Vector3.Project(hmd.position - trackingOriginTransform.position, trackingOriginTransform.up);
                    return eyeOffset.magnitude / trackingOriginTransform.lossyScale.x;
                }
                return 0.0f;
            }
        }

        //-------------------------------------------------
        // Guess for the world-space position of the player's feet, directly beneath the HMD.
        //-------------------------------------------------
        public Vector3 feetPositionGuess
        {
            get
            {
                Transform hmd = hmdTransform;
                if (hmd)
                {
                    return trackingOriginTransform.position + Vector3.ProjectOnPlane(hmd.position - trackingOriginTransform.position, trackingOriginTransform.up);
                }
                return trackingOriginTransform.position;
            }
        }

        //-------------------------------------------------
        // Guess for the world-space direction of the player's hips/torso. This is effectively just the gaze direction projected onto the floor plane.
        //-------------------------------------------------
        public Vector3 bodyDirectionGuess
        {
            get
            {
                Transform hmd = hmdTransform;
                if (hmd)
                {
                    Vector3 direction = Vector3.ProjectOnPlane(hmd.forward, trackingOriginTransform.up);
                    if (Vector3.Dot(hmd.up, trackingOriginTransform.up) < 0.0f)
                    {
                        // The HMD is upside-down. Either
                        // -The player is bending over backwards
                        // -The player is bent over looking through their legs
                        direction = -direction;
                    }
                    return direction;
                }
                return trackingOriginTransform.forward;
            }
        }

        //-------------------------------------------------
        public void Init()
        {
            if (trackingOriginTransform == null)
            {
                trackingOriginTransform = transform;
            }

#if OPENVR_XR_API && UNITY_LEGACY_INPUT_HELPERS
            if (hmdTransforms != null)
            {
                foreach (var hmd in hmdTransforms)
                {
                    if (hmd.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>() == null)
                        hmd.gameObject.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                }
            }
#endif
        }

        //-------------------------------------------------
        [HideFromIl2Cpp]
        public IEnumerator Start()
        {
            _instance = this;

            while (SteamVR.initializedState == SteamVR.InitializedStates.None || SteamVR.initializedState == SteamVR.InitializedStates.Initializing)
            {
                yield return null;
            }

            if (SteamVR.Instance != null)
            {
                ActivateRig(rigSteamVR);
            }
        }

        protected virtual void Update()
        {
            if (SteamVR.initializedState != SteamVR.InitializedStates.InitializeSuccess)
            {
                return;
            }
        }

        //-------------------------------------------------
        private void ActivateRig(GameObject rig)
        {
            rigSteamVR.SetActive(rig == rigSteamVR);

            if (audioListener)
            {
                audioListener.transform.parent = hmdTransform;
                audioListener.transform.localPosition = Vector3.zero;
                audioListener.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
