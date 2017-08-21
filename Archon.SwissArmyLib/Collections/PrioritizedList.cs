using System.Collections;
using System.Collections.Generic;

namespace Archon.SwissArmyLib.Collections
{
    public class PrioritizedList<T> : IList<T>
    {
        private readonly List<PrioritizedItem> _items = new List<PrioritizedItem>();

        public struct PrioritizedItem
        {
            public T Item;
            public int Priority;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _items.Count; i++)
                yield return _items[i].Item;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            Add(item, 0);
        }

        public void Add(T item, int priority)
        {
            var prioritizedItem = new PrioritizedItem
            {
                Item = item,
                Priority = priority
            };

            if (_items.Count == 0)
            {
                _items.Add(prioritizedItem);
                return;
            }

            for (var i = 0; i < _items.Count; i++)
            {
                if (priority < _items[i].Priority)
                {
                    _items.Insert(i, prioritizedItem);
                    return;
                }
            }

            _items.Add(prioritizedItem);
        }

        public void Clear()
        {
            _items.Clear();
        }

        public bool Contains(T item)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Equals(item))
                    return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (var i = arrayIndex; i < array.Length && i < _items.Count; i++)
                array[i] = _items[i].Item;
        }

        public bool Remove(T item)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Equals(item))
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public int Count { get { return _items.Count; }}
        public bool IsReadOnly { get { return false; }}

        public int IndexOf(T item)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].Equals(item))
                    return i;
            }

            return -1;
        }

        public void Insert(int index, T item)
        {
            throw new System.NotImplementedException("Inserting at a specific index is not supported by PrioritizedList since it wouldn't make sense.");
        }

        public void RemoveAt(int index)
        {
            _items.RemoveAt(index);
        }

        public T this[int index]
        {
            get { return _items[index].Item; }
            set
            {
                var item = _items[index];
                item.Item = value;
                _items[index] = item;
            }
        }
    }
}
