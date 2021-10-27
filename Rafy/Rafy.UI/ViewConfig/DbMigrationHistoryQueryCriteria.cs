/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：20130107 09:30
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20130107 09:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;

namespace Rafy.Domain.ORM.DbMigration.Presistence
{
    internal class DbMigrationHistoryQueryCriteriaConfig_Web : WebViewConfig<DbMigrationHistoryQueryCriteria>
    {
        protected internal override void ConfigView()
        {
            using (View.OrderProperties())
            {
                View.Property(DbMigrationHistoryQueryCriteria.DatabaseProperty).HasLabel("数据库名").ShowIn(ShowInWhere.Detail);
                View.Property(DbMigrationHistoryQueryCriteria.StartTimeProperty).HasLabel("开始时间").ShowIn(ShowInWhere.Detail);
                View.Property(DbMigrationHistoryQueryCriteria.EndTimeProperty).HasLabel("结束时间").ShowIn(ShowInWhere.Detail);
            }
        }
    }
    internal class DbMigrationHistoryQueryCriteriaConfig_WPF : WPFViewConfig<DbMigrationHistoryQueryCriteria>
    {
        protected internal override void ConfigView()
        {
            using (View.OrderProperties())
            {
                View.Property(DbMigrationHistoryQueryCriteria.DatabaseProperty).HasLabel("数据库名").ShowIn(ShowInWhere.Detail);
                View.Property(DbMigrationHistoryQueryCriteria.StartTimeProperty).HasLabel("开始时间").ShowIn(ShowInWhere.Detail);
                View.Property(DbMigrationHistoryQueryCriteria.EndTimeProperty).HasLabel("结束时间").ShowIn(ShowInWhere.Detail);
            }
        }
    }
}