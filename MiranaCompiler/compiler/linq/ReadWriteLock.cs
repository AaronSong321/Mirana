using System;
using System.Threading;

namespace Common
{
    public sealed class ReadWriteLock
    {
        public sealed class UsingWrite: IDisposable
        {
            private readonly ReaderWriterLockSlim underLock;
            internal UsingWrite(ReaderWriterLockSlim underLock)
            {
                this.underLock = underLock;
                underLock.EnterWriteLock();
            }
            public void Dispose()
            {
                underLock.ExitWriteLock();
            }
        }
        public sealed class UsingRead: IDisposable
        {
            private readonly ReaderWriterLockSlim underLock;
            internal UsingRead(ReaderWriterLockSlim underLock)
            {
                this.underLock = underLock;
                underLock.EnterReadLock();
            }
            public void Dispose()
            {
                underLock.ExitReadLock();
            }
        }
        
        private readonly ReaderWriterLockSlim underLock = new();
        public UsingWrite Write() => new UsingWrite(underLock);
        public UsingRead Read() => new UsingRead(underLock);
    }
}
