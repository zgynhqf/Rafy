/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:36
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:36
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
    /// 子属性集合
    /// </summary>
    public class ChildCollection : PropertyCollection<Child>
    {
        internal ChildCollection(EntityType parent) : base(parent) { }
    }
}
