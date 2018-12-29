using System;
using System.Collections;

namespace TShirtCannon.Util
{
    class HashSet : ICollection, IEnumerable
    {
        private static readonly object PRESENT = new object();

        private readonly Hashtable hashTable = new Hashtable();

        public int Count {
            get {
                return hashTable.Count;
            }
        }

        public object SyncRoot
        {
            get
            {
                return hashTable.SyncRoot;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return hashTable.IsSynchronized;
            }
        }

        public void Add(object obj)
        {
            hashTable.Add(obj, PRESENT);
        }

        public bool Contains(object obj)
        {
            return hashTable.Contains(obj);
        }

        public void CopyTo(Array array, int index)
        {
            hashTable.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return hashTable.GetEnumerator();
        }

        public void Remove(object obj)
        {
            hashTable.Remove(obj);
        }
    }
}
