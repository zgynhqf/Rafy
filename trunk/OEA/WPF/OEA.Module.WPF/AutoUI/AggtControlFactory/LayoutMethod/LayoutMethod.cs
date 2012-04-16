/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110215
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100215
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 布局方法：为多个控件进行布局
    /// 
    /// 所有子类必须是无状态的。
    /// </summary>
    public abstract class LayoutMethod
    {
        /// <summary>
        /// 为多个控件进行布局，然后返回组合的控件。
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        public FrameworkElement Arrange(RegionContainer regions)
        {
            if (regions == null) throw new ArgumentNullException("regions");

            var result = this.ArrageCore(regions);

            return result;
        }

        /// <summary>
        /// 为多个控件进行布局，然后返回组合的控件。
        /// </summary>
        /// <param name="regions"></param>
        /// <returns></returns>
        protected abstract FrameworkElement ArrageCore(RegionContainer regions);
    }
}