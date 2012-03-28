/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110406
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110406
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OEA.MetaModel;

namespace OEA.MetaModel.Attributes
{
    /// <summary>
    /// 选择新增记录时根据此方法进行判断是否可以添加进父窗口
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class CanAddMethodAttribute : Attribute { }
}
