// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace JoltPhysicsSharp
{
    public static class Foundation
    {
        public const float DefaultCollisionTolerance = 1.0e-4f;
        public const float DefaultPenetrationTolerance = 1.0e-4f;
        public const float DefaultConvexRadius = 0.05f;
        public const float CapsuleProjectionSlop = 0.02f;
        public const int MaxPhysicsJobs = 2048;
        public const int MaxPhysicsBarriers = 8;

        // joltc의 기본 TempAllocator와 전역 초기화 상태는 여러 월드가 공유한다.
        internal static readonly object SyncRoot = new object();
        private static int _initializations;
        private static int _nativeObjects;

        public static bool IsInitialized
        {
            get { lock (SyncRoot) return _initializations != 0; }
        }

        public static bool Init(bool doublePrecision = false)
        {
            if (doublePrecision)
                throw new NotSupportedException("이 패키지는 단정밀도만 지원합니다.");

            lock (SyncRoot)
            {
                if (_initializations != 0)
                {
                    checked { _initializations++; }
                    return true;
                }

                ValidateLayout();
                try
                {
                    if (!JoltApi.JPH_Init()) return false;
                }
                catch (DllNotFoundException exception)
                {
                    throw new DllNotFoundException("joltc 플러그인을 찾을 수 없습니다. 패키지의 플랫폼·CPU 임포트 설정을 확인하세요. 현재 포함된 바이너리는 macOS용입니다.", exception);
                }
                catch (EntryPointNotFoundException exception)
                {
                    throw new InvalidOperationException("로드한 joltc의 C ABI가 패키지와 일치하지 않습니다. upstream.lock.json의 바이너리를 사용하세요.", exception);
                }
                _initializations = 1;
                return true;
            }
        }

        public static void Shutdown()
        {
            lock (SyncRoot)
            {
                if (_initializations == 0) return;
                if (_initializations > 1)
                {
                    _initializations--;
                    return;
                }
                if (_nativeObjects != 0)
                    throw new InvalidOperationException("월드·형상·설정·JobSystem을 모두 Dispose한 뒤 마지막 Shutdown을 호출해야 합니다.");

                JoltApi.JPH_Shutdown();
                _initializations = 0;
            }
        }

        internal static void RequireInitialized()
        {
            if (_initializations == 0)
                throw new InvalidOperationException("먼저 Foundation.Init을 호출하세요.");
        }

        internal static void RegisterObject() { checked { _nativeObjects++; } }
        internal static void UnregisterObject() { _nativeObjects--; }

        private static void ValidateLayout()
        {
            if (IntPtr.Size != 8 || Marshal.SizeOf<Vector3>() != 12
                || Marshal.SizeOf<Quaternion>() != 16 || Marshal.SizeOf<BodyID>() != 4
                || Marshal.SizeOf<Triangle>() != 40
                || Marshal.SizeOf<JoltApi.NativePhysicsSystemSettings>() != 48
                || Marshal.OffsetOf<JoltApi.NativePhysicsSystemSettings>(nameof(JoltApi.NativePhysicsSystemSettings.BroadPhaseLayerInterface)).ToInt32() != 24
                || Marshal.SizeOf<JobSystemThreadPoolConfig>() != 12)
                throw new PlatformNotSupportedException("이 패키지는 확인된 64비트 단정밀도 ABI 배치를 요구합니다.");
        }
    }
}
