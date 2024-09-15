using MelonLoader;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SteamVR_Melon.Standalone
{
    public class OpenVRMagic
    {

        public static string DLLName = "openvr_api";

        public const int k_nRenderEventID_WaitGetPoses = 201510020;

        public const int k_nRenderEventID_SubmitL = 201510021;

        public const int k_nRenderEventID_SubmitR = 201510022;

        public const int k_nRenderEventID_Flush = 201510023;

        public const int k_nRenderEventID_PostPresentHandoff = 201510024;
    }

    /// <summary>
    /// Most of this code by @Knah https://github.com/knah/VRCMods/blob/master/TrueShaderAntiCrash/TrueShaderAntiCrashMod.cs
    /// Thank him for supporting il2cpp modding ^^
    /// </summary>

    public static class ExternalPluginFunctionExtractor
    {
        // 2019.4.1f1  : 0x786D00
        // 2019.4.21f1 : 0x792350
        // 2020.3.16f1 : 0x5b71b0 cdecl FindAndLoadPlugin
        // 2020.3.16f1 : 0x76e350 fastcall FindAndLoadPluginIl2CppWrapper

        /// <summary>
        /// Use this if you're using a different engine version. Decompile UnityPlayer.dll with IDA PRO and get the pdb files for it 
        /// </summary>
        public static void SetFindAndLoadPluginFunctionOffset(int offset)
        {
            FindAndLoadUnityPluginOffset = offset;
        }

        public static int FindAndLoadUnityPluginOffset = 0x5b71b0;

        public static void GetLoadPluginFunction()
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

                var findAndLoadPlugin = Marshal.GetDelegateForFunctionPointer<FindAndLoadUnityPlugin>(loadLibraryAddress);
                MelonLogger.Msg("[HPVR] got the delegate");

                var strPtr = Marshal.StringToHGlobalAnsi(OpenVRMagic.DLLName);
                MelonLogger.Msg($"[HPVR] callind the delegate with {strPtr:x} ({OpenVRMagic.DLLName})");

                var retName = findAndLoadPlugin(strPtr, out var loaded, 1);

                MelonLogger.Msg("[HPVR] unity loaded the plugin from: " + Marshal.PtrToStringAnsi(retName));

                if (loaded == IntPtr.Zero)
                {
                    MelonLogger.Error("[HPVR] Module load failed");
                    return;
                }
                MelonLogger.Msg("[HPVR] module loaded");

                Marshal.FreeHGlobal(strPtr);

                break;
            }
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr FindAndLoadUnityPlugin(IntPtr name, out IntPtr loadedModule, byte param3);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr CallbackPointer();

        private static CallbackPointer ourGetRenderEventFunc;

        public static IntPtr GetRenderEventFunc() => ourGetRenderEventFunc();

    }
}
