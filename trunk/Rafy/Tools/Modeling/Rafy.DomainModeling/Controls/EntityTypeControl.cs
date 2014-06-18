/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 15:53
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 15:53
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    /// 实体类型控件。
    /// </summary>
    [TemplatePart(Name = "PART_Items", Type = typeof(ListBox))]
    [ContentProperty("Items")]
    public class EntityTypeControl : BlockControl
    {
        static EntityTypeControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EntityTypeControl), new FrameworkPropertyMetadata(typeof(EntityTypeControl)));
        }

        private ListBox _listBox;

        private ObservableCollection<Property> _items;

        public EntityTypeControl()
        {
            this._items = new ObservableCollection<Property>();
            this._items.CollectionChanged += _items_CollectionChanged;
        }

        void _items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.HasDetails = this._items.Count > 0;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _listBox = this.Template.FindName("PART_Items", this) as ListBox;
            _listBox.ItemsSource = this._items;
        }

        public ObservableCollection<Property> Items
        {
            get { return _items; }
        }

        #region IsAggtRoot DependencyProperty

        public static readonly DependencyProperty IsAggtRootProperty = DependencyProperty.Register(
            "IsAggtRoot", typeof(bool), typeof(EntityTypeControl)
            );

        /// <summary>
        /// 表示当前的实体类型是否是一个聚合根。
        /// </summary>
        public bool IsAggtRoot
        {
            get { return (bool)this.GetValue(IsAggtRootProperty); }
            set { this.SetValue(IsAggtRootProperty, value); }
        }

        #endregion
    }
}