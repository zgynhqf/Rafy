/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120413
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120413
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC.WPF
{
    internal class AutoCodeInfoConfig : WPFViewConfig<AutoCodeInfo>
    {
        protected override void ConfigView()
        {
            View.DomainName("自动编码信息").HasDelegate(AutoCodeInfo.MingChengProperty);

            View.UseDefaultCommands();

            using (View.OrderProperties())
            {
                View.Property(AutoCodeInfo.MingChengProperty).HasLabel("参数名称").ShowIn(ShowInWhere.All);
                View.Property(AutoCodeInfo.CanShuZhiProperty).HasLabel("参数值").ShowIn(ShowInWhere.ListDetail);
                View.Property(AutoCodeInfo.BeiZhuProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail().Readonly();
            }
        }
    }
}