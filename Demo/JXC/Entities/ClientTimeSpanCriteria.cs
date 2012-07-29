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
    public class ClientTimeSpanCriteria : TimeSpanCriteria
    {
        public static readonly RefProperty<ClientInfo> ClientInfoRefProperty =
            P<ClientTimeSpanCriteria>.RegisterRef(e => e.ClientInfo, ReferenceType.Normal);
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

    internal class ClientTimeSpanCriteriaConfig : EntityConfig<ClientTimeSpanCriteria>
    {
        protected override void ConfigView()
        {
            using (View.OrderProperties())
            {
                View.Property(TimeSpanCriteria.TimeSpanTypeProperty);
                View.Property(TimeSpanCriteria.FromProperty);
                View.Property(TimeSpanCriteria.ToProperty);
                View.Property(ClientTimeSpanCriteria.ClientInfoRefProperty)
                    .HasLabel("相关单位：").ShowIn(ShowInWhere.Detail);
            }
        }
    }
}