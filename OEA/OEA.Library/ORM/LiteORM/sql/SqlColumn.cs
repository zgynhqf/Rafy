using System;
using System.Data;
using System.Diagnostics;

namespace OEA.ORM.sqlserver
{
    [DebuggerDisplay("Name:{Name}")]
    public class SqlColumn : IColumn
    {
        private ITable _table;
        private IDataBridge _data;
        private string _name;
        private string _propertyName;
        private string _refPropertyName;
        private bool _pk = false;
        private bool _id = false;
        private int _ordinal = -1;

        public SqlColumn(ITable table, string name, IDataBridge data)
        {
            this._table = table;
            this._name = name.ToLower();
            this._data = data;
        }

        /// <summary>
        /// 顺序号
        /// </summary>
        public int Ordinal
        {
            get { return _ordinal; }
            set { _ordinal = value; }
        }

        public ITable Table
        {
            get { return _table; }
        }

        public Type DataType
        {
            get { return _data.DataType; }
        }

        public string Name
        {
            get { return _name; }
        }

        public string PropertyName
        {
            get
            {
                if (string.IsNullOrEmpty(_propertyName)) return this._name;

                return _propertyName;
            }
            set { _propertyName = (value == null) ? null : value.ToLower(); }
        }

        public string RefPropertyName
        {
            get
            {
                if (string.IsNullOrEmpty(this._refPropertyName)) return this._name;

                return this._refPropertyName;
            }
            set { this._refPropertyName = (value == null) ? null : value.ToLower(); }
        }

        public bool IsPK
        {
            get { return _pk; }
            set { _pk = value; }
        }

        public bool IsID
        {
            get { return _id; }
            set { _id = value; }
        }

        public bool IsReadable
        {
            get { return _data.Readable; }
        }

        public bool IsWriteable
        {
            get { return _data.Writeable; }
        }

        public void SetParameterValue(IDbDataParameter p, object obj)
        {
            object val = _data.Read(obj);
            if (val == null)
                p.Value = DBNull.Value;
            else
                p.Value = val;
        }

        public void SetValue(object obj, object val)
        {
            if (DBNull.Value.Equals(val))
                _data.Write(obj, null);
            else
                _data.Write(obj, val);
        }

        public object GetValue(object obj)
        {
            object val = _data.Read(obj);
            if (val == null)
                return DBNull.Value;
            else
                return val;
        }
    }
}
