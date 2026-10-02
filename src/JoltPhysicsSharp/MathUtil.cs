// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#nullable enable

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#else
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

using System;

namespace JoltPhysicsSharp
{

public static class MathUtil
{
    /// <summary>
    /// Converts degrees to radians.
    /// </summary>
    /// <param name="degree">Converts degrees to radians.</param>
    /// <returns>The converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float DegreesToRadians(float degree) => degree * (MathF.PI / 180.0f);

    /// <summary>
    /// Converts radians to degrees.
    /// </summary>
    /// <param name="radians">The angle in radians.</param>
    /// <returns>The converted value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float RadiansToDegrees(float radians) => radians * (180.0f / MathF.PI);

    public static Vector3 GetNormalizedPerpendicular(this Vector3 vector)
    {
#if UNITY_5_3_OR_NEWER
        if (MathF.Abs(vector.x) > MathF.Abs(vector.y))
        {
            float len = MathF.Sqrt(vector.x * vector.x + vector.z * vector.z);
            return new(vector.z / len, 0.0f, -vector.x / len);
        }
        else
        {
            float len = MathF.Sqrt(vector.y * vector.y + vector.z * vector.z);
            return new(0.0f, vector.z / len, -vector.y / len);
        }
#else
        if (MathF.Abs(vector.X) > MathF.Abs(vector.Y))
        {
            float len = MathF.Sqrt(vector.X * vector.X + vector.Z * vector.Z);
            return new(vector.Z / len, 0.0f, -vector.X / len);
        }
        else
        {
            float len = MathF.Sqrt(vector.Y * vector.Y + vector.Z * vector.Z);
            return new(0.0f, vector.Z / len, -vector.Y / len);
        }
#endif
    }
}
}
