/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110222
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100222
 * 
*******************************************************/

using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using OEA.Module.WPF.Controls;
using System;
using OEA.Module.WPF.Automation;


namespace OEA.Module.WPF.Layout
{
    /// <summary>
    /// 下拉主列表的布局
    /// </summary>
    public partial class DropDownListDetailLayout : TraditionalLayout
    {
        public DropDownListDetailLayout()
        {
            InitializeComponent();
        }

        private ListObjectView _mainView;

        public override void TryArrangeMain(ControlResult control)
        {
            //使用下拉框显示主区域。

            if (control != null)
            {
                this._mainView = control.MainView as ListObjectView;
                this._mainView.Control.RemoveFromParent();

                ////选中数据的第一行。
                ////由于模板都会自动获取数据，而UI线程一直在构造界面，所以这里直接监听数据到达函数就可以了。
                //view.DataLoader.ListenDataChangedOnce(() =>
                //{
                //    if (view.Data.Count > 0)
                //    {
                //        view.CurrentObject = view.Data[0] as Entity;
                //    }
                //});

                var cmbList = ControlHelper.CreateComboListControl(this._mainView);
                AutomationHelper.SetEditingElement(cmbList);

                main.Content = cmbList;
            }
            else
            {
                main.RemoveFromParent();
            }
        }

        public override void TryArrangeCommandsContainer(ControlResult toolBar)
        {
            if (toolBar != null)
            {
                toolBarContainer.Content = toolBar.Control;
            }
            else
            {
                toolBarContainer.RemoveFromParent();
            }
        }

        protected override TabControl ChildrenTab
        {
            get
            {
                return childrenTab;
            }
        }
    }
}