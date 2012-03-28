using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace hxy.Common.Collection
{
    public class LRUCache<K, V>
    {
        private readonly Dictionary<K, V> _dict;

        private readonly LinkedList<K> _queue = new LinkedList<K>();

        private readonly object _syncRoot = new object();

        private readonly int _max;

        public LRUCache(int capacity, int max)
        {
            _dict = new Dictionary<K, V>(capacity);
            _max = max;
        }

        public void Add(K key, V value)
        {
            lock (_syncRoot)
            {
                checkAndTruncate();
                _queue.AddFirst(key);   //O(1)
                _dict[key] = value;     //O(1)
            }
        }

        private void checkAndTruncate()
        {
            lock (_syncRoot)
            {
                int count = _dict.Count;                        //O(1)
                if (count >= _max)
                {
                    int needRemoveCount = count / 10;
                    for (int i = 0; i < needRemoveCount; i++)
                    {
                        _dict.Remove(_queue.Last.Value);        //O(1)
                        _queue.RemoveLast();                    //O(1)
                    }
                }
            }
        }

        public void Delete(K key)
        {
            lock (_syncRoot)
            {
                _dict.Remove(key); //(1)
                _queue.Remove(key); // O(n)
            }
        }

        public V Get(K key)
        {
            lock (_syncRoot)
            {
                V ret;
                _dict.TryGetValue(key, out ret);    //O(1)
                _queue.Remove(key);                 //O(n)
                _queue.AddFirst(key);               //(1)
                return ret;
            }
        }
    }
}
