// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#nullable enable

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#else
using System.Numerics;
#endif

using System;

namespace JoltPhysicsSharp
{

public readonly unsafe struct CollideShapeResult
{
    public readonly Vector3 ContactPointOn1;
    public readonly Vector3 ContactPointOn2;
    public readonly Vector3 PenetrationAxis;
    public readonly float PenetrationDepth;
    public readonly SubShapeID SubShapeID1;
    public readonly SubShapeID SubShapeID2;
    public readonly BodyID BodyID2;
#pragma warning disable CS0649
    internal readonly uint Shape1FaceCount;
    internal readonly Vector3* Shape1Faces;
    internal readonly uint Shape2FaceCount;
    internal readonly Vector3* Shape2Faces;
#pragma warning restore CS0649
    public Span<Vector3> Shape1Face => new(Shape1Faces, checked((int)Shape1FaceCount));
    public Span<Vector3> Shape2Face => new(Shape2Faces, checked((int)Shape2FaceCount));
}
}
