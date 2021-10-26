/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 13:42
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.ComponentModel;
using Rafy.MetaModel.View;

namespace Rafy.Customization.Web
{
    public class CustomizationWebPlugin : UIPlugin
    {
        public override void Initialize(IApp app)
        {
            var customeUICmd = "Rafy.customization.cmd.CustomizeUI";
            WebCommandNames.CustomizeUI = customeUICmd;
            WebCommandNames.SysCommands.Add(customeUICmd);
            WebCommandNames.SysQueryCommands.Add(customeUICmd);
            if (RafyEnvironment.IsDebuggingEnabled)
            {
                WebCommandNames.TreeCommands.Insert(0, customeUICmd);
                WebCommandNames.CommonCommands.Insert(0, customeUICmd);
            }

            app.MetaCreating += (o, e) =>
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

                    blocks.Layout.Class = "Rafy.autoUI.layouts.RightChildren";

                    return blocks;
                });
            };
        }
    }
}
