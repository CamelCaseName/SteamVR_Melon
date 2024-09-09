using MelonLoader;
using UnityEngine;

namespace Assets.SteamVR_Melon.Standalone
{
    public static class VRShaders
    {
        public enum VRShader
        {
            blit,
            blitFlip,
            overlay,
            occlusion,
            fade
        }

        static AssetBundle assetBundle;

        static Shader blit;
        static Shader blitFlip;
        static Shader overlay;
        static Shader occlusion;
        static Shader fade;

        public static Shader GetShader(VRShader shader)
        {
            if (blit == null)
            {
                TryLoadShaders();
            }

            switch (shader)
            {
                case VRShader.blit:
                    return blit;
                case VRShader.blitFlip:
                    return blitFlip;
                case VRShader.overlay:
                    return overlay;
                case VRShader.occlusion:
                    return occlusion;
                case VRShader.fade:
                    return fade;
            }
            MelonLogger.Warning("[HPVR] No valid shader found");
            return null;
        }

        public static void TryLoadShaders()
        {
            if (assetBundle == null)
            {
                MelonLogger.Msg($"[HPVR] loading assetbundle from {Application.streamingAssetsPath}/vrshaders");
                assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/vrshaders");
                if (assetBundle == null)
                {
                    MelonLogger.Error("[HPVR] No assetbundle present!");
                    return;
                }
            }
            MelonLogger.Msg("[HPVR] Loading shaders from asset bundle...");

            occlusion = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_hiddenarea.shader").Cast<Shader>();
            blit = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_blit.shader").Cast<Shader>();
            blitFlip = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_blitFlip.shader").Cast<Shader>();
            overlay = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_overlay.shader").Cast<Shader>();
            fade = assetBundle.LoadAsset("assets/steamvr/resources/steamvr_fade.shader").Cast<Shader>();
            string[] allAssetNames = assetBundle.GetAllAssetNames();
            for (int i = 0; i < allAssetNames.Length; i++)
            {
                MelonLogger.Msg("[HPVR] " + allAssetNames[i]);
            }
        }
    }
}
