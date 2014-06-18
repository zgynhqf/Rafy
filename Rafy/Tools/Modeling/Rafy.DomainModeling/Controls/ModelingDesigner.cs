/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130402 13:46
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130402 13:46
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DesignerEngine;
using Rafy.DomainModeling.Commands;

namespace Rafy.DomainModeling.Controls
{
    /// <summary>
    /// ODML 模型设计器。
    /// </summary>
    /// 基于设计器引擎实现。
    [TemplatePart(Name = "PART_DesignerCanvas", Type = typeof(DesignerCanvas))]
    public class ModelingDesigner : Control
    {
        static ModelingDesigner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ModelingDesigner), new FrameworkPropertyMetadata(typeof(ModelingDesigner)));
        }

        private DesignerCanvas _canvas;

        private ITitleFormatter _blockTitleFormatter = new BlockTitleFormatter();

        /// <summary>
        /// 块标题的格式化器
        /// </summary>
        public ITitleFormatter BlockTitleFormatter
        {
            get { return _blockTitleFormatter; }
            set
            {
                if (_blockTitleFormatter != null)
                {
                    _blockTitleFormatter = value;

                    foreach (var item in _blocks)
                    {
                        item.ResetTitle();
                    }
                }
            }
        }

        #region 集合属性

        private BlockControlCollection _blocks;

        private BlockRelationCollection _relations;

        public ModelingDesigner()
        {
            this._blocks = new BlockControlCollection();
            this._relations = new BlockRelationCollection();

            this._blocks.CollectionChanged += OnBlocksChanged;
            this._relations.CollectionChanged += OnRelationsChanged;

            //默认添加几个命令。
            CommandBinder.Bind<DeleteBlockCommand>(this);
            CommandBinder.Bind<SelectAllCommand>(this);
            CommandBinder.Bind<OpenODMLCommand>(this);
            CommandBinder.Bind<SaveODMLCommand>(this);
            CommandBinder.Bind<RestoreODMLCommand>(this);
            CommandBinder.Bind<HideRelationCommand>(this);
            //CommandBinder.Bind<ToggleHiddenRelationsCommand>(this);//不易用。
        }

        /// <summary>
        /// 所有的块
        /// </summary>
        public BlockControlCollection Blocks
        {
            get { return _blocks; }
        }

        /// <summary>
        /// 所有的连接线
        /// </summary>
        public BlockRelationCollection Relations
        {
            get { return _relations; }
        }

        private void OnBlocksChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = e.OldItems ?? this.Blocks.PopClearedItems();
            if (oldItems != null)
            {
                foreach (BlockControl item in oldItems)
                {
                    item.Designer = null;
                    RemoveItemFromCanvas(item);
                }
            }

            if (e.NewItems != null)
            {
                foreach (BlockControl item in e.NewItems)
                {
                    item.Designer = this;
                    if (_canvas != null)
                    {
                        //新添加的块，都全部生成相关的引擎元素，加入到画布中。
                        AddItemIntoCanvas(item);
                    }
                }
            }
        }

        private void OnRelationsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var oldItems = e.OldItems ?? this.Relations.PopClearedItems();
            if (oldItems != null)
            {
                foreach (BlockRelation item in oldItems)
                {
                    item.Designer = null;
                    RemoveConnectionFromCanvas(item);
                }
            }

            if (e.NewItems != null)
            {
                foreach (BlockRelation item in e.NewItems)
                {
                    item.Designer = this;
                    if (_canvas != null)
                    {
                        //新添加的关系，都全部生成相关的引擎元素，加入到画布中。
                        AddConnectionIntoCanvas(item);
                    }
                }
            }
        }

        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _canvas = this.Template.FindName("PART_DesignerCanvas", this) as DesignerCanvas;
            if (_canvas == null) throw new InvalidProgramException("ModelingDesigner 的模板中必须要有命名为 PART_DesignerCanvas 的 DesignerCanvas 元素。");
            _canvas.SelectionChanged += (o, e) => this.OnSelectionChanged();

            _canvas.DragLineCompleted += _canvas_DragLineCompleted;

            //在首次加载时，把所有元素加入到画布中。
            AddElementsIntoCanvas();
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            //在点击时，如果还没有获得焦点，则获取键盘焦点。
            //否则接下来的按键都不会被 OnKeyDown 捕获到。
            if (!this.IsFocused)
            {
                Keyboard.Focus(this);
            }

            base.OnPreviewMouseDown(e);
        }

        #region CanChangeRelation DependencyProperty

        public static readonly DependencyProperty CanChangeRelationProperty = DependencyProperty.Register(
            "CanChangeRelation", typeof(bool), typeof(ModelingDesigner),
            new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否允许变更连接线的两个端点。
        /// </summary>
        public bool CanChangeRelation
        {
            get { return (bool)this.GetValue(CanChangeRelationProperty); }
            set { this.SetValue(CanChangeRelationProperty, value); }
        }

        #endregion

        #region ShowHiddenRelations DependencyProperty

        public static readonly DependencyProperty ShowHiddenRelationsProperty = DependencyProperty.Register(
            "ShowHiddenRelations", typeof(bool), typeof(ModelingDesigner),
            new PropertyMetadata((d, e) => (d as ModelingDesigner).OnShowHiddenRelationsChanged(e))
            );

        /// <summary>
        /// 是否显示所有状态为隐藏的关系。
        /// </summary>
        public bool ShowHiddenRelations
        {
            get { return (bool)this.GetValue(ShowHiddenRelationsProperty); }
            set { this.SetValue(ShowHiddenRelationsProperty, value); }
        }

        private void OnShowHiddenRelationsChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (bool)e.NewValue;
            foreach (var relation in this.Relations)
            {
                relation.ResetVisibility();
            }
        }

        #region 按住空格键时，显示隐藏线

        private bool _isSpacePressed;

        private bool _oldShowHiddenRelations;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Space)
            {
                if (!_isSpacePressed)
                {
                    _isSpacePressed = true;
                    _oldShowHiddenRelations = this.ShowHiddenRelations;
                    this.ShowHiddenRelations = true;
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            if (e.Key == Key.Space)
            {
                this.ShowHiddenRelations = _oldShowHiddenRelations;
                _isSpacePressed = false;
            }
        }

        #endregion

        #endregion

        #region 添加新的连接

        #region CanAddRelation DependencyProperty

        public static readonly DependencyProperty CanAddRelationProperty = DependencyProperty.Register(
            "CanAddRelation", typeof(bool), typeof(ModelingDesigner),
            new PropertyMetadata(true)
            );

        /// <summary>
        /// 是否能添加连接线。
        /// </summary>
        public bool CanAddRelation
        {
            get { return (bool)this.GetValue(CanAddRelationProperty); }
            set { this.SetValue(CanAddRelationProperty, value); }
        }

        #endregion

        private void _canvas_DragLineCompleted(object sender, CanvasDragLineCompletedEventArgs e)
        {
            var source = BlockControl.GetBlockControl(e.Source);
            var sink = BlockControl.GetBlockControl(e.Sink);

            var relation = new BlockRelation
            {
                FromBlock = source.TypeFullName,
                ToBlock = sink.TypeFullName
            };
            this.Relations.Insert(0, relation);

            this.OnRelationAdded(relation);
        }

        /// <summary>
        /// 关系添加完成事件。
        /// </summary>
        public event EventHandler<RelationAddedEventArgs> RelationAdded;

        protected virtual void OnRelationAdded(BlockRelation relation)
        {
            var handler = this.RelationAdded;
            if (handler != null) handler(this, new RelationAddedEventArgs(relation));
        }

        #endregion

        #region 为块、连接生成可视控件

        #region BlockContainerStyle DependencyProperty

        public static readonly DependencyProperty BlockContainerStyleProperty = DependencyProperty.Register(
            "BlockContainerStyle", typeof(Style), typeof(ModelingDesigner)
            );

        /// <summary>
        /// 块状控件的 DesignerItem 容器样式
        /// </summary>
        public Style BlockContainerStyle
        {
            get { return (Style)this.GetValue(BlockContainerStyleProperty); }
            set { this.SetValue(BlockContainerStyleProperty, value); }
        }

        #endregion

        #region Component AttachedDependencyProperty

        private static readonly DependencyPropertyKey ComponentPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "Component", typeof(IModelingDesignerComponent), typeof(ModelingDesigner),
            new PropertyMetadata(ComponentPropertyChanged)
            );

        public static readonly DependencyProperty ComponentProperty = ComponentPropertyKey.DependencyProperty;

        /// <summary>
        /// 获取外层的控件。
        /// 
        /// DesignerItem 返回 BlockControl。
        /// Connection 返回 Controls.Connection
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IModelingDesignerComponent GetComponent(Control element)
        {
            return element.GetValue(ComponentProperty) as IModelingDesignerComponent;
        }

        public static void SetComponent(Control element, IModelingDesignerComponent value)
        {
            element.SetValue(ComponentPropertyKey, value);
        }

        private static void ComponentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var value = (IModelingDesignerComponent)e.NewValue;
        }

        #endregion

        private void AddElementsIntoCanvas()
        {
            //先加入 Item，再加入 Connection。
            foreach (var item in _blocks)
            {
                this.AddItemIntoCanvas(item);
            }

            foreach (var item in _relations)
            {
                this.AddConnectionIntoCanvas(item);
            }
        }

        private void AddItemIntoCanvas(BlockControl block)
        {
            var container = new DesignerItemContainer();
            container.Content = block;
            block.Container = container;
            SetComponent(container, block);

            //应用样式
            var containerStyle = this.BlockContainerStyle;
            if (containerStyle != null) container.Style = containerStyle;

            container.DataContext = block;
            container.SetBinding(Canvas.LeftProperty, new Binding { Path = new PropertyPath(BlockControl.LeftProperty), Mode = BindingMode.TwoWay });
            container.SetBinding(Canvas.TopProperty, new Binding { Path = new PropertyPath(BlockControl.TopProperty), Mode = BindingMode.TwoWay });

            _canvas.Children.Add(container);
        }

        private void AddConnectionIntoCanvas(BlockRelation relation)
        {
            var con = relation.CreateConnection();

            ModelingDesigner.SetComponent(con, relation);

            //从 0 开始添加，可以保证连接显示在元素之后。
            _canvas.Children.Insert(0, con);
        }

        private void RemoveItemFromCanvas(BlockControl block)
        {
            //先把所有的连接都移除。
            foreach (var con in block.Container.Connections)
            {
                var connection = GetComponent(con) as BlockRelation;
                this.Relations.Remove(connection);
            }

            _canvas.Children.Remove(block.Container);
        }

        private void RemoveConnectionFromCanvas(BlockRelation con)
        {
            _canvas.Children.Remove(con.EngineControl);
        }

        #endregion

        #region 选择项

        /// <summary>
        /// 当前的所有选择项。
        /// </summary>
        public IEnumerable<IModelingDesignerComponent> SelectedItems
        {
            get
            {
                if (_canvas != null)
                {
                    return _canvas.SelectedItems.Select(i => GetComponent(i as Control));
                }

                return new IModelingDesignerComponent[0];
            }
        }

        /// <summary>
        /// 选择项变更事件
        /// </summary>
        public event EventHandler SelectionChanged;

        protected virtual void OnSelectionChanged()
        {
            var handler = this.SelectionChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        #endregion

        #region 选择 API

        /// <summary>
        /// 全选
        /// </summary>
        public void SelectAll()
        {
            var allComponents = this.Blocks.Cast<IModelingDesignerComponent>().Union(this.Relations);
            var allDesignerControls = allComponents.Select(d => d.EngineControl as IDesignerCanvasItemInternal).ToArray();
            _canvas.AddSelection(false, allDesignerControls);
        }

        #endregion

        #region 帮助方法

        /// <summary>
        /// 获取某个元素外部的设计器。
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static ModelingDesigner GetDesigner(DependencyObject element)
        {
            return ModelingHelper.GetVisualParent<ModelingDesigner>(element);
        }

        #endregion
    }

    /// <summary>
    /// 添加关系完成的事件。
    /// </summary>
    public class RelationAddedEventArgs : EventArgs
    {
        public RelationAddedEventArgs(BlockRelation relation)
        {
            this.Relation = relation;
        }

        /// <summary>
        /// 添加的连接
        /// </summary>
        public BlockRelation Relation { get; private set; }
    }
}