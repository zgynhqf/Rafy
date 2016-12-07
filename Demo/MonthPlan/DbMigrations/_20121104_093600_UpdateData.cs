using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.DbMigration;
using Rafy.Domain;

namespace MP.DbMigrations
{
    public class _20121104_093600_UpdateData : DataMigration
    {
        public override string Description
        {
            get { return "更新类别、任务的可空字段。"; }
        }

        protected override void Up()
        {
            this.RunCode((Action<Rafy.Data.IDbAccesser>)(db =>
            {
                var repo = RF.ResolveInstance<MonthPlanRepository>();

                var plans = repo.GetAll();

                foreach (MonthPlan plan in plans)
                {
                    foreach (TaskOrCategory item in plan.TaskOrCategoryList)
                    {
                        if (item.IsCategoryRO)
                        {
                            item.ObjectiveNum = null;
                            item.WeightInCategory = null;
                            item.Score = null;
                        }
                        else
                        {
                            item.MonthPercent = null;
                            item.MonthScore = null;
                        }
                    }
                }

                RF.Save(plans);
            }));
        }
    }
}
