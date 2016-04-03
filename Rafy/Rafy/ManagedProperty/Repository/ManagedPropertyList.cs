/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20140513
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20140513 18:11
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 一个托管属性的只读列表。
    /// </summary>
    public class ManagedPropertyList : IList<IManagedProperty>
    {
        /*********************** 代码块解释 *********************************
         * 存储两个列表，用于优化查询性能。
         * List 用于最常用的索引查询操作，
         * Dictionary 用于根据名字来检索的操作。
        **********************************************************************/
        private List<IManagedProperty> _list = new List<IManagedProperty>(10);
        private IDictionary<string ,IManagedProperty> _dic = new Dictionary<string, IManagedProperty>(10);

        internal ManagedPropertyList() { }

        /// <summary>
        /// 唯一的，可修改集合内部数据的操作。
        /// </summary>
        /// <param name="properties"></param>
        internal void AddRange(IEnumerable<IManagedProperty> properties)
        {
            _list.AddRange(properties);
            foreach (var property in properties)
            {
                _dic[property.Name] = property;
            }
        }

        /// <summary>
        /// 可以通过属性的名称来快速查找集合中的托管属性。
        /// 复杂度：Log(n)
        /// </summary>
        /// <param name="propertyName">托管属性名称</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">propertyName</exception>
        public IManagedProperty Find(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) throw new ArgumentNullException("propertyName");

            IManagedProperty res = null;
            if (_dic.TryGetValue(propertyName, out res))
            {
                return res;
            }

            return null;
        }

        /// <summary>
        /// 可以通过属性的名称来快速查找集合中的托管属性。
        /// 复杂度：Log(n)
        /// </summary>
        /// <param name="propertyName">托管属性名称</param>
        /// <param name="ignoreCase">是否忽略大小写。
        /// 注意，如果是忽略大小写的方式，那么是按顺序在集合中进行查询。</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">propertyName</exception>
        public IManagedProperty Find(string propertyName, bool ignoreCase)
        {
            if (ignoreCase)
            {
                for (int i = 0, c = _list.Count; i < c; i++)
                {
                    var item = _list[i];
                    if (item.Name.EqualsIgnoreCase(propertyName))
                    {
                        return item;
                    }
                }
                return null;
            }

            return this.Find(propertyName);
        }

        /// <summary>
        /// 列表的枚举器。
        /// </summary>
        /// <returns></returns>
        public ManagedPropertyListEnumerator GetEnumerator()
        {
            return new ManagedPropertyListEnumerator { _list = _list, _index = -1 };
        }

        //其实，完全可以 List 直接实现：
        //public List<IManagedProperty>.Enumerator GetEnumerator()
        //{
        //    return _list.GetEnumerator();
        //}

        #region IList<IManagedProperty>

        public int IndexOf(IManagedProperty item)
        {
            return _list.IndexOf(item);
        }

        public IManagedProperty this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public bool Contains(IManagedProperty item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(IManagedProperty[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        IEnumerator<IManagedProperty> IEnumerable<IManagedProperty>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (_list as IEnumerable).GetEnumerator();
        }

        #endregion

        #region 只读实现 IList<IManagedProperty>

        public bool IsReadOnly
        {
            get { return true; }
        }

        void IList<IManagedProperty>.Insert(int index, IManagedProperty item)
        {
            throw new NotSupportedException();
        }

        void IList<IManagedProperty>.RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        void ICollection<IManagedProperty>.Add(IManagedProperty item)
        {
            throw new NotSupportedException();
        }

        void ICollection<IManagedProperty>.Clear()
        {
            throw new NotSupportedException();
        }

        bool ICollection<IManagedProperty>.Remove(IManagedProperty item)
        {
            throw new NotSupportedException();
        }

        #endregion
    }

    public struct ManagedPropertyListEnumerator : IEnumerator<IManagedProperty>
    {
        internal List<IManagedProperty> _list;
        internal int _index;

        public IManagedProperty Current
        {
            get { return _list[_index]; }
        }

        void IDisposable.Dispose() { }

        object IEnumerator.Current
        {
            get { return _list[_index]; }
        }

        public bool MoveNext()
        {
            return ++_index < _list.Count;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
