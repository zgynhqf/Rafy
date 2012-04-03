/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Reflection;
using OEA.Core;
using OEA.ManagedProperty;
using OEA.Library;

namespace OEA.ORM.sqlserver
{
    /// <summary>
    /// 从 IManagedProperty 到 IDataBridge
    /// </summary>
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
            get { return true; }
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
            var refPropertyInfo = this._property as IRefProperty;
            if (refPropertyInfo != null)
            {
                var lazyRef = entity.GetLazyRef(refPropertyInfo);
                return lazyRef.NullableId;
            }
            else
            {
                return entity.GetProperty(this._property);
            }
        }

        public void Write(object obj, object val)
        {
            var entity = obj as Entity;

            //如果是外键，则只需要设置 Id
            var refPropertyInfo = this._property as IRefProperty;
            if (refPropertyInfo != null)
            {
                if (val != null)
                {
                    var lazyRef = entity.GetLazyRef(refPropertyInfo).CastTo<ILazyEntityRefInternal>();
                    var id = TypeHelper.CoerceValue<int>(val);
                    lazyRef.LoadId(id);
                }
                //else do nothing
            }
            else
            {
                entity.LoadProperty(this._property, val);
            }
        }
    }
}
