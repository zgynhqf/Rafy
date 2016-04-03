/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20111110
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20111110
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Rafy.ManagedProperty;

namespace Rafy.Domain
{
    /// <summary>
    /// Rafy 实体框架中的托管属性
    /// </summary>
    public interface IProperty : IManagedProperty
    {
        /// <summary>
        /// Rafy 属性的类型
        /// </summary>
        PropertyCategory Category { get; }

        #region 冗余属性

        /// <summary>
        /// 本托管属性是否是一个冗余属性。
        /// </summary>
        bool IsRedundant { get; }

        /// <summary>
        /// 如果本托管属性是一个冗余属性，则这里返回它对应的冗余路径。
        /// </summary>
        RedundantPath RedundantPath { get; }

        /// <summary>
        /// 本托管属性是否在其它类上被声明了冗余属性的路径上
        /// </summary>
        bool IsInRedundantPath { get; }

        /// <summary>
        /// 其它类声明的本依赖属性的冗余属性路径
        /// </summary>
        IEnumerable<RedundantPath> InRedundantPathes { get; }

        #endregion
    }
}