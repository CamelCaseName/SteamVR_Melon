//======= Copyright (c) Valve Corporation, All rights reserved. ===============

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Valve.VR
{
    struct SteamVREnumEqualityComparer<TEnum> : IEqualityComparer<TEnum> where TEnum : struct
    {
        static class BoxAvoidance
        {
            static readonly Func<TEnum, int> _wrapper;

            public static int ToInt(TEnum enu)
            {
                return _wrapper(enu);
            }

            static BoxAvoidance()
            {
                var p = Expression.Parameter(typeof(TEnum), null);
                var c = Expression.ConvertChecked(p, typeof(int));

                _wrapper = Expression.Lambda<Func<TEnum, int>>(c, p).Compile();
            }
        }

        public readonly bool Equals(TEnum firstEnum, TEnum secondEnum)
        {
            return BoxAvoidance.ToInt(firstEnum) == BoxAvoidance.ToInt(secondEnum);
        }

        public readonly int GetHashCode(TEnum firstEnum)
        {
            return BoxAvoidance.ToInt(firstEnum);
        }
    }

    public struct SteamVRInputSourcesComparer : IEqualityComparer<SteamVRInputSources>
    {
        public readonly bool Equals(SteamVRInputSources x, SteamVRInputSources y)
        {
            return x == y;
        }

        public readonly int GetHashCode(SteamVRInputSources obj)
        {
            return (int)obj;
        }
    }
}