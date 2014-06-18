/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130412 14:12
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130412 14:12
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using Rafy.DomainModeling.Controls;

namespace Rafy.DomainModeling.Commands
{
    /// <summary>
    /// 保存当前内容到指定的 odml 文件中。
    /// </summary>
    public class SaveODMLCommand : WPFDesignerCommand
    {
        public static RoutedUICommand WPFCommand = new RoutedUICommand("保存文件", "SaveODML", typeof(SaveODMLCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.S, ModifierKeys.Control)
            });

        protected override void ExecuteCore(ModelingDesigner designer)
        {
            var file = ModelingDesignerExtension.GetCurrentOdmlFile(designer);
            if (file == null)
            {
                var dialog = new SaveFileDialog { Filter = "Rafy Domain Mackup File (*.odml)|*.odml" };
                var res = dialog.ShowDialog();
                if (res != true) return;

                file = dialog.FileName;
            }

            designer.SaveDocument(file);
        }
    }
}