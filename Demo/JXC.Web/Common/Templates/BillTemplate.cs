/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120417
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120417
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.MetaModel.View;

namespace JXC.Web.Templates
{
    /// <summary>
    /// 单据模块
    /// </summary>
    public class BillTemplate : CodeBlocksTemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            var blocks = base.DefineBlocks();

            blocks.Surrounders.Clear();

            //只需要把主块的生成方式变为 Detail 就行了。
            blocks.MainBlock.BlockType = BlockType.Detail;
            blocks.MainBlock.ViewMeta.AsWebView().ClearCommands();

            return blocks;
        }

        public static void MakeBlockReadonly(AggtBlocks block)
        {
            var childMeta = block.MainBlock.ViewMeta;
            childMeta.DisableEditing();

            var commands = childMeta.AsWebView().Commands;
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                var cmd = commands[i];
                if (cmd.Group != CommandGroupType.View && cmd.Group != CommandGroupType.System)
                {
                    commands.Remove(cmd);
                }
            }
        }
    }

    /// <summary>
    /// 一个只读的单据模块
    /// </summary>
    public class ReadonlyBillTemplate : BillTemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            var blocks = base.DefineBlocks();

            blocks.MainBlock.ViewMeta.DisableEditing();

            //把所有孩子块上的非查询型命令都删除
            foreach (var child in blocks.Children)
            {
                MakeBlockReadonly(child);
            }

            return blocks;
        }
    }
}
