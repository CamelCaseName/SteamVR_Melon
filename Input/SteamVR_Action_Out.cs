//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;

namespace Valve.VR
{
    [Serializable]
    public abstract class SteamVRActionOut<SourceMap, SourceElement> : SteamVRAction<SourceMap, SourceElement>, ISteamVRActionOut
        where SourceMap : SteamVRActionSourceMap<SourceElement>, new()
        where SourceElement : SteamVRActionOutSource, new()
    {
    }

    public abstract class SteamVRActionOutSource : SteamVRActionSource, ISteamVRActionOutSource
    {
    }

    public interface ISteamVRActionOut : ISteamVRAction, ISteamVRActionOutSource
    {
    }

    public interface ISteamVRActionOutSource : ISteamVRActionSource
    {
    }
}