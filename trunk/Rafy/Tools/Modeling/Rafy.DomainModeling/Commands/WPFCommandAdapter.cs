/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 14:02
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 14:02
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Rafy.DomainModeling.Controls;

namespace Rafy.DomainModeling.Commands
{
    /// <summary>
    /// 把 WPFDesignerCommand 适配到 WPF CommandBinding 上去。
    /// </summary>
    internal class WPFCommandAdapter
    {
        private WPFDesignerCommand _cmd;

        private ModelingDesigner _designer;

        internal WPFCommandAdapter(WPFDesignerCommand cmd, ModelingDesigner designer)
        {
            _cmd = cmd;
            _designer = designer;
        }

        internal static CommandBinding CreateCommandBinding(ModelingDesigner designer, WPFDesignerCommand cmd)
        {
            var adpater = new WPFCommandAdapter(cmd, designer);

            var wpfCmd = cmd.GetWPFCommand();

            var binding = new CommandBinding(wpfCmd);
            binding.Executed += adpater.binding_Executed;
            binding.CanExecute += adpater.binding_CanExecute;

            return binding;
        }

        void binding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _cmd.CanExecute(_designer);
            e.ContinueRouting = false;
        }

        void binding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _cmd.Execute(_designer);
            e.Handled = true;
        }
    }
}