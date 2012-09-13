using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ORM;
using OEA.ManagedProperty;
using OEA.Library;
using OEA.Reflection;

namespace OEA.ORM
{
    public class ManagedPropertyBridge : IDataBridge
    {
        private IManagedProperty _property;

        public ManagedPropertyBridge(IManagedProperty field)
        {
            this._property = field;
        }

        public bool Readable
        {
            get { return true; }
        }

        public bool Writeable
        {
            get { return !this._property.IsReadOnly; }
        }

        public Type DataType
        {
            get
            {
                var refPropertyInfo = this._property as IRefProperty;
                return refPropertyInfo != null ? typeof(int) : this._property.PropertyType;
            }
        }

        public object Read(object obj)
        {
            var entity = obj as Entity;

            //如果是外键，则只需要设置 Id
            var refPropertyInfo = this.Property as IRefProperty;
            if (refPropertyInfo != null)
            {
                var lazyRef = entity.GetLazyRef(refPropertyInfo);
                return lazyRef.NullableId;
            }
            else
            {
                var value = entity.GetProperty(this.Property);
                return this.OnValueRead(value);
            }
        }

        public void Write(object obj, object val)
        {
            var entity = obj as Entity;

            //如果是外键，则只需要设置 Id
            var refPropertyInfo = this.Property as IRefProperty;
            if (refPropertyInfo != null)
            {
                if (val != null)
                {
                    var lazyRef = entity.GetLazyRef(refPropertyInfo);
                    var id = TypeHelper.CoerceValue<int>(val);
                    lazyRef.LoadId(id);
                }
                //else do nothing
            }
            else
            {
                val = this.OnValueWriting(val);
                entity.LoadProperty(this.Property, val);
            }
        }

        protected virtual object OnValueWriting(object value)
        {
            return value;
        }

        protected virtual object OnValueRead(object value)
        {
            return value;
        }

        protected IManagedProperty Property
        {
            get { return this._property; }
        }
    }
}
