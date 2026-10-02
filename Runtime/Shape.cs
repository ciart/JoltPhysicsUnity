// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using UnityEngine;

namespace JoltPhysicsSharp
{
    public abstract class ShapeSettings : NativeObject
    {
        protected override void DestroyNative(IntPtr handle) => JoltApi.JPH_ShapeSettings_Destroy(handle);
        public abstract Shape Create();
    }

    public abstract class Shape : NativeObject
    {
        protected override void DestroyNative(IntPtr handle) => JoltApi.JPH_Shape_Destroy(handle);

        public Vector3 CenterOfMass
        {
            get
            {
                lock (Foundation.SyncRoot)
                {
                    JoltApi.JPH_Shape_GetCenterOfMass(GetHandle(), out Vector3 result);
                    return result;
                }
            }
        }
    }

    public abstract class ConvexShape : Shape { }
    public abstract class ConvexShapeSettings : ShapeSettings { }

    public sealed class BoxShapeSettings : ConvexShapeSettings
    {
        public BoxShapeSettings(in Vector3 halfExtent, float convexRadius = Foundation.DefaultConvexRadius)
        {
            BoxShape.Validate(halfExtent, convexRadius);
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_BoxShapeSettings_Create(in halfExtent, convexRadius));
            }
        }

        public override Shape Create() => new BoxShape(this);
    }

    public sealed class BoxShape : ConvexShape
    {
        public BoxShape(in Vector3 halfExtent, float convexRadius = Foundation.DefaultConvexRadius)
        {
            Validate(halfExtent, convexRadius);
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_BoxShape_Create(in halfExtent, convexRadius));
            }
        }

        public BoxShape(BoxShapeSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_BoxShapeSettings_CreateShape(settings.GetHandle()));
            }
        }

        internal static void Validate(in Vector3 halfExtent, float convexRadius)
        {
            MathValidation.Finite(halfExtent, nameof(halfExtent));
            MathValidation.NonNegative(convexRadius, nameof(convexRadius));
            if (halfExtent.x <= 0 || halfExtent.y <= 0 || halfExtent.z <= 0)
                throw new ArgumentOutOfRangeException(nameof(halfExtent));
            if (convexRadius > Math.Min(halfExtent.x, Math.Min(halfExtent.y, halfExtent.z)))
                throw new ArgumentOutOfRangeException(nameof(convexRadius), "가장 작은 half extent보다 클 수 없습니다.");
        }

        public Vector3 HalfExtent
        {
            get { GetHalfExtent(out Vector3 result); return result; }
        }

        public void GetHalfExtent(out Vector3 halfExtent)
        {
            lock (Foundation.SyncRoot) JoltApi.JPH_BoxShape_GetHalfExtent(GetHandle(), out halfExtent);
        }

        public float ConvexRadius
        {
            get { lock (Foundation.SyncRoot) return JoltApi.JPH_BoxShape_GetConvexRadius(GetHandle()); }
        }
    }

    public abstract class CompoundShapeShapeSettings : ShapeSettings
    {
        protected uint ShapeCount;
        private bool _created;

        public void AddShape(in Vector3 position, in Quaternion rotation, Shape shape, uint userData = 0)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            MathValidation.Finite(position, nameof(position));
            MathValidation.Rotation(rotation, nameof(rotation));
            lock (Foundation.SyncRoot)
            {
                RequireEditable();
                JoltApi.JPH_CompoundShapeSettings_AddShape2(GetHandle(), in position, in rotation, shape.GetHandle(), userData);
                ShapeCount++;
            }
        }

        public void AddShape(in Vector3 position, in Quaternion rotation, ShapeSettings shapeSettings, uint userData = 0)
        {
            if (shapeSettings == null) throw new ArgumentNullException(nameof(shapeSettings));
            MathValidation.Finite(position, nameof(position));
            MathValidation.Rotation(rotation, nameof(rotation));
            lock (Foundation.SyncRoot)
            {
                RequireEditable();
                using (Shape shape = shapeSettings.Create())
                    JoltApi.JPH_CompoundShapeSettings_AddShape2(GetHandle(), in position, in rotation, shape.GetHandle(), userData);
                ShapeCount++;
            }
        }

        internal void RequireShapes()
        {
            GetHandle();
            if (ShapeCount == 0) throw new InvalidOperationException("Compound에 하나 이상의 형상을 추가해야 합니다.");
        }

        private void RequireEditable()
        {
            GetHandle();
            if (_created) throw new InvalidOperationException("형상을 생성한 Compound 설정은 변경할 수 없습니다. 새 설정을 만드세요.");
        }

        internal void MarkCreated() { _created = true; }
    }

    public abstract class CompoundShape : Shape { }

    public sealed class StaticCompoundShapeSettings : CompoundShapeShapeSettings
    {
        public StaticCompoundShapeSettings()
        {
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_StaticCompoundShapeSettings_Create());
            }
        }

        public override Shape Create() => new StaticCompoundShape(this);
    }

    public sealed class StaticCompoundShape : CompoundShape
    {
        public StaticCompoundShape(StaticCompoundShapeSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                settings.RequireShapes();
                Initialize(JoltApi.JPH_StaticCompoundShape_Create(settings.GetHandle()));
                settings.MarkCreated();
            }
        }
    }
}
