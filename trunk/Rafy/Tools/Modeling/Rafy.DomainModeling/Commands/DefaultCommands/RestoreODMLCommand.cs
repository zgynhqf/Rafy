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
    /// 撤销当前的所有操作，直接返回文档最初的内容。
    /// </summary>
    public class RestoreODMLCommand : WPFDesignerCommand
    {
        public static RoutedUICommand WPFCommand = new RoutedUICommand("还原odml", "RestoreODML", typeof(RestoreODMLCommand),
            new InputGestureCollection()
            {
                new KeyGesture(Key.Z, ModifierKeys.Control)
            });

        protected override void ExecuteCore(ModelingDesigner designer)
        {
            var file = ModelingDesignerExtension.GetCurrentOdmlFile(designer);
            if (file != null)
            {
                designer.LoadDocument(file);
            }
        }
    }
}