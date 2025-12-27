//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Valve.VR
{
    public static class SteamVRInputSource
    {
        public static int numSources = System.Enum.GetValues(typeof(SteamVRInputSources)).Length;

        private static ulong[] inputSourceHandlesBySource;
        private static Dictionary<ulong, SteamVRInputSources> inputSourceSourcesByHandle = new();

        private static readonly Type enumType = typeof(SteamVRInputSources);
        private static readonly Type descriptionType = typeof(DescriptionAttribute);

        private static SteamVRInputSources[] allSources;

        public static ulong GetHandle(SteamVRInputSources inputSource)
        {
            int index = (int)inputSource;
            if (index < inputSourceHandlesBySource.Length)
            {
                return inputSourceHandlesBySource[index];
            }

            return 0;
        }
        public static SteamVRInputSources GetSource(ulong handle)
        {
            if (inputSourceSourcesByHandle.ContainsKey(handle))
            {
                return inputSourceSourcesByHandle[handle];
            }

            return SteamVRInputSources.Any;
        }

        public static SteamVRInputSources[] GetAllSources()
        {
            if (allSources == null)
            {
                allSources = (SteamVRInputSources[])System.Enum.GetValues(typeof(SteamVRInputSources));
            }

            return allSources;
        }

        private static string GetPath(string inputSourceEnumName)
        {
            //MelonLoader.MelonLogger.Msg("[][][][] " + inputSourceEnumName);
            return ((DescriptionAttribute)enumType.GetMember(inputSourceEnumName)[0]
                                                  .GetCustomAttributes(descriptionType, false)[0]).Description;
        }

        public static void Initialize()
        {
            List<SteamVRInputSources> allSourcesList = new();
            string[] enumNames = System.Enum.GetNames(enumType);
            inputSourceHandlesBySource = new ulong[enumNames.Length];
            inputSourceSourcesByHandle = new Dictionary<ulong, SteamVRInputSources>();

            for (int enumIndex = 0; enumIndex < enumNames.Length; enumIndex++)
            {
                string path = GetPath(enumNames[enumIndex]);

                ulong handle = 0;
                EVRInputError err = OpenVR.Input.GetInputSourceHandle(path, ref handle);

                if (err != EVRInputError.None)
                {
                    MelonLoader.MelonLogger.Error("[HPVR] GetInputSourceHandle (" + path + ") error: " + err.ToString());
                }

                if (enumNames[enumIndex] == SteamVRInputSources.Any.ToString())
                {
                    inputSourceHandlesBySource[enumIndex] = 0;
                    inputSourceSourcesByHandle.Add(0, (SteamVRInputSources)enumIndex);
                }
                else
                {
                    inputSourceHandlesBySource[enumIndex] = handle;
                    inputSourceSourcesByHandle.Add(handle, (SteamVRInputSources)enumIndex);
                }

                allSourcesList.Add((SteamVRInputSources)enumIndex);
            }

            allSources = allSourcesList.ToArray();
        }
    }
}