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
using System.Text;
using OEA.MetaModel.View;
using OEA.Module.WPF;

namespace JXC.WPF.Templates
{
    class DetailChildrenTemplate : CustomTemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            var blocks = base.DefineBlocks();

            //只需要把主块的生成方式变为 Detail 就行了。
            blocks.MainBlock.BlockType = BlockType.Detail;

            return blocks;
        }
    }
}
