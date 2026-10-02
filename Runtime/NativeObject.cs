// Copyright (c) Amer Koleci and Contributors.
// Licensed under the MIT License (MIT). See LICENSE.md in the package root.

using System;
using System.Collections.Generic;

namespace JoltPhysicsSharp
{
    public abstract class NativeObject : IDisposable
    {
        private IntPtr _handle;
        private NativeObject _owner;
        private List<NativeObject> _dependencies;
        private int _dependents;

        public bool IsDisposed
        {
            get { lock (Foundation.SyncRoot) return _handle == IntPtr.Zero; }
        }

        public IntPtr Handle
        {
            get { lock (Foundation.SyncRoot) return GetHandle(); }
        }

        // 모든 내부 호출은 SyncRoot를 보유하여 스텝과 Dispose의 충돌을 막는다.
        internal IntPtr GetHandle()
        {
            if (_handle == IntPtr.Zero) throw new ObjectDisposedException(GetType().Name);
            return _handle;
        }

        protected void Initialize(IntPtr handle)
        {
            Foundation.RequireInitialized();
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException(GetType().Name + " 네이티브 생성에 실패했습니다.");
            if (_handle != IntPtr.Zero) throw new InvalidOperationException("이미 초기화된 객체입니다.");
            Foundation.RegisterObject();
            _handle = handle;
        }

        internal void RequireOwned()
        {
            GetHandle();
            if (_owner != null)
                throw new InvalidOperationException("이미 월드에 소유권을 넘긴 필터를 다시 사용할 수 없습니다.");
        }

        internal void TransferTo(NativeObject owner)
        {
            RequireOwned();
            _owner = owner;
        }

        internal void RequireMutable()
        {
            RequireOwned();
            if (_dependents != 0)
                throw new InvalidOperationException("이 테이블을 사용하는 필터를 생성한 뒤에는 구성을 바꿀 수 없습니다.");
        }

        protected void RetainDependency(NativeObject dependency)
        {
            dependency.GetHandle();
            if (_dependencies == null) _dependencies = new List<NativeObject>();
            _dependencies.Add(dependency);
            dependency._dependents++;
        }

        internal void DestroyedByOwner(NativeObject owner)
        {
            if (_owner != owner) throw new InvalidOperationException("네이티브 소유자가 다릅니다.");
            CompleteDisposal();
        }

        public void Dispose()
        {
            lock (Foundation.SyncRoot)
            {
                if (_handle == IntPtr.Zero) return;
                if (_owner != null)
                    throw new InvalidOperationException("월드가 소유한 필터는 월드를 Dispose할 때 함께 파괴됩니다.");
                if (_dependents != 0)
                    throw new InvalidOperationException("이 객체를 사용하는 네이티브 객체를 먼저 Dispose하세요.");
                DestroyNative(_handle);
                CompleteDisposal();
            }
            GC.SuppressFinalize(this);
        }

        private void CompleteDisposal()
        {
            _handle = IntPtr.Zero;
            _owner = null;
            if (_dependencies != null)
            {
                foreach (NativeObject dependency in _dependencies) dependency._dependents--;
                _dependencies.Clear();
            }
            Foundation.UnregisterObject();
        }

        protected abstract void DestroyNative(IntPtr handle);

        ~NativeObject()
        {
            // 명시적 Dispose가 기본이다. 소유권을 넘긴 객체는 소유자가 해제한다.
            try
            {
                lock (Foundation.SyncRoot)
                {
                    if (_handle == IntPtr.Zero || _owner != null) return;
                    DestroyNative(_handle);
                    CompleteDisposal();
                }
            }
            catch { /* 종료 중 finalizer 예외를 다른 스레드로 전파하지 않는다. */ }
        }
    }
}
