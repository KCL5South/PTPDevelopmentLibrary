using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.Linq;
using PTPDevelopmentLibrary.Constants;
namespace PTPDevelopmentLibrary.Framework
{
    /// <summary>
    /// A simple implementation of a Dictionary that is friendly with WPF and Silverlight's binding engine.
    /// </summary>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    [CollectionDataContract(Name = "DictionaryModel_{0}_{1}", Namespace = SerializationConstants.FrameworkNamespace)]
    public class DictionaryModel<TKey, TValue> : CollectionModel<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public DictionaryModel() 
            : base()
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dic">An <see cref="IDictionary{TKey, TValue}"/> object that will be copied into 
        /// this dictionary.</param>
        public DictionaryModel(IDictionary<TKey, TValue> dic)
            : base(dic)
        { }

        #region IDictionary<TKey,TValue> Members

        /// <summary>
        /// Adds a new entry to the dictionary.
        /// </summary>
        /// <param name="key">The key to designate the mapping.</param>
        /// <param name="value">The value of the mapping.</param>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.ArgumentException">An element with the same key already exists in the dictionary.</exception>
        /// <exception cref="System.NotSupportedException">The Dictionary is Read-Only.</exception>
        public void Add(TKey key, TValue value)
        {
            if (key == null)
                throw new System.ArgumentNullException("key");
            if (this.ContainsKey(key))
                throw new System.ArgumentException("An element with the same key already exists in the dictionary.");
            base.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// Call this method to see if a given key exists in the dictionary.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <returns>A boolean value indicating whether the key exists in the dictionary.</returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public bool ContainsKey(TKey key)
        {
            if (key == null)
                throw new System.ArgumentNullException("key");
            return this.Any(a => a.Key.Equals(key));
        }

        /// <summary>
        /// Gets the collection of keys that exist in the dictionary.
        /// </summary>
        public ICollection<TKey> Keys
        {
            get
            {
                return (from i in this
                        select i.Key).ToList();
            }
        }

        /// <summary>
        /// Call this method to remove a mapping from the dictionary.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <returns>A boolean value signifiying if the removal was successful.</returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.NotSupportedException">The Dictionary is Read-Only.</exception>
        public bool Remove(TKey key)
        {
            if (key == null)
                throw new System.ArgumentNullException("key");
            KeyValuePair<TKey, TValue> target = this.FirstOrDefault(a => a.Key.Equals(key));
            return base.Remove(target);
        }

        /// <summary>
        /// Call this method to try to retrieve a value given a key.
        /// </summary>
        /// <param name="key">The key to search for.</param>
        /// <param name="value">The value that will be populated with the value if the mapping exists.</param>
        /// <returns>A boolean value indicating if the key was found or not.</returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null)
                throw new System.ArgumentNullException("key");

            KeyValuePair<TKey, TValue> temp = new KeyValuePair<TKey, TValue>();
            value = temp.Value;
            try
            {
                KeyValuePair<TKey, TValue> target = this.First(a => a.Key.Equals(key));
                value = target.Value;
                return true;
            }
            catch
            { return false; }
        }

        /// <summary>
        /// Gets a collection of the values within the dictionary.
        /// </summary>
        public ICollection<TValue> Values
        {
            get
            {
                return (from i in this
                        select i.Value).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the value associated with the given key.
        /// </summary>
        /// <param name="key">The key who's value is being retrieved or replaced.</param>
        /// <returns>The value mapped to the key.</returns>
        /// <exception cref="System.ArgumentNullException">key is null.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">The key is not found.</exception>
        /// <exception cref="System.NotSupportedException">The dictionary is read-only.</exception>
        public TValue this[TKey key]
        {
            get
            {
                if (key == null)
                    throw new System.ArgumentNullException("key");
                return this.FirstOrDefault(a => a.Key.Equals(key)).Value;
            }
            set
            {
                if (key == null)
                    throw new System.ArgumentNullException("key");  
                KeyValuePair<TKey, TValue> newPair = new KeyValuePair<TKey,TValue>(key, value);
                int index = this.IndexOf(this.FirstOrDefault(a => a.Key.Equals(key)));
                this[index] = newPair;
            }
        }

        #endregion
    }
}