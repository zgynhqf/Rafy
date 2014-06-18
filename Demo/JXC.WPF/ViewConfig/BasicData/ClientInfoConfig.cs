/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20130821
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130821 15:29
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.WPF.ViewConfig.BasicData
{
    internal class ClientInfoConfig : WPFViewConfig<ClientInfo>
    {
        protected override void ConfigView()
        {
            View.DomainName("客户").HasDelegate(ClientInfo.NameProperty);

            View.UseDefaultCommands();

            using (View.OrderProperties())
            {
                View.Property(ClientInfo.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(ClientInfo.ZhuJiMaProperty).HasLabel("助记码").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.FaRenDaiBiaoProperty).HasLabel("法人代表").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.YouXiangProperty).HasLabel("邮箱").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.ClientCategoryProperty).HasLabel("客户类别").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.KaiHuYinHangProperty).HasLabel("开户银行").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.ShouJiaJiBieProperty).HasLabel("售价级别").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.YinHangZhangHuProperty).HasLabel("银行帐户").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.BeiZhuProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}
