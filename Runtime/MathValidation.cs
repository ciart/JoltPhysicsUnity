using System;
using UnityEngine;

namespace JoltPhysicsSharp
{
    internal static class MathValidation
    {
        internal static void Finite(float value, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(name, "유한한 값이어야 합니다.");
        }

        internal static void Finite(in Vector3 value, string name)
        {
            Finite(value.x, name); Finite(value.y, name); Finite(value.z, name);
        }

        internal static void NonNegative(float value, string name)
        {
            Finite(value, name);
            if (value < 0) throw new ArgumentOutOfRangeException(name);
        }

        internal static void Rotation(in Quaternion value, string name)
        {
            Finite(value.x, name); Finite(value.y, name); Finite(value.z, name); Finite(value.w, name);
            float lengthSquared = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
            if (Math.Abs(lengthSquared - 1.0f) > 1.0e-4f)
                throw new ArgumentException("정규화된 Quaternion을 전달하세요.", name);
        }

        internal static void Activation(Activation activation)
        {
            if (activation != JoltPhysicsSharp.Activation.Activate && activation != JoltPhysicsSharp.Activation.DontActivate)
                throw new ArgumentOutOfRangeException(nameof(activation));
        }
    }
}
