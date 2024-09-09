//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Adds SteamVR render support to existing camera objects
//
//=============================================================================

using Assets.SteamVR_Melon.Standalone;
using Il2CppInterop.Runtime;
using MelonLoader;
using Standalone;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace Valve.VR
{
    public class SteamVR_Camera : MonoBehaviour
    {
        public SteamVR_Camera(IntPtr value) : base(value) { }
        private Transform _head;
        public Transform head { get { return _head; } }
        public Transform origin { get { return _head.parent; } }

        public Camera camera { get; private set; }

        private Transform _ears;
        public Transform ears { get { return _ears; } }

        public Ray GetRay()
        {
            return new Ray(_head.position, _head.forward);
        }

        public bool wireframe = false;

        static public float sceneResolutionScale
        {
            get { return XRSettings.eyeTextureResolutionScale; }
            set { XRSettings.eyeTextureResolutionScale = value; }
        }

        private SteamVR_CameraFlip flip;

        #region Materials

        static public Material blitMaterial;

        public static Action<int, int> OnResolutionChanged;

        // Using a single shared offscreen buffer to render the scene.  This needs to be larger
        // than the backbuffer to account for distortion correction.  The default resolution
        // gives us 1:1 sized pixels in the center of view, but quality can be adjusted up or
        // down using the following scale value to balance performance.
        static public float sceneResolutionScaleMultiplier = 1f;

        static private RenderTexture _sceneTexture;

        public static Resolution GetSceneResolution()
        {
            var vr = SteamVR.instance;
            Resolution r = new Resolution();
            int w = (int)(vr.sceneWidth * sceneResolutionScale * sceneResolutionScaleMultiplier);
            int h = (int)(vr.sceneHeight * sceneResolutionScale * sceneResolutionScaleMultiplier);
            r.width = w;
            r.height = h;
            return r;
        }

        public static Resolution GetResolutionForAspect(int aspectW, int aspectH)
        {
            Resolution hmdResolution = GetSceneResolution();

            // We calcuate an optimal 16:9 resolution to use with the HMD resolution because that's the best aspect for the UI rendering
            Resolution closestToAspect = hmdResolution;
            closestToAspect.height = closestToAspect.width / aspectW * aspectH;
            closestToAspect.width += closestToAspect.width % 2;
            closestToAspect.height += closestToAspect.height % 2;
            return closestToAspect;
        }

        public static Resolution GetUnscaledSceneResolution()
        {
            var vr = SteamVR.instance;
            Resolution r = new Resolution();
            r.width = (int)vr.sceneWidth;
            r.height = (int)vr.sceneHeight;
            r.width += r.width % 2;
            r.height += r.height % 2;
            return r;
        }

        static public RenderTexture GetSceneTexture(bool hdr)
        {
            var vr = SteamVR.instance;
            if (vr == null)
                return null;

            int w = (int)(vr.sceneWidth * sceneResolutionScale * sceneResolutionScaleMultiplier);
            int h = (int)(vr.sceneHeight * sceneResolutionScale * sceneResolutionScaleMultiplier);
            w += w % 2;
            h += h % 2;


            int aa = QualitySettings.antiAliasing == 0 ? 1 : QualitySettings.antiAliasing;
            var format = hdr ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGB32;
            bool recreatedTex = false;
            if (_sceneTexture != null)
            {
                if (_sceneTexture.width != w || _sceneTexture.height != h)
                {
                    MelonLogger.Msg($"Recreating scene texture.. Old: {_sceneTexture.width}x{_sceneTexture.height} MSAA={_sceneTexture.antiAliasing} [{aa}] New: {w}x{h} MSAA={aa} [{format}]");
                    Destroy(_sceneTexture);
                    _sceneTexture = null;
                    recreatedTex = true;
                }
            }

            if (_sceneTexture == null)
            {
                _sceneTexture = new RenderTexture(w, h, 0, format, 0);
                _sceneTexture.depth = 32;
                _sceneTexture.antiAliasing = aa;

                if (recreatedTex)
                {
                    OnResolutionChanged?.Invoke(w, h);
                }
            }

            return _sceneTexture;
        }

        #endregion


        #region Enable / Disable

        void OnDisable()
        {
            SteamVR_Render.Remove(this);
        }

        public static bool doomp = false;

        void Update()
        {
            doomp = false;
            if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                MelonLogger.Msg("[HPVR] Doomping rendertextures...");
                doomp = true;
            }
        }

        void LateUpdate()
        {
            doomp = false;
            if (Keyboard.current.f4Key.wasPressedThisFrame)
            {
                MelonLogger.Msg("[HPVR] Doomping rendertextures...");
                doomp = true;
            }
        }

        void OnEnable()
        {
            // Bail if no hmd is connected
            var vr = SteamVR.instance;
            if (vr == null)
            {
                MelonLogger.Warning("[HPVR] No hmd detected, aborting!");
                if (head != null)
                {
                    head.GetComponent<SteamVR_GameView>().enabled = false;
                    head.GetComponent<SteamVR_TrackedObject>().enabled = false;
                }

                if (flip != null)
                    flip.enabled = false;

                enabled = false;
                return;
            }
            MelonLogger.Msg("[HPVR] building camera rig");
            // Ensure rig is properly set up
            Expand();

            if (blitMaterial == null)
            {
                blitMaterial = new Material(VRShaders.GetShader(VRShaders.VRShader.blit));
            }

            MelonLogger.Msg("[HPVR] setting camera settings");
            // Set remaining hmd specific settings
            var camera = Camera.main;
            MelonLogger.Msg("[HPVR] steamvr camera camera is null: " + (camera is null));
            camera.fieldOfView = vr.fieldOfView;
            camera.aspect = vr.aspect;
            camera.eventMask = 0;           // disable mouse events
            camera.orthographic = false;    // force perspective
            camera.enabled = false;         // manually rendered by SteamVR_Render

            if (camera.actualRenderingPath != RenderingPath.Forward && QualitySettings.antiAliasing > 1)
            {
                MelonLogger.Warning("[HPVR] MSAA only supported in Forward rendering path. (disabling MSAA)");
                QualitySettings.antiAliasing = 0;
            }

            // Ensure game view camera hdr setting matches
            var headCam = head.GetComponent<Camera>();
            if (headCam != null)
            {
                headCam.renderingPath = camera.renderingPath;
            }

            if (ears == null)
            {
                var e = transform.GetComponentInChildren<SteamVR_Ears>();
                if (e != null)
                    _ears = e.transform;
            }

            if (ears != null)
                ears.GetComponent<SteamVR_Ears>().vrcam = this;

            SteamVR_Render.Add(this);
            MelonLogger.Msg("[HPVR] Done creating the cameras");
        }

        #endregion

        #region Functionality to ensure SteamVR_Camera component is always the last component on an object

        void Awake()
        {
            camera = Camera.main; // cached to avoid runtime lookup
            ForceLast();
        }

        static Hashtable values;

        public void ForceLast()
        {
            if (isLast)
            {
                if (flip == null)
                {
                    flip = gameObject.GetComponent<SteamVR_CameraFlip>();
                }
                return;
            }
            Component[] components = GetComponents<Component>();
            if (this != components[components.Length - 1] || flip == null)
            {
                if (flip == null)
                {
                    flip = gameObject.AddComponent<SteamVR_CameraFlip>();
                }
                GameObject g = gameObject;
                DestroyImmediate(this);
                isLast = true;
                g.AddComponent<SteamVR_Camera>().ForceLast();
            }
        }
        static bool isLast;

        #endregion

        #region Expand / Collapse object hierarchy

        const string eyeSuffix = " (eye)";
        const string earsSuffix = " (ears)";
        const string headSuffix = " (head)";
        const string originSuffix = " (origin)";
        public string baseName { get { return name.EndsWith(eyeSuffix) ? name.Substring(0, name.Length - eyeSuffix.Length) : name; } }

        // Object hierarchy creation to make it easy to parent other objects appropriately,
        // otherwise this gets called on demand at runtime. Remaining initialization is
        // performed at startup, once the hmd has been identified.
        public void Expand()
        {
            MelonLogger.Msg("[HPVR] expanding...");
            var _origin = transform.parent;
            if (_origin == null)
            {
                _origin = new GameObject(name + originSuffix).transform;
                _origin.localPosition = transform.localPosition;
                _origin.localRotation = transform.localRotation;
                _origin.localScale = transform.localScale;
            }

            if (head == null)
            {
                _head = new GameObject(name + headSuffix, Il2CppType.Of<SteamVR_TrackedObject>()).transform;
                head.parent = _origin;
                head.position = transform.position;
                head.rotation = transform.rotation;
                head.localScale = Vector3.one;
                head.tag = tag;
            }

            if (transform.parent != head)
            {
                transform.parent = head;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                while (transform.childCount > 0)
                    transform.GetChild(0).parent = head;

                var audioListener = GetComponent<AudioListener>();
                if (audioListener != null)
                {
                    DestroyImmediate(audioListener);
                    _ears = new GameObject(name + earsSuffix, Il2CppType.Of<SteamVR_Ears>()).transform;
                    ears.parent = _head;
                    ears.localPosition = Vector3.zero;
                    ears.localRotation = Quaternion.identity;
                    ears.localScale = Vector3.one;
                }
            }

            if (!name.EndsWith(eyeSuffix))
                name += eyeSuffix;
            MelonLogger.Msg("[HPVR] ...done");
        }

        public void Collapse()
        {
            transform.parent = null;

            // Move children and components from head back to camera.
            while (head.childCount > 0)
                head.GetChild(0).parent = transform;
            if (ears != null)
            {
                while (ears.childCount > 0)
                    ears.GetChild(0).parent = transform;

                DestroyImmediate(ears.gameObject);
                _ears = null;

                gameObject.AddComponent<AudioListener>();
            }

            if (origin != null)
            {
                // If we created the origin originally, destroy it now.
                if (origin.name.EndsWith(originSuffix))
                {
                    // Reparent any children so we don't accidentally delete them.
                    var _origin = origin;
                    while (_origin.childCount > 0)
                        _origin.GetChild(0).parent = _origin.parent;

                    DestroyImmediate(_origin.gameObject);
                }
                else
                {
                    transform.parent = origin;
                }
            }

            DestroyImmediate(head.gameObject);
            _head = null;

            if (name.EndsWith(eyeSuffix))
                name = name.Substring(0, name.Length - eyeSuffix.Length);
        }

        #endregion


        public static void DumpRenderTexture(RenderTexture rt, string pngOutPath)
        {
            Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            byte[] textureBytes = ImageConversion.EncodeToPNG(tex);
            MelonLogger.Msg($"Writing texture to {pngOutPath}");
            File.WriteAllBytes(pngOutPath, textureBytes);
        }

        public static bool useHeadTracking = true;
    }
}