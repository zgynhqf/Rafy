/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20101124
 * 说明：分组算法类
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20101124
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
    /// 分组算法
    /// 
    /// 使用分组算法进行分组，并为每个组创建对应的控件生成器。
    /// 
    /// 此类没有从CommandAutoUIComponent上继承的原因是因为Context要支持internal动态设置，所以直接自成体系
    /// </summary>
    public abstract class GroupAlgorithm
    {
        /// <summary>
        /// 分组时的上下文
        /// </summary>
        protected internal CommandAutoUIContext Context { get; internal set; }

        /// <summary>
        /// 已经生成的组。
        /// 所有的结果都存放在这里。
        /// </summary>
        protected internal Dictionary<string, GeneratableCommandGroup> FinalGroups { get; internal set; }

        /// <summary>
        /// 调用此方法为Command生成一个可生成控件的组，并加入到ExistingGroups中
        /// </summary>
        /// <param name="cmd"></param>
        public void GroupCommand(WPFCommand cmd)
        {
            this.GroupCommandCore(cmd);
        }

        /// <summary>
        /// 此方法为Command生成一个可生成控件的组，并加入到 FinalGroups 中。
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        protected abstract void GroupCommandCore(WPFCommand cmd);

        /// <summary>
        /// 添加一个新的组到ExistingGroups中，组名为groupName
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        protected GeneratableCommandGroup AddNewGroup(string groupName)
        {
            var group = new GeneratableCommandGroup(groupName);
            FinalGroups[group.Name] = group;
            return group;
        }

        /// <summary>
        /// 在ExistingGroups中查找指定的组
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns></returns>
        protected GeneratableCommandGroup TryFindGroup(string groupName)
        {
            GeneratableCommandGroup group = null;
            FinalGroups.TryGetValue(groupName, out group);
            return group;
        }
    }
}