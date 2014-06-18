/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:59
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:59
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 枚举类型
    /// </summary>
    public class EnumType : EOMObject
    {
        public EnumType()
        {
            this.Items = new List<EnumItem>();
        }

        /// <summary>
        /// 枚举名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 枚举类型全名称。
        /// </summary>
        public string TypeFullName { get; set; }

        /// <summary>
        /// 是否聚合根
        /// </summary>
        public bool IsAggtRoot { get; set; }

        /// <summary>
        /// 枚举项
        /// </summary>
        public IList<EnumItem> Items { get; private set; }
    }
}
