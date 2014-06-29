/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110406
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110406
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Rafy.Domain;
using System.Windows.Input;
using Rafy.Utils;

namespace Rafy.WPF.Editors
{
    public class DateRangePropertyEditor : EntityDynamicPropertyEditor
    {
        protected override FrameworkElement CreateDynamicEditingElement(Entity curEntity)
        {
            var value = this.PropertyValue;

            //支持两种属性类型：DateRange,String，所以这里使用这个变量进行分辨
            var useDateRangeType = this.Meta.PropertyMeta.Runtime.PropertyType == typeof(DateRange);

            var range = useDateRangeType ?
                new DateRange(value as DateRange) :
                DateRange.Parse(value != null ? value as string : string.Empty);

            var control = new DateRangePropertyEditorControl(range);
            control.Confirm += (oo, ee) =>
            {
                if (useDateRangeType)
                {
                    var raw = value as DateRange;
                    raw.BeginValue = ee.Range.BeginValue;
                    raw.EndValue = ee.Range.EndValue;
                }
                else
                {
                    this.PropertyValue = ee.Range.ToString();
                }
            };

            control.KeyDown += (oo, ee) =>
            {
                if (ee.Key == Key.Enter)
                {
                    FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    control.MoveFocus(request);
                }
            };

            this.AddReadOnlyComponent(control);

            this.SetAutomationElement(control);

            return control;
        }
    }
}