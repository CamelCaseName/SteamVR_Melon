//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Simple event system for SteamVR.
//
// Example usage:
//
//			void OnDeviceConnected(int i, bool connected) { ... }
//			SteamVR_Events.DeviceConnected.Listen(OnDeviceConnected); // Usually in OnEnable
//			SteamVR_Events.DeviceConnected.Remove(OnDeviceConnected); // Usually in OnDisable
//
// Alternatively, if Listening/Removing often these can be cached as follows:
//
//			SteamVR_Event.Action deviceConnectedAction;
//			void OnAwake() { deviceConnectedAction = SteamVR_Event.DeviceConnectedAction(OnDeviceConnected); }
//			void OnEnable() { deviceConnectedAction.enabled = true; }
//			void OnDisable() { deviceConnectedAction.enabled = false; }
//
//=============================================================================

using System;
using UnityEngine;

namespace Valve.VR
{
    public static class SteamVREvents
    {

        public abstract class Action
        {
            public abstract void Enable(bool enabled);
            public bool enabled { set { Enable(value); } }
        }

        [Serializable]
        public class ActionNoArgs : Action
        {
            public ActionNoArgs(Event Event, System.Action action)
            {
                this._event = Event;
                this.action = action;
            }

            public override void Enable(bool enabled)
            {
                if (enabled)
                {
                    _event.Listen(action);
                }
                else
                {
                    _event.Remove(action);
                }
            }

            readonly Event _event;
            readonly System.Action action;
        }

        [Serializable]
        public class Action<T> : Action
        {
            public Action(Event<T> Event, System.Action<T> action)
            {
                this._event = Event;
                this.action = action;
            }

            public override void Enable(bool enabled)
            {
                if (enabled)
                {
                    _event.Listen(action);
                }
                else
                {
                    _event.Remove(action);
                }
            }

            readonly Event<T> _event;
            readonly System.Action<T> action;
        }

        [Serializable]
        public class Action<T0, T1> : Action
        {
            public Action(Event<T0, T1> Event, System.Action<T0, T1> action)
            {
                this._event = Event;
                this.action = action;
            }

            public override void Enable(bool enabled)
            {
                if (enabled)
                {
                    _event.Listen(action);
                }
                else
                {
                    _event.Remove(action);
                }
            }

            readonly Event<T0, T1> _event;
            readonly System.Action<T0, T1> action;
        }

        [Serializable]
        public class Action<T0, T1, T2> : Action
        {
            public Action(Event<T0, T1, T2> Event, System.Action<T0, T1, T2> action)
            {
                this._event = Event;
                this.action = action;
            }

            public override void Enable(bool enabled)
            {
                if (enabled)
                {
                    _event.Listen(action);
                }
                else
                {
                    _event.Remove(action);
                }
            }

            readonly Event<T0, T1, T2> _event;
            readonly System.Action<T0, T1, T2> action;
        }

        public class Event
        {
            public event System.Action OnEvent = new(() => { });

            public void Listen(System.Action action) { OnEvent += action; }
            public void Remove(System.Action action) { OnEvent -= action; }
            public void Send() { OnEvent?.Invoke(); }
            public void RemoveAllListeners()
            {
                Delegate[] subscribers = OnEvent.GetInvocationList();

                Delegate currentDelegate = OnEvent;
                for (int i = 0; i < subscribers.Length; i++)
                {
                    currentDelegate = Delegate.RemoveAll(currentDelegate, subscribers[i])!;
                }
            }
        }

        public class Event<T>
        {
            public event System.Action<T> OnEvent = new((T t0) => { });

            public void Listen(System.Action<T> action) { OnEvent += action; }
            public void Remove(System.Action<T> action) { OnEvent -= action; }
            public void Send(T arg0)
            {
                if (OnEvent != null)
                {
                    {
                        OnEvent.Invoke(arg0);
                    }
                }
            }
            public void RemoveAllListeners()
            {
                Delegate[] subscribers = OnEvent.GetInvocationList();

                Delegate currentDelegate = OnEvent;
                for (int i = 0; i < subscribers.Length; i++)
                {
                    currentDelegate = Delegate.RemoveAll(currentDelegate, subscribers[i])!;
                }
            }
        }

        public class Event<T0, T1>
        {
            public event System.Action<T0, T1> OnEvent = new((T0 t0, T1 t1) => { });

            public void Listen(System.Action<T0, T1> action) { OnEvent += action; }
            public void Remove(System.Action<T0, T1> action) { OnEvent -= action; }
            public void Send(T0 arg0, T1 arg1)
            {
                if (OnEvent != null)
                {
                    {
                        OnEvent.Invoke(arg0, arg1);
                    }
                }
            }
            public void RemoveAllListeners()
            {
                Delegate[] subscribers = OnEvent.GetInvocationList();

                Delegate currentDelegate = OnEvent;
                for (int i = 0; i < subscribers.Length; i++)
                {
                    currentDelegate = Delegate.RemoveAll(currentDelegate, subscribers[i])!;
                }
            }
        }

        public class Event<T0, T1, T2>
        {
            public event System.Action<T0, T1, T2> OnEvent = new((T0 t0, T1 t1, T2 t2) => { });

            public void Listen(System.Action<T0, T1, T2> action) { OnEvent += action; }
            public void Remove(System.Action<T0, T1, T2> action) { OnEvent -= action; }
            public void Send(T0 arg0, T1 arg1, T2 arg2)
            {
                if (OnEvent != null)
                {
                    {
                        OnEvent.Invoke(arg0, arg1, arg2);
                    }
                }
            }
            public void RemoveAllListeners()
            {
                Delegate[] subscribers = OnEvent.GetInvocationList();

                Delegate currentDelegate = OnEvent;
                for (int i = 0; i < subscribers.Length; i++)
                {
                    currentDelegate = Delegate.RemoveAll(currentDelegate, subscribers[i])!;
                }
            }
        }
        public class Event<T0, T1, T2, T3>
        {
            public event Action<T0, T1, T2, T3> OnEvent = new((T0 t0, T1 t1, T2 t2, T3 t3) => { });

            public void Listen(Action<T0, T1, T2, T3> action) { OnEvent += action; }
            public void Remove(Action<T0, T1, T2, T3> action) { OnEvent -= action; }
            public void Send(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
            {
                if (OnEvent != null)
                {
                    {
                        OnEvent.Invoke(arg0, arg1, arg2, arg3);
                    }
                }
            }
            public void RemoveAllListeners()
            {
                Delegate[] subscribers = OnEvent.GetInvocationList();

                Delegate currentDelegate = OnEvent;
                for (int i = 0; i < subscribers.Length; i++)
                {
                    currentDelegate = Delegate.RemoveAll(currentDelegate, subscribers[i])!;
                }
            }
        }

        public static Event<bool> Calibrating = new();
        public static Action CalibratingAction(System.Action<bool> action) { return new Action<bool>(Calibrating, action); }

        public static Event<int, bool> DeviceConnected = new();
        public static Action DeviceConnectedAction(System.Action<int, bool> action) { return new Action<int, bool>(DeviceConnected, action); }

        public static Event<Color, float, bool> Fade = new();
        public static Action FadeAction(System.Action<Color, float, bool> action) { return new Action<Color, float, bool>(Fade, action); }

        public static Event FadeReady = new();
        public static Action FadeReadyAction(System.Action action) { return new ActionNoArgs(FadeReady, action); }

        public static Event<bool> HideRenderModels = new();
        public static Action HideRenderModelsAction(System.Action<bool> action) { return new Action<bool>(HideRenderModels, action); }

        public static Event<bool> Initializing = new();
        public static Action InitializingAction(System.Action<bool> action) { return new Action<bool>(Initializing, action); }

        public static Event<bool> InputFocus = new();
        public static Action InputFocusAction(System.Action<bool> action) { return new Action<bool>(InputFocus, action); }

        public static Event<bool> Loading = new();
        public static Action LoadingAction(System.Action<bool> action) { return new Action<bool>(Loading, action); }

        public static Event<float> LoadingFadeIn = new();
        public static Action LoadingFadeInAction(System.Action<float> action) { return new Action<float>(LoadingFadeIn, action); }

        public static Event<float> LoadingFadeOut = new();
        public static Action LoadingFadeOutAction(System.Action<float> action) { return new Action<float>(LoadingFadeOut, action); }

        public static Event<TrackedDevicePoseT[]> NewPoses = new();
        public static Action NewPosesAction(System.Action<TrackedDevicePoseT[]> action)
        {
            return new Action<TrackedDevicePoseT[]>(NewPoses, action);
        }

        public static Event NewPosesApplied = new();
        public static Action NewPosesAppliedAction(System.Action action) { return new ActionNoArgs(NewPosesApplied, action); }

        public static Event<bool> Initialized = new();
        public static Action InitializedAction(System.Action<bool> action) { return new Action<bool>(Initialized, action); }

        public static Event<bool> OutOfRange = new();
        public static Action OutOfRangeAction(System.Action<bool> action) { return new Action<bool>(OutOfRange, action); }

        public static Event<SteamVRRenderModel, bool> RenderModelLoaded = new();
        public static Action RenderModelLoadedAction(System.Action<SteamVRRenderModel, bool> action) { return new Action<SteamVRRenderModel, bool>(RenderModelLoaded, action); }

        static readonly System.Collections.Generic.Dictionary<EVREventType, Event<VREventT>> systemEvents = new();
        public static Event<VREventT> System(EVREventType eventType)
        {
            Event<VREventT> e;
            if (!systemEvents.TryGetValue(eventType, out e))
            {
                e = new Event<VREventT>();
                systemEvents.Add(eventType, e);
            }
            return e;
        }

        public static Action SystemAction(EVREventType eventType, System.Action<VREventT> action)
        {
            return new Action<VREventT>(System(eventType), action);
        }
    }
}