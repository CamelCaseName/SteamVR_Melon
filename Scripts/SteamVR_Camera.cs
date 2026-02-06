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

namespace Valve.VR
{
    [RegisterTypeInIl2Cpp()]
    public class SteamVRCamera : MonoBehaviour
    {
        public SteamVRCamera(IntPtr value) : base(value) { }
        private Transform _head;
        public Transform head { get { return _head; } }
        public Transform offset { get { return _head; } } // legacy
        public Transform origin { get { return _head.parent; } }

        public Camera camera { get; private set; }

        public new Transform transform { get { return base.transform; } }

        private Transform _ears;
        public Transform ears { get { return _ears; } }

        public static SteamVRCamera instance = null;

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

        #endregion
        #region Enable / Disable


        void OnEnable()
        {
            // Bail if no hmd is connected
            var vr = SteamVR.Instance;
            if (vr == null)
            {
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
                {
                    head.GetChild(0).parent = t;
                }

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
                var e = transform.GetComponentInChildren<SteamVREars>();
                if (e != null)
                {
                    _ears = e.transform;
                }
            }

            if (ears != null)
            {
                ears.GetComponent<SteamVREars>().vrcam = this;
            }
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
                    var c = components[i] as SteamVRCamera;
                    if (c != null && c != this)
                    {
                        DestroyImmediate(c);
                    }
                }

                components = GetComponents<Component>();

                if (this != components[^1])
                {
                    // Store off values to be restored on new instance
                    values = new Hashtable();
                    var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    foreach (var f in fields)
                    {
                        if (f.IsPublic || f.IsDefined(typeof(SerializeField), true))
                        {
                            values[f] = f.GetValue(this);
                        }
                    }

                    var go = gameObject;
                    DestroyImmediate(this);
                    go.AddComponent<SteamVRCamera>().ForceLast();
                }
            }
        }

        #endregion

        #region Expand / Collapse object hierarchy

        const string eyeSuffix = " (eye)";
        const string earsSuffix = " (ears)";
        const string headSuffix = " (head)";
        const string originSuffix = " (origin)";
        public string baseName { get { return name.EndsWith(eyeSuffix) ? name[..^eyeSuffix.Length] : name; } }

        // Object hierarchy creation to make it easy to parent other objects appropriately,
        // otherwise this gets called on demand at runtime. Remaining initialization is
        // performed at startup, once the hmd has been identified.
        public void Expand()
        {
            var _origin = transform.parent;
            if (_origin == null)
            {
                _origin = new GameObject(name + originSuffix).transform;
                _origin.localPosition = transform.localPosition;
                _origin.localRotation = transform.localRotation;
                _origin.localScale = transform.localScale;
                //.Msg("origin: " + _origin.name + " parent: " + _origin.parent?.name);
            }

            if (_head == null)
            {
                _head = new GameObject(name + headSuffix).transform;
                head.parent = _origin;
                head.position = transform.position;
                head.rotation = transform.rotation;
                head.localScale = Vector3.one;
                head.tag = tag;
                //MelonLogger.Msg("head: " + head.name + " parent: " + head.parent?.name);
            }

            if (transform.parent != head)
            {
                transform.parent = head;
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                transform.localScale = Vector3.one;

                while (transform.childCount > 0)
                {
                    transform.GetChild(0).parent = head;
                }

                var audioListener = GetComponent<AudioListener>();
                if (audioListener != null)
                {
                    DestroyImmediate(audioListener);
                    _ears = new GameObject(name + earsSuffix, Il2CppType.Of<SteamVREars>(), Il2CppType.Of<AudioListener>()).transform;
                    ears.parent = _head;
                    ears.localPosition = Vector3.zero;
                    ears.localRotation = Quaternion.identity;
                    ears.localScale = Vector3.one;
                    //MelonLogger.Msg("ears: " + ears.name + " parent: " + ears.parent?.name);
                }
            }

            if (!name.EndsWith(eyeSuffix))
            {
                name += eyeSuffix;
            }

            MelonLogger.Msg("[HPVR] SteamVR Camera expanded");
            instance = this;
            GameObject.DontDestroyOnLoad(this.gameObject);
            //MelonLogger.Msg("origin: " + head.parent.name +"/" + origin.name + " - head: " + transform.parent.name + "/" + (origin.GetChild(0)?.name ?? "none") + "/" + head.name + " - eye: " + (origin.GetChild(0)?.GetChild(0)?.name ?? "none") + "/" + this.name);
        }

        public void Collapse()
        {
            transform.parent = null;

            // Move children and components from head back to camera.
            while (head.childCount > 0)
            {
                head.GetChild(0).parent = transform;
            }

            if (ears != null)
            {
                while (ears.childCount > 0)
                {
                    ears.GetChild(0).parent = transform;
                }

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
                    {
                        _origin.GetChild(0).parent = _origin.parent;
                    }

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
            {
                name = name[..^eyeSuffix.Length];
            }
        }

        #endregion

        public static void DumpRenderTexture(RenderTexture rt, string pngOutPath)
        {
            var oldRT = RenderTexture.active;
            Texture2D tex = new(1920, 1080, TextureFormat.RGB24, false);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            byte[] textureBytes = ImageConversion.EncodeToPNG(tex);
            MelonLogger.Msg($"Writing texture to {pngOutPath}");
            File.WriteAllBytes(pngOutPath, textureBytes);

            RenderTexture.active = oldRT;
        }
    }
}