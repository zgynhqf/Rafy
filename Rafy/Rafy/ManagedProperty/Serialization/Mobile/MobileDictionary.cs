/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System.Collections.Generic;
using System.Linq;
using Rafy.Serialization;
using System;
using System.Runtime.Serialization;

namespace Rafy.Serialization.Mobile
{
    /// <summary>
    /// Defines a dictionary that can be serialized through
    /// the MobileFormatter.
    /// </summary>
    /// <typeparam name="K">Key value: any primitive or IMobileObject type.</typeparam>
    /// <typeparam name="V">Value: any primitive or IMobileObject type.</typeparam>
    [Serializable]
    public class MobileDictionary<K, V> : Dictionary<K, V>, IMobileObject
    {
        private bool _keyIsMobile;
        private bool _valueIsMobile;

        /// <summary>
        /// Creates an instance of the object.
        /// </summary>
        public MobileDictionary()
        {
            DetermineTypes();
        }

        /// <summary>
        /// Creates an instance of the object based
        /// on the supplied dictionary, whose elements
        /// are copied to the new dictionary.
        /// </summary>
        /// <param name="capacity">The initial number of elements 
        /// the dictionary can contain.</param>
        public MobileDictionary(int capacity)
            : base(capacity)
        {
            DetermineTypes();
        }

        /// <summary>
        /// Creates an instance of the object based
        /// on the supplied dictionary, whose elements
        /// are copied to the new dictionary.
        /// </summary>
        /// <param name="comparer">The comparer to use when
        /// comparing keys.</param>
        public MobileDictionary(IEqualityComparer<K> comparer)
            : base(comparer)
        {
            DetermineTypes();
        }

        /// <summary>
        /// Creates an instance of the object based
        /// on the supplied dictionary, whose elements
        /// are copied to the new dictionary.
        /// </summary>
        /// <param name="dict">Source dictionary.</param>
        public MobileDictionary(Dictionary<K, V> dict)
            : base(dict)
        {
            DetermineTypes();
        }

        private void DetermineTypes()
        {
            _keyIsMobile = typeof(IMobileObject).IsAssignableFrom(typeof(K));
            _valueIsMobile = typeof(IMobileObject).IsAssignableFrom(typeof(V));
        }

        #region Mobile Serialization

        private const string _keyPrefix = "k";

        private const string _valuePrefix = "v";

        void IMobileObject.SerializeRef(ISerializationContext context)
        {
            var formatter = context.RefFormatter;

            int count = 0;
            foreach (var key in this.Keys)
            {
                if (_keyIsMobile)
                {
                    context.AddRef(_keyPrefix + count, key);
                }
                else
                {
                    context.AddState(_keyPrefix + count, key);
                }

                if (_valueIsMobile)
                {
                    context.AddRef(_valuePrefix + count, this[key]);
                }
                else
                {
                    V value = this[key];
                    context.AddState(_valuePrefix + count, value);
                }

                count++;
            }
        }

        void IMobileObject.SerializeState(ISerializationContext info)
        {
            info.AddState("count", this.Keys.Count);
        }

        void IMobileObject.DeserializeState(ISerializationContext info) { }

        void IMobileObject.DeserializeRef(ISerializationContext info)
        {
            int count = info.GetState<int>("count");

            for (int index = 0; index < count; index++)
            {
                K key;
                if (_keyIsMobile)
                    key = (K)info.GetRef(_keyPrefix + index);
                else
                    key = info.GetState<K>(_keyPrefix + index);

                V value;
                if (_valueIsMobile)
                    value = (V)info.GetRef(_valuePrefix + index);
                else
                    value = info.GetState<V>(_valuePrefix + index);

                Add(key, value);
            }
        }

        #endregion

        #region System Serialization

        protected MobileDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        #endregion
    }
}