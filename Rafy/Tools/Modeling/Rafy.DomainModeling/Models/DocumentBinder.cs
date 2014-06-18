/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 18:56
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 18:56
 * 
*******************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Rafy.DomainModeling.Controls;
using Rafy.DomainModeling.Models.Xml;

namespace Rafy.DomainModeling.Models
{
    /// <summary>
    /// 此类型用于把 odml 文档对象完整双向绑定到设计器控件上。
    /// </summary>
    internal class DocumentBinder
    {
        #region 绑定逻辑

        internal static void BindDocument(ModelingDesigner designer, ODMLDocument document)
        {
            var binder = new DocumentBinder
            {
                _designer = designer,
                _document = document
            };
            binder.BindDocument();
        }

        private ModelingDesigner _designer;

        private ODMLDocument _document;

        private void BindDocument()
        {
            var oldDocument = _designer.DataContext as ODMLDocument;
            if (oldDocument != _document)
            {
                //清空当前所有项。
                _designer.Blocks.Clear();
                _designer.Relations.Clear();

                _designer.DataContext = _document;

                try
                {
                    SetIsReBinding(_designer, true);

                    CreateEntityTypeBlocks(_document.EntityTypes);
                    _document.EntityTypes.CollectionChanged += EntityTypes_CollectionChanged;

                    CreateEnumTypeBlocks(_document.EnumTypes);
                    _document.EnumTypes.CollectionChanged += EnumTypes_CollectionChanged;

                    CreateRelations(_document.Connections);
                    _document.Connections.CollectionChanged += Connections_CollectionChanged;

                    _designer.Blocks.CollectionChanged += Blocks_CollectionChanged;
                    _designer.Relations.CollectionChanged += Relations_CollectionChanged;
                }
                finally
                {
                    SetIsReBinding(_designer, false);
                }

                //不可用 Unloaded 事件，否则会造成切换页签时重新加载的问题。
                //_designer.Unloaded += (o, e) =>
                //{
                //    _designer.DataContext = null;
                //    _document.EntityTypes.CollectionChanged -= EntityTypes_CollectionChanged;
                //    _document.EnumTypes.CollectionChanged -= EnumTypes_CollectionChanged;
                //    _document.Connections.CollectionChanged -= Connections_CollectionChanged;
                //};
            }
        }

        #endregion

        #region 绑定 EntityTypes

        void EntityTypes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TriggerBinding(BindingSource.Document, () =>
            {
                var oldItems = e.OldItems ?? _document.EntityTypes.PopClearedItems();
                if (oldItems != null)
                {
                    foreach (EntityTypeElement oldItem in oldItems)
                    {
                        var block = _designer.Blocks.Find(oldItem.FullName);
                        _designer.Blocks.Remove(block);
                    }
                }
                if (e.NewItems != null)
                {
                    var newItems = e.NewItems;
                    CreateEntityTypeBlocks(newItems);
                }
            });
        }

        private void CreateEntityTypeBlocks(IEnumerable newItems)
        {
            foreach (EntityTypeElement newItem in newItems)
            {
                var control = CreateEntityTypeControl(newItem);
                _designer.Blocks.Add(control);
            }
        }

        private EntityTypeControl CreateEntityTypeControl(EntityTypeElement type)
        {
            var control = new EntityTypeControl();
            control.DataContext = type;

            BindBlockControl(control);
            SetBinding(control, EntityTypeControl.HideDetailsProperty, "HideProperties");
            SetBinding(control, EntityTypeControl.IsAggtRootProperty, "IsAggtRoot");

            CreateProperties(control, type.Properties);
            type.Properties.CollectionChanged += (o, e) =>
            {
                var oldItems = e.OldItems ?? type.Properties.PopClearedItems();
                if (oldItems != null)
                {
                    foreach (PropertyElement oldItem in oldItems)
                    {
                        var property = control.Items.First(i => i.DataContext == oldItem);
                        control.Items.Remove(property);
                    }
                }
                if (e.NewItems != null)
                {
                    CreateProperties(control, e.NewItems);
                }
            };

            return control;
        }

        private void CreateProperties(EntityTypeControl control, IEnumerable newItems)
        {
            foreach (PropertyElement newItem in newItems)
            {
                var property = new Property();
                property.DataContext = newItem;

                SetBinding(property, Property.PropertyNameProperty, "Name");
                SetBinding(property, Property.PropertyTypeProperty, "PropertyType");
                SetBinding(property, Property.LabelProperty, "Label");

                control.Items.Add(property);
            }
        }

        private static void BindBlockControl(BlockControl control)
        {
            BindingOperations.SetBinding(control, BlockControl.TypeNameProperty, new Binding("Name"));//OneWay
            SetBinding(control, BlockControl.LabelProperty, "Label");
            SetBinding(control, BlockControl.TypeFullNameProperty, "FullName");
            SetBinding(control, BlockControl.LeftProperty, "Left");
            SetBinding(control, BlockControl.TopProperty, "Top");
            SetBinding(control, BlockControl.WidthProperty, "Width", _ignoreEmptySizeConverter);
            SetBinding(control, BlockControl.HeightProperty, "Height", _ignoreEmptySizeConverter);
        }

        #endregion

        #region 绑定 EnumTypes

        void EnumTypes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TriggerBinding(BindingSource.Document, () =>
            {
                var oldItems = e.OldItems ?? _document.EnumTypes.PopClearedItems();
                if (oldItems != null)
                {
                    foreach (EnumElement oldItem in oldItems)
                    {
                        var block = _designer.Blocks.Find(oldItem.FullName);
                        _designer.Blocks.Remove(block);
                    }
                }
                if (e.NewItems != null)
                {
                    var newItems = e.NewItems;
                    CreateEnumTypeBlocks(newItems);
                }
            });
        }

        private void CreateEnumTypeBlocks(IEnumerable newItems)
        {
            foreach (EnumElement newItem in newItems)
            {
                var control = CreateEnumControl(newItem);
                _designer.Blocks.Add(control);
            }
        }

        private EnumControl CreateEnumControl(EnumElement type)
        {
            var control = new EnumControl();
            control.DataContext = type;

            BindBlockControl(control);

            CreateEnumItems(control, type.Items);
            type.Items.CollectionChanged += (o, e) =>
            {
                var oldItems = e.OldItems ?? type.Items.PopClearedItems();
                if (oldItems != null)
                {
                    foreach (EnumItemElement oldItem in oldItems)
                    {
                        var property = control.Items.First(i => i.DataContext == oldItem);
                        control.Items.Remove(property);
                    }
                }
                if (e.NewItems != null)
                {
                    CreateEnumItems(control, e.NewItems);
                }
            };

            return control;
        }

        private void CreateEnumItems(EnumControl control, IEnumerable newItems)
        {
            foreach (EnumItemElement newItem in newItems)
            {
                var item = new EnumItem();
                item.DataContext = newItem;

                SetBinding(item, EnumItem.ItemNameProperty, "Name");
                SetBinding(item, EnumItem.LabelProperty, "Label");
                //SetBinding(item, EnumItem.ValueProperty, "Value");

                control.Items.Add(item);
            }
        }

        #endregion

        #region 绑定 Connections

        void Connections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TriggerBinding(BindingSource.Document, () =>
            {
                var oldItems = e.OldItems ?? _document.Connections.PopClearedItems();
                if (oldItems != null)
                {
                    foreach (ConnectionElement oldItem in oldItems)
                    {
                        var relation = _designer.Relations.FindByKey(oldItem);
                        _designer.Relations.Remove(relation);
                    }
                }
                if (e.NewItems != null)
                {
                    CreateRelations(e.NewItems);
                }
            });
        }

        private void CreateRelations(IEnumerable newItems)
        {
            var items = _designer.Blocks;

            foreach (ConnectionElement conEl in newItems)
            {
                //一些多余的连接，需要去除。
                if (string.IsNullOrEmpty(conEl.From) || string.IsNullOrEmpty(conEl.To)) { continue; }
                if (items.Find(conEl.From) == null || items.Find(conEl.To) == null) { continue; }

                var connection = new BlockRelation();
                connection.DataContext = conEl;

                SetBinding(connection, BlockRelation.FromBlockProperty, "From");
                SetBinding(connection, BlockRelation.ToBlockProperty, "To");
                SetBinding(connection, BlockRelation.LabelProperty, "Label");
                SetBinding(connection, BlockRelation.ConnectionTypeProperty, "ConnectionType");
                SetBinding(connection, BlockRelation.FromPointPosProperty, "FromPointPos");
                SetBinding(connection, BlockRelation.ToPointPosProperty, "ToPointPos");
                SetBinding(connection, BlockRelation.HiddenProperty, "Hidden");
                SetBinding(connection, BlockRelation.LabelVisibilityProperty, "LabelVisible", _btvConverter);

                _designer.Relations.Add(connection);
            }
        }

        #endregion

        #region 在删除界面元素时，修改文档中的元素

        void Blocks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TriggerBinding(BindingSource.Designer, () =>
            {
                var oldItems = e.OldItems ?? _designer.Blocks.PopClearedItems();
                if (oldItems != null)
                {
                    foreach (BlockControl item in oldItems)
                    {
                        if (item is EntityTypeControl)
                        {
                            _document.EntityTypes.Remove(item.DataContext as EntityTypeElement);
                        }
                        else
                        {
                            _document.EnumTypes.Remove(item.DataContext as EnumElement);
                        }
                    }
                }
                //if (e.NewItems != null)
                //{
                //    throw new NotSupportedException("不支持直接在设计器中添加元素，请使用对应的文档对象进行操作。");
                //}
            });
        }

        void Relations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TriggerBinding(BindingSource.Designer, () =>
            {
                var oldItems = e.OldItems ?? _designer.Relations.PopClearedItems();
                if (oldItems != null)
                {
                    foreach (BlockRelation item in oldItems)
                    {
                        _document.Connections.Remove(item.DataContext as ConnectionElement);
                    }
                }
                //由于一个 Designer 可以使用了多个 DocumentBinder，
                //所以 Binder A 的处理函数，可以被 Binder B 触发，暂时不处理这种情况。
                //if (e.NewItems != null)
                //{
                //    throw new NotSupportedException("不支持直接在设计器中添加元素，请使用对应的文档对象进行操作。");
                //}
            });
        }

        #endregion

        #region 帮助方法

        private static void SetBinding(FrameworkElement element, DependencyProperty property, string propertyName)
        {
            BindingOperations.SetBinding(element, property, new Binding(propertyName) { Mode = BindingMode.TwoWay });
        }

        private static void SetBinding(FrameworkElement element, DependencyProperty property, string propertyName, IValueConverter converter)
        {
            BindingOperations.SetBinding(element, property, new Binding(propertyName)
            {
                Mode = BindingMode.TwoWay,
                Converter = converter
            });
        }

        private static BooleanToVisibilityConverter _btvConverter = new BooleanToVisibilityConverter();

        private static readonly IgnoreEmptySizeConverter _ignoreEmptySizeConverter = new IgnoreEmptySizeConverter();

        private class IgnoreEmptySizeConverter : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var dValue = (double)value;
                if (dValue == 0) return double.NaN;

                return value;
            }
            public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
            {
                var dValue = (double)value;
                if (double.IsNaN(dValue)) return 0;

                return value;
            }
        }

        #endregion

        #region 判断绑定操作的事件源头，防止混乱

        #region IsReBinding AttachedDependencyProperty

        /// <summary>
        /// 正在进行绑定操作。
        /// </summary>
        private static readonly DependencyProperty IsReBindingProperty = DependencyProperty.RegisterAttached(
            "IsReBinding", typeof(bool), typeof(DocumentBinder)
            );

        private static bool GetIsReBinding(ModelingDesigner element)
        {
            return (bool)element.GetValue(IsReBindingProperty);
        }

        private static void SetIsReBinding(ModelingDesigner element, bool value)
        {
            element.SetValue(IsReBindingProperty, value);
        }

        #endregion

        private BindingSource _source = BindingSource.None;

        private void TriggerBinding(BindingSource source, Action action)
        {
            if (!GetIsReBinding(_designer))
            {
                if (_source == BindingSource.None)
                {
                    _source = source;

                    try
                    {
                        action();
                    }
                    finally
                    {
                        _source = BindingSource.None;
                    }
                }
            }
        }

        private enum BindingSource { None, Document, Designer }

        #endregion
    }
}
