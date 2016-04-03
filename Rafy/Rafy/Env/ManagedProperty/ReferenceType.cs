/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110316
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100316
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.ManagedProperty
{
    /// <summary>
    /// 引用的类型。
    /// </summary>
    public enum ReferenceType
    {
        /// <summary>
        /// 一般的外键引用
        /// </summary>
        Normal,

        /// <summary>
        /// 此引用表示父实体的引用
        /// </summary>
        Parent,

        /// <summary>
        /// 此引用表示子实体的引用，一对一的子实体关系。
        /// <remarks>
        /// 后期，可能不再需要这种一对一的子实体关系。
        /// 主要是因为用的地方比较少，而且也可以直接使用一对多的子实体来表示。另外，Web 框架中目前也不支持。
        /// 由于目前已经写了比较多的代码来支持，所以先暂时不删除。
        /// </remarks>
        /// </summary>
        Child
    }
}
