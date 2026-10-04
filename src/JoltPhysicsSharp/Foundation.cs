// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#if UNITY_5_3_OR_NEWER
#nullable disable
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
        internal static readonly object SyncRoot = new object();
        internal static int LiveObjects;
        private static int users;
        private static TraceDelegate traceHandler;
        private static AssertFailedDelegate assertHandler;
        private static readonly JoltApi.TraceCallback traceCallback = OnNativeTrace;
        private static readonly JoltApi.AssertCallback assertCallback = OnNativeAssert;
        public delegate void TraceDelegate(string message);
        public delegate bool AssertFailedDelegate(string expression, string message, string file, uint line);

        public static bool Init(bool doublePrecision = false)
        {
            if (doublePrecision)
                throw new NotSupportedException("The Unity runtime currently supports single precision only.");
            lock (SyncRoot)
            {
                if (users > 0)
                {
                    users++;
                    return true;
                }
                if (!JoltApi.JPH_Init())
                    return false;
                users = 1;
                JoltApi.JPH_SetTraceHandler(traceCallback);
                JoltApi.JPH_SetAssertFailureHandler(assertCallback);
                return true;
            }
        }
        public static void Shutdown()
        {
            lock (SyncRoot)
            {
                if (users == 0)
                    return;
                if (users > 1)
                {
                    users--;
                    return;
                }
                if (LiveObjects != 0)
                    throw new InvalidOperationException("Dispose all Jolt native objects before shutting down the runtime.");
                JoltApi.JPH_SetTraceHandler(null);
                JoltApi.JPH_SetAssertFailureHandler(null);
                JoltApi.JPH_Shutdown();
                users = 0;
            }
        }
        internal static void RequireInitialized()
        {
            if (users == 0)
                throw new InvalidOperationException("Call Foundation.Init before creating native objects.");
        }
        public static void SetTraceHandler(TraceDelegate callback) => traceHandler = callback;
        public static void SetAssertFailureHandler(AssertFailedDelegate callback) => assertHandler = callback;
        internal static void ReportCallbackException(Exception exception) => Debug.LogException(exception);

        [AOT.MonoPInvokeCallback(typeof(JoltApi.TraceCallback))]
        private static void OnNativeTrace(IntPtr message)
        {
            try { traceHandler?.Invoke(JoltApi.ConvertToManaged(message)); }
            catch (Exception exception) { ReportCallbackException(exception); }
        }
        [AOT.MonoPInvokeCallback(typeof(JoltApi.AssertCallback))]
        private static bool OnNativeAssert(IntPtr expression, IntPtr message, IntPtr file, uint line)
        {
            try
            {
                if (assertHandler != null)
                    return assertHandler(JoltApi.ConvertToManaged(expression), JoltApi.ConvertToManaged(message), JoltApi.ConvertToManaged(file), line);
                Debug.LogError("Jolt assertion: " + JoltApi.ConvertToManaged(expression) + " at " + JoltApi.ConvertToManaged(file) + ":" + line);
            }
            catch (Exception exception) { ReportCallbackException(exception); }
            return false;
        }
    }
}
#else
using System.Runtime.InteropServices;
using static JoltPhysicsSharp.JoltApi;

namespace JoltPhysicsSharp;

public static class Foundation
{
    /// <summary>
    /// If objects are closer than this distance, they are considered to be colliding (used for GJK) (unit: meter)
    /// </summary>
    public const float DefaultCollisionTolerance = 1.0e-4f;

    /// <summary>
    /// A factor that determines the accuracy of the penetration depth calculation. If the change of the squared distance is less than tolerance * current_penetration_depth^2 the algorithm will terminate. (unit: dimensionless)
    /// </summary>
    public const float DefaultPenetrationTolerance = 1.0e-4f;

    /// <summary>
    /// How much padding to add around objects
    /// </summary>
    public const float DefaultConvexRadius = 0.05f;

    /// <summary>
    /// Used by (Tapered)CapsuleShape to determine when supporting face is an edge rather than a point (unit: meter)
    /// </summary>
    public const float CapsuleProjectionSlop = 0.02f;

    /// <summary>
    /// Maximum amount of jobs to allow
    /// </summary>
    public const int MaxPhysicsJobs = 2048;

    /// <summary>
    /// Maximum amount of barriers to allow
    /// </summary>
    public const int MaxPhysicsBarriers = 8;

    public static bool Init(bool doublePrecision = false)
    {
        JoltApi.DoublePrecision = doublePrecision;
        return JPH_Init();
    }

    public static void Shutdown() => JPH_Shutdown();

    private static TraceDelegate? s_traceCallback;
    private static AssertFailedDelegate? s_assertCallback;

    public static unsafe void SetTraceHandler(TraceDelegate callback)
    {
        s_traceCallback = callback;

        JPH_SetTraceHandler(callback != null ? &OnNativeTraceCallback : null);
    }

    public static unsafe void SetAssertFailureHandler(AssertFailedDelegate callback)
    {
        s_assertCallback = callback;

        JPH_SetAssertFailureHandler(callback != null ? &OnNativeAssertCallback : null);
    }

    public delegate void TraceDelegate(string message);

    public delegate bool AssertFailedDelegate(string expression, string message, string file, uint line);

    [UnmanagedCallersOnly]
    private static unsafe void OnNativeTraceCallback(byte* messagePtr)
    {
        if (s_traceCallback != null)
        {
            string message = ConvertToManaged(messagePtr)!;
            s_traceCallback(message);
        }
    }

    [UnmanagedCallersOnly]
    private static unsafe Bool8 OnNativeAssertCallback(byte* expressionPtr, byte* messagePtr, byte* filePtr, uint line)
    {
        string expression = ConvertToManaged(expressionPtr)!;
        string message = ConvertToManaged(messagePtr)!;
        string file = ConvertToManaged(filePtr)!;

        if (s_assertCallback != null)
        {
            return s_assertCallback(expression, message, file, line);
        }

        return Bool8.True;
    }
}

#endif
