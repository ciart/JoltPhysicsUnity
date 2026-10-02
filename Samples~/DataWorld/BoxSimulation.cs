using System;
using UnityEngine;

namespace JoltPhysicsSharp.Samples
{
    public readonly struct SimulationResult
    {
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public int Steps { get; }
        public Vector3 CompoundPosition { get; }
        public Quaternion CompoundRotation { get; }
        public Vector3 CompoundCenterOfMass { get; }
        public SimulationResult(Vector3 position, Quaternion rotation, int steps)
            : this(position, rotation, steps, Vector3.zero, Quaternion.identity, Vector3.zero) { }

        public SimulationResult(Vector3 position, Quaternion rotation, int steps,
            Vector3 compoundPosition, Quaternion compoundRotation, Vector3 compoundCenterOfMass)
        {
            Position = position; Rotation = rotation; Steps = steps;
            CompoundPosition = compoundPosition; CompoundRotation = compoundRotation;
            CompoundCenterOfMass = compoundCenterOfMass;
        }
    }

    // 씬·MonoBehaviour·GameObject 없이 호출하는 최소 사용 예제다.
    public static class BoxSimulation
    {
        public static SimulationResult Run(int steps = 120)
        {
            if (steps <= 0) throw new ArgumentOutOfRangeException(nameof(steps));
            if (!Foundation.Init()) throw new InvalidOperationException("Jolt 초기화 실패");
            try
            {
                using var pairs = new ObjectLayerPairFilterTable(2);
                pairs.EnableCollision(0u, 1u);
                pairs.EnableCollision(1u, 1u);
                using var broadPhase = new BroadPhaseLayerInterfaceTable(2, 2);
                broadPhase.MapObjectToBroadPhaseLayer(0u, (byte)0);
                broadPhase.MapObjectToBroadPhaseLayer(1u, (byte)1);
                using var objectVsBroadPhase = new ObjectVsBroadPhaseLayerFilterTable(broadPhase, 2, pairs, 2);
                using var system = new PhysicsSystem(new PhysicsSystemSettings
                {
                    MaxBodies = 32,
                    MaxBodyPairs = 64,
                    MaxContactConstraints = 64,
                    ObjectLayerPairFilter = pairs,
                    BroadPhaseLayerInterface = broadPhase,
                    ObjectVsBroadPhaseLayerFilter = objectVsBroadPhase
                });
                using var jobs = new JobSystemThreadPool(new JobSystemThreadPoolConfig { numThreads = 2 });
                using var floor = new BoxShape(new Vector3(10, 0.5f, 10));
                using var floorSettings = new BodyCreationSettings(floor, new Vector3(0, -0.5f, 0), Quaternion.identity, MotionType.Static, 0u);
                BodyInterface bodies = system.BodyInterface;
                bodies.CreateAndAddBody(floorSettings, Activation.DontActivate);

                using var box = new BoxShape(new Vector3(0.5f, 0.5f, 0.5f));
                using var boxSettings = new BodyCreationSettings(box, new Vector3(0, 3, 0), Quaternion.identity, MotionType.Dynamic, 1u);
                BodyID boxID = bodies.CreateAndAddBody(boxSettings, Activation.Activate);

                using var compoundSettings = new StaticCompoundShapeSettings();
                Quaternion quarterTurn = new Quaternion(0, 0, 0.70710677f, 0.70710677f);
                using (var child = new BoxShape(new Vector3(0.25f, 0.5f, 0.25f)))
                {
                    compoundSettings.AddShape(new Vector3(-0.5f, 0, 0), quarterTurn, child);
                    compoundSettings.AddShape(new Vector3(0.75f, 0, 0), quarterTurn, child);
                }
                // 자식 래퍼를 Dispose한 이후에도 Compound 설정의 네이티브 참조가 유지된다.
                using var compound = new StaticCompoundShape(compoundSettings);
                using var compoundBodySettings = new BodyCreationSettings(compound,
                    new Vector3(3, 3, 0), Quaternion.identity, MotionType.Dynamic, 1u);
                BodyID compoundID = bodies.CreateAndAddBody(compoundBodySettings, Activation.Activate);
                system.OptimizeBroadPhase();
                for (int step = 0; step < steps; step++)
                {
                    PhysicsUpdateError error = system.Update(1.0f / 60.0f, 1, jobs);
                    if (error != PhysicsUpdateError.None)
                        throw new InvalidOperationException("시뮬레이션 용량 초과: " + error);
                }
                bodies.GetPositionAndRotation(boxID, out Vector3 position, out Quaternion rotation);
                bodies.GetPositionAndRotation(compoundID, out Vector3 compoundPosition, out Quaternion compoundRotation);
                return new SimulationResult(position, rotation, steps, compoundPosition, compoundRotation, compound.CenterOfMass);
            }
            finally
            {
                Foundation.Shutdown();
            }
        }
    }
}
