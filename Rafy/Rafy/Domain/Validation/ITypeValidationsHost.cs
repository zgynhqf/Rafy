/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120330
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120330
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.Domain.Validation
{
    /// <summary>
    /// 类型规则的存储器。
    /// 内部使用。
    /// </summary>
    internal interface ITypeValidationsHost
    {
        /// <summary>
        /// 类型规则集合
        /// </summary>
        ValidationRulesManager Rules { get; set; }

        /// <summary>
        /// 是否已经添加了所有的类型规则。
        /// </summary>
        bool TypeRulesAdded { get; set; }
    }

    internal class TypeValidationsHost : ITypeValidationsHost
    {
        private static Dictionary<Type, TypeValidationsHost> _dic = new Dictionary<Type, TypeValidationsHost>();

        public static TypeValidationsHost FindOrCreate(Type type)
        {
            TypeValidationsHost res = null;

            if (!_dic.TryGetValue(type, out res))
            {
                lock (_dic)
                {
                    if (!_dic.TryGetValue(type, out res))
                    {
                        res = new TypeValidationsHost();

                        _dic.Add(type, res);
                    }
                }
            }

            return res;
        }

        private TypeValidationsHost() { }

        public ValidationRulesManager Rules { get; set; }

        public bool TypeRulesAdded { get; set; }
    }
}