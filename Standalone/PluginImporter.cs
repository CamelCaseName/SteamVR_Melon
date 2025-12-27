using MelonLoader;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SteamVR_Melon.Standalone
{
    public class OpenVRMagic
    {

        public const string openvrApi = "openvr_api";
        public const string XRSDKOpenVR = "XRSDKOpenVR";

        public const int kNRenderEventIDWaitGetPoses = 201510020;

        public const int kNRenderEventIDSubmitL = 201510021;

        public const int kNRenderEventIDSubmitR = 201510022;

        public const int kNRenderEventIDFlush = 201510023;

        public const int kNRenderEventIDPostPresentHandoff = 201510024;
    }

    /// <summary>
    /// Most of this code by @Knah https://github.com/knah/VRCMods/blob/master/TrueShaderAntiCrash/TrueShaderAntiCrashMod.cs
    /// Thank him for supporting il2cpp modding ^^
    /// </summary>

    public static class PluginImporter
    {
        // need to use cdecl, fastcall is not supported by c#
        // 2019.4.1f1  : 0x786D00 FindAndLoadUnityPlugin
        // 2019.4.21f1 : 0x792350 FindAndLoadUnityPlugin
        // 2022.3.16f1 : 0x5b71b0 cdecl FindAndLoadUnityPlugin
        // 2022.3.16f1 : 0x76e350 fastcall FindAndLoadUnityPluginIl2CppWrapper
        // 2022.3.62f2 : 0x5c4210 cdecl FindAndLoadUnityPlugin
        // 2022.3.62f2 : 0x77a4a0 unknown FindAndLoadUnityPluginIl2CppWrapper

        private static int FindAndLoadUnityPluginOffset = 0x5c4210;

        public static void UpdateOffsetForUnityVersion()
        {
            string version = Application.unityVersion;
            MelonLogger.Msg(version);
            switch (version)
            {
                case "2019.4.1f1":
                    FindAndLoadUnityPluginOffset = 0x786D00;
                    break;
                case "2019.4.21f1":
                    FindAndLoadUnityPluginOffset = 0x792350;
                    break;
                //house party < 1.4.2
                case "2022.3.16f1":
                    FindAndLoadUnityPluginOffset = 0x5b71b0;
                    break;
                //house party 1.4.2
                case "2022.3.62f2":
                    FindAndLoadUnityPluginOffset = 0x5c4210;
                    break;
                //office party
                case "6000.2.9f1":
                    FindAndLoadUnityPluginOffset = 0x5b71b0;
                    break;
            }
        }

        public static void GetPluginLoadFunction()
        {
            MelonLogger.Msg("[HPVR] Loading external plugin load function");
            var process = Process.GetCurrentProcess();
            foreach (ProcessModule module in process.Modules)
            {
                MelonLogger.Msg("[HPVR] " + module.FileName);
                if (!module.FileName.Contains("UnityPlayer"))
                {
                    continue;
                }

                MelonLogger.Msg("[HPVR] Found the unityplayer module");

                var loadLibraryAddress = module.BaseAddress + FindAndLoadUnityPluginOffset;
                MelonLogger.Msg($"[HPVR] loadLibrary Address: {loadLibraryAddress:x} (offset {FindAndLoadUnityPluginOffset:x})");

                method = Marshal.GetDelegateForFunctionPointer<FindAndLoadUnityPlugin>(loadLibraryAddress);
                MelonLogger.Msg("[HPVR] got the delegate");

                break;
            }
        }

        public static void LoadPlugin(string name)
        {
            if (method is null)
            {
                GetPluginLoadFunction();
            }

            var name_ = name;
            var strPtr = Marshal.StringToHGlobalAnsi(name);
            MelonLogger.Msg($"loading plugin {strPtr:x} ({name})");

            var retName = method(strPtr, out var loaded, 1);

            MelonLogger.Msg("unity loaded the plugin from: " + Marshal.PtrToStringAnsi(retName));

            if (loaded == IntPtr.Zero)
            {
                MelonLogger.Error("Module load failed");
                return;
            }
            MelonLogger.Msg("plugin loaded");

            Marshal.FreeHGlobal(strPtr);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr FindAndLoadUnityPlugin(IntPtr name, out IntPtr loadedModule, byte param3);
        private static FindAndLoadUnityPlugin method;
    }
}
