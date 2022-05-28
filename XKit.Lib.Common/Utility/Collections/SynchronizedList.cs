using System;
using System.Collections.Generic;

namespace XKit.Lib.Common.Utility.Collections {

    [Serializable()]
    public class SynchronizedList<T> : IList<T> {
        private readonly List<T> list;
        private readonly object syncRoot;

        public SynchronizedList()
            :this(new List<T>()) { }

        public SynchronizedList(int initialCapacity)
            :this(new List<T>(initialCapacity)) { }

        public SynchronizedList(IEnumerable<T> src)
            :this(new List<T>(src)) { }

        public SynchronizedList(List<T> list) {
            this.list = list;
            syncRoot = ((System.Collections.ICollection) list).SyncRoot;
        }
        public int Count {
            get {
                lock(syncRoot) {
                    return list.Count;
                }
            }
        }

        public bool IsReadOnly {
            get {
                return ((ICollection<T>) list).IsReadOnly;
            }
        }

        public void Add(T item) {
            lock(syncRoot) {
                list.Add(item);
            }
        }

        public void Clear() {
            lock(syncRoot) {
                list.Clear();
            }
        }

        public bool Contains(T item) {
            lock(syncRoot) {
                return list.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex) {
            lock(syncRoot) {
                list.CopyTo(array, arrayIndex);
            }
        }

        public bool Remove(T item) {
            lock(syncRoot) {
                return list.Remove(item);
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            lock(syncRoot) {
                return list.ToArray().GetEnumerator();
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            lock(syncRoot) {
                return ((IEnumerable<T>) list.ToArray()).GetEnumerator();
            }
        }

        public T this [int index] {
            get {
                lock(syncRoot) {
                    return list[index];
                }
            }
            set {
                lock(syncRoot) {
                    list[index] = value;
                }
            }
        }

        public int IndexOf(T item) {
            lock(syncRoot) {
                return list.IndexOf(item);
            }
        }

        public void Insert(int index, T item) {
            lock(syncRoot) {
                list.Insert(index, item);
            }
        }

        public void RemoveAt(int index) {
            lock(syncRoot) {
                list.RemoveAt(index);
            }
        }
    }
}
