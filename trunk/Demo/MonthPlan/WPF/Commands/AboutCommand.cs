/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20121104 10:13
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20121104 10:13
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.WPF;
using Rafy.WPF.Controls;
using Rafy.WPF.Command;

namespace MP.WPF.Commands
{
    [Command(Label = "关于", GroupType = CommandGroupType.None)]
    public class About : ListViewCommand
    {
        public override void Execute(ListLogicalView view)
        {
            //关于是放在 MP VersionLog 中的。
            var resource = Application.GetResourceStream(new Uri("/MonthPlan;component/_Other/MP VersionLog.txt", UriKind.RelativeOrAbsolute));
            using (var reader = new StreamReader(resource.Stream))
            {
                var content = reader.ReadToEnd();
                App.MessageBox.Show(content, "关于", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
        }
    }
}