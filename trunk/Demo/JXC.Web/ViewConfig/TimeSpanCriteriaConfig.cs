using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.Web.ViewConfig
{
    internal class TimeSpanCriteriaConfig : WebViewConfig<TimeSpanCriteria>
    {
        protected override void ConfigView()
        {
            View.DomainName("查询条件");

            using (View.OrderProperties())
            {
                View.Property(TimeSpanCriteria.FromProperty).HasLabel("从").ShowIn(ShowInWhere.Detail);
                View.Property(TimeSpanCriteria.ToProperty).HasLabel("至").ShowIn(ShowInWhere.Detail);
            }
        }
    }
}