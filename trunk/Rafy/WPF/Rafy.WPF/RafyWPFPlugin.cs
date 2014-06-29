/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20100326
 * 说明：当前工程所对应的模块类
 * 运行环境：.NET 3.5 SP1
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20100326
 * 
*******************************************************/

using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Threading;
using Rafy;
using Rafy.ComponentModel;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.Domain.ORM.DbMigration;
using Rafy.Threading;
using Rafy.WPF;
using Rafy.WPF.Command;

namespace Rafy.WPF
{
    /// <summary>
    /// 当前工程所对应的模块类。
    /// </summary>
    internal class RafyWPFPlugin : UIPlugin
    {
        protected override int SetupLevel
        {
            get { return PluginSetupLevel.System; }
        }

        public override void Initialize(IApp app)
        {
            AddResources();

            SetupWPFCommands();

            SqlErrorInfo.AttachAlertCommandSqlErrorBehavior();
        }

        private static void AddResources()
        {
            RafyResources.AddResource(typeof(RafyWPFPlugin),
                "Resources/ControlsCommon.xaml",

                "Resources/SysControls/Button.xaml",
                "Resources/SysControls/CheckBox.xaml",
                "Resources/SysControls/RadioButton.xaml",
                "Resources/SysControls/Expander.xaml",
                "Resources/SysControls/TextBox.xaml",
                "Resources/SysControls/PasswordBox.xaml",
                "Resources/SysControls/RichTextBox.xaml",
                "Resources/SysControls/Thumb.xaml",
                "Resources/SysControls/Slider.xaml",
                "Resources/SysControls/ScrollBar.xaml",
                "Resources/SysControls/ScrollViewer.xaml",
                "Resources/SysControls/FlowDocumentScrollViewer.xaml",
                "Resources/SysControls/ListBox.xaml",
                "Resources/SysControls/TabControl.xaml",
                "Resources/SysControls/Menu.xaml",
                "Resources/SysControls/ToolBar.xaml",
                "Resources/SysControls/ToolTip.xaml",
                "Resources/SysControls/ProgressBar.xaml",
                "Resources/SysControls/TreeView.xaml",
                "Resources/SysControls/BusyAnimation.xaml",
                "Resources/SysControls/OtherSysControls.xaml",

                "Resources/RafyControls/MessageBox.xaml",
                "Resources/RafyControls/ComboBoxComboListControl.xaml",
                "Resources/RafyControls/SplitButton.xaml",
                "Resources/RafyControls/CloseableTabItem.xaml",
                "Resources/RafyControls/LeftRightSplitter.xaml",
                "Resources/RafyControls/DockPanelSplitter.xaml",
                "Resources/RafyControls/EditorHost.xaml",
                "Resources/RafyControls/Form.xaml",
                "Resources/RafyControls/WatermarkTextBox.xaml",
                "Resources/RafyControls/ButtonSpinner.xaml",
                "Resources/RafyControls/NumericUpDown.xaml",
                "Resources/RafyControls/DateTimeInputControl.xaml",
                "Resources/RafyControls/OtherRafyControls.xaml"
                );
        }

        /// <summary>
        /// 初始化命令列表
        /// </summary>
        private static void SetupWPFCommands()
        {
            WPFCommandNames.FireQuery = typeof(QueryObjectCommand);
            WPFCommandNames.RefreshDataSourceInRDLC = typeof(RefreshDataSourceInRDLC);
            WPFCommandNames.ShowReportData = typeof(ShowReportData);
            WPFCommandNames.PopupAdd = typeof(PopupAddCommand);
            WPFCommandNames.Add = typeof(AddCommand);
            WPFCommandNames.SaveBill = typeof(SaveBillCommand);
            WPFCommandNames.SaveList = typeof(SaveListCommand);
            WPFCommandNames.Cancel = typeof(CancelCommand);
            WPFCommandNames.Refresh = typeof(RefreshCommand);
            WPFCommandNames.Filter = typeof(FilterCommand);
            WPFCommandNames.Delete = typeof(DeleteListObjectCommand);
            WPFCommandNames.Edit = typeof(EditDetailCommand);
            WPFCommandNames.ExportToExcel = typeof(ExportToExcelCommand);

            WPFCommandNames.MoveUp = typeof(MoveUpCommand);
            WPFCommandNames.MoveDown = typeof(MoveDownCommand);
            WPFCommandNames.LevelUp = typeof(LevelUpCommand);
            WPFCommandNames.LevelDown = typeof(LevelDownCommand);
            WPFCommandNames.InsertBefore = typeof(InsertBeforeCommand);
            WPFCommandNames.InsertChild = typeof(AddChildCommand);
            WPFCommandNames.ExpandAll = typeof(ExpandAllCommand);
            WPFCommandNames.ExpandOne = typeof(ExpandToLevelOneCommand);
            WPFCommandNames.ExpandTwo = typeof(ExpandToLevelTwoCommand);
            WPFCommandNames.ExpandThree = typeof(ExpandToLevelThreeCommand);
            WPFCommandNames.ExpandFour = typeof(ExpandToLevelFourCommand);

            WPFCommandNames.SelectAll = typeof(SelectAllCommand);
            WPFCommandNames.SelectReverse = typeof(SelectReverseCommand);

            WPFCommandNames.InitCommonCommands();
        }
    }
}