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
using Rafy.WPF;

namespace JXC.WPF.Templates
{
    /// <summary>
    /// 简单列表模块
    /// </summary>
    public class ListTemplate : UITemplate
    {
        protected override AggtBlocks DefineBlocks()
        {
            var blocks = base.DefineBlocks();
            blocks.Children.Clear();
            return blocks;
        }
    }
}
