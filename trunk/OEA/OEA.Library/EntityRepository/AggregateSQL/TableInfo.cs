/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101229
 * 说明：获取对象表信息的相关类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101229
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;

using System.Diagnostics;
using OEA.MetaModel;

namespace OEA.Library
{
    /// <summary>
    /// ITable的简单实现。
    /// 可序列化。
    /// </summary>
    [Serializable]
    class TableInfo : ITable
    {
        private string _name;

        private IColumn[] _columns;

        public TableInfo() { }

        public TableInfo(string name, ColumnInfo[] columns)
        {
            this._name = name;
            this._columns = columns;
        }

        public static TableInfo Convert(ITable table)
        {
            var result = new TableInfo();
            result._name = table.Name;

            result._columns = new ColumnInfo[table.Columns.Length];

            for (int i = 0, c = table.Columns.Length; i < c; i++)
            {
                var item = table.Columns[i];
                result._columns[i] = new ColumnInfo(item.Name, item.DataType);
            }

            return result;
        }

        #region ITable Members

        public Type Class
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public IColumn[] Columns
        {
            get
            {
                return _columns;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Schema
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        #endregion
    }

    /// <summary>
    /// IColumn的简单实现。
    /// 可序列化。
    /// </summary>
    [Serializable]
    class ColumnInfo : IColumn
    {
        private string _name;

        private Type _dataType;

        public ColumnInfo(string name, Type dataType)
        {
            _name = name;
            _dataType = dataType;
        }

        public string Name { get { return this._name; } }

        public Type DataType { get { return this._dataType; } }

        public ITable Table { get { throw new NotSupportedException(); } }

        public object GetValue(object obj) { return obj.GetPropertyValue(this._name); }

        public void SetValue(object obj, object val)
        {
            if (DBNull.Value.Equals(val))
                obj.SetPropertyValue(this._name, null);                
            else
                obj.SetPropertyValue(this._name, val);
        }
    }
}
