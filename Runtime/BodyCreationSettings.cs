// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using UnityEngine;

namespace JoltPhysicsSharp
{
    public sealed class BodyCreationSettings : NativeObject
    {
        public ObjectLayer ObjectLayer { get; }
        public MotionType MotionType { get; }

        public BodyCreationSettings(Shape shape, in Vector3 position, in Quaternion rotation,
            MotionType motionType, ObjectLayer objectLayer)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            Validate(position, rotation, motionType, objectLayer);
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_BodyCreationSettings_Create3(shape.GetHandle(), in position, in rotation, motionType, objectLayer.Value));
                ObjectLayer = objectLayer;
                MotionType = motionType;
            }
        }

        public BodyCreationSettings(ShapeSettings shapeSettings, in Vector3 position, in Quaternion rotation,
            MotionType motionType, ObjectLayer objectLayer)
        {
            if (shapeSettings == null) throw new ArgumentNullException(nameof(shapeSettings));
            Validate(position, rotation, motionType, objectLayer);
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                // 유효하지 않은 ShapeSettings는 바디 생성의 네이티브 assert에 도달하기 전에 거부한다.
                using (Shape shape = shapeSettings.Create())
                    Initialize(JoltApi.JPH_BodyCreationSettings_Create3(shape.GetHandle(), in position, in rotation, motionType, objectLayer.Value));
                ObjectLayer = objectLayer;
                MotionType = motionType;
            }
        }

        private static void Validate(in Vector3 position, in Quaternion rotation, MotionType motionType, ObjectLayer objectLayer)
        {
            MathValidation.Finite(position, nameof(position));
            MathValidation.Rotation(rotation, nameof(rotation));
            if (motionType < MotionType.Static || motionType > MotionType.Dynamic)
                throw new ArgumentOutOfRangeException(nameof(motionType));
            if (objectLayer.IsInvalid) throw new ArgumentOutOfRangeException(nameof(objectLayer));
        }

        public float Friction
        {
            get { lock (Foundation.SyncRoot) return JoltApi.JPH_BodyCreationSettings_GetFriction(GetHandle()); }
            set
            {
                MathValidation.NonNegative(value, nameof(value));
                lock (Foundation.SyncRoot) JoltApi.JPH_BodyCreationSettings_SetFriction(GetHandle(), value);
            }
        }

        public float Restitution
        {
            get { lock (Foundation.SyncRoot) return JoltApi.JPH_BodyCreationSettings_GetRestitution(GetHandle()); }
            set
            {
                MathValidation.NonNegative(value, nameof(value));
                if (value > 1) throw new ArgumentOutOfRangeException(nameof(value));
                lock (Foundation.SyncRoot) JoltApi.JPH_BodyCreationSettings_SetRestitution(GetHandle(), value);
            }
        }

        public float GravityFactor
        {
            get { lock (Foundation.SyncRoot) return JoltApi.JPH_BodyCreationSettings_GetGravityFactor(GetHandle()); }
            set
            {
                MathValidation.Finite(value, nameof(value));
                lock (Foundation.SyncRoot) JoltApi.JPH_BodyCreationSettings_SetGravityFactor(GetHandle(), value);
            }
        }

        public bool AllowSleeping
        {
            get { lock (Foundation.SyncRoot) return JoltApi.JPH_BodyCreationSettings_GetAllowSleeping(GetHandle()); }
            set { lock (Foundation.SyncRoot) JoltApi.JPH_BodyCreationSettings_SetAllowSleeping(GetHandle(), value); }
        }

        protected override void DestroyNative(IntPtr handle) => JoltApi.JPH_BodyCreationSettings_Destroy(handle);
    }
}
