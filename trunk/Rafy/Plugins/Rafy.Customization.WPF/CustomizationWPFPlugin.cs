/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 13:44
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.Customization.WPF.Commands;
using Rafy.MetaModel.View;

namespace Rafy.Customization.WPF
{
    internal class CustomizationWPFPlugin : UIPlugin
    {
        protected override int SetupLevel
        {
            get { return PluginSetupLevel.System; }
        }

        public override void Initialize(IApp app)
        {
            WPFCommandNames.CustomizeUI = typeof(CustomizeUI);
            if (RafyEnvironment.IsDebuggingEnabled)
            {
                WPFCommandNames.SysCommands.Insert(0, typeof(CustomizeUI));
                WPFCommandNames.SysQueryCommands.Insert(0, typeof(CustomizeUI));
            }

            app.MetaCompiled += (o, e) =>
            {
                UIModel.AggtBlocks.DefineBlocks("ViewConfigurationModel模块界面", m =>
                {
                    var blocks = new AggtBlocks
                    {
                        MainBlock = new Block(typeof(ViewConfigurationModel))
                        {
                            BlockType = BlockType.Detail
                        },
                        Children = 
                        {
                            new ChildBlock("属性", ViewConfigurationModel.ViewConfigurationPropertyListProperty),
                            new ChildBlock("命令", ViewConfigurationModel.ViewConfigurationCommandListProperty)
                        }
                    };

                    blocks.Layout.IsLayoutChildrenHorizonal = true;

                    return blocks;
                });
            };
        }
    }
}