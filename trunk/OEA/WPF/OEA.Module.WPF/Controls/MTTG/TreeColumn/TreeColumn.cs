/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110217
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110217
 * 
*******************************************************/

using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module.WPF.Editors;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// 树型表的列的基类
    /// </summary>
    public abstract class TreeColumn : TreeGridColumn
    {
        #region 字段

        /// <summary>
        /// 该字段可能为空，表示该列没有对应任何元数据。
        /// </summary>
        private EntityPropertyViewMeta _meta;

        #endregion

        internal void IntializeViewMeta(EntityPropertyViewMeta meta, PropertyEditorFactory editorFactory)
        {
            this._meta = meta;

            this.Editor = editorFactory.Create(meta, true);
        }

        /// <summary>
        /// 默认没有Editor
        /// </summary>
        public WPFPropertyEditor Editor { get; private set; }

        /// <summary>
        /// 正在编辑的属性
        /// </summary>
        public EntityPropertyViewMeta Meta
        {
            get { return this._meta; }
        }

        #region 编辑状态

        protected override bool TryCheckIsReadonly(object dataItem)
        {
            //如果一个树用于多个对象,此方法不适用，需要切换PropertyInfo
            if (dataItem != null && dataItem.GetType() == this.Meta.Owner.EntityType)
            {
                return PropertyEditorHelper.CheckIsReadonly(dataItem as Entity, this.Meta);
            }

            return false;
        }

        protected override void PrepareElementForEdit(FrameworkElement editingElement, RoutedEventArgs editingEventArgs)
        {
            if (this.Editor != null)
            {
                this.Editor.PrepareElementForEdit(editingElement, editingEventArgs);
            }
        }

        #endregion

        /// <summary>
        /// 生成这个Column使用的编辑Element
        /// 未实现
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement GenerateEditingElementCore()
        {
            var editor = this.Editor;
            if (editor == null) throw new ArgumentNullException("只读！");

            //由于树形可以包含多个对象，一个列可能编辑多个对象，所以现在简单处理为每次重新生成控件
            var control = editor.Control;

            return control;
        }

        internal void UpdateVisibility(Entity currData)
        {
            if (this._meta != null)
            {
                bool isVisible = false;
                var visibilityIndicator = this._meta.VisibilityIndicator;

                //如果是动态计算，则尝试从数据中获取是否可见的值。
                if (visibilityIndicator.IsDynamic)
                {
                    if (currData != null)
                    {
                        isVisible = (bool)currData.GetProperty(visibilityIndicator.Property);
                    }
                    else
                    {
                        isVisible = true;
                    }
                }
                else
                {
                    isVisible = visibilityIndicator.VisiblityType == VisiblityType.AlwaysShow;
                }

                this.IsVisible = isVisible;
            }
        }
    }
}