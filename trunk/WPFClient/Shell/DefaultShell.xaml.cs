using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using OEA;
using OEA.Library;

using OEA.MetaModel;
using OEA.MetaModel.View;
using OEA.Module;
using OEA.Module.WPF;
using Common;

namespace OEA.Module.WPF.Shell
{
    [Export(ComposableNames.MainWindow, typeof(Window))]
    public partial class DefaultShell : Window, IShell
    {
        private TabControlWorkSpace _workspace;

        public DefaultShell()
        {
            InitializeComponent();

            this._workspace = new TabControlWorkSpace(workspace);

            //绑定皮肤
            themes.ItemsSource = ThemeManager.GetThemes();

            //选择皮肤
            string theme = ConfigurationHelper.GetAppSettingOrDefault("Theme", "Blue");
            themes.SelectedValue = theme;

            base.Loaded += new RoutedEventHandler(OnLoaded);
        }

        public IWorkspace Workspace
        {
            get { return this._workspace; }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //topBannerContainer
            var content = App.Current.CompositionContainer
                .TryGetExportedLastVersionValue<UserControl>(ComposableNames.MainWindow_TopBanner);
            topBannerContainer.Content = content;

            this.ShowModules();
        }

        private void ShowModules()
        {
            var modules = App.Current.GetUserRootModules();

            //初始化 TreeView
            tvModules.ItemsSource = modules;
            ShellHelper.ForeachItemContainer<TreeViewItem>(tvModules, treeViewItem =>
            {
                treeViewItem.IsExpanded = true;
                return false;
            });

            //行为
            tvModules.SelectedItemChanged += (o, e) =>
            {
                var item = e.NewValue as ModuleViewModel;
                modules.SelectSingle(item);
            };

            //根据模块页签切换时模块列表自动切换
            App.Current.Workspace.WindowActived += (s, e) =>
            {
                var title = e.ActiveWindow.Title;

                var vm = modules.FindByLabel(title);

                ShellHelper.ForeachItemContainer<TreeViewItem>(tvModules, tvi =>
                {
                    if (tvi.DataContext == vm)
                    {
                        tvi.IsSelected = true;
                        return true;
                    }
                    return false;
                });

                modules.SelectSingle(vm);
            };
        }

        private void sdScale_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            sdScale.Value = 1.0;
        }
    }
}