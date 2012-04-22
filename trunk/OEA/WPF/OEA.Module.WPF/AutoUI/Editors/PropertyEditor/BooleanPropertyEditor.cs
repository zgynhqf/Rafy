/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100217
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using OEA.Editors;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.Editors
{
    public class BooleanPropertyEditor : WPFPropertyEditor
    {
        protected BooleanPropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            var cb = new CheckBox()
            {
                Name = this.Meta.Name
            };

            this.ResetBinding(cb);

            this.SetAutomationElement(cb);

            return cb;
        }

        protected override DependencyProperty BindingProperty()
        {
            return CheckBox.IsCheckedProperty;
        }
    }
}