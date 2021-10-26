/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120326
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120326
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel.Attributes;
using Rafy.WPF.Command;
using Rafy.WPF;
using Rafy.Domain;

namespace Rafy.RBAC.Old.WPF
{
    [Command(Label = "展开模块")]
    class ExpandAllModules : ClientCommand<OperationSelectionView>
    {
        public override void Execute(OperationSelectionView view)
        {
            if (this.Label == "展开模块")
            {
                view.Control.ExpandModules();
                this.Label = "折叠模块";
            }
            else
            {
                view.Control.CollapseModules();
                this.Label = "展开模块";
            }
        }
    }
}