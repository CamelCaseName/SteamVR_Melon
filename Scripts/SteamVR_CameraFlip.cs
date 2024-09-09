using Assets.SteamVR_Melon.Standalone;
using System;
using UnityEngine;

namespace Valve.VR
{

    public class SteamVR_CameraFlip : MonoBehaviour
    {
        public SteamVR_CameraFlip(IntPtr value)
: base(value) { }
        private void OnEnable()
        {
            if (blitMaterial == null)
            {
                blitMaterial = new Material(VRShaders.GetShader(VRShaders.VRShader.blitFlip));
            }
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, blitMaterial);
            if (SteamVR_Camera.doomp)
            {
                SteamVR_Camera.DumpRenderTexture(src, Application.streamingAssetsPath + "/CameraFlip_OnRenderImage_src.png");
            }
            
            if (SteamVR_Camera.doomp)
            {
                MelonLoader.MelonLogger.Msg(Time.frameCount.ToString() + "/CameraFlip_OnRenderImage");
                SteamVR_Camera.DumpRenderTexture(dest, Application.streamingAssetsPath + "/CameraFlip_OnRenderImage_dst.png");
            }
        }

        public static Material blitMaterial;
    }
}
