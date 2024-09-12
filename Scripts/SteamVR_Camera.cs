//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Adds SteamVR render support to existing camera objects
//
//=============================================================================

using Il2CppInterop.Runtime;
using MelonLoader;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.XR;

namespace Valve.VR
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class SteamVR_Camera : MonoBehaviour
    {
        public SteamVR_Camera(IntPtr value) : base(value) { }
        private Transform _head;
        public Transform head { get { return _head; } }
        public Transform offset { get { return _head; } } // legacy
        public Transform origin { get { return _head.parent; } }

        public Camera camera { get; private set; }


        private Transform _ears;
        public Transform ears { get { return _ears; } }

        public Ray GetRay()
        {
            return new Ray(_head.position, _head.forward);
        }

        public bool wireframe = false;

        static public float sceneResolutionScale = 1.0f;

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
            Resolution r = new Resolution();
            if (SteamVR.instance is not null)
            {
                int w = (int)(SteamVR.instance.sceneWidth * sceneResolutionScale * sceneResolutionScaleMultiplier);
                int h = (int)(SteamVR.instance.sceneHeight * sceneResolutionScale * sceneResolutionScaleMultiplier);
                r.width = w;
                r.height = h;
                //MelonLogger.Msg($"[HPVR] {sceneResolutionScale}|{sceneResolutionScaleMultiplier}");
            }
            else
            {
                MelonLogger.Msg(SteamVR.enabled);
                MelonLogger.Warning("[HPVR] steamvr instance was null when getting scene resolution!");
            }
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

        void OnEnable()
        {
            // Bail if no hmd is connected
            var vr = SteamVR.instance;
            if (vr == null)
            {
                if (head != null)
                {
                    head.GetComponent<SteamVR_TrackedObject>().enabled = false;
                }

                enabled = false;
                return;
            }

            // Convert camera rig for native OpenVR integration.
            var t = transform;
            if (head != t)
            {
                Expand();

                t.parent = origin;

                while (head.childCount > 0)
                    head.GetChild(0).parent = t;

                // Keep the head around, but parent to the camera now since it moves with the hmd
                // but existing content may still have references to this object.
                head.parent = t;
                head.localPosition = Vector3.zero;
                head.localRotation = Quaternion.identity;
                head.localScale = Vector3.one;
                head.gameObject.SetActive(false);

                _head = t;
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
        }

        #endregion

        #region Functionality to ensure SteamVR_Camera component is always the last component on an object

        void Awake()
        {
            camera = GetComponent<Camera>(); // cached to avoid runtime lookup
            ForceLast();
        }

        static Hashtable values;

        public void ForceLast()
        {
            if (values != null)
            {
                // Restore values on new instance
                foreach (DictionaryEntry entry in values)
                {
                    var f = entry.Key as FieldInfo;
                    f.SetValue(this, entry.Value);
                }
                values = null;
            }
            else
            {
                // Make sure it's the last component
                var components = GetComponents<Component>();

                // But first make sure there aren't any other SteamVR_Cameras on this object.
                for (int i = 0; i < components.Length; i++)
                {
                    var c = components[i] as SteamVR_Camera;
                    if (c != null && c != this)
                    {
                        DestroyImmediate(c);
                    }
                }

                components = GetComponents<Component>();

                if (this != components[components.Length - 1])
                {
                    // Store off values to be restored on new instance
                    values = new Hashtable();
                    var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (var f in fields)
                        if (f.IsPublic || f.IsDefined(typeof(SerializeField), true))
                            values[f] = f.GetValue(this);

                    var go = gameObject;
                    DestroyImmediate(this);
                    go.AddComponent<SteamVR_Camera>().ForceLast();
                }
            }
        }

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
    }
}