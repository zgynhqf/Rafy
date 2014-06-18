/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 11:34
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 11:34
 * 
*******************************************************/

using System;
using System.Collections.Generic;
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
using Rafy.WPF;
using Rafy.Domain.ORM.DbMigration;
using Rafy.DbMigration;

namespace Rafy.DevTools.DbManagement
{
    /// <summary>
    /// 选择数据库的控件。
    /// </summary>
    public partial class DatabaseMigrationControl : UserControl
    {
        private ListLogicalView _dbSettingView;

        public DatabaseMigrationControl()
        {
            InitializeComponent();

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this._dbSettingView = AutoUI.ViewFactory.CreateListView(typeof(DbSettingItem));
            this._dbSettingView.CheckingMode = CheckingMode.CheckingRow;

            dbContainer.Content = this._dbSettingView.Control;
        }

        /// <summary>
        /// 获取用户选择的数据库
        /// </summary>
        /// <returns></returns>
        public MigratingOptions GetSelectionResult()
        {
            var res = new MigratingOptions();
            res.ReserveHistory = cbReserveHistory.IsChecked.GetValueOrDefault();
            res.RunDataLossOperation = cbRunLossOperation.IsChecked.GetValueOrDefault()
                ? DataLossOperation.All : DataLossOperation.None;
            res.Databases = this._dbSettingView.SelectedEntities.Cast<DbSettingItem>().Select(s => s.Name).ToArray();
            return res;
        }

        private void btnSelectReverse_Click(object sender, RoutedEventArgs e)
        {
            var view = this._dbSettingView;
            var selection = view.SelectedEntities;
            if (selection.Count > 0)
            {
                var toSelect = view.Data.Except(selection).ToArray();

                selection.Clear();
                foreach (var item in toSelect) { selection.Add(item); }
            }
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            this._dbSettingView.SelectAll();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this._dbSettingView.DataLoader.LoadDataAsync();
        }
    }
}