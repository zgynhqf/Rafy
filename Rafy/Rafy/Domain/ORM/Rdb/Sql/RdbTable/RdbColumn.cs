using System;
using System.Data;
using System.Diagnostics;
using Rafy.ManagedProperty;
using Rafy.Reflection;
using Rafy.MetaModel;
using Rafy.DbMigration;

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

        /// <summary>
        /// 此方法用于判断是否需要将本列在 Insert 语句中插入。
        /// </summary>
        /// <param name="withIdentity">表示当前的 Insert 语句是需要强制插入 Identity 列的。</param>
        /// <returns></returns>
        public virtual bool ShouldInsert(bool withIdentity)
        {
            //默认情况下，Identity 都不应该在 Insert 语句中插入。
            return withIdentity || !_columnInfo.IsIdentity;
        }

        /// <summary>
        /// 读取实体中本列对应的属性的值，该值将被写入到数据库中对应的列。
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public object ReadDbParameterValue(Entity entity)
        {
            object value = this.Read(entity);
            value = _table.DbTypeConverter.ToDbParameterValue(value);
            return value;
        }

        /// <summary>
        /// 把数据库中列的值写入到实体对应的属性中。
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="value"></param>
        public void WritePropertyValue(Entity entity, object value)
        {
            value = _table.DbTypeConverter.ToClrValue(value, _columnInfo.PropertyType);
            this.Write(entity, value);
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

        private void Write(Entity entity, object value)
        {
            var refIdProperty = _columnInfo.Property as IRefIdProperty;
            if (refIdProperty != null)
            {
                if (value != null)
                {
                    var id = TypeHelper.CoerceValue(refIdProperty.PropertyType, value);
                    entity.LoadProperty(refIdProperty, id);
                }
                return;
            }

            entity.LoadProperty(_columnInfo.Property, value);
        }
    }
}