// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct BroadPhaseLayer : IEquatable<BroadPhaseLayer>
    {
        public BroadPhaseLayer(byte value) { Value = value; }
        public byte Value { get; }
        public static implicit operator BroadPhaseLayer(byte value) => new BroadPhaseLayer(value);
        public static implicit operator byte(in BroadPhaseLayer layer) => layer.Value;
        public static bool operator ==(BroadPhaseLayer left, BroadPhaseLayer right) => left.Value == right.Value;
        public static bool operator !=(BroadPhaseLayer left, BroadPhaseLayer right) => left.Value != right.Value;
        public static bool operator ==(BroadPhaseLayer left, byte right) => left.Value == right;
        public static bool operator !=(BroadPhaseLayer left, byte right) => left.Value != right;
        public bool Equals(BroadPhaseLayer other) => Value == other.Value;
        public override bool Equals(object obj) => obj is BroadPhaseLayer other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }
}
