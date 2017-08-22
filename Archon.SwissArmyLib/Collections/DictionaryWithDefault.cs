using System.Collections.Generic;

namespace Archon.SwissArmyLib.Collections
{
    /// <summary>
    /// A <see cref="Dictionary{TKey,TValue}"/> but with a default value for missing entries.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DictionaryWithDefault<TKey, TValue> : Dictionary<TKey, TValue>
    {
        /// <summary>
        /// Default value for missing entries.
        /// </summary>
        public TValue DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the value associated with the given key.
        /// 
        /// If the key isn't in the dictionary, <see cref="DefaultValue"/> will be returned.
        /// </summary>
        /// <param name="key"></param>
        public new TValue this[TKey key]
        {
            get
            {
                TValue t;
                return TryGetValue(key, out t) ? t : DefaultValue;
            }
            set
            {
                base[key] = value;
            }
        }

        /// <summary>
        /// Creates a new Dictionary with DefaultValue set to TValue's default value.
        /// </summary>
        public DictionaryWithDefault()
        {
        }

        /// <summary>
        /// Creates a new Dictionary using the supplied value as the default for missing entries.
        /// </summary>
        /// <param name="defaultValue"></param>
        public DictionaryWithDefault(TValue defaultValue)
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Creates a new Dictionary using the supplied value as the default for missing entries and a specific comparer.
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <param name="comparer"></param>
        public DictionaryWithDefault(TValue defaultValue, IEqualityComparer<TKey> comparer) : base(comparer)
        {
            DefaultValue = defaultValue;
        }
    }
}
