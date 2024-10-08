﻿using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Globalization;
using MelonLoader;
using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace SteamVR_Melon.Util
{
    /// <summary>
    /// Hook into BeforeRenderHelper in your plugin to make these events work!
    /// </summary>
    public static class UnityHooks
    {
        public static event Action OnBeforeRender;

        public static void InvokeOnBeforeRender()
        {
            OnBeforeRender?.Invoke();
            //MelonLogger.Msg("prerender");
        }

        public static void Init()
        {
            //RenderPipelineManager.add_beginCameraRendering(new System.Action<ScriptableRenderContext, Camera>(OnPreRender));
            var system = PlayerLoop.GetCurrentPlayerLoop();

            for (int i = 0; i < system.subSystemList.Count; i++)
            {
                PlayerLoopSystem item = system.subSystemList[i];
                //MelonLogger.Msg($"{item.type?.Name ?? "none"} {(item.loopConditionFunction == null ? IntPtr.Zero : item.loopConditionFunction):x} {item.updateDelegate?.method_info?.Name ?? "none"} {(item.updateFunction == null ? IntPtr.Zero : item.updateFunction):x}");

                if (item.type != Il2CppType.Of<EarlyUpdate>())
                {
                    continue;
                }

                foreach (var item2 in item.subSystemList)
                {
                    if (item2.type == Il2CppType.Of<UnityHook>())
                    {
                        return;
                    }
                }

                PlayerLoopSystem UnityHookSystem = new PlayerLoopSystem()
                {
                    type = Il2CppType.Of<UnityHook>(),
                    updateDelegate = new Action(OnPreRender),
                    subSystemList = new PlayerLoopSystem[0]
                };

                //MelonLogger.Msg(item.subSystemList.Count + 1);
                var list = new PlayerLoopSystem[item.subSystemList.Count + 1];

                for (int k = 0; k < item.subSystemList.Count; k++)
                {
                    list[k] = item.subSystemList[k];
                }
                list[^1] = UnityHookSystem;

                item.subSystemList = list;
                system.subSystemList[i] = item;
                break;
            }

            PlayerLoop.SetPlayerLoop(system);

            MelonLogger.Msg("Initialized Unity Hooks");
        }

        //private static void OnPreRender(ScriptableRenderContext ctx, Camera cam)
        //{
        //    if (OnPreRenderCam == null || !OnPreRenderCam.enabled)
        //        OnPreRenderCam = cam;
        //    if (OnPreRenderCam == cam)
        //        InvokeOnBeforeRender();
        //}
        private static void OnPreRender()
        {
            InvokeOnBeforeRender();
        }

        //private static Camera OnPreRenderCam = null;


        [RegisterTypeInIl2Cpp(true)]
        public class UnityHook : Il2CppSystem.Object
        {
            public UnityHook(IntPtr nativePtr) : base(nativePtr) { }
        }
    }
}