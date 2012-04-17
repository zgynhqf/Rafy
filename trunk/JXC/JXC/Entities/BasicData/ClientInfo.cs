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
using System.Text;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;
using OEA.MetaModel.View;

namespace JXC
{
    [RootEntity, Serializable]
    public class ClientInfo : JXCEntity
    {
        public static readonly RefProperty<ClientCategory> ClientCategoryRefProperty =
            P<ClientInfo>.RegisterRef(e => e.ClientCategory, ReferenceType.Normal);
        public int ClientCategoryId
        {
            get { return this.GetRefId(ClientCategoryRefProperty); }
            set { this.SetRefId(ClientCategoryRefProperty, value); }
        }
        public ClientCategory ClientCategory
        {
            get { return this.GetRefEntity(ClientCategoryRefProperty); }
            set { this.SetRefEntity(ClientCategoryRefProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<ClientInfo>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> ZhuJiMaProperty = P<ClientInfo>.Register(e => e.ZhuJiMa);
        public string ZhuJiMa
        {
            get { return this.GetProperty(ZhuJiMaProperty); }
            set { this.SetProperty(ZhuJiMaProperty, value); }
        }

        public static readonly Property<string> FaRenDaiBiaoProperty = P<ClientInfo>.Register(e => e.FaRenDaiBiao);
        public string FaRenDaiBiao
        {
            get { return this.GetProperty(FaRenDaiBiaoProperty); }
            set { this.SetProperty(FaRenDaiBiaoProperty, value); }
        }

        public static readonly Property<string> YouXiangProperty = P<ClientInfo>.Register(e => e.YouXiang);
        public string YouXiang
        {
            get { return this.GetProperty(YouXiangProperty); }
            set { this.SetProperty(YouXiangProperty, value); }
        }

        public static readonly Property<string> KaiHuYinHangProperty = P<ClientInfo>.Register(e => e.KaiHuYinHang);
        public string KaiHuYinHang
        {
            get { return this.GetProperty(KaiHuYinHangProperty); }
            set { this.SetProperty(KaiHuYinHangProperty, value); }
        }

        public static readonly Property<ShouJiaJiBie> ShouJiaJiBieProperty = P<ClientInfo>.Register(e => e.ShouJiaJiBie, ShouJiaJiBie.LinShouDanJia);
        public ShouJiaJiBie ShouJiaJiBie
        {
            get { return this.GetProperty(ShouJiaJiBieProperty); }
            set { this.SetProperty(ShouJiaJiBieProperty, value); }
        }

        public static readonly Property<string> YinHangZhangHuProperty = P<ClientInfo>.Register(e => e.YinHangZhangHu);
        public string YinHangZhangHu
        {
            get { return this.GetProperty(YinHangZhangHuProperty); }
            set { this.SetProperty(YinHangZhangHuProperty, value); }
        }

        public static readonly Property<string> BeiZhuProperty = P<ClientInfo>.Register(e => e.BeiZhu);
        public string BeiZhu
        {
            get { return this.GetProperty(BeiZhuProperty); }
            set { this.SetProperty(BeiZhuProperty, value); }
        }
    }

    public enum ShouJiaJiBie
    {
        [Label("零售单价")]
        LinShouDanJia,
        [Label("一级零售价")]
        JiBie_1,
        [Label("二级零售价")]
        JiBie_2,
        [Label("三级零售价")]
        JiBie_3
    }

    [Serializable]
    public class ClientInfoList : JXCEntityList { }

    public class ClientInfoRepository : EntityRepository
    {
        protected ClientInfoRepository() { }
    }

    internal class ClientInfoConfig : EntityConfig<ClientInfo>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().HasColumns(
                ClientInfo.ClientCategoryRefProperty,
                ClientInfo.NameProperty,
                ClientInfo.ZhuJiMaProperty,
                ClientInfo.FaRenDaiBiaoProperty,
                ClientInfo.YouXiangProperty,
                ClientInfo.KaiHuYinHangProperty,
                ClientInfo.ShouJiaJiBieProperty,
                ClientInfo.YinHangZhangHuProperty,
                ClientInfo.BeiZhuProperty
                );
        }

        protected override void ConfigView()
        {
            View.DomainName("客户").HasDelegate(ClientInfo.NameProperty);

            using (View.OrderProperties())
            {
                View.Property(ClientInfo.NameProperty).HasLabel("名称").ShowIn(ShowInWhere.All);
                View.Property(ClientInfo.ZhuJiMaProperty).HasLabel("助记码").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.FaRenDaiBiaoProperty).HasLabel("法人代表").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.YouXiangProperty).HasLabel("邮箱").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.ClientCategoryRefProperty).HasLabel("客户类别").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.KaiHuYinHangProperty).HasLabel("开户银行").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.ShouJiaJiBieProperty).HasLabel("售价级别").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.YinHangZhangHuProperty).HasLabel("银行帐户").ShowIn(ShowInWhere.ListDetail);
                View.Property(ClientInfo.BeiZhuProperty).HasLabel("备注").ShowIn(ShowInWhere.ListDetail)
                    .ShowMemoInDetail();
            }
        }
    }
}