using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Essential.Collections
{
    class OrderedDictionary<TKey, TValue> : KeyedCollection<TKey, KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>
    {
        public OrderedDictionary()
        {
        }

        public OrderedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> keyValuePairs)
        {
            foreach (var kvp in keyValuePairs)
            {
                Add(kvp);
            }
        }

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get
            {
                return this[key].Value;
            }
            set
            {
                if (ContainsKey(key))
                {
                    Remove(key);
                }
                Add(new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var keys = new Collection<TKey>();
                foreach (var item in this)
                {
                    keys.Add(item.Key);
                }
                return keys;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var values = new Collection<TValue>();
                foreach (var item in this)
                {
                    values.Add(item.Value);
                }
                return values;
            }
        }

        public void Add(TKey key, TValue value)
        {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public bool ContainsKey(TKey key)
        {
            return Contains(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (Contains(key))
            {
                value = this[key].Value;
                return true;
            }
            value = default(TValue);
            return false;
        }

        protected override TKey GetKeyForItem(KeyValuePair<TKey, TValue> item)
        {
            return item.Key;
        }
    }
}
