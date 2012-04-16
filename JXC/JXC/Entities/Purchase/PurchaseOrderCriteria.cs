/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20120416
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20120416
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
    [QueryEntity, Serializable]
    public class PurchaseOrderCriteria : TimeSpanCriteria
    {
        public static readonly RefProperty<ClientInfo> ClientInfoRefProperty =
            P<PurchaseOrderCriteria>.RegisterRef(e => e.ClientInfo, ReferenceType.Normal);
        public int? ClientInfoId
        {
            get { return this.GetRefNullableId(ClientInfoRefProperty); }
            set { this.SetRefNullableId(ClientInfoRefProperty, value); }
        }
        public ClientInfo ClientInfo
        {
            get { return this.GetRefEntity(ClientInfoRefProperty); }
            set { this.SetRefEntity(ClientInfoRefProperty, value); }
        }
    }

    internal class PurchaseOrderCriteriaConfig : EntityConfig<PurchaseOrderCriteria>
    {
        protected override void ConfigView()
        {
            //横向显示查询面板。
            View.DetailAsHorizontal = true;

            using (View.OrderProperties())
            {
                View.Property(PurchaseOrderCriteria.TimeSpanTypeProperty)
                    .HasLabel("入库日期").ShowIn(ShowInWhere.Detail);
                View.Property(PurchaseOrderCriteria.FromProperty)
                    .HasLabel("从").ShowInDetail(labelWidth: 30);
                View.Property(PurchaseOrderCriteria.ToProperty)
                    .HasLabel("至").ShowInDetail(labelWidth: 30);
                View.Property(PurchaseOrderCriteria.ClientInfoRefProperty)
                    .HasLabel("相关单位：").ShowIn(ShowInWhere.Detail);
            }
        }
    }
}