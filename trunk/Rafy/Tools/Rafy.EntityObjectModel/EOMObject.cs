/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130328 23:19
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130328 23:19
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Rafy;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 实体对象模型对象。
    /// </summary>
    public abstract class EOMObject : Extendable
    {
        ///// <summary>
        ///// 用于显示的文本。
        ///// </summary>
        //public string Label { get; set; }

        internal object CodeElement;

        public override string ToString()
        {
            var property = this.GetType().GetProperty("Name");
            if (property != null)
            {
                return property.GetValue(this, null) as string;
            }

            return base.ToString();
        }
    }
}
