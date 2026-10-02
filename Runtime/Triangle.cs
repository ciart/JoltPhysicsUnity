// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace JoltPhysicsSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Triangle : IEquatable<Triangle>
    {
        public Triangle(in Vector3 v1, in Vector3 v2, in Vector3 v3, uint materialIndex = 0)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            MaterialIndex = materialIndex;
        }

        public Vector3 V1 { get; }
        public Vector3 V2 { get; }
        public Vector3 V3 { get; }
        public uint MaterialIndex { get; }

        public static bool operator ==(Triangle left, Triangle right)
        {
            // Unity Vector3의 ==는 근사 비교이므로 원본의 정확한 비교 의미를 유지한다.
            return left.V1.Equals(right.V1)
                && left.V2.Equals(right.V2)
                && left.V3.Equals(right.V3)
                && left.MaterialIndex == right.MaterialIndex;
        }

        public static bool operator !=(Triangle left, Triangle right) => !(left == right);
        public bool Equals(Triangle other) => this == other;
        public override bool Equals(object obj) => obj is Triangle other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(V1, V2, V3, MaterialIndex);
        public override string ToString() => $"V1: {V1}, V2: {V2}, V3: {V3}, MaterialIndex: {MaterialIndex}";
    }
}
