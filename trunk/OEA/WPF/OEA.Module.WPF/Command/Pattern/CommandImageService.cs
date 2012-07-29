/*******************************************************
 * 
 * 作者：http://www.codeproject.com/Articles/25445/WPF-Command-Pattern-Applied
 * 创建时间：周金根 2009
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 周金根 2009
 * 重新整理 胡庆访 20120518
 * 
*******************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using OEA.WPF.Command;

namespace OEA.WPF.Command
{
    public static class CommandImageService
    {
        public static Uri GetCommandImageUri(UICommand command)
        {
            if (command == null) { throw new ArgumentNullException("command"); }

            var core = command.CoreCommand;
            var imgName = core.CommandInfo.ImageName;
            if (!string.IsNullOrEmpty(imgName))
            {
                //图片应该是放在 CoreCommand 的程序集的 Images 文件夹中。
                string cmdAssembly = core.GetType().Assembly.GetName().Name;
                return new Uri(string.Concat(
                    "pack://application:,,,/", cmdAssembly, ";Component/Images/", imgName
                    ));
            }

            return null;
        }
    }
}