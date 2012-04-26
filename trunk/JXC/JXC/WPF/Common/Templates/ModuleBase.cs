/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120415
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120415
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA.Module.WPF;
using OEA.MetaModel.View;

namespace JXC.WPF.Templates
{
    public abstract class ModuleBase : CallbackTemplate
    {
        //for extend

        public static void MakeBlockReadonly(AggtBlocks block)
        {
            var childMeta = block.MainBlock.ViewMeta;
            childMeta.DisableEditing();

            var commands = childMeta.WPFCommands;
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                var cmd = commands[i];
                if (cmd.GroupType != CommandGroupType.View)
                {
                    commands.Remove(cmd);
                }
            }
        }
    }
}