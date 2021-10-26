using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.WPF;

namespace Rafy.WPF.Shell
{
    public partial class DefaultShell : Window, IShell
    {
        public static Type TopBarControlType = typeof(DefaultTopBanner);

        private TabControlWorkSpace _workspace;

        public DefaultShell()
        {
            InitializeComponent();

            this._workspace = new TabControlWorkSpace(workspace);

            //绑定皮肤
            skins.ItemsSource = SkinManager.GetSkins();
            skins.SelectedValue = SkinManager.Current;
            skins.SelectionChanged += (o, e) =>
            {
                SkinManager.Apply(skins.SelectedValue as string);
            };

            base.Loaded += new RoutedEventHandler(OnLoaded);
        }

        public IWorkspace Workspace
        {
            get { return this._workspace; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Title = ConfigurationHelper.GetAppSettingOrDefault("WPFClient_主窗口标题", "管理信息系统")
                .Translate();

            //topBannerContainer
            if (TopBarControlType != null && ConfigurationHelper.GetAppSettingOrDefault("WPFClient_主窗口顶栏是否显示", true))
            {
                topBannerContainer.Content = Activator.CreateInstance(TopBarControlType) as UserControl;
            }
        }

        internal void ShowModules()
        {
            //绑定数据
            tvModules.ItemsSource = App.Current.UserRootModules;

            //直接展开所有 TreeView 结点
            ItemsControlHelper.ForeachItemContainer<TreeViewItem>(tvModules, treeViewItem =>
            {
                treeViewItem.IsExpanded = true;
                return false;
            });
        }

        private void sdScale_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            sdScale.Value = 1.1;
        }
    }
}