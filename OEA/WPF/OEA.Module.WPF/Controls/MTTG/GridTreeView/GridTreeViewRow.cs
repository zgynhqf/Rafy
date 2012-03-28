/*******************************************************
 * 
 * ���ߣ������
 * ����ʱ�䣺20111123
 * ˵�������ļ�ֻ����һ���࣬�������ݼ�����ע�͡�
 * ���л�����.NET 4.0
 * �汾�ţ�1.0.0
 * 
 * ��ʷ��¼��
 * �����ļ� ����� 20111123
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using OEA.Module.WPF.Controls;
using System.Diagnostics;
using SimpleCsla;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using OEA.MetaModel;
using OEA.MetaModel.View;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using OEA.Module.WPF.Automation;
using Hardcodet.Wpf.GenericTreeView;

namespace OEA.Module.WPF.Controls
{
    /// <summary>
    /// GridTreeView �ؼ��е�һ��
    /// </summary>
    [TemplatePart(Name = "PART_RowPresenter", Type = typeof(GridTreeViewRowPresenter))]
    public class GridTreeViewRow : TreeViewItem
    {
        static GridTreeViewRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridTreeViewRow), new FrameworkPropertyMetadata(typeof(GridTreeViewRow)));
        }

        public GridTreeViewRow(MultiTypesTreeGrid treeGrid, EntityViewMeta entityViewMeta)
        {
            this._treeGrid = treeGrid;
            this._entityViewMeta = entityViewMeta;

            this.PrepareToAdjustFirstColumnWidth();
        }

        #region �ֶ�������

        private EntityViewMeta _entityViewMeta;

        private MultiTypesTreeGrid _treeGrid;

        public MultiTypesTreeGrid TreeGrid
        {
            get { return this._treeGrid; }
        }

        public EntityViewMeta EntityViewMeta
        {
            get { return this._entityViewMeta; }
        }

        #endregion

        #region Cells

        private List<MTTGCell> _cells = new List<MTTGCell>();

        internal IList<MTTGCell> Cells
        {
            get { return this._cells; }
        }

        internal void AddCell(MTTGCell cell)
        {
            if (!this._cells.Contains(cell))
            {
                this._cells.Add(cell);
            }
        }

        #endregion

        #region Level

        private int _level = -1;

        /// <summary>
        /// ���еļ���
        /// ���� 0������ÿһ���� 1��
        /// </summary>
        public int Level
        {
            get
            {
                if (this._level == -1)
                {
                    var parent = ItemsControl.ItemsControlFromItemContainer(this) as GridTreeViewRow;
                    this._level = parent != null ? parent.Level + 1 : 0;
                }

                return this._level;
            }
        }

        #endregion

        #region RowNoProperty

        internal static readonly DependencyProperty RowNoProperty = DependencyProperty.Register(
            "RowNo", typeof(int), typeof(GridTreeViewRow),
            new PropertyMetadata(0, (d, e) => (d as GridTreeViewRow).OnRowNoChanged(e))
            );

        /// <summary>
        /// ����Ӧ����ʾ�к�
        /// </summary>
        internal int RowNo
        {
            get { return (int)this.GetValue(RowNoProperty); }
            set { this.SetValue(RowNoProperty, value); }
        }

        private void OnRowNoChanged(DependencyPropertyChangedEventArgs e)
        {
            //��ǰֻ�ǰ�ֵ�򵥵�ͬ���� RowHeader �ϣ�������
            //���� GridTreeViewRowPresenter �������ֵ���Ϳ��԰����������ʾ������
            this.RowHeader = e.NewValue;
        }

        #endregion

        #region RowHeaderProperty

        public static readonly DependencyProperty RowHeaderProperty = DependencyProperty.Register(
            "RowHeader", typeof(object), typeof(GridTreeViewRow)
            );

        /// <summary>
        /// RowHeader ����ͷ�ؼ���
        /// </summary>
        public object RowHeader
        {
            get { return this.GetValue(RowHeaderProperty); }
            set { this.SetValue(RowHeaderProperty, value); }
        }

        #endregion

        #region ʵ�� ItemsControl ����д�����һЩ����

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new GridTreeViewRow(this._treeGrid, this._entityViewMeta);
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is GridTreeViewRow;
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            base.OnCollapsed(e);

            this._treeGrid.OnNodeCollapsed(this);
        }

        protected override void OnExpanded(RoutedEventArgs e)
        {
            base.OnExpanded(e);

            this._treeGrid.OnNodeExpanded(this);
        }

        /// <summary>
        /// ��ֹ TreeViewItem ����ʱ�������� TreeViewItem Ҳ���� SelectedItemChanged
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Source != this) { e.Handled = true; }
        }

        #endregion

        #region �Զ����ŵ�һ�еĿ��
        //������⣺http://ipm.grandsoft.com.cn/issues/166987

        /// <summary>
        /// �����ʵ����¼����Դ���������ȵĴ��롣
        /// 
        /// ���캯�����ô˷�����
        /// </summary>
        private void PrepareToAdjustFirstColumnWidth()
        {
            /*********************** �������� *********************************
             * 
             * Ŀǰ��ѡ��Ĵ��������ǣ�
             * ��һ���ڵ㱻 Expand ʱ���ýڵ��µ��ӽڵ�����ɣ�
             * ItemContainerGenerator.StatusChanged �¼��ᷢ������ Status �� GeneratorStatus.ContainersGenerated��
             * ���Ǵ�ʱ���� GridTreeViewRow ��û���������Ǹ��Ե� VisualChild��������Ҫ�ٴμ������һ���ӽڵ�ĵ� Loaded �¼���
             * ��ʱ���нڵ㼰�� VisualChild ���Ѿ�������ɣ����Կ�ʼ������һ�еĿ�ȡ�
             * ����û��ѡ�� Expanded �¼���ԭ����Ҫ���Ǹ��¼�����ʱ�������ӽڵ� GridTreeViewRow ��û�����ɡ�
             * 
            **********************************************************************/

            this.ItemContainerGenerator.StatusChanged += (o, e) =>
            {
                if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                {
                    var children = this.Items;
                    if (children.Count > 0)
                    {
                        var lastChild = children[children.Count - 1];
                        var row = this.ItemContainerGenerator.ContainerFromItem(lastChild) as GridTreeViewRow;
                        row.Loaded += (oo, ee) =>
                        {
                            var columns = this.TreeGrid.Columns;
                            if (columns.Count > 0)
                            {
                                var firstColumn = columns[0] as GridTreeViewColumn;
                                firstColumn.RequestDataWidth();
                            }
                        };
                    }
                }
            };
        }

        #endregion

        #region IsMultiSelectedProperty

        public static readonly DependencyProperty IsMultiSelectedProperty = DependencyProperty.Register(
            "IsMultiSelected", typeof(bool), typeof(GridTreeViewRow),
            new UIPropertyMetadata(false, (d, e) => (d as GridTreeViewRow).OnIsMultiSelectedChanged(e))
            );

        /// <summary>
        /// ����������ڱ�ʾ��ǰ���Ƿ��Ѿ���ѡ�С�
        /// Ŀǰ��Ҫ����֪ͨ Style ������Ӧ����ɫ�任��
        /// </summary>
        public bool IsMultiSelected
        {
            get { return (bool)this.GetValue(IsMultiSelectedProperty); }
            set { this.SetValue(IsMultiSelectedProperty, value); }
        }

        private void OnIsMultiSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.IsSettingInternal)
            {
                //ֵ�ı����Դ�ǣ���ѡ��ͼʱ����ѡ���߷���ѡĳһ�С�
                var value = (bool)e.NewValue;
                this.TreeGrid.CheckRowWithCascade(this, (bool)e.NewValue);
            }
        }

        /// <summary>
        /// ���� IsMultiSelected ��ֵ�Ƿ����ڱ��ڲ������ı䡣
        /// </summary>
        internal bool IsSettingInternal;

        #endregion

        #region �Զ���

        internal bool _isAutomationFired = false;

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            /**********************************************************************
             * 
             * ������Զ��������Ľ����¼�������Ҫ���λ��� OnGotFocus �����е� Select ������
             * �������ɸ��е�ѡ�У�������������Ĺرա�
             * 
            **********************************************************************/

            if (!this._isAutomationFired)
            {
                //�����ڻ�ȡ�����ʱ���ѡ�е�ǰ��
                base.OnGotFocus(e);
            }
            else
            {
                if (base.IsKeyboardFocused)
                {
                    this.BringIntoView();
                }
                this.RaiseEvent(e);
            }
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new GridTreeViewRowAutomationPeer(this);
        }

        #endregion
    }

    public class GridTreeViewRowAutomationPeer : TreeViewItemAutomationPeer, ISelectionItemProvider
    {
        private GridTreeViewRow _owner;

        public GridTreeViewRowAutomationPeer(GridTreeViewRow owner)
            : base(owner)
        {
            this._owner = owner;
        }

        protected override string GetNameCore()
        {
            var data = this._owner.DataContext;
            if (data != null)
            {
                var autoProperty = this._owner.EntityViewMeta.TryGetPrimayDisplayProperty();
                if (!string.IsNullOrWhiteSpace(autoProperty))
                {
                    var value = data.GetPropertyValue(autoProperty);
                    if (value != null) { return value.ToString(); }
                }
            }

            return base.GetNameCore();
        }

        protected override void SetFocusCore()
        {
            try
            {
                this._owner._isAutomationFired = true;

                base.SetFocusCore();
            }
            finally
            {
                this._owner._isAutomationFired = false;
            }
        }

        #region ISelectionItemProvider

        /**************************************************
         * 
         * �ڹ�ѡģʽ�£�ֱ��ʹ�� IsMultiSelected ������ʵ�� ISelectionItemProvider��
         * ������ǹ�ѡģʽ����Ӧ��ʹ�� IsSelected ���ԡ�
         * 
         ***************************************************/

        bool ISelectionItemProvider.IsSelected
        {
            get { return this._owner.IsMultiSelected; }
        }

        void ISelectionItemProvider.AddToSelection()
        {
            this._owner.SetValue(this.GetSelectedProperty(), true);
        }

        void ISelectionItemProvider.RemoveFromSelection()
        {
            this._owner.SetValue(this.GetSelectedProperty(), false);
        }

        void ISelectionItemProvider.Select()
        {
            this._owner.SetValue(this.GetSelectedProperty(), true);
        }

        private DependencyProperty GetSelectedProperty()
        {
            if (this._owner.TreeGrid.CheckingMode == CheckingMode.CheckingRow)
            {
                return GridTreeViewRow.IsMultiSelectedProperty;
            }
            else
            {
                return GridTreeViewRow.IsSelectedProperty;
            }
        }

        #endregion
    }
}
