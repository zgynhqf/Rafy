using System;
using System.Data;
using System.Diagnostics;

namespace OEA.ORM
{
    [DebuggerDisplay("Name:{Name}")]
    public class DbColumn : IColumn
    {
        private ITable _table;
        private IDataBridge _data;
        private string _name;
        private string _propertyName;
        private string _refPropertyName;
        private bool _pkId = false;

        public DbColumn(ITable table, string name, IDataBridge data)
        {
            this._table = table;
            this._name = name.ToLower();
            this._data = data;
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

        public bool IsPKID
        {
            get { return _pkId; }
            set { _pkId = value; }
        }

        public bool IsReadable
        {
            get { return _data.Readable; }
        }

        public bool IsWriteable
        {
            get { return _data.Writeable; }
        }

        public object ReadParameterValue(object obj)
        {
            object val = _data.Read(obj);
            return val == null ? DBNull.Value : val;
        }

        public void LoadValue(object obj, object val)
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
