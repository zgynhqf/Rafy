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

namespace OEA
{
    /// <summary>
    /// 传统的布局方法
    /// </summary>
    /// <typeparam name="TContainerType"></typeparam>
    public class TraditionalLayoutMethod<TContainerType> : LayoutMethod
        where TContainerType : TraditionalLayout, new()
    {
        protected override FrameworkElement ArrageCore(RegionContainer regions)
        {
            //尝试布局以下内容：
            //Main, Toolbar, Navigate, Condition, Result, List, Detail, Children

            var container = new TContainerType();

            container.OnArraging(regions.BlocksInfo);

            var control = regions.TryGetControl(TraditionalRegions.Main);
            container.TryArrangeMain(control);

            control = regions.TryGetControl(TraditionalRegions.CommandsContainer);
            container.TryArrangeCommandsContainer(control);

            control = regions.TryGetControl(SurrounderType.Navigation.GetDescription());
            container.TryArrangeNavigate(control);

            control = regions.TryGetControl(SurrounderType.Condition.GetDescription());
            container.TryArrangeCondition(control);

            control = regions.TryGetControl(SurrounderType.Result.GetDescription());
            container.TryArrangeResult(control);

            control = regions.TryGetControl(SurrounderType.List.GetDescription());
            container.TryArrangeList(control);

            control = regions.TryGetControl(SurrounderType.Detail.GetDescription());
            container.TryArrangeDetail(control);

            var children = regions.GetChildrenRegions().ToList();
            container.TryArrangeChildren(children);

            container.OnArranged();

            return container;
        }
    }
}