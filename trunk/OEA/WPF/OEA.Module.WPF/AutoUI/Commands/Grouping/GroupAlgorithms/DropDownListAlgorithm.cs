/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101125
 * 说明：把命令分为一个下拉组中
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101125
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using Itenso.Windows.Input;
using OEA.WPF.Command;
using OEA.MetaModel;
using OEA.MetaModel.View;

namespace OEA.Module.WPF.CommandAutoUI
{
    /// <summary>
    /// 把命令分为一个下拉组中
    /// </summary>
    public class DropDownListAlgorithm : GroupAlgorithm
    {
        protected override void GroupCommandCore(WPFCommand cmd)
        {
            if (cmd.Groups.Count != 1) throw new ArgumentNullException("groupName");

            string groupName = cmd.Groups[0];
            var group = this.TryFindGroup(groupName);

            if (group == null)
            {
                group = this.AddNewGroup(groupName);
                group.Generator = new SplitButtonGroupGenerator(group, this.Context);
            }

            group.AddCommand(cmd);
        }
    }
}