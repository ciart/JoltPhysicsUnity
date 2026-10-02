// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Runtime.InteropServices;

namespace JoltPhysicsSharp
{
    public abstract class JobSystem : NativeObject
    {
        protected override void DestroyNative(IntPtr handle) => JoltApi.JPH_JobSystem_Destroy(handle);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JobSystemThreadPoolConfig
    {
        public uint maxJobs;
        public uint maxBarriers;
        public int numThreads;
    }

    public sealed class JobSystemThreadPool : JobSystem
    {
        public JobSystemThreadPool() : this(default(JobSystemThreadPoolConfig)) { }

        public JobSystemThreadPool(in JobSystemThreadPoolConfig config)
        {
            if (config.numThreads < -1) throw new ArgumentOutOfRangeException(nameof(config));
            lock (Foundation.SyncRoot)
            {
                Foundation.RequireInitialized();
                Initialize(JoltApi.JPH_JobSystemThreadPool_Create(in config));
            }
        }
    }
}
