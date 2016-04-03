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


namespace Rafy.MetaModel.Attributes
{
    /// <summary>
    /// 所有孩子实体对象都应该标记这个属性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ChildEntityAttribute : EntityAttribute { }
}