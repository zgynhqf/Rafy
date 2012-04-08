/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120408
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120408
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 默认的 DetailPanel 控件，继承自 AutoGrid，用于承载所有的字段显示控件。
    /// </summary>
    public class DefaultDetailPanel : AutoGrid
    {
        static DefaultDetailPanel()
        {
            EditorHost.DetailObjectViewProperty.OverrideMetadata(
                typeof(DefaultDetailPanel),
                new PropertyMetadata((d, e) => (d as DefaultDetailPanel).OnDetailObjectViewChanged(e))
                );
        }

        /// <summary>
        /// 在 DetailObjectView 被设置时
        /// </summary>
        /// <param name="e"></param>
        private void OnDetailObjectViewChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (DetailObjectView)e.NewValue;

            if (value != null)
            {
                this.ColumnsCount = value.ColumnsCount;

                //加入所有标记了ShowInDetail的属性
                foreach (var propertyView in value.Meta.OrderedEntityProperties())
                {
                    if (propertyView.CanShowIn(ShowInWhere.Detail))
                    {
                        this.Children.Add(new EditorHost
                        {
                            EntityProperty = propertyView
                        });
                    }
                }
            }
        }

        #region ColumnsCount DependencyProperty

        public static readonly DependencyProperty ColumnsCountProperty = DependencyProperty.Register(
            "ColumnsCount", typeof(int), typeof(DefaultDetailPanel),
            new PropertyMetadata((d, e) => (d as DefaultDetailPanel).OnColumnsCountChanged(e))
            );

        public int ColumnsCount
        {
            get { return (int)this.GetValue(ColumnsCountProperty); }
            set { this.SetValue(ColumnsCountProperty, value); }
        }

        private void OnColumnsCountChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (int)e.NewValue;
            var columns = this.ColumnDefinitions;

            columns.Clear();

            for (int i = 0; i < value; i++)
            {
                columns.Add(new ColumnDefinition());
            }
        }

        #endregion
    }
}