// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.
// 단정밀도 joltc C ABI. 헤더와 빌드 기준은 upstream.lock.json에 고정한다.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace JoltPhysicsSharp
{
    internal static class JoltApi
    {
        private const string LibraryName = "joltc";

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativePhysicsSystemSettings
        {
            internal uint MaxBodies;
            internal uint NumBodyMutexes;
            internal uint MaxBodyPairs;
            internal uint MaxContactConstraints;
            internal uint Padding;
            internal IntPtr BroadPhaseLayerInterface;
            internal IntPtr ObjectLayerPairFilter;
            internal IntPtr ObjectVsBroadPhaseLayerFilter;
        }

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool JPH_Init();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_Shutdown();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_JobSystemThreadPool_Create(in JobSystemThreadPoolConfig config);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_JobSystem_Destroy(IntPtr jobSystem);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_ObjectLayerPairFilterTable_Create(uint numObjectLayers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_ObjectLayerPairFilter_Destroy(IntPtr filter);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_ObjectLayerPairFilterTable_EnableCollision(IntPtr filter, uint layer1, uint layer2);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_ObjectLayerPairFilterTable_DisableCollision(IntPtr filter, uint layer1, uint layer2);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool JPH_ObjectLayerPairFilterTable_ShouldCollide(IntPtr filter, uint layer1, uint layer2);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_BroadPhaseLayerInterfaceTable_Create(uint numObjectLayers, uint numBroadPhaseLayers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BroadPhaseLayerInterface_Destroy(IntPtr broadPhase);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(IntPtr broadPhase, uint objectLayer, byte broadPhaseLayer);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_ObjectVsBroadPhaseLayerFilterTable_Create(IntPtr broadPhase, uint numBroadPhaseLayers, IntPtr pairFilter, uint numObjectLayers);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_ObjectVsBroadPhaseLayerFilter_Destroy(IntPtr filter);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_PhysicsSystem_Create(in NativePhysicsSystemSettings settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_PhysicsSystem_Destroy(IntPtr system);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_PhysicsSystem_GetBodyInterface(IntPtr system);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern uint JPH_PhysicsSystem_GetNumBodies(IntPtr system);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_PhysicsSystem_GetGravity(IntPtr system, out Vector3 result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_PhysicsSystem_SetGravity(IntPtr system, in Vector3 gravity);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_PhysicsSystem_OptimizeBroadPhase(IntPtr system);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern PhysicsUpdateError JPH_PhysicsSystem_Update(IntPtr system, float deltaTime, int collisionSteps, IntPtr jobSystem);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_ShapeSettings_Destroy(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_Shape_Destroy(IntPtr shape);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_Shape_GetCenterOfMass(IntPtr shape, out Vector3 result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_BoxShapeSettings_Create(in Vector3 halfExtent, float convexRadius);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_BoxShapeSettings_CreateShape(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_BoxShape_Create(in Vector3 halfExtent, float convexRadius);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BoxShape_GetHalfExtent(IntPtr shape, out Vector3 result);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern float JPH_BoxShape_GetConvexRadius(IntPtr shape);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_StaticCompoundShapeSettings_Create();

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_StaticCompoundShape_Create(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_CompoundShapeSettings_AddShape(IntPtr settings, in Vector3 position, in Quaternion rotation, IntPtr shapeSettings, uint userData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_CompoundShapeSettings_AddShape2(IntPtr settings, in Vector3 position, in Quaternion rotation, IntPtr shape, uint userData);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_BodyCreationSettings_Create2(IntPtr shapeSettings, in Vector3 position, in Quaternion rotation, MotionType motionType, uint objectLayer);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_BodyCreationSettings_Create3(IntPtr shape, in Vector3 position, in Quaternion rotation, MotionType motionType, uint objectLayer);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyCreationSettings_Destroy(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern float JPH_BodyCreationSettings_GetFriction(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyCreationSettings_SetFriction(IntPtr settings, float value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern float JPH_BodyCreationSettings_GetRestitution(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyCreationSettings_SetRestitution(IntPtr settings, float value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern float JPH_BodyCreationSettings_GetGravityFactor(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyCreationSettings_SetGravityFactor(IntPtr settings, float value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool JPH_BodyCreationSettings_GetAllowSleeping(IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyCreationSettings_SetAllowSleeping(IntPtr settings, [MarshalAs(UnmanagedType.U1)] bool value);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern IntPtr JPH_BodyInterface_CreateBody(IntPtr bodyInterface, IntPtr settings);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_AddBody(IntPtr bodyInterface, uint bodyID, Activation activation);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_RemoveBody(IntPtr bodyInterface, uint bodyID);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_DestroyBody(IntPtr bodyInterface, uint bodyID);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern uint JPH_Body_GetID(IntPtr body);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_GetPosition(IntPtr bodyInterface, uint bodyID, out Vector3 position);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_GetRotation(IntPtr bodyInterface, uint bodyID, out Quaternion rotation);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_GetPositionAndRotation(IntPtr bodyInterface, uint bodyID, out Vector3 position, out Quaternion rotation);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_GetCenterOfMassPosition(IntPtr bodyInterface, uint bodyID, out Vector3 position);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_SetPositionAndRotation(IntPtr bodyInterface, uint bodyID, in Vector3 position, in Quaternion rotation, Activation activation);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_MoveKinematic(IntPtr bodyInterface, uint bodyID, in Vector3 position, in Quaternion rotation, float deltaTime);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_GetLinearVelocity(IntPtr bodyInterface, uint bodyID, out Vector3 velocity);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_SetLinearVelocity(IntPtr bodyInterface, uint bodyID, in Vector3 velocity);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern MotionType JPH_BodyInterface_GetMotionType(IntPtr bodyInterface, uint bodyID);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U1)]
        internal static extern bool JPH_BodyInterface_IsActive(IntPtr bodyInterface, uint bodyID);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_ActivateBody(IntPtr bodyInterface, uint bodyID);

        [DllImport(LibraryName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        internal static extern void JPH_BodyInterface_DeactivateBody(IntPtr bodyInterface, uint bodyID);

    }
}
