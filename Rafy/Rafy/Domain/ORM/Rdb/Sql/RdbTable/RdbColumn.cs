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
        private IRdbColumnInfo _columnInfo;

        internal RdbColumn(RdbTable table, IRdbColumnInfo columnInfo)
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
            get { return _table; }
        }

        /// <summary>
        /// 列的信息
        /// </summary>
        public IRdbColumnInfo Info
        {
            get { return _columnInfo; }
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
            value = _table.DbTypeConverter.ToClrValue(value, _columnInfo.CorePropertyType);
            this.Write(entity, value);
        }

        protected virtual object Read(Entity entity)
        {
            var property = _columnInfo.Property;

            if (RefPropertyHelper.IsRefKeyProperty(property, out var refProperty))
            {
                object key = refProperty.Nullable ?
                    entity.GetRefNullableKey(refProperty) : entity.GetProperty(property);
                return key;
            }

            var value = entity.GetProperty(property);

            return value;
        }

        protected virtual void Write(Entity entity, object value)
        {
            var property = _columnInfo.Property;

            if (RefPropertyHelper.IsRefKeyProperty(property))
            {
                if (value != null)
                {
                    var id = TypeHelper.CoerceValue(property.PropertyType, value);
                    entity.LoadProperty(property, id);
                }
                return;
            }

            if (this.ForceReset(value, property))
            {
                entity.ResetProperty(property);
            }
            else
            {
                entity.LoadProperty(property, value);
            }
        }

        /// <summary>
        /// 判断在设置指定的值时，是否直接使用重设方法。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private bool ForceReset(object value, IProperty property)
        {
            //如果把 null 赋值给一个值类型，则直接还原此属性为默认值。
            bool reset = false;
            if (value == null)
            {
                var propertyType = property.PropertyType;
                //值类型，且这个值类型不是 Nullable时，设置为 null 表示需要重设
                if (propertyType.IsValueType && (!propertyType.IsGenericType || propertyType.GetGenericTypeDefinition() != typeof(Nullable<>)))
                {
                    reset = true;
                }
            }
            else
            {
                //如果是默认值，为节省内存、传输空间，那么不需要设置本地值。
                var meta = property.GetMeta(this);
                reset = object.Equals(value, meta.DefaultValue);
            }

            return reset;
        }
    }
}