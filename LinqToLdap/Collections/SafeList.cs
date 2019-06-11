using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace LinqToLdap.Collections
{
    internal class SafeList<T>
    {
        private ReaderWriterLockSlim _locker = new ReaderWriterLockSlim();
        private List<T> _internal = new List<T>();

        ~SafeList()
        {
            var locker = _locker;
            if (locker != null)
            {
                try
                {
                    locker.Dispose();
                }
                catch
                {
                }

                _locker = locker = null;
            }
            var list = _internal;
            if (list != null)
            {
                list.Clear();
                _internal = list = null;
            }
        }

        public void Add(T item)
        {
            _locker.EnterWriteLock();
            try
            {
                _internal.Add(item);
            }
            finally
            {
                _locker.ExitWriteLock();
            }
        }

        public List<TValue> OfType<TValue>()
        {
            List<TValue> values;
            _locker.EnterReadLock();
            try
            {
                values = _internal.OfType<TValue>().ToList();
            }
            finally
            {
                _locker.ExitReadLock();
            }

            return values;
        }

        public List<T> ToList()
        {
            List<T> values;
            _locker.EnterReadLock();
            try
            {
                values = _internal.ToList();
            }
            finally
            {
                _locker.ExitReadLock();
            }
            return values;
        }
    }
}