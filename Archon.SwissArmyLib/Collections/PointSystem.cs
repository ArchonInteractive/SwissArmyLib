using System;
using System.Collections.Generic;

namespace Archon.SwissArmyLib.Collections
{
    public class PointSystem<TOwner, TKey>
    {
        public TKey StartScore
        {
            get { return _scores.DefaultValue; }
            set { _scores.DefaultValue = value; }
        }

        public IComparer<TKey> Comparer { get; set; }

        private readonly DictionaryWithDefault<TOwner, TKey> _scores = new DictionaryWithDefault<TOwner, TKey>();
        private readonly List<TOwner> _sortedOwners = new List<TOwner>();

        public PointSystem(TKey startScore, IComparer<TKey> comparer)
        {
            StartScore = startScore;
            Comparer = comparer;
        }

        public TKey this[TOwner owner]
        {
            get { return Get(owner); }
            set { Set(owner, value); }
        }

        public TKey Get(TOwner owner)
        {
            return _scores[owner];
        }

        public void Set(TOwner owner, TKey score)
        {
            _scores[owner] = score;
            // todo: update sortedowners
        }

        public void SetAll(IEnumerable<TOwner> owners, TKey score)
        {
            foreach (var owner in owners)
                Set(owner, score);
        }

        public TOwner GetHighestScore()
        {
            if (_sortedOwners.Count > 0)
                return _sortedOwners[0];

            return default(TOwner);
        }

        public TOwner GetHighestScore(IEnumerable<TOwner> owners)
        {
            throw new NotImplementedException();
        }

        public TOwner GetLowestScore()
        {
            if (_sortedOwners.Count > 0)
                return _sortedOwners[_sortedOwners.Count - 1];

            return default(TOwner);
        }

        public TOwner GetLowestScore(IEnumerable<TOwner> owners)
        {
            throw new NotImplementedException();
        }

        public bool HasExplicitScore(TOwner owner)
        {
            return _scores.ContainsKey(owner);
        }

        public void Clear()
        {
            _scores.Clear();
            _sortedOwners.Clear();
        }

        public void Clear(TKey startScore)
        {
            Clear();
            StartScore = startScore;
        }
    }

    public class IntegerPointSystem<TOwner> : PointSystem<TOwner, int>
    {
        public IntegerPointSystem(int startScore = 0) : base(startScore, Comparer<int>.Default)
        {
            
        }

        public IntegerPointSystem(int startScore = 0, Comparer<int> comparer) : base(startScore, comparer)
        {
            
        }

        public int Increment(TOwner owner, int amount = 1)
        {
            return this[owner] += amount;
        }

        public int Decrement(TOwner owner, int amount = 1)
        {
            return this[owner] -= amount;
        }

        public int Sum(IEnumerable<TOwner> owners)
        {
            var sum = 0;

            foreach (var owner in owners)
                sum += Get(owner);

            return sum;
        }
    }
}
