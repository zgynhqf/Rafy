/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101125
 * 说明：默认分组算法
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
    /// 默认分组算法。
    /// 
    /// 使用场景：
    /// 1. 一般的自成一组的按钮，如果不标记CommandGroup时，则默认使用这个算法。
    /// 2. 和原有系统进行兼容：由于原来的按钮都没有标记CommandGroup，也使用这个算法。
    /// </summary>
    internal class DefaultAlgorithm : GroupAlgorithm
    {
        protected override void GroupCommandCore(WPFCommand cmd)
        {
            var newGroup = new GeneratableCommandGroup(cmd.Name);
            var generator = new CompoundGenerator
            {
                CommandMetaGroup = newGroup,
                Context = this.Context
            };
            newGroup.Generator = generator;

            //是否生成到菜单中
            if (cmd.HasLocation(CommandLocation.Menu))
            {
                //为每一个命令都声明一个单独的组
                //构造并设置控件生成器
                generator.Add(new MenuItemGenerator
                {
                    CommandMetaGroup = newGroup,
                    Context = this.Context
                });
            }

            if (cmd.HasLocation(CommandLocation.Toolbar))
            {
                if (cmd.Hierarchy.Count != 1)
                {
                    generator.Add(new ButtonItemGenerator
                    {
                        CommandMetaGroup = newGroup,
                        Context = this.Context
                    });
                }
                else
                {
                    string groupName = cmd.Hierarchy[0];
                    newGroup = this.TryFindGroup(groupName);

                    if (newGroup == null)
                    {
                        newGroup = this.AddNewGroup(groupName);
                        newGroup.Generator = new SplitButtonGroupGenerator
                        {
                            CommandMetaGroup = newGroup,
                            Context = this.Context
                        };
                    }
                }
            }

            newGroup.AddCommand(cmd);

            FinalGroups[newGroup.Name] = newGroup;
        }
    }
}