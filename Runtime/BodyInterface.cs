// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using UnityEngine;

namespace JoltPhysicsSharp
{
    public readonly struct BodyInterface
    {
        private readonly PhysicsSystem _system;
        internal BodyInterface(PhysicsSystem system) { _system = system; }

        private PhysicsSystem System
        {
            get
            {
                if (_system == null) throw new InvalidOperationException("초기화되지 않은 BodyInterface입니다.");
                _system.GetHandle();
                return _system;
            }
        }

        public IntPtr Handle
        {
            get { lock (Foundation.SyncRoot) return System.GetBodyInterfaceHandle(); }
        }

        public Body CreateBody(BodyCreationSettings settings)
        {
            lock (Foundation.SyncRoot)
            {
                PhysicsSystem system = System;
                system.CheckSettings(settings);
                BodyID id = system.TrackBody(JoltApi.JPH_BodyInterface_CreateBody(system.GetBodyInterfaceHandle(), settings.GetHandle()));
                return new Body(system, id);
            }
        }

        public BodyID CreateAndAddBody(BodyCreationSettings settings, Activation activationMode)
        {
            MathValidation.Activation(activationMode);
            lock (Foundation.SyncRoot)
            {
                PhysicsSystem system = System;
                system.CheckSettings(settings);
                // 바디 포인터와 세대 ID를 추적한 뒤 추가하여 파괴 후 접근을 제어한다.
                BodyID id = system.TrackBody(JoltApi.JPH_BodyInterface_CreateBody(system.GetBodyInterfaceHandle(), settings.GetHandle()));
                JoltApi.JPH_BodyInterface_AddBody(system.GetBodyInterfaceHandle(), id.ID, activationMode);
                system.MarkAdded(id);
                return id;
            }
        }

        public void AddBody(in BodyID bodyID, Activation activationMode)
        {
            MathValidation.Activation(activationMode);
            lock (Foundation.SyncRoot)
            {
                PhysicsSystem system = System;
                if (system.IsAdded(bodyID)) throw new InvalidOperationException("이미 월드에 추가된 바디입니다.");
                JoltApi.JPH_BodyInterface_AddBody(system.GetBodyInterfaceHandle(), bodyID.ID, activationMode);
                system.MarkAdded(bodyID);
            }
        }

        public void AddBody(in Body body, Activation activationMode)
        {
            if (body == null) throw new ArgumentNullException(nameof(body));
            if (body.System != _system) throw new ArgumentException("다른 월드의 바디입니다.", nameof(body));
            AddBody(body.ID, activationMode);
        }

        public void RemoveBody(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot)
            {
                PhysicsSystem system = System;
                if (!system.IsAdded(bodyID)) throw new InvalidOperationException("월드에 추가되지 않은 바디입니다.");
                JoltApi.JPH_BodyInterface_RemoveBody(system.GetBodyInterfaceHandle(), bodyID.ID);
                system.MarkRemoved(bodyID);
            }
        }

        public void DestroyBody(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot)
            {
                PhysicsSystem system = System;
                if (system.IsAdded(bodyID)) throw new InvalidOperationException("RemoveBody 이후에 파괴하거나 RemoveAndDestroyBody를 사용하세요.");
                JoltApi.JPH_BodyInterface_DestroyBody(system.GetBodyInterfaceHandle(), bodyID.ID);
                system.ForgetBody(bodyID);
            }
        }

        public void RemoveAndDestroyBody(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot)
            {
                PhysicsSystem system = System;
                if (system.IsAdded(bodyID)) JoltApi.JPH_BodyInterface_RemoveBody(system.GetBodyInterfaceHandle(), bodyID.ID);
                JoltApi.JPH_BodyInterface_DestroyBody(system.GetBodyInterfaceHandle(), bodyID.ID);
                system.ForgetBody(bodyID);
            }
        }

        public bool IsAdded(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot) return System.IsAdded(bodyID);
        }

        private IntPtr ForBody(BodyID id)
        {
            PhysicsSystem system = System;
            system.RequireBody(id);
            return system.GetBodyInterfaceHandle();
        }

        public Vector3 GetPosition(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot)
            {
                JoltApi.JPH_BodyInterface_GetPosition(ForBody(bodyID), bodyID.ID, out Vector3 value);
                return value;
            }
        }

        public Quaternion GetRotation(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot)
            {
                JoltApi.JPH_BodyInterface_GetRotation(ForBody(bodyID), bodyID.ID, out Quaternion value);
                return value;
            }
        }

        public void GetPositionAndRotation(in BodyID bodyID, out Vector3 position, out Quaternion rotation)
        {
            lock (Foundation.SyncRoot)
                JoltApi.JPH_BodyInterface_GetPositionAndRotation(ForBody(bodyID), bodyID.ID, out position, out rotation);
        }

        public Vector3 GetCenterOfMassPosition(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot)
            {
                JoltApi.JPH_BodyInterface_GetCenterOfMassPosition(ForBody(bodyID), bodyID.ID, out Vector3 value);
                return value;
            }
        }

        public void SetPositionAndRotation(in BodyID bodyID, in Vector3 position, in Quaternion rotation, Activation activationMode)
        {
            MathValidation.Finite(position, nameof(position)); MathValidation.Rotation(rotation, nameof(rotation));
            MathValidation.Activation(activationMode);
            lock (Foundation.SyncRoot)
                JoltApi.JPH_BodyInterface_SetPositionAndRotation(ForBody(bodyID), bodyID.ID, in position, in rotation, activationMode);
        }

        public void MoveKinematic(in BodyID bodyID, in Vector3 targetPosition, in Quaternion targetRotation, float deltaTime)
        {
            MathValidation.Finite(targetPosition, nameof(targetPosition)); MathValidation.Rotation(targetRotation, nameof(targetRotation));
            MathValidation.Finite(deltaTime, nameof(deltaTime));
            if (deltaTime <= 0) throw new ArgumentOutOfRangeException(nameof(deltaTime));
            lock (Foundation.SyncRoot)
            {
                IntPtr handle = ForBody(bodyID);
                if (JoltApi.JPH_BodyInterface_GetMotionType(handle, bodyID.ID) != MotionType.Kinematic)
                    throw new InvalidOperationException("MoveKinematic에는 키네마틱 바디가 필요합니다.");
                JoltApi.JPH_BodyInterface_MoveKinematic(handle, bodyID.ID, in targetPosition, in targetRotation, deltaTime);
            }
        }

        public Vector3 GetLinearVelocity(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot)
            {
                JoltApi.JPH_BodyInterface_GetLinearVelocity(ForBody(bodyID), bodyID.ID, out Vector3 value);
                return value;
            }
        }

        public void SetLinearVelocity(in BodyID bodyID, in Vector3 velocity)
        {
            MathValidation.Finite(velocity, nameof(velocity));
            lock (Foundation.SyncRoot)
            {
                IntPtr handle = ForBody(bodyID);
                if (JoltApi.JPH_BodyInterface_GetMotionType(handle, bodyID.ID) == MotionType.Static)
                    throw new InvalidOperationException("정적 바디에는 속도를 설정할 수 없습니다.");
                JoltApi.JPH_BodyInterface_SetLinearVelocity(handle, bodyID.ID, in velocity);
            }
        }

        public MotionType GetMotionType(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot) return JoltApi.JPH_BodyInterface_GetMotionType(ForBody(bodyID), bodyID.ID);
        }

        public bool IsActive(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot) return JoltApi.JPH_BodyInterface_IsActive(ForBody(bodyID), bodyID.ID);
        }

        public void ActivateBody(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot) JoltApi.JPH_BodyInterface_ActivateBody(ForBody(bodyID), bodyID.ID);
        }

        public void DeactivateBody(in BodyID bodyID)
        {
            lock (Foundation.SyncRoot) JoltApi.JPH_BodyInterface_DeactivateBody(ForBody(bodyID), bodyID.ID);
        }
    }

    public sealed class Body
    {
        internal PhysicsSystem System { get; }
        public BodyID ID { get; }
        internal Body(PhysicsSystem system, BodyID id) { System = system; ID = id; }
        public Vector3 Position => System.BodyInterface.GetPosition(ID);
        public Quaternion Rotation => System.BodyInterface.GetRotation(ID);
        public MotionType MotionType => System.BodyInterface.GetMotionType(ID);
        public bool IsActive => System.BodyInterface.IsActive(ID);
        public bool IsAdded => System.BodyInterface.IsAdded(ID);
    }
}
