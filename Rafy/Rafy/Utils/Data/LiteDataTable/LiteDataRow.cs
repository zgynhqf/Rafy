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
using System.Data;

namespace Rafy.Data
{
    /// <summary>
    /// 数据容器行
    /// <remarks>
    /// 用于存储行数据。
    /// 空值使用 <c>null</c> 表示，而不是 <see cref="DBNull.Value"/>。
    /// </remarks>
    /// </summary>
    /// Title: DataContainerRow
    /// Author: Steven
    /// Version: 1.0
    /// History:
    ///     2006-03-17 Steven [创建] 
    ///     2013-06-07 Huqf [使用 Convert 类来进行类型转换，防止 Oracle 中一些数据类型的转换失败。] 
    [DataContract(Name = "Row")]
    [Serializable]
    public sealed class LiteDataRow : IDataRecord
    {
        internal LiteDataTable _table;

        private object[] _values;

        internal LiteDataRow(LiteDataTable table)
        {
            _table = table;
            _values = new object[_table.Columns.Count];
        }

        /// <summary>
        /// 获取或设置指定列号的数据。
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public object this[int columnIndex]
        {
            get { return _values[columnIndex]; }
            set { _values[columnIndex] = value; }
        }

        /// <summary>
        /// 获取或设置指定列的数据。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[string name]
        {
            get { return this[this.GetOrdinal(name)]; }
            set { this[this.GetOrdinal(name)] = value; }
        }

        /// <summary>
        /// 获取或设置指定列的数据。
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public object this[IHasName name]
        {
            get { return this[this.GetOrdinal(name.Name)]; }
            set { this[this.GetOrdinal(name.Name)] = value; }
        }

        /// <summary>
        /// 所属表格对象。
        /// </summary>
        public LiteDataTable Table
        {
            get { return _table; }
        }

        /// <summary>
        /// 原始数据
        /// </summary>
        [DataMember]
        public object[] Values
        {
            get { return _values; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");

                //只有在反序列化时，_table 会是 null。
                if (_table != null)
                {
                    if (value.Length != _table.Columns.Count)
                    {
                        throw new InvalidOperationException("columns count dismatch.");
                    }
                }

                _values = value;
            }
        }

        /// <summary>
        /// 当前行的列的个数。
        /// </summary>
        public int FieldCount
        {
            get { return _values.Length; }
        }

        #region 获取数据的方法

        public bool IsNull(int i)
        {
            var v = _values[i];
            return v == null || v == DBNull.Value;
        }

        public string GetName(int i)
        {
            return _table.Columns[i].ColumnName;
        }

        public int GetOrdinal(string name)
        {
            return _table.Columns.GetIndex(name);
        }

        public bool GetBoolean(string name)
        {
            return Convert.ToBoolean(_values[GetOrdinal(name)]);
        }

        public byte GetByte(string name)
        {
            return Convert.ToByte(_values[GetOrdinal(name)]);
        }

        public char GetChar(string name)
        {
            return Convert.ToChar(_values[GetOrdinal(name)]);
        }

        public DateTime GetDateTime(string name)
        {
            return Convert.ToDateTime(_values[GetOrdinal(name)]);
        }

        public decimal GetDecimal(string name)
        {
            return Convert.ToDecimal(_values[GetOrdinal(name)]);
        }

        public double GetDouble(string name)
        {
            return Convert.ToDouble(_values[GetOrdinal(name)]);
        }

        public float GetFloat(string name)
        {
            return Convert.ToSingle(_values[GetOrdinal(name)]);
        }

        public Guid GetGuid(string name)
        {
            return (Guid)_values[GetOrdinal(name)];
        }

        public short GetInt16(string name)
        {
            return Convert.ToInt16(_values[GetOrdinal(name)]);
        }

        public int GetInt32(string name)
        {
            return Convert.ToInt32(_values[GetOrdinal(name)]);
        }

        public long GetInt64(string name)
        {
            return Convert.ToInt64(_values[GetOrdinal(name)]);
        }

        public string GetString(string name)
        {
            return Convert.ToString(_values[GetOrdinal(name)]);
        }

        public object GetValue(string name)
        {
            return _values[GetOrdinal(name)];
        }

        public bool GetBoolean(int i)
        {
            return Convert.ToBoolean(_values[i]);
        }

        public byte GetByte(int i)
        {
            return Convert.ToByte(_values[i]);
        }

        public char GetChar(int i)
        {
            return Convert.ToChar(_values[i]);
        }

        public DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(_values[i]);
        }

        public decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(_values[i]);
        }

        public double GetDouble(int i)
        {
            return Convert.ToDouble(_values[i]);
        }

        public float GetFloat(int i)
        {
            return Convert.ToSingle(_values[i]);
        }

        public Guid GetGuid(int i)
        {
            return (Guid)_values[i];
        }

        public short GetInt16(int i)
        {
            return Convert.ToInt16(_values[i]);
        }

        public int GetInt32(int i)
        {
            return Convert.ToInt32(_values[i]);
        }

        public long GetInt64(int i)
        {
            return Convert.ToInt64(_values[i]);
        }

        public string GetString(int i)
        {
            return Convert.ToString(_values[i]);
        }

        public object GetValue(int i)
        {
            return _values[i];
        }

        public int GetOrdinal(IHasName name)
        {
            return _table.Columns.GetIndex(name.Name);
        }

        public bool GetBoolean(IHasName name)
        {
            return Convert.ToBoolean(_values[GetOrdinal(name.Name)]);
        }

        public byte GetByte(IHasName name)
        {
            return Convert.ToByte(_values[GetOrdinal(name.Name)]);
        }

        public char GetChar(IHasName name)
        {
            return Convert.ToChar(_values[GetOrdinal(name.Name)]);
        }

        public DateTime GetDateTime(IHasName name)
        {
            return Convert.ToDateTime(_values[GetOrdinal(name.Name)]);
        }

        public decimal GetDecimal(IHasName name)
        {
            return Convert.ToDecimal(_values[GetOrdinal(name.Name)]);
        }

        public double GetDouble(IHasName name)
        {
            return Convert.ToDouble(_values[GetOrdinal(name.Name)]);
        }

        public float GetFloat(IHasName name)
        {
            return Convert.ToSingle(_values[GetOrdinal(name.Name)]);
        }

        public Guid GetGuid(IHasName name)
        {
            return (Guid)_values[GetOrdinal(name.Name)];
        }

        public short GetInt16(IHasName name)
        {
            return Convert.ToInt16(_values[GetOrdinal(name.Name)]);
        }

        public int GetInt32(IHasName name)
        {
            return Convert.ToInt32(_values[GetOrdinal(name.Name)]);
        }

        public long GetInt64(IHasName name)
        {
            return Convert.ToInt64(_values[GetOrdinal(name.Name)]);
        }

        public string GetString(IHasName name)
        {
            return Convert.ToString(_values[GetOrdinal(name.Name)]);
        }

        public object GetValue(IHasName name)
        {
            return _values[GetOrdinal(name.Name)];
        }

        #endregion

        /// <summary>
        /// 传入一个Index数组，返回对应的值，当传入的Index为<value>-1</value>时，其对应的值为<value>Null</value>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal object[] GetValuesByIndex(int[] index)
        {
            List<object> objects = new List<object>(index.Length);
            foreach (int i in index)
            {
                if (i == -1)
                {
                    objects.Add(null);
                }
                else
                {
                    objects.Add(_values[i]);
                }

            }
            return objects.ToArray();
        }

        #region 显式实现 IDataRecord 接口。

        string IDataRecord.GetDataTypeName(int i)
        {
            return _table.Columns[i].Type.Name;
        }

        Type IDataRecord.GetFieldType(int i)
        {
            return _table.Columns[i].Type;
        }

        bool IDataRecord.IsDBNull(int i)
        {
            return DBNull.Value == _values[i];
        }

        long IDataRecord.GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        long IDataRecord.GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        IDataReader IDataRecord.GetData(int i)
        {
            throw new NotSupportedException();
        }

        int IDataRecord.GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
