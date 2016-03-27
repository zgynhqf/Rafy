/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130830
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130830 15:04
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.RBAC.Old.Audit;
using Rafy.Web;

namespace Rafy.RBAC.Old.WPF.ViewConfig
{
    internal class PositionConfig : WPFViewConfig<Position>
    {
        protected override void ConfigView()
        {
            View.HasDelegate(Position.NameProperty).DomainName("岗位");

            View.UseDefaultCommands();

            View.Property(Position.CodeProperty).HasLabel("编码").ShowIn(ShowInWhere.All);
            View.Property(Position.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
        }
    }
}