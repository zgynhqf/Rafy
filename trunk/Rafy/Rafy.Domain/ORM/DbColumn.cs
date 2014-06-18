using System;
using System.Data;
using System.Diagnostics;
using Rafy.ManagedProperty;
using Rafy.Reflection;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    [DebuggerDisplay("Name:{Name}")]
    internal class DbColumn : IColumn
    {
        private DbTable _table;
        private string _name;
        private bool _isIdentity;
        private PropertyMeta _meta;
        private IProperty _property;

        internal DbColumn(DbTable table, string name, PropertyMeta meta)
        {
            _table = table;
            _name = name;
            _meta = meta;
            _property = meta.ManagedProperty as IProperty;
        }

        public ITable Table
        {
            get { return this._table; }
        }

        public Type DataType
        {
            get { return _meta.PropertyType; }
        }

        public string Name
        {
            get { return this._name; }
        }

        public IProperty Property
        {
            get { return _property; }
        }

        /// <summary>
        /// 是否为自增长列。
        /// </summary>
        public bool IsIdentity
        {
            get { return this._isIdentity; }
            set { this._isIdentity = value; }
        }

        /// <summary>
        /// 是否为主键列。
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        public bool IsLOB
        {
            get { return _property.Category == PropertyCategory.LOB; }
        }

        public bool IsReadOnly
        {
            get { return _property.IsReadOnly; }
        }

        public object ReadParameterValue(Entity entity)
        {
            object val = this.Read(entity);
            return val == null ? DBNull.Value : val;
        }

        public void LoadValue(Entity entity, object val)
        {
            if (val == DBNull.Value) { val = null; }
            this.Write(entity, val);
        }

        public object GetValue(Entity entity)
        {
            object val = this.Read(entity);
            return val == null ? DBNull.Value : val;
        }

        private object Read(Entity entity)
        {
            var refIdProperty = this._property as IRefIdProperty;
            if (refIdProperty != null)
            {
                object id = refIdProperty.Nullable ?
                    entity.GetRefNullableId(refIdProperty) : entity.GetRefId(refIdProperty);
                return id;
            }

            var value = entity.GetProperty(this._property);
            return value;
        }

        internal virtual void Write(Entity entity, object val)
        {
            var refIdProperty = this._property as IRefIdProperty;
            if (refIdProperty != null)
            {
                if (val != null)
                {
                    var id = TypeHelper.CoerceValue(refIdProperty.PropertyType, val);
                    entity.LoadProperty(refIdProperty, id);
                }
                return;
            }

            entity.LoadProperty(this._property, val);
        }
    }
}