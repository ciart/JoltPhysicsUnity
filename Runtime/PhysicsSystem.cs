// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace JoltPhysicsSharp
{
    public struct PhysicsSystemSettings
    {
        public int MaxBodies { get; set; }
        public int NumBodyMutexes { get; set; }
        public int MaxBodyPairs { get; set; }
        public int MaxContactConstraints { get; set; }
        public ObjectLayerPairFilter ObjectLayerPairFilter { get; set; }
        public BroadPhaseLayerInterface BroadPhaseLayerInterface { get; set; }
        public ObjectVsBroadPhaseLayerFilter ObjectVsBroadPhaseLayerFilter { get; set; }

        // C# 9의 default(struct)는 0으로 초기화한다. 0인 용량은 joltc 기본값을 사용한다.
        public static PhysicsSystemSettings Default => new PhysicsSystemSettings
        {
            MaxBodies = 10240, MaxBodyPairs = 65536, MaxContactConstraints = 10240
        };
    }

    public sealed class PhysicsSystem : NativeObject
    {
        private readonly ObjectLayerPairFilterTable _pairFilter;
        private readonly BroadPhaseLayerInterfaceTable _broadPhase;
        private readonly ObjectVsBroadPhaseLayerFilterTable _objectVsBroadPhase;
        private readonly Dictionary<uint, IntPtr> _bodies = new Dictionary<uint, IntPtr>();
        private readonly HashSet<uint> _addedBodies = new HashSet<uint>();
        private readonly IntPtr _bodyInterface;

        public PhysicsSystem(PhysicsSystemSettings settings)
        {
            if (settings.MaxBodies < 0 || settings.MaxBodies > BodyID.MaxBodyIndex || settings.NumBodyMutexes < 0
                || settings.MaxBodyPairs < 0 || settings.MaxContactConstraints < 0)
                throw new ArgumentOutOfRangeException(nameof(settings));
            if (!(settings.ObjectLayerPairFilter is ObjectLayerPairFilterTable pairFilter)
                || !(settings.BroadPhaseLayerInterface is BroadPhaseLayerInterfaceTable broadPhase)
                || !(settings.ObjectVsBroadPhaseLayerFilter is ObjectVsBroadPhaseLayerFilterTable objectVsBroadPhase))
                throw new ArgumentException("세 가지 테이블 기반 충돌 필터를 설정해야 합니다.", nameof(settings));

            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                pairFilter.RequireOwned(); broadPhase.RequireOwned(); objectVsBroadPhase.RequireOwned();
                if (objectVsBroadPhase.PairFilter != pairFilter || objectVsBroadPhase.BroadPhase != broadPhase)
                    throw new ArgumentException("동일한 PairFilter와 BroadPhase로 만든 필터를 사용해야 합니다.", nameof(settings));
                broadPhase.RequireAllMapped();
                var nativeSettings = new JoltApi.NativePhysicsSystemSettings
                {
                    MaxBodies = (uint)settings.MaxBodies,
                    NumBodyMutexes = (uint)settings.NumBodyMutexes,
                    MaxBodyPairs = (uint)settings.MaxBodyPairs,
                    MaxContactConstraints = (uint)settings.MaxContactConstraints,
                    BroadPhaseLayerInterface = broadPhase.GetHandle(),
                    ObjectLayerPairFilter = pairFilter.GetHandle(),
                    ObjectVsBroadPhaseLayerFilter = objectVsBroadPhase.GetHandle()
                };
                Initialize(JoltApi.JPH_PhysicsSystem_Create(in nativeSettings));
                _pairFilter = pairFilter; _broadPhase = broadPhase; _objectVsBroadPhase = objectVsBroadPhase;
                // joltc의 PhysicsSystem_Destroy가 필터를 delete하므로 공유하지 않는다.
                objectVsBroadPhase.TransferTo(this); broadPhase.TransferTo(this); pairFilter.TransferTo(this);
                _bodyInterface = JoltApi.JPH_PhysicsSystem_GetBodyInterface(GetHandle());
            }
        }

        public BodyInterface BodyInterface
        {
            get { lock (Foundation.SyncRoot) { GetHandle(); return new BodyInterface(this); } }
        }

        internal IntPtr GetBodyInterfaceHandle()
        {
            GetHandle();
            return _bodyInterface;
        }

        internal IntPtr RequireBody(BodyID id)
        {
            GetHandle();
            if (!_bodies.TryGetValue(id.ID, out IntPtr pointer))
                throw new ArgumentException("이 월드에 존재하는 바디 ID가 아닙니다. 파괴한 ID는 재사용할 수 없습니다.", nameof(id));
            return pointer;
        }

        internal bool IsAdded(BodyID id) { RequireBody(id); return _addedBodies.Contains(id.ID); }
        internal void MarkAdded(BodyID id) => _addedBodies.Add(id.ID);
        internal void MarkRemoved(BodyID id) => _addedBodies.Remove(id.ID);
        internal void ForgetBody(BodyID id) { _addedBodies.Remove(id.ID); _bodies.Remove(id.ID); }

        internal void CheckSettings(BodyCreationSettings settings)
        {
            GetHandle();
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            settings.GetHandle();
            _pairFilter.CheckLayer(settings.ObjectLayer);
        }

        internal BodyID TrackBody(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero) throw new InvalidOperationException("바디 생성에 실패했습니다. 월드 용량과 형상을 확인하세요.");
            uint id = JoltApi.JPH_Body_GetID(pointer);
            _bodies.Add(id, pointer);
            return new BodyID(id);
        }

        public uint BodiesCount
        {
            get { lock (Foundation.SyncRoot) return JoltApi.JPH_PhysicsSystem_GetNumBodies(GetHandle()); }
        }

        public Vector3 Gravity
        {
            get
            {
                lock (Foundation.SyncRoot)
                {
                    JoltApi.JPH_PhysicsSystem_GetGravity(GetHandle(), out Vector3 result);
                    return result;
                }
            }
            set
            {
                MathValidation.Finite(value, nameof(value));
                lock (Foundation.SyncRoot) JoltApi.JPH_PhysicsSystem_SetGravity(GetHandle(), in value);
            }
        }

        public void SetGravity(in Vector3 gravity) => Gravity = gravity;
        public Vector3 GetGravity() => Gravity;

        public void OptimizeBroadPhase()
        {
            lock (Foundation.SyncRoot) JoltApi.JPH_PhysicsSystem_OptimizeBroadPhase(GetHandle());
        }

        public PhysicsUpdateError Update(float deltaTime, int collisionSteps, JobSystem jobSystem)
        {
            MathValidation.Finite(deltaTime, nameof(deltaTime));
            if (deltaTime <= 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            if (collisionSteps <= 0) throw new ArgumentOutOfRangeException(nameof(collisionSteps));
            if (jobSystem == null) throw new ArgumentNullException(nameof(jobSystem));
            lock (Foundation.SyncRoot)
                return JoltApi.JPH_PhysicsSystem_Update(GetHandle(), deltaTime, collisionSteps, jobSystem.GetHandle());
        }

        protected override void DestroyNative(IntPtr handle)
        {
            foreach (KeyValuePair<uint, IntPtr> body in _bodies)
            {
                if (_addedBodies.Contains(body.Key)) JoltApi.JPH_BodyInterface_RemoveBody(_bodyInterface, body.Key);
                JoltApi.JPH_BodyInterface_DestroyBody(_bodyInterface, body.Key);
            }
            _bodies.Clear(); _addedBodies.Clear();
            JoltApi.JPH_PhysicsSystem_Destroy(handle);
            _objectVsBroadPhase.DestroyedByOwner(this);
            _broadPhase.DestroyedByOwner(this);
            _pairFilter.DestroyedByOwner(this);
        }
    }
}
