// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;

namespace JoltPhysicsSharp
{
    public abstract class ObjectLayerPairFilter : NativeObject
    {
        protected override void DestroyNative(IntPtr handle) => JoltApi.JPH_ObjectLayerPairFilter_Destroy(handle);
    }

    public abstract class BroadPhaseLayerInterface : NativeObject
    {
        protected override void DestroyNative(IntPtr handle) => JoltApi.JPH_BroadPhaseLayerInterface_Destroy(handle);
    }

    public abstract class ObjectVsBroadPhaseLayerFilter : NativeObject
    {
        protected override void DestroyNative(IntPtr handle) => JoltApi.JPH_ObjectVsBroadPhaseLayerFilter_Destroy(handle);
    }

    public sealed class ObjectLayerPairFilterTable : ObjectLayerPairFilter
    {
        public uint NumObjectLayers { get; }

        public ObjectLayerPairFilterTable(uint numObjectLayers)
        {
            if (numObjectLayers == 0 || numObjectLayers > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(numObjectLayers));
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_ObjectLayerPairFilterTable_Create(numObjectLayers));
                NumObjectLayers = numObjectLayers;
            }
        }

        internal void CheckLayer(ObjectLayer layer)
        {
            GetHandle();
            if (layer.Value >= NumObjectLayers) throw new ArgumentOutOfRangeException(nameof(layer));
        }

        public void EnableCollision(ObjectLayer layer1, ObjectLayer layer2)
        {
            lock (Foundation.SyncRoot)
            {
                RequireMutable(); CheckLayer(layer1); CheckLayer(layer2);
                JoltApi.JPH_ObjectLayerPairFilterTable_EnableCollision(GetHandle(), layer1.Value, layer2.Value);
            }
        }

        public void DisableCollision(ObjectLayer layer1, ObjectLayer layer2)
        {
            lock (Foundation.SyncRoot)
            {
                RequireMutable(); CheckLayer(layer1); CheckLayer(layer2);
                JoltApi.JPH_ObjectLayerPairFilterTable_DisableCollision(GetHandle(), layer1.Value, layer2.Value);
            }
        }

        public bool ShouldCollide(ObjectLayer layer1, ObjectLayer layer2)
        {
            lock (Foundation.SyncRoot)
            {
                CheckLayer(layer1); CheckLayer(layer2);
                return JoltApi.JPH_ObjectLayerPairFilterTable_ShouldCollide(GetHandle(), layer1.Value, layer2.Value);
            }
        }
    }

    public sealed class BroadPhaseLayerInterfaceTable : BroadPhaseLayerInterface
    {
        public uint NumObjectLayers { get; }
        public uint NumBroadPhaseLayers { get; }
        private readonly bool[] _mapped;

        public BroadPhaseLayerInterfaceTable(uint numObjectLayers, uint numBroadPhaseLayers)
        {
            if (numObjectLayers == 0 || numObjectLayers > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(numObjectLayers));
            if (numBroadPhaseLayers == 0 || numBroadPhaseLayers > 256)
                throw new ArgumentOutOfRangeException(nameof(numBroadPhaseLayers));
            _mapped = new bool[(int)numObjectLayers];
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_BroadPhaseLayerInterfaceTable_Create(numObjectLayers, numBroadPhaseLayers));
                NumObjectLayers = numObjectLayers;
                NumBroadPhaseLayers = numBroadPhaseLayers;
            }
        }

        public void MapObjectToBroadPhaseLayer(ObjectLayer objectLayer, BroadPhaseLayer broadPhaseLayer)
        {
            lock (Foundation.SyncRoot)
            {
                RequireMutable();
                if (objectLayer.Value >= NumObjectLayers) throw new ArgumentOutOfRangeException(nameof(objectLayer));
                if (broadPhaseLayer.Value >= NumBroadPhaseLayers) throw new ArgumentOutOfRangeException(nameof(broadPhaseLayer));
                JoltApi.JPH_BroadPhaseLayerInterfaceTable_MapObjectToBroadPhaseLayer(GetHandle(), objectLayer.Value, broadPhaseLayer.Value);
                _mapped[objectLayer.Value] = true;
            }
        }

        internal void RequireAllMapped()
        {
            GetHandle();
            foreach (bool mapped in _mapped)
                if (!mapped) throw new InvalidOperationException("모든 ObjectLayer를 BroadPhaseLayer에 매핑해야 합니다.");
        }
    }

    public sealed class ObjectVsBroadPhaseLayerFilterTable : ObjectVsBroadPhaseLayerFilter
    {
        internal BroadPhaseLayerInterfaceTable BroadPhase { get; }
        internal ObjectLayerPairFilterTable PairFilter { get; }

        public ObjectVsBroadPhaseLayerFilterTable(BroadPhaseLayerInterface broadPhaseLayerInterface,
            uint numBroadPhaseLayers, ObjectLayerPairFilter objectLayerPairFilter, uint numObjectLayers)
        {
            if (!(broadPhaseLayerInterface is BroadPhaseLayerInterfaceTable broadPhase))
                throw new ArgumentException("현재 구현은 테이블 기반 BroadPhase를 요구합니다.", nameof(broadPhaseLayerInterface));
            if (!(objectLayerPairFilter is ObjectLayerPairFilterTable pairFilter))
                throw new ArgumentException("현재 구현은 테이블 기반 PairFilter를 요구합니다.", nameof(objectLayerPairFilter));
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                broadPhase.RequireMutable(); pairFilter.RequireMutable(); broadPhase.RequireAllMapped();
                if (numObjectLayers != pairFilter.NumObjectLayers || numObjectLayers != broadPhase.NumObjectLayers
                    || numBroadPhaseLayers != broadPhase.NumBroadPhaseLayers)
                    throw new ArgumentException("필터와 매핑 테이블의 레이어 개수가 일치해야 합니다.");
                Initialize(JoltApi.JPH_ObjectVsBroadPhaseLayerFilterTable_Create(broadPhase.GetHandle(),
                    numBroadPhaseLayers, pairFilter.GetHandle(), numObjectLayers));
                BroadPhase = broadPhase;
                PairFilter = pairFilter;
                RetainDependency(broadPhase); RetainDependency(pairFilter);
            }
        }
    }
}
