using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rafy.MetaModel;
using Rafy.MetaModel.View;

namespace JXC.WPF.ViewConfig
{
    internal class ClientTimeSpanCriteriaConfig : WPFViewConfig<ClientTimeSpanCriteria>
    {
        protected override void ConfigView()
        {
            using (View.OrderProperties())
            {
                View.Property(TimeSpanCriteria.TimeSpanTypeProperty);
                View.Property(TimeSpanCriteria.FromProperty);
                View.Property(TimeSpanCriteria.ToProperty);
                View.Property(ClientTimeSpanCriteria.ClientInfoProperty)
                    .HasLabel("相关单位：").ShowIn(ShowInWhere.Detail);
            }
        }
    }
}