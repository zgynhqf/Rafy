/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120602 19:18
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120602 19:18
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Automation;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace OEA.Module.WPF.Editors
{
    /// <summary>
    /// 整型数字使用的带上下箭头的编辑器
    /// </summary>
    public class IntegerUpDownPropertyEditor : WPFPropertyEditor
    {
        protected IntegerUpDownPropertyEditor() { }

        protected override FrameworkElement CreateEditingElement()
        {
            var updown = new AutomationIntegerUpDown() { Name = Meta.Name };

            this.ResetBinding(updown);

            this.SetAutomationElement(updown);

            return updown;
        }

        protected override DependencyProperty BindingProperty()
        {
            return IntegerUpDown.ValueProperty;
        }
    }

    public class AutomationIntegerUpDown : IntegerUpDown
    {
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var tb = this.GetTemplateChild("Text") as TextBox;

            if (tb != null)
            {
                AutomationProperties.SetName(tb, AutomationProperties.GetName(this));
            }
        }
    }
}
