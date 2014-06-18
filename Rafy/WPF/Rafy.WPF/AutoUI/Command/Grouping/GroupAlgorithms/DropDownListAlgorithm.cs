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
using Rafy.WPF.Command;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace Rafy.WPF.Command.UI
{
    /// <summary>
    /// 把命令分为一个下拉组中
    /// </summary>
    public class DropDownListAlgorithm : GroupAlgorithm
    {
        protected override void GroupCommandCore(WPFCommand cmd)
        {
            if (cmd.Hierarchy.Count != 1) throw new ArgumentNullException("groupName");

            string groupName = cmd.Hierarchy[0];
            var group = this.TryFindGroup(groupName);

            if (group == null)
            {
                group = this.AddNewGroup(groupName);
                group.Generator = new SplitButtonGroupGenerator
                {
                    CommandMetaGroup = group,
                    Context = this.Context
                };
            }

            group.AddCommand(cmd);
        }
    }
}