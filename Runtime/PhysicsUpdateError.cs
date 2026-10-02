// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;

namespace JoltPhysicsSharp
{
    [Flags]
    public enum PhysicsUpdateError
    {
        None = 0,
        ManifoldCacheFull = 1 << 0,
        BodyPairCacheFull = 1 << 1,
        ContactConstraintsFull = 1 << 2
    }
}
