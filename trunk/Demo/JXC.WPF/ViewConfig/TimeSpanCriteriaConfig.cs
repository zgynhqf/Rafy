using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.WPF.ViewConfig
{
    internal class TimeSpanCriteriaConfig : WPFViewConfig<TimeSpanCriteria>
    {
        protected override void ConfigView()
        {
            View.DomainName("查询条件");

            //横向显示查询面板。
            View.UseDetailAsHorizontal();

            using (View.OrderProperties())
            {
                View.Property(TimeSpanCriteria.TimeSpanTypeProperty)
                    .HasLabel("入库日期").ShowIn(ShowInWhere.Detail);
                View.Property(TimeSpanCriteria.FromProperty)
                    .HasLabel("从").ShowInDetail(labelSize: 30);
                View.Property(TimeSpanCriteria.ToProperty)
                    .HasLabel("至").ShowInDetail(labelSize: 30);
            }
        }
    }
}