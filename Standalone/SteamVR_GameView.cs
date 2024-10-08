﻿using Assets.SteamVR_Melon.Standalone;
using System;
using UnityEngine;
using Valve.VR;

namespace Standalone
{
    [MelonLoader.RegisterTypeInIl2Cpp(true)]
    public class SteamVR_GameView : MonoBehaviour
    {
        public SteamVR_GameView(IntPtr value) : base(value) { }
        static Camera cam;

        private void OnEnable()
        {
            if (cam == null)
            {
                cam = GetComponent<Camera>();
                if (cam == null)
                {
                    cam = gameObject.AddComponent<Camera>();
                }
            }
            if (overlayMaterial == null)
            {
                overlayMaterial = new Material(VRShaders.GetShader(VRShaders.VRShader.overlay));
            }
            if (mirrorTexture == null)
            {
                SteamVR instance = SteamVR.instance;
                if (instance != null && instance.textureType == ETextureType.DirectX)
                {
                    Texture2D texture2D = new Texture2D(2, 2);
                    IntPtr zero = IntPtr.Zero;

                    if (instance.compositor.GetMirrorTextureD3D11(EVREye.Eye_Left, texture2D.GetNativeTexturePtr(), ref zero) == EVRCompositorError.None)
                    {
                        uint width = 0u;
                        uint height = 0u;
                        OpenVR.System.GetRecommendedRenderTargetSize(ref width, ref height);
                        mirrorTexture = Texture2D.CreateExternalTexture((int)width, (int)height, TextureFormat.RGBA32, false, false, zero);
                    }
                }
            }
        }

        private void OnPostRender()
        {
            SteamVR instance = SteamVR.instance;
            if (!cam)
            {
                cam = GetComponent<Camera>();
            }
            float num = scale * cam.aspect / instance.aspect;
            float x = -scale;
            float x2 = scale;
            float y = num;
            float y2 = -num;
            Material blitMaterial = SteamVR_Camera.blitMaterial;
            if (mirrorTexture != null)
            {
                blitMaterial.mainTexture = mirrorTexture;
            }
            else
            {
                blitMaterial.mainTexture = SteamVR_Camera.GetSceneTexture(cam.allowHDR);
            }
            blitMaterial.SetPass(0);
            GL.PushMatrix();
            GL.LoadOrtho();

            GL.Begin(7);
            GL.TexCoord2(0f, 1f);
            GL.Vertex3(x, y, 0f);
            GL.TexCoord2(1f, 1f);
            GL.Vertex3(x2, y, 0f);
            GL.TexCoord2(1f, 0f);
            GL.Vertex3(x2, y2, 0f);
            GL.TexCoord2(0f, 0f);
            GL.Vertex3(x, y2, 0f);
            GL.End();
            GL.PopMatrix();
            SteamVR_Overlay instance2 = SteamVR_Overlay.instance;
            if (instance2 && instance2.texture && overlayMaterial && drawOverlay)
            {
                Texture texture = instance2.texture;
                overlayMaterial.mainTexture = texture;
                float x3 = 0f;
                float y3 = 1f - (float)Screen.height / (float)texture.height;
                float x4 = (float)Screen.width / (float)texture.width;
                float y4 = 1f;
                GL.PushMatrix();
                GL.LoadOrtho();
                overlayMaterial.SetPass((QualitySettings.activeColorSpace == ColorSpace.Linear) ? 1 : 0);
                GL.Begin(7);
                GL.TexCoord2(x3, y3);
                GL.Vertex3(-1f, -1f, 0f);
                GL.TexCoord2(x4, y3);
                GL.Vertex3(1f, -1f, 0f);
                GL.TexCoord2(x4, y4);
                GL.Vertex3(1f, 1f, 0f);
                GL.TexCoord2(x3, y4);
                GL.Vertex3(-1f, 1f, 0f);
                GL.End();
                GL.PopMatrix();
            }
        }

        public float scale = 1.2f;

        public bool drawOverlay = true;

        private static Material overlayMaterial;

        private static Texture2D mirrorTexture;
    }
}

