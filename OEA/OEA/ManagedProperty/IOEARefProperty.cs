using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.ManagedProperty;

namespace OEA.MetaModel
{
    /// <summary>
    /// 引用属性都必须实现这个接口
    /// </summary>
    public interface IOEARefProperty : IManagedProperty
    {
        /// <summary>
        /// 为某个对象获取本属性的元数据
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        new IOEARefPropertyMetadata GetMeta(object owner);

        /// <summary>
        /// 为某个类型获取本属性的元数据
        /// </summary>
        /// <param name="ownerType"></param>
        /// <returns></returns>
        new IOEARefPropertyMetadata GetMeta(Type ownerType);
    }
}
