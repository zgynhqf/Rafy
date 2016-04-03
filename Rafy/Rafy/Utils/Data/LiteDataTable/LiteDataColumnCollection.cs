/*******************************************************
 * 
 * 作者：Steven
 * 创建日期：2006-03-17
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130524 09:55
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Collections.ObjectModel;

namespace Rafy.Data
{
    /// <summary>
    /// 列定义集合
    /// </summary>
    /// <remarks>
    /// 因为考虑到执行的sql语句中返回的schema和函数中对应的名称会有不同，
    /// 所以需要手工定义Column，如果实际情况中绝大多数情况都是相同的话，
    /// 可以考虑直接从IDataReader中生成列定义
    /// </remarks>
    /// Title: ColumnCollection
    /// Author: Steven
    /// Version: 1.0
    /// History:
    ///     2006-04-15 Steven [创建] 
    [CollectionDataContract]
    [Serializable]
    public class LiteDataColumnCollection : Collection<LiteDataColumn>
    {
        private Dictionary<string, int> _columnIndexCache = null;

        /// <summary>
        /// 查找指定名称的列。
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public LiteDataColumn this[string columnName]
        {
            get
            {
                var index = this.GetIndex(columnName);
                return this[index];
            }
        }

        /// <summary>
        /// return the index of a column according it's name.
        /// </summary>
        /// <param name="columnName">Column Name. Ignore case.</param>
        /// <returns>-1 if not found.</returns>
        public int IndexOf(string columnName)
        {
            if (_columnIndexCache == null)
            {
                _columnIndexCache = new Dictionary<string, int>();
            }

            int columnIndex = 0;
            if (_columnIndexCache.TryGetValue(columnName, out columnIndex))
            {
                return columnIndex;
            }

            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].ColumnName.EqualsIgnoreCase(columnName))
                {
                    _columnIndexCache.Add(columnName, i);
                    return i;
                }
            }

            return -1;
        }

        internal int GetIndex(string columnName)
        {
            var index = this.IndexOf(columnName);
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(string.Format(
                    "column name not found: {0}.", columnName));
            }
            return index;
        }

        /// <summary>
        /// 查找指定名称的列。
        /// 
        /// 如果没有找到，则返回 null。
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        internal LiteDataColumn Find(string columnName)
        {
            var index = this.IndexOf(columnName);
            return index >= 0 ? this[index] : null;
        }

        protected override void ClearItems()
        {
            _columnIndexCache = null;

            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            _columnIndexCache = null;

            base.RemoveItem(index);
        }

        protected override void InsertItem(int index, LiteDataColumn item)
        {
            CheckConflict(item);

            _columnIndexCache = null;

            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, LiteDataColumn item)
        {
            CheckConflict(item);

            _columnIndexCache = null;

            base.SetItem(index, item);
        }

        private void CheckConflict(LiteDataColumn item)
        {
            var exists = this.Find(item.ColumnName);
            if (exists != null) throw new ArgumentException("column already exists.");
        }
    }
}