/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100817
 * 说明：视图数据的过滤器
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100817
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Rafy
{
    /// <summary>
    /// 视图数据的过滤器
    /// 如果View.Data在控件上显示的数据需要进行视图级别的过滤，那么它需要实现此接口。
    /// </summary>
    public interface IHasViewFilter
    {
        /// <summary>
        /// 用于检测每个实体是否需要在界面显示的过滤器。
        /// </summary>
        Predicate<object> ViewModelFilter { get; }
    }
}
