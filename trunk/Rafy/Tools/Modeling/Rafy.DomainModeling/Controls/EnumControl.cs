/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130403 16:22
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130403 16:22
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 枚举元素控件
    /// </summary>
    [TemplatePart(Name = "PART_Items", Type = typeof(ListBox))]
    [ContentProperty("Items")]
    public class EnumControl : BlockControl
    {
        static EnumControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EnumControl), new FrameworkPropertyMetadata(typeof(EnumControl)));
        }

        private ListBox _listBox;

        private ObservableCollection<EnumItem> _items = new ObservableCollection<EnumItem>();

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listBox = this.Template.FindName("PART_Items", this) as ListBox;
            _listBox.ItemsSource = this._items;
        }

        public ObservableCollection<EnumItem> Items
        {
            get { return _items; }
        }
    }
}