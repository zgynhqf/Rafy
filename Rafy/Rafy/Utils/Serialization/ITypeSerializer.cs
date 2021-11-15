/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20211116
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20211116 03:24
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Rafy
{
    /// <summary>
    /// Type 与 TypeName 的映射管理器
    /// </summary>
    public interface ITypeSerializer
    {
        /// <summary>
        /// 获取指定的类型的类型缩写
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string Serialize(Type type);

        /// <summary>
        /// 通过缩写，返回对应的类型。
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        Type Deserialize(string typeName);
    }

    /// <summary>
    /// 使用 type.AssemblyQualifiedName 来实现的名称检索器。
    /// </summary>
    public class TypeSerializer : ITypeSerializer
    {
        /// <summary>
        /// 系统默认使用的 ITypeSerializer。
        /// 开发者可以设置此属性来重写自己的 Type 序列化逻辑。
        /// </summary>
        public static ITypeSerializer Provider { get; set; } = new TypeSerializer();

        private TypeSerializer() { }

        Type ITypeSerializer.Deserialize(string typeName)
        {
            var res = Type.GetType(typeName);
            if (res == null) throw new InvalidOperationException(typeName + " 不是可加载的类型，加载失败。");
            return res;
        }

        string ITypeSerializer.Serialize(Type type)
        {
            return type.AssemblyQualifiedName;
        }

        public static string Serialize(Type type)
        {
            return Provider.Serialize(type);
        }

        public static Type Deserialize(string typeName)
        {
            return Provider.Deserialize(typeName);
        }
    }
}