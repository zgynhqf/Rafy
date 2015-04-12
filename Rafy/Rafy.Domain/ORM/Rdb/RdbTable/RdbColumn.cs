using System;
using System.Data;
using System.Diagnostics;
using Rafy.ManagedProperty;
using Rafy.Reflection;
using Rafy.MetaModel;

namespace Rafy.Domain.ORM
{
    [DebuggerDisplay("Name:{Name}")]
    internal class RdbColumn
    {
        private RdbTable _table;
        private IPersistanceColumnInfo _columnInfo;

        internal RdbColumn(RdbTable table, IPersistanceColumnInfo columnInfo)
        {
            _table = table;
            _columnInfo = columnInfo;
        }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name
        {
            get { return _columnInfo.Name; }
        }

        public RdbTable Table
        {
            get { return this._table; }
        }

        /// <summary>
        /// 列的信息
        /// </summary>
        public IPersistanceColumnInfo Info
        {
            get { return this._columnInfo; }
        }

        public bool IsLOB
        {
            get { return _columnInfo.Property.Category == PropertyCategory.LOB; }
        }

        public bool IsReadOnly
        {
            get { return _columnInfo.Property.IsReadOnly; }
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
            var refIdProperty = _columnInfo.Property as IRefIdProperty;
            if (refIdProperty != null)
            {
                object id = refIdProperty.Nullable ?
                    entity.GetRefNullableId(refIdProperty) : entity.GetRefId(refIdProperty);
                return id;
            }

            var value = entity.GetProperty(_columnInfo.Property);
            return value;
        }

        internal virtual void Write(Entity entity, object val)
        {
            var refIdProperty = _columnInfo.Property as IRefIdProperty;
            if (refIdProperty != null)
            {
                if (val != null)
                {
                    var id = TypeHelper.CoerceValue(refIdProperty.PropertyType, val);
                    entity.LoadProperty(refIdProperty, id);
                }
                return;
            }

            entity.LoadProperty(_columnInfo.Property, val);
        }
    }
}