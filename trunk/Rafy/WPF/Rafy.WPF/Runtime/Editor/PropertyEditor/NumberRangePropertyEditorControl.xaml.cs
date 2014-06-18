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
using Rafy.WPF;

namespace Rafy.WPF.Editors
{
    public partial class NumberRangePropertyEditorControl : UserControl
    {
        private NumberRange _range;

        public NumberRangePropertyEditorControl(NumberRange range)
        {
            InitializeComponent();

            this.txtStart.Text = range.BeginValue.ToString();
            this.txtEnd.Text = range.EndValue.ToString();

            if (range == null) throw new ArgumentNullException("range");
            this._range = range;
        }

        public event EventHandler<NumberRangeClickEventArgs> Confirm;

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this._range != null)
            {
                double start = 0;
                double end = 0;

                if (!string.IsNullOrWhiteSpace(txtStart.Text) &&
                    !double.TryParse(txtStart.Text, out start))
                {
                    App.MessageBox.Show("只能输入数字".Translate());
                    return;
                }

                if (!string.IsNullOrWhiteSpace(txtEnd.Text) &&
                    !double.TryParse(txtEnd.Text, out end))
                {
                    App.MessageBox.Show("只能输入数字".Translate());
                    return;
                }

                this._range.BeginValue = start;
                this._range.EndValue = end;

                this.OnSubmit();
            }
        }

        private void OnSubmit()
        {
            if (this.Confirm != null)
            {
                this.Confirm(this, new NumberRangeClickEventArgs(this._range));
            }
        }
    }

    public class NumberRangeClickEventArgs : EventArgs
    {
        private NumberRange _range;

        public NumberRangeClickEventArgs(NumberRange range)
        {
            this._range = range;
        }

        public NumberRange Range
        {
            get
            {
                return this._range;
            }
        }
    }
}