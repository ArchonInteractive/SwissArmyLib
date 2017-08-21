using System;
using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Extensions;

namespace Archon.SwissArmyLib.Collections
{
    public class ShuffleBag<T>
    {
        public List<Item<T>> Items { get { return _data; } }

        private readonly List<Item<T>> _data = new List<Item<T>>();

        private readonly Random _random = new Random();

        public class Item<T2>
        {
            public T2 Data { get; set; }
            public int Weight { get; set; }
        }

        public void Add(T item, int weight)
        {
            _data.Add(new Item<T> { Data = item, Weight = weight });
        }

        public void Remove(T item)
        {
            _data.RemoveAll(i => i.Data.Equals(item));
        }

        public void Shuffle()
        {
            _data.Shuffle();
        }

        public T Next()
        {
            var sum = GetSum();
            var val = _random.NextDouble() * sum;

            for (var i = 0; i < _data.Count; i++)
                _data[i].Weight++;

            for (var i = 0; i < _data.Count; i++)
            {
                var item = _data[i];

                if (item.Weight < 0)
                    continue;

                val -= item.Weight;

                if (val < 0)
                {
                    item.Weight -= _data.Count;
                    return item.Data;
                }
            }

            return default(T);
        }

        private int GetSum()
        {
            var sum = 0;
            for (var i = 0; i < _data.Count; i++)
            {
                var weight = _data[i].Weight;
                if (weight > 0)
                    sum += weight;
            }
            return sum;
        }
    }
}
