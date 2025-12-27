//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: This object will get hover events and can be attached to the hands
//
//=============================================================================

using MelonLoader;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valve.VR.InteractionSystem
{
    //-------------------------------------------------------------------------
    [MelonLoader.RegisterTypeInIl2Cpp()]
    public class Interactable : MonoBehaviour
    {
        public Interactable(IntPtr value) : base(value) { }
        /// <summary>Activates an action set on attach and deactivates on detach</summary>
        public SteamVR_ActionSet activateActionSetOnAttach;

        /// <summary>Hide the whole hand on attachment and show on detach</summary>
        public bool hideHandOnAttach = true;

        /// <summary>Hide the skeleton part of the hand on attachment and show on detach</summary>
        public bool hideSkeletonOnAttach = false;

        /// <summary>Hide the controller part of the hand on attachment and show on detach</summary>
        public bool hideControllerOnAttach = false;

        /// <summary>The integer in the animator to trigger on pickup. 0 for none</summary>
        public int handAnimationOnPickup = 0;

        /// <summary>The range of motion to set on the skeleton. None for no change.</summary>
        public SkeletalMotionRangeChange setRangeOfMotionOnPickup = SkeletalMotionRangeChange.None;

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand> OnAttachedToHand = new(h => { });
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand> OnDetachedFromHand = new(h => { });

        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand, Vector2, bool> HandHoverUpdate = new((h, v, b) => { });
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand> HandAttachedUpdate = new(h => { });
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand, Vector2, bool> OnHandHoverBegin = new((h, v, b) => { });
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand> OnHandHoverEnd = new(h => { });
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand> OnHandFocusLost = new(h => { });
        [Il2CppInterop.Runtime.Attributes.HideFromIl2Cpp]
        public event Action<Hand> OnHandFocusAcquired = new(h => { });

        /// <summary>Specify whether you want to snap to the hand's object attachment point, or just the raw hand</summary>
        public bool useHandObjectAttachmentPoint = true;

        public bool attachEaseIn = false;

        public bool WasSetByHand = false;

        public AnimationCurve snapAttachEaseInCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        public float snapAttachEaseInTime = 0.15f;

        public bool snapAttachEaseInCompleted = false;

        // /// <summary>The skeleton pose to apply when grabbing. Can only set this or handFollowTransform.</summary>

        public SteamVR_Skeleton_Poser skeletonPoser;

        /// <summary>Should the rendered hand lock on to and follow the object</summary>
        public bool handFollowTransform = true;

        /// <summary>Set whether or not you want this interactible to highlight when hovering over it</summary>
        public bool highlightOnHover = true;
        protected MeshRenderer[] highlightRenderers;
        protected MeshRenderer[] existingRenderers;
        protected GameObject highlightHolder;
        protected SkinnedMeshRenderer[] highlightSkinnedRenderers;
        protected SkinnedMeshRenderer[] existingSkinnedRenderers;
        protected static Material highlightMat;
        /// <summary>An array of child gameObjects to not render a highlight for. Things like transparent parts, vfx, etc.</summary>
        public GameObject[] hideHighlight = Array.Empty<GameObject>();

        /// <summary>Higher is better</summary>
        public int hoverPriority = 0;

        [NonSerialized]
        public Hand attachedToHand;

        [NonSerialized]
        public List<Hand> hoveringHands = new(2);
        public Hand hoveringHand
        {
            get
            {
                if (hoveringHands.Count > 0)
                {
                    return hoveringHands[0];
                }

                return null;
            }
        }

        public bool isDestroying { get; protected set; }
        public bool isHovering { get; protected set; }
        public bool wasHovering { get; protected set; }

        protected virtual void Awake()
        {
            skeletonPoser = GetComponent<SteamVR_Skeleton_Poser>();
        }

        protected virtual void Start()
        {
            //if (highlightMat == null)
            //{
            //    highlightMat = Resources.Load<Material>("SteamVR_HoverHighlight_URP");
            //}

            //if (highlightMat == null)
            //{
            //    MelonLoader.MelonLogger.Error("[HPVR Interaction] Hover Highlight Material is missing. Please create a material named 'SteamVR_HoverHighlight' and place it in a Resources folder", this);
            //}

            if (skeletonPoser != null)
            {
                if (useHandObjectAttachmentPoint)
                {
                    //MelonLoader.MelonLogger.Warning("[HPVR Interaction] SkeletonPose and useHandObjectAttachmentPoint both set at the same time. Ignoring useHandObjectAttachmentPoint.");
                    useHandObjectAttachmentPoint = false;
                }
            }
        }

        protected virtual bool ShouldIgnoreHighlight(Component component)
        {
            return ShouldIgnore(component.gameObject);
        }

        protected virtual bool ShouldIgnore(GameObject check)
        {
            for (int ignoreIndex = 0; ignoreIndex < hideHighlight.Length; ignoreIndex++)
            {
                if (check == hideHighlight[ignoreIndex])
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void CreateHighlightRenderers()
        {
            existingSkinnedRenderers = this.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            highlightHolder = new GameObject("Highlighter");
            highlightSkinnedRenderers = new SkinnedMeshRenderer[existingSkinnedRenderers.Length];

            for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];

                if (ShouldIgnoreHighlight(existingSkinned))
                {
                    continue;
                }

                GameObject newSkinnedHolder = new GameObject("SkinnedHolder");
                newSkinnedHolder.transform.parent = highlightHolder.transform;
                SkinnedMeshRenderer newSkinned = newSkinnedHolder.AddComponent<SkinnedMeshRenderer>();
                Material[] materials = new Material[existingSkinned.sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = highlightMat;
                }

                newSkinned.sharedMaterials = materials;
                newSkinned.sharedMesh = existingSkinned.sharedMesh;
                newSkinned.rootBone = existingSkinned.rootBone;
                newSkinned.updateWhenOffscreen = existingSkinned.updateWhenOffscreen;
                newSkinned.bones = existingSkinned.bones;

                highlightSkinnedRenderers[skinnedIndex] = newSkinned;
            }

            MeshFilter[] existingFilters = this.GetComponentsInChildren<MeshFilter>(true);
            existingRenderers = new MeshRenderer[existingFilters.Length];
            highlightRenderers = new MeshRenderer[existingFilters.Length];

            for (int filterIndex = 0; filterIndex < existingFilters.Length; filterIndex++)
            {
                MeshFilter existingFilter = existingFilters[filterIndex];
                MeshRenderer existingRenderer = existingFilter.GetComponent<MeshRenderer>();

                if (existingFilter == null || existingRenderer == null || ShouldIgnoreHighlight(existingFilter))
                {
                    continue;
                }

                GameObject newFilterHolder = new GameObject("FilterHolder");
                newFilterHolder.transform.parent = highlightHolder.transform;
                MeshFilter newFilter = newFilterHolder.AddComponent<MeshFilter>();
                newFilter.sharedMesh = existingFilter.sharedMesh;
                MeshRenderer newRenderer = newFilterHolder.AddComponent<MeshRenderer>();

                Material[] materials = new Material[existingRenderer.sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < materials.Length; materialIndex++)
                {
                    materials[materialIndex] = highlightMat;
                }
                newRenderer.sharedMaterials = materials;

                highlightRenderers[filterIndex] = newRenderer;
                existingRenderers[filterIndex] = existingRenderer;
            }
        }

        protected virtual void UpdateHighlightRenderers()
        {
            if (highlightHolder == null)
            {
                return;
            }

            for (int skinnedIndex = 0; skinnedIndex < existingSkinnedRenderers.Length; skinnedIndex++)
            {
                SkinnedMeshRenderer existingSkinned = existingSkinnedRenderers[skinnedIndex];
                SkinnedMeshRenderer highlightSkinned = highlightSkinnedRenderers[skinnedIndex];

                if (existingSkinned != null && highlightSkinned != null && attachedToHand == false)
                {
                    highlightSkinned.transform.position = existingSkinned.transform.position;
                    highlightSkinned.transform.rotation = existingSkinned.transform.rotation;
                    highlightSkinned.transform.localScale = existingSkinned.transform.lossyScale;
                    highlightSkinned.localBounds = existingSkinned.localBounds;
                    highlightSkinned.enabled = isHovering && existingSkinned.enabled && existingSkinned.gameObject.activeInHierarchy;

                    int blendShapeCount = existingSkinned.sharedMesh.blendShapeCount;
                    for (int blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                    {
                        highlightSkinned.SetBlendShapeWeight(blendShapeIndex, existingSkinned.GetBlendShapeWeight(blendShapeIndex));
                    }
                }
                else if (highlightSkinned != null)
                {
                    highlightSkinned.enabled = false;
                }
            }

            for (int rendererIndex = 0; rendererIndex < highlightRenderers.Length; rendererIndex++)
            {
                MeshRenderer existingRenderer = existingRenderers[rendererIndex];
                MeshRenderer highlightRenderer = highlightRenderers[rendererIndex];

                if (existingRenderer != null && highlightRenderer != null && attachedToHand == false)
                {
                    highlightRenderer.transform.position = existingRenderer.transform.position;
                    highlightRenderer.transform.rotation = existingRenderer.transform.rotation;
                    highlightRenderer.transform.localScale = existingRenderer.transform.lossyScale;
                    highlightRenderer.enabled = isHovering && existingRenderer.enabled && existingRenderer.gameObject.activeInHierarchy;
                }
                else if (highlightRenderer != null)
                {
                    highlightRenderer.enabled = false;
                }
            }
        }

        /// <summary>
        /// Called when a Hand starts hovering over this object
        /// </summary>
        public virtual void OnHandHoverBegin_Internal(Hand hand, Vector2 position, bool PoseIsvalid)
        {
            wasHovering = isHovering;
            isHovering = true;

            hoveringHands.Add(hand);

            if (highlightOnHover == true && wasHovering == false)
            {
                CreateHighlightRenderers();
                UpdateHighlightRenderers();
            }
            OnHandHoverBegin(hand, position, PoseIsvalid);
        }

        /// <summary>
        /// Called when a Hand stops hovering over this object
        /// </summary>
        public virtual void OnHandHoverEnd_Internal(Hand hand)
        {
            wasHovering = isHovering;
            WasSetByHand = false;

            hoveringHands.Remove(hand);

            if (hoveringHands.Count == 0)
            {
                isHovering = false;

                if (highlightOnHover && highlightHolder != null)
                {
                    Destroy(highlightHolder);
                }
            }
            OnHandHoverEnd(hand);
        }

        public void HandHoverUpdate_Internal(Hand hand, Vector2 position, bool posIsValid)
        {
            //we correctly get till here. the event works for UI, but not in game??
            //MelonLogger.Msg("hover update called on " + name + " by " + hand.name + ", running for: " + HandHoverUpdate.GetInvocationList().Length);
            HandHoverUpdate(hand, position, posIsValid);
        }

        protected virtual void Update()
        {
            if (highlightOnHover)
            {
                UpdateHighlightRenderers();

                if (isHovering == false && highlightHolder != null)
                {
                    Destroy(highlightHolder);
                }
            }
        }

        protected float blendToPoseTime = 0.1f;
        protected float releasePoseBlendTime = 0.2f;

        public virtual void OnAttachedToHand_Internal(Hand hand)
        {
            activateActionSetOnAttach?.Activate(hand.handType);

            OnAttachedToHand?.Invoke(hand);

            if (skeletonPoser != null && hand.skeleton != null)
            {
                hand.skeleton.BlendToPoser(skeletonPoser, blendToPoseTime);
            }

            attachedToHand = hand;
        }

        public virtual void OnDetachedFromHand_Internal(Hand hand)
        {
            if (activateActionSetOnAttach != null)
            {
                if (hand.otherHand == null || hand.otherHand.currentAttachedObjectInfo.HasValue == false ||
                    (hand.otherHand.currentAttachedObjectInfo.Value.interactable != null &&
                     hand.otherHand.currentAttachedObjectInfo.Value.interactable.activateActionSetOnAttach != this.activateActionSetOnAttach))
                {
                    activateActionSetOnAttach.Deactivate(hand.handType);
                }
            }

            OnDetachedFromHand?.Invoke(hand);

            if (skeletonPoser != null)
            {
                hand.skeleton?.BlendToSkeleton(releasePoseBlendTime);
            }

            attachedToHand = null;
        }

        protected virtual void OnDestroy()
        {
            isDestroying = true;

            if (attachedToHand != null)
            {
                attachedToHand.DetachObject(this.gameObject, false);
                attachedToHand.skeleton.BlendToSkeleton(0.1f);
            }

            if (highlightHolder != null)
            {
                Destroy(highlightHolder);
            }
        }

        protected virtual void OnDisable()
        {
            isDestroying = true;

            attachedToHand?.ForceHoverUnlock();

            if (highlightHolder != null)
            {
                Destroy(highlightHolder);
            }
        }

        internal void HandAttachedUpdate_Internal(Hand hand)
        {
            HandAttachedUpdate(hand);
        }

        internal void OnHandFocusAcquired_Internal(Hand hand) => OnHandFocusAcquired(hand);

        internal void OnHandFocusLost_Internal(Hand hand) => OnHandFocusLost(hand);
    }
}
