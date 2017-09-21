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

        public virtual bool CanInsert
        {
            get
            {
                //Sql Server 中的 Identity 列是不需要插入的。
                return !_columnInfo.IsIdentity;
            }
        }

        /// <summary>
        /// 读取实体中本列对应的属性的值，该值将被写入到数据库中对应的列。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public virtual object ReadParameterValue(Entity entity)
        {
            object value = this.Read(entity);
            return this.ConvertToParameterValue(value);
        }

        public virtual object ConvertToParameterValue(object value)
        {
            return value ?? DBNull.Value;
        }

        /// <summary>
        /// 把数据库中列的值写入到实体对应的属性中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="val"></param>
        public void LoadValue(Entity entity, object val)
        {
            if (val == DBNull.Value) { val = null; }
            this.Write(entity, val);
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

        /// <summary>
        /// 写入操作，PatrickLiu修改了访问修饰符，从internal修改为pubic
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="val"></param>
        public virtual void Write(Entity entity, object val)
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