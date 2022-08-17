using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BreakthroughWPF
{
    public class ReaderWriterCustomLock<T>
    {
        private T value;
        private ReaderWriterLockSlim local_lock = new ReaderWriterLockSlim();
        public T Value
        {
            get
            {
                try
                {
                    local_lock.EnterReadLock();
                    return value;
                }
                finally
                {
                    local_lock.ExitReadLock();
                }
            }
            set
            {
                try
                {
                    local_lock.EnterWriteLock();
                    this.value = value;
                }
                finally
                {
                    local_lock.ExitWriteLock();
                }
            }
        }

        public ReaderWriterCustomLock(T t) { value = t; }
        public ReaderWriterCustomLock() { }
    }
}
