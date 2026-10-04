// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

#if UNITY_5_3_OR_NEWER
#nullable disable
using System;
namespace JoltPhysicsSharp
{
    public abstract class NativeObject : IDisposable
    {
        private IntPtr handle;
        private bool registered;
        private NativeObject owner;
        public bool IsDisposed { get; private set; }
        protected internal virtual bool OwnsHandle { get; protected set; } = true;
        protected internal bool IgnorePublicDispose { get; set; }

        protected NativeObject() { }
        internal NativeObject(IntPtr handle, bool ownsHandle = true)
        {
            OwnsHandle = ownsHandle;
            Handle = handle;
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException("Native object creation returned a null handle.");
        }
        public IntPtr Handle
        {
            get
            {
                ThrowIfDisposed();
                return handle;
            }
            protected set
            {
                lock (Foundation.SyncRoot)
                {
                    if (handle != IntPtr.Zero || IsDisposed)
                        throw new InvalidOperationException("A native handle cannot be replaced.");
                    Foundation.RequireInitialized();
                    if (value == IntPtr.Zero)
                        throw new InvalidOperationException("Native object creation returned a null handle.");
                    handle = value;
                    if (OwnsHandle)
                    {
                        Foundation.LiveObjects++;
                        registered = true;
                    }
                }
            }
        }
        internal void ThrowIfDisposed()
        {
            if (IsDisposed || handle == IntPtr.Zero)
                throw new ObjectDisposedException(GetType().Name);
        }
        internal void RequireTransferable()
        {
            ThrowIfDisposed();
            if (owner != null || !OwnsHandle)
                throw new InvalidOperationException("A collision filter cannot be shared between worlds.");
        }
        internal void TransferTo(NativeObject newOwner)
        {
            RequireTransferable();
            owner = newOwner;
            OwnsHandle = false;
            IgnorePublicDispose = true;
            Unregister();
        }
        internal void InvalidateFromOwner()
        {
            handle = IntPtr.Zero;
            owner = null;
            IgnorePublicDispose = false;
            IsDisposed = true;
            GC.SuppressFinalize(this);
        }
        public void Dispose()
        {
            lock (Foundation.SyncRoot)
            {
                if (IsDisposed || IgnorePublicDispose)
                    return;
                if (handle != IntPtr.Zero && OwnsHandle)
                    DisposeNative();
                DisposeManaged();
                handle = IntPtr.Zero;
                IsDisposed = true;
                Unregister();
                GC.SuppressFinalize(this);
            }
        }
        private void Unregister()
        {
            if (registered)
            {
                Foundation.LiveObjects--;
                registered = false;
            }
        }
        protected virtual void DisposeNative() { }
        protected virtual void DisposeManaged() { }
        ~NativeObject()
        {
            try { Dispose(); }
            catch (Exception exception) { Foundation.ReportCallbackException(exception); }
        }
    }
}
#else
using System.Collections.Concurrent;

namespace JoltPhysicsSharp;

/// <summary>
/// A native object which is disposable.
/// </summary>
public abstract class NativeObject : IDisposable
{
    private volatile uint _isDisposed;
    internal bool fromFinalizer = false;
    private nint _handle;
    private readonly object _locker = new();
    private ConcurrentDictionary<nint, NativeObject>? _ownedObjects;

    protected NativeObject()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeObject" /> class.
    /// <param name="handle">The handle to initialize width.</param>
    /// </summary>
    internal NativeObject(nint handle)
        : this(handle, true)
    {
    }

    internal NativeObject(nint handle, bool ownsHandle)
    {
        Handle = handle;
        OwnsHandle = ownsHandle;
    }

    ~NativeObject()
    {
        fromFinalizer = true;

        Dispose(false);
    }


    /// <summary>Gets <c>true</c> if the object has been disposed; otherwise, <c>false</c>.</summary>
    public bool IsDisposed => _isDisposed != 0;
    protected internal bool IgnorePublicDispose { get; set; }

    internal ConcurrentDictionary<nint, NativeObject> OwnedObjects
    {
        get
        {
            if (_ownedObjects == null)
            {
                lock (_locker)
                {
                    _ownedObjects ??= new();
                }
            }
            return _ownedObjects;
        }
    }

    public nint Handle
    {
        get => _handle;
        protected set
        {
            if (value == 0)
            {
                UnregisterHandle(Handle, this);
                _handle = value;
            }
            else
            {
                _handle = value;
                RegisterHandle(Handle, this);
            }
        }
    }

    protected internal virtual bool OwnsHandle { get; protected set; }

    public void Dispose()
    {
        if (IgnorePublicDispose)
            return;

        DisposeInternal();
    }

    protected internal void DisposeInternal()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) != 0)
            return;

        // dispose any objects that are owned/created by native code
        if (disposing)
            DisposeUnownedManaged();

        // destroy the native object
        if (Handle != IntPtr.Zero && OwnsHandle)
            DisposeNative();

        // dispose any remaining managed-created objects
        if (disposing)
            DisposeManaged();

        Handle = IntPtr.Zero;
    }

    private void DisposeUnownedManaged()
    {
        if (_ownedObjects != null)
        {
            foreach (var child in _ownedObjects)
            {
                if (child.Value is NativeObject c && !c.OwnsHandle)
                    c.DisposeInternal();
            }
        }
    }

    protected virtual void DisposeManaged()
    {
        // dispose of any managed resources
    }

    protected virtual void DisposeNative()
    {
        // dispose of any unmanaged resources
    }

    internal static void RegisterHandle(nint handle, NativeObject? instance)
    {
        if (handle != 0 && instance != null)
        {
            HandleDictionary.RegisterHandle(handle, instance);
        }
    }

    internal static void UnregisterHandle(nint handle, NativeObject instance)
    {
        if (handle != 0)
        {
            HandleDictionary.UnregisterHandle(handle, instance);
        }
    }

    internal static TNativeObject? GetOrAddObject<TNativeObject>(nint handle)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
        {
            return null;
        }

        return HandleDictionary.GetOrAddObject<TNativeObject>(handle);
    }

    internal static TNativeObject? GetOrAddObject<TNativeObject>(nint handle, Func<nint, TNativeObject> objectFactory)
        where TNativeObject : NativeObject
    {
        if (handle == 0)
        {
            return null;
        }

        return HandleDictionary.GetOrAddObject(handle, objectFactory);
    }
}

#endif
