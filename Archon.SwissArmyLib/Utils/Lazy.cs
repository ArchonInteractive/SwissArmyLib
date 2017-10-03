using System;

namespace Archon.SwissArmyLib.Utils
{
    /// <summary>
    /// Provides support for lazy initialization.
    /// 
    /// If you're on .NET 4.0 or higher you might want to use System.Lazy instead.
    /// </summary>
    /// <typeparam name="T">The type of the lazily initialized value.</typeparam>
    public class Lazy<T>
    {
        private T _value;
        private readonly Func<T> _valueFactory;
        private bool _isValueCreated;

        /// <summary>
        /// Gets the lazily initialized value of this <see cref="Lazy{T}"/> instance.
        /// </summary>
        public T Value
        {
            get
            {
                if (!_isValueCreated)
                    Initialize();

                return _value;
            }
        }

        /// <summary>
        /// Gets whether the value has been initialized.
        /// </summary>
        public bool IsValueCreated
        {
            get { return _isValueCreated; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class. 
        /// The default constructor will be used to create the lazily initialized value.
        /// </summary>
        public Lazy() : this(Activator.CreateInstance<T>)
        {
                
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lazy{T}"/> class. 
        /// The specified initialization function will be used.
        /// </summary>
        /// <param name="valueFactory">The function to use for producing the lazily initialized value.</param>
        public Lazy(Func<T> valueFactory)
        {
            if (ReferenceEquals(valueFactory, null))
                throw new ArgumentNullException("valueFactory");

            _valueFactory = valueFactory;
        }

        private void Initialize()
        {
            _value = _valueFactory();
            _isValueCreated = true;
        }

        /// <summary>
        /// Creates a string representation of <see cref="Value"/> property for this instance.
        /// </summary>
        /// <returns>The result of ToString() on the lazily initialized value.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        /// <summary>
        /// Explicitly casts the <paramref name="lazy"/> to its lazily initialized value.
        /// </summary>
        /// <param name="lazy"></param>
        /// <returns><see cref="Value"/></returns>
        public static explicit operator T(Lazy<T> lazy)
        {
            return lazy.Value;
        }
    }
}
