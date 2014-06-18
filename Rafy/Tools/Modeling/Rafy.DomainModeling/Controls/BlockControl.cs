/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130329 15:57
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130329 15:57
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DesignerEngine;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// 块状控件
    /// 
    /// 此控件会被包装在 Designer.DesignerItem 中进行显示。
    /// </summary>
    public abstract class BlockControl : Control, IModelingDesignerComponent
    {
        static BlockControl()
        {
            CommandManager.RegisterClassCommandBinding(typeof(BlockControl), new CommandBinding(TriggerCommand, (s, e) =>
            {
                (s as BlockControl).OnTriggerCommandExecuted(e);
            }));
        }

        internal ModelingDesigner Designer;

        internal DesignerItemContainer Container;

        #region TriggerCommand

        /// <summary>
        /// 触发变更是否显示详细信息的命令
        /// </summary>
        public static readonly RoutedCommand TriggerCommand = new RoutedCommand("TriggerCommand", typeof(BlockControl));

        private void OnTriggerCommandExecuted(ExecutedRoutedEventArgs e)
        {
            this.HideDetails = !this.HideDetails;
        }

        #endregion

        #region TypeName DependencyProperty

        public static readonly DependencyProperty TypeNameProperty = DependencyProperty.Register(
            "TypeName", typeof(string), typeof(BlockControl),
            new PropertyMetadata((d, e) => (d as BlockControl).OnTypeNameChanged(e))
            );

        /// <summary>
        /// 类型名称
        /// </summary>
        public string TypeName
        {
            get { return (string)this.GetValue(TypeNameProperty); }
            set { this.SetValue(TypeNameProperty, value); }
        }

        private void OnTypeNameChanged(DependencyPropertyChangedEventArgs e)
        {
            //var value = (string)e.NewValue;
            ResetTitle();
        }

        #endregion

        #region TypeFullName DependencyProperty

        public static readonly DependencyProperty TypeFullNameProperty = DependencyProperty.Register(
            "TypeFullName", typeof(string), typeof(BlockControl),
            new PropertyMetadata((d, e) => (d as BlockControl).OnTypeFullNameChanged(e))
            );

        /// <summary>
        /// 类型全名称
        /// </summary>
        public string TypeFullName
        {
            get { return (string)this.GetValue(TypeFullNameProperty); }
            set { this.SetValue(TypeFullNameProperty, value); }
        }

        private void OnTypeFullNameChanged(DependencyPropertyChangedEventArgs e)
        {
            //var value = (string)e.NewValue;
            ResetTitle();
        }

        #endregion

        #region Label DependencyProperty

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
            "Label", typeof(string), typeof(BlockControl),
            new PropertyMetadata((d, e) => (d as BlockControl).OnLabelChanged(e))
            );

        /// <summary>
        /// 枚举/实体的领域名称。
        /// </summary>
        public string Label
        {
            get { return (string)this.GetValue(LabelProperty); }
            set { this.SetValue(LabelProperty, value); }
        }

        private void OnLabelChanged(DependencyPropertyChangedEventArgs e)
        {
            //var value = (string)e.NewValue;
            ResetTitle();
        }

        #endregion

        #region Title DependencyProperty

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(BlockControl)
            );

        /// <summary>
        /// 块用于显示的标题。
        /// </summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            private set { this.SetValue(TitleProperty, value); }
        }

        internal void ResetTitle()
        {
            var designer = ModelingDesigner.GetDesigner(this);
            if (designer != null)
            {
                this.Title = designer.BlockTitleFormatter.Format(this);
            }
        }

        #endregion

        #region TitleFontWeight DependencyProperty

        public static readonly DependencyProperty TitleFontWeightProperty = DependencyProperty.Register(
            "TitleFontWeight", typeof(FontWeight), typeof(BlockControl),
            new PropertyMetadata(FontWeights.Normal)
            );

        /// <summary>
        /// 标题的字体显示
        /// </summary>
        public FontWeight TitleFontWeight
        {
            get { return (FontWeight)this.GetValue(TitleFontWeightProperty); }
            set { this.SetValue(TitleFontWeightProperty, value); }
        }

        #endregion

        #region HideDetails DependencyProperty

        public static readonly DependencyProperty HideDetailsProperty = DependencyProperty.Register(
            "HideDetails", typeof(bool), typeof(BlockControl)
            );

        /// <summary>
        /// 是否隐藏块中的详细信息
        /// </summary>
        public bool HideDetails
        {
            get { return (bool)this.GetValue(HideDetailsProperty); }
            set { this.SetValue(HideDetailsProperty, value); }
        }

        #endregion

        #region HasDetails DependencyProperty

        public static readonly DependencyProperty HasDetailsProperty = DependencyProperty.Register(
            "HasDetails", typeof(bool), typeof(BlockControl)
            );

        /// <summary>
        /// 是否存在详细视图
        /// </summary>
        public bool HasDetails
        {
            get { return (bool)this.GetValue(HasDetailsProperty); }
            set { this.SetValue(HasDetailsProperty, value); }
        }

        #endregion

        #region Left DependencyProperty

        public static readonly DependencyProperty LeftProperty = DependencyProperty.Register(
            "Left", typeof(double), typeof(BlockControl)
            );

        /// <summary>
        /// 在设计器中的位置
        /// </summary>
        public double Left
        {
            get { return (double)this.GetValue(LeftProperty); }
            set { this.SetValue(LeftProperty, value); }
        }

        #endregion

        #region Top DependencyProperty

        public static readonly DependencyProperty TopProperty = DependencyProperty.Register(
            "Top", typeof(double), typeof(BlockControl)
            );

        /// <summary>
        /// 在设计器中的位置
        /// </summary>
        public double Top
        {
            get { return (double)this.GetValue(TopProperty); }
            set { this.SetValue(TopProperty, value); }
        }

        #endregion

        #region BlockControl

        /// <summary>
        /// 获取引擎元素对应的实体/枚举控件。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static BlockControl GetBlockControl(DesignerItemContainer item)
        {
            return item.Content as BlockControl;
        }

        #endregion

        /// <summary>
        /// 用于底层显示的图形控件。
        /// </summary>
        public Control EngineControl
        {
            get { return Container; }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ResetTitle();
        }

        DesignerComponentKind IModelingDesignerComponent.Kind
        {
            get { return DesignerComponentKind.Block; }
        }
    }
}