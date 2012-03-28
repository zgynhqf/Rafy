/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110425
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110425
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace OEA.Module.WPF.Controls
{
    public static class ControlHelper
    {
        public static ComboListControl CreateComboListControl(ListObjectView view)
        {
            var titleProperty = view.Meta.TitleProperty;
            if (titleProperty == null) throw new InvalidProgramException(view.EntityType.Name + "类没有设置 Title 属性。");

            var displayMember = titleProperty.Name;
            var ddl = new ComboListControl(view)
            {
                TextPath = displayMember,
                Name = "下拉列表",
                VerticalAlignment = VerticalAlignment.Center,
                MinWidth = 150,
            };

            ddl.DataContext = view;
            ddl.SetBinding(ComboListControl.TextProperty, "CurrentObject." + displayMember);

            //当 View 发生 Refreshed 事件时，很可能表示有底层数据改变了，但是没有级联通知到界面上，
            //所以当 View 的控件被 Refresh 后，这个下拉框也需要主动进行刷新。
            view.Refreshed += (o, e) =>
            {
                ddl.GetBindingExpression(ComboListControl.TextProperty).UpdateTarget();
            };

            return ddl;
        }
    }
}
