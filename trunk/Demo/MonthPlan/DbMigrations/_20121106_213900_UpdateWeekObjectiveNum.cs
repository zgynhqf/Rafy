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
    public class _20121106_213900_UpdateWeekObjectiveNum : DataMigration
    {
        public override string Description
        {
            get { return "更新周目标量字段。"; }
        }

        protected override void Up()
        {
            this.RunCode(db =>
            {
                var repo = RF.Concrete<MonthPlanRepository>();

                var plans = repo.GetAll();

                foreach (MonthPlan plan in plans)
                {
                    foreach (TaskOrCategory item in plan.TaskOrCategoryList)
                    {
                        if (item.IsTaskRO)
                        {
                            var oldValue = item.ObjectiveNum;
                            item.ObjectiveNum = 0;
                            item.ObjectiveNum = oldValue;
                        }
                    }
                }

                RF.Save(plans);
            });
        }
    }
}
