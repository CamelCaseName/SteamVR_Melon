using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using MelonLoader;
using System;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace SteamVR_Melon.Util
{
    /// <summary>
    /// Hook into BeforeRenderHelper in your plugin to make these events work!
    /// </summary>
    public static class UnityHooks
    {
        public static Action PreUpdate;
        public static Action EarlyUpdate;
        public static Action OnPreCull;

        public static void Init()
        {
            //RenderPipelineManager.add_beginCameraRendering(new System.Action<ScriptableRenderContext, Camera>(OnPreRender));
            var system = PlayerLoop.GetCurrentPlayerLoop();
            AddLoopSystem<UnityEngine.PlayerLoop.PreUpdate>(ref system, () => PreUpdate?.Invoke());
            AddLoopSystem<UnityEngine.PlayerLoop.EarlyUpdate>(ref system, () => EarlyUpdate?.Invoke());
            AddLoopSystem<UnityEngine.PlayerLoop.PostLateUpdate>(ref system, () => OnPreCull?.Invoke());

            PlayerLoop.SetPlayerLoop(system);

            MelonLogger.Msg("Initialized Unity Hooks");
        }

        private static void AddLoopSystem<T>(ref PlayerLoopSystem system, Action action) where T : struct
        {
            for (int i = 0; i < system.subSystemList.Count; i++)
            {
                PlayerLoopSystem item = system.subSystemList[i];
                //MelonLogger.Msg($"{item.type?.Name ?? "none"} {(item.loopConditionFunction == null ? IntPtr.Zero : item.loopConditionFunction):x} {item.updateDelegate?.method_info?.Name ?? "none"} {(item.updateFunction == null ? IntPtr.Zero : item.updateFunction):x}");

                if (item.type != Il2CppType.Of<T>())
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

                PlayerLoopSystem UnityHookSystem = new()
                {
                    type = Il2CppType.Of<UnityHook>(),
                    updateDelegate = action,
                    subSystemList = Array.Empty<PlayerLoopSystem>()
                };

                //MelonLogger.Msg(item.subSystemList.Count + 1);
                var list = new PlayerLoopSystem[item.subSystemList.Count + 1];

                for (int k = 0; k < item.subSystemList.Count; k++)
                {
                    //MelonLogger.Msg(item.subSystemList[k].GetIl2CppType().ToString());
                    list[k] = item.subSystemList[k];
                }
                list[^1] = UnityHookSystem;

                item.subSystemList = list;
                system.subSystemList[i] = item;
                break;
            }

            return;
        }

        [RegisterTypeInIl2Cpp(true)]
        public class UnityHook : Il2CppSystem.Object
        {
            public UnityHook(IntPtr nativePtr) : base(nativePtr) { }
        }
    }
}