// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct BodyID : IEquatable<BodyID>
    {
        public const uint InvalidBodyID = 0xffffffff;
        public const uint BroadPhaseBit = 0x00800000;
        public const uint MaxBodyIndex = 0x7fffff;
        public const byte MaxSequenceNumber = 0xff;

        public BodyID(uint id)
        {
            ID = id;
        }

        public uint ID { get; }
        public bool IsValid => ID != InvalidBodyID;
        public bool IsInvalid => ID == InvalidBodyID;
        public uint Index => ID & MaxBodyIndex;
        public byte SequenceNumber => (byte)(ID >> 24);
        public uint IndexAndSequenceNumber => ID;

        public static BodyID Invalid => new BodyID(InvalidBodyID);
        public static implicit operator BodyID(uint id) => new BodyID(id);
        public static implicit operator uint(in BodyID id) => id.IndexAndSequenceNumber;

        public static bool operator ==(BodyID left, BodyID right) => left.ID == right.ID;
        public static bool operator !=(BodyID left, BodyID right) => left.ID != right.ID;
        public static bool operator ==(BodyID left, uint right) => left.ID == right;
        public static bool operator !=(BodyID left, uint right) => left.ID != right;

        public bool Equals(BodyID other) => ID == other.ID;
        public override bool Equals(object obj) => obj is BodyID other && Equals(other);
        public override int GetHashCode() => ID.GetHashCode();
        public override string ToString() => ID.ToString();
    }
}
