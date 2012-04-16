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
using System.Windows.Controls;
using OEA.Module;
using OEA.Module.WPF.Layout;
using OEA.MetaModel.View;

namespace OEA.Module.WPF
{
    /// <summary>
    /// 传统的布局方法
    /// 90% 以上的场景都可以通过传统布局实现
    /// </summary>
    /// <typeparam name="TTraditionalLayoutControl">
    /// 传统布局控件（用户自定义控件），注意，此控件需要继承自 FrameworkElement
    /// </typeparam>
    public class TraditionalLayoutMethod<TTraditionalLayoutControl> : LayoutMethod
        where TTraditionalLayoutControl : ITraditionalLayoutControl, new()
    {
        protected override FrameworkElement ArrageCore(RegionContainer regions)
        {
            var container = new TTraditionalLayoutControl();

            var components = new TraditionalComponents(regions);
            container.Arrange(components);

            return container.CastTo<FrameworkElement>();
        }
    }
}