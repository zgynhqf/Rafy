/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121120 20:09
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121120 20:09
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;
using Rafy.MetaModel;

namespace Rafy.Domain
{
    /// <summary>
    /// 声明引用 Id 属性的参数对象。
    /// </summary>
    public class RegisterRefIdArgs<TKey> : PropertyMetadata<TKey>
    {
        public RegisterRefIdArgs()
        {
            //引用 Id 属性如果是引用类型，那它的默认值，应该是 null。
            if (typeof(TKey).IsClass)
            {
                this.DefaultValue = (TKey)(object)null;
            }
        }

        /// <summary>
        /// 引用属性的类型。
        /// </summary>
        public ReferenceType ReferenceType { get; set; }
    }
}