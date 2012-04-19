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

using System.Windows;
using System.Windows.Input;
using OEA.Module.WPF.Editors;
using System.Windows.Controls;
using OEA.Library;

namespace OEA.Module.WPF.Editors
{
    public class NumberRangePropertyEditor : EntityDynamicPropertyEditor
    {
        protected override FrameworkElement CreateDynamicEditingElement(Entity curEntity)
        {
            var value = this.PropertyValue;

            //支持两种属性类型：DateRange,String，所以这里使用这个变量进行分辨
            var useNumberRangeType = this.Meta.PropertyMeta.Runtime.PropertyType == typeof(NumberRange);

            var range = useNumberRangeType ?
                new NumberRange(value as NumberRange) :
                NumberRange.Parse(value != null ? value as string : string.Empty);

            var rangeControl = new NumberRangePropertyEditorControl(range);
            rangeControl.Confirm += (ss, ee) =>
            {
                if (useNumberRangeType)
                {
                    var raw = value as NumberRange;
                    raw.BeginValue = ee.Range.BeginValue;
                    raw.EndValue = ee.Range.EndValue;
                }
                else
                {
                    this.PropertyValue = ee.Range.ToString();
                }
            };

            rangeControl.KeyDown += (oo, ee) =>
            {
                if (ee.Key == Key.Enter)
                {
                    FocusNavigationDirection focusDirection = FocusNavigationDirection.Next;
                    TraversalRequest request = new TraversalRequest(focusDirection);
                    rangeControl.MoveFocus(request);
                }
            };

            this.BindElementReadOnly(rangeControl);

            this.SetAutomationElement(rangeControl);

            return rangeControl;
        }
    }
}