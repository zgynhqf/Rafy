/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 10:25
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 10:25
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.EntityObjectModel
{
    /// <summary>
    /// 引用的类型。
    /// </summary>
    public enum ReferenceType
    {
        /// <summary>
        /// 一般的外键引用
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 此引用表示父实体的引用
        /// </summary>
        Parent = 1,

        /// <summary>
        /// 此引用表示子实体的引用，一对一的子实体关系。
        /// </summary>
        Child = 2
    }
}