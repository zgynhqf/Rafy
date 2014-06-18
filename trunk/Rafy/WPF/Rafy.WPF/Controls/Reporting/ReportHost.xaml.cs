/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120903 15:54
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120903 15:54
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Reporting.WinForms;

namespace Rafy.WPF.Controls
{
    public partial class ReportHost : UserControl
    {
        private Dictionary<string, string> _parameters = new Dictionary<string, string>(0);

        public ReportHost()
        {
            InitializeComponent();

            //设置报表的背景和外围背景一致。（目前为定值，后期可以从 Resource 中读取值来转换为 winform 中的 Color 类型。）
            report.BackColor = System.Drawing.Color.FromArgb(242, 248, 255);

            this.ReloadOnRefresh();
        }

        /// <summary>
        /// 本地报表的数据源。
        /// </summary>
        public ReportDataSourceCollection DataSources
        {
            get { return this.LocalReport.DataSources; }
        }

        /// <summary>
        /// 为报表设置指定名称的数据源。
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        internal void SetParameter(string name, string value)
        {
            this._parameters[name] = value;
        }

        /// <summary>
        /// 对应的报表路径.
        /// </summary>
        public string ReportPath { get; set; }

        /// <summary>
        /// 返回 RDLC 文件是否已经准备完毕（是否存在。）
        /// </summary>
        public bool IsReady
        {
            get
            {
                return !string.IsNullOrEmpty(this.ReportPath) &&
                    File.Exists(this.ReportPath);
            }
        }

        /// <summary>
        /// 如果准备完毕，则刷新整个报表。
        /// </summary>
        public void RefreshReportIfReady()
        {
            if (this.IsReady)
            {
                this._refreshCount = 0;

                var localReport = this.LocalReport;

                localReport.ReportPath = this.ReportPath;

                this.SyncParameters(localReport);

                this.ReportViewer.RefreshReport();
            }
        }

        private int _refreshCount;

        /// <summary>
        /// 在刷新报表时，重新加载最新的 RDLC 文件。
        /// </summary>
        private void ReloadOnRefresh()
        {
            report.ReportRefresh += (o, e) =>
            {
                this._refreshCount++;
                if (this._refreshCount > 1)
                {
                    //更新 ReportPath 属性，报表才会重新加载文件中的 XML。
                    var localReport = report.LocalReport;
                    var rawPath = localReport.ReportPath;

                    localReport.ReportPath = string.Empty;

                    localReport.ReportPath = rawPath;
                    this.SyncParameters(localReport);
                }
            };
        }

        private ReportViewer ReportViewer
        {
            get { return report; }
        }

        private LocalReport LocalReport
        {
            get { return this.ReportViewer.LocalReport; }
        }

        private void SyncParameters(LocalReport localReport)
        {
            //设置参数值
            foreach (var kv in this._parameters)
            {
                localReport.SetParameters(new ReportParameter(kv.Key, kv.Value));
            }
        }

        //public override void OnApplyTemplate()
        //{
        //    base.OnApplyTemplate();

        //    this.ReportViewer.RefreshReport();
        //}
    }
}
