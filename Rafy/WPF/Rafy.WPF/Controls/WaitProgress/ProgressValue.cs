using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rafy.WPF
{
    public class ProgressValue
    {
        /// <summary>
        /// 当前的进度百分比值。（0-100）
        /// </summary>
        public double Percent { get; set; }

        /// <summary>
        /// 要显示的内容
        /// </summary>
        public string Text { get; set; }
    }

    public interface IProgressReporter
    {
        ProgressValue ProgressValue { get; set; }

        void Report(double percent, string text = null);
    }

    public class EmptyProgressReporter : IProgressReporter
    {
        public ProgressValue ProgressValue { get; set; }

        public void Report(double percent, string text = null) { }
    }
}
