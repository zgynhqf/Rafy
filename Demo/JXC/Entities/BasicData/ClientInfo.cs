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
using Rafy;
using Rafy.Domain;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace JXC
{
    /// <summary>
    /// 客户
    /// </summary>
    [RootEntity, Serializable]
    public partial class ClientInfo : JXCEntity
    {
        public static readonly IRefIdProperty ClientCategoryIdProperty =
            P<ClientInfo>.RegisterRefId(e => e.ClientCategoryId, ReferenceType.Normal);
        public int ClientCategoryId
        {
            get { return (int)this.GetRefId(ClientCategoryIdProperty); }
            set { this.SetRefId(ClientCategoryIdProperty, value); }
        }
        public static readonly RefEntityProperty<ClientCategory> ClientCategoryProperty =
            P<ClientInfo>.RegisterRef(e => e.ClientCategory, ClientCategoryIdProperty);
        public ClientCategory ClientCategory
        {
            get { return this.GetRefEntity(ClientCategoryProperty); }
            set { this.SetRefEntity(ClientCategoryProperty, value); }
        }

        public static readonly Property<string> CategoryNameProperty = P<ClientInfo>.RegisterRedundancy(e => e.CategoryName,
            new RedundantPath(ClientCategoryProperty, ClientCategory.NameProperty));
        public string CategoryName
        {
            get { return this.GetProperty(CategoryNameProperty); }
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
    public partial class ClientInfoList : JXCEntityList { }

    public partial class ClientInfoRepository : JXCEntityRepository
    {
        protected ClientInfoRepository() { }

        public ClientInfoList GetSuppliers()
        {
            return this.GetByCategoryName(ClientCategory.SupplierName);
        }

        public ClientInfoList GetCustomers()
        {
            return this.GetByCategoryName(ClientCategory.CustomerName);
        }

        public ClientInfoList GetByCategoryName(string name)
        {
            var list = this.CacheAll()
                .Where(e => e.CastTo<ClientInfo>().ClientCategory.Name == name);

            var result = this.NewList();
            result.AddRange(list);

            return result as ClientInfoList;
        }
    }

    internal class ClientInfoConfig : EntityConfig<ClientInfo>
    {
        protected override void ConfigMeta()
        {
            Meta.MapTable().MapProperties(
                ClientInfo.ClientCategoryIdProperty,
                ClientInfo.NameProperty,
                ClientInfo.ZhuJiMaProperty,
                ClientInfo.FaRenDaiBiaoProperty,
                ClientInfo.YouXiangProperty,
                ClientInfo.KaiHuYinHangProperty,
                ClientInfo.ShouJiaJiBieProperty,
                ClientInfo.YinHangZhangHuProperty,
                ClientInfo.BeiZhuProperty,

                ClientInfo.CategoryNameProperty
                );

            Meta.EnableClientCache();
        }
    }
}