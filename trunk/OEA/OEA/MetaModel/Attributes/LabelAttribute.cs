/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110315
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100315
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OEA.MetaModel.Attributes
{
    /// <summary>
    /// 用于描述某个类型或成员在界面上显示的字符
    /// </summary>
    public class LabelAttribute : Attribute
    {
        public LabelAttribute(string label)
        {
            Label = label;
        }

        public string Label { get; private set; }
    }
}
