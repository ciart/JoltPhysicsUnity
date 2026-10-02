// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ObjectLayer : IEquatable<ObjectLayer>
    {
        public const int Bits = 32;
        public const uint ObjectLayerInvalid = ~0U;
        public ObjectLayer(uint value) { Value = value; }
        public uint Value { get; }
        public bool IsValid => Value != ObjectLayerInvalid;
        public bool IsInvalid => Value == ObjectLayerInvalid;
        public static ObjectLayer Invalid => new ObjectLayer(ObjectLayerInvalid);
        public static implicit operator ObjectLayer(uint value) => new ObjectLayer(value);
        public static implicit operator uint(in ObjectLayer layer) => layer.Value;
        public static bool operator ==(ObjectLayer left, ObjectLayer right) => left.Value == right.Value;
        public static bool operator !=(ObjectLayer left, ObjectLayer right) => left.Value != right.Value;
        public static bool operator ==(ObjectLayer left, uint right) => left.Value == right;
        public static bool operator !=(ObjectLayer left, uint right) => left.Value != right;
        public bool Equals(ObjectLayer other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ObjectLayer other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
