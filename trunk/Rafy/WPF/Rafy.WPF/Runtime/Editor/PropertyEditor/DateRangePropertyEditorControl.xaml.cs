/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20110704
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20110704
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
using Rafy.Utils;

namespace Rafy.WPF.Editors
{
    public partial class DateRangePropertyEditorControl : UserControl
    {
        private DateRange _range;

        public DateRangePropertyEditorControl(DateRange range)
        {
            InitializeComponent();

            this.dpStart.SelectedDate = range.BeginValue;
            this.dpEnd.SelectedDate = range.EndValue;

            if (range == null) throw new ArgumentNullException("range");
            this._range = range;
        }

        public event EventHandler<DateRangeClickEventArgs> Confirm;

        private void OnSubmit()
        {
            if (this.Confirm != null)
            {
                this.Confirm(this, new DateRangeClickEventArgs(this._range));
            }
        }

        private void date_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._range != null)
            {
                this._range.BeginValue = dpStart.SelectedDate.GetValueOrDefault();
                this._range.EndValue = dpEnd.SelectedDate.GetValueOrDefault(); ;

                this.OnSubmit();
            }
        }
    }

    public class DateRangeClickEventArgs : EventArgs
    {
        private DateRange _range;

        public DateRangeClickEventArgs(DateRange range)
        {
            this._range = range;
        }

        public DateRange Range
        {
            get
            {
                return this._range;
            }
        }
    }
}
