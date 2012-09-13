/*******************************************************
 * 
 * 作者：李智
 * 创建时间：20100101
 * 说明：文件描述
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 李智 20100101
 * 
*******************************************************/

using System.Windows;
using System.Windows.Data;


namespace OEA.Module.WPF.Controls
{
    public class DateTreeColumn : TreeColumn
    {
        protected DateTreeColumn() { }

        protected override void OnBindingChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnBindingChanged(e);

            //如果 stringformat 是空的，则使用默认的格式化。
            if (string.IsNullOrEmpty(this.HeaderStringFormat))
            {
                var value = (BindingBase)e.NewValue;

                var propertyMeta = this.Meta.PropertyMeta;
                var meta = propertyMeta.ManagedProperty.GetMeta(propertyMeta.Owner.EntityType) as IPropertyMetadata;
                switch (meta.DateTimePart)
                {
                    case DateTimePart.DateTime:
                        break;
                    case DateTimePart.Date:
                        value.StringFormat = "d";
                        break;
                    case DateTimePart.Time:
                        value.StringFormat = "t";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}