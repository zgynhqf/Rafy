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
using Rafy.MetaModel.Attributes;

namespace Rafy.Domain.ORM.DbMigration.Presistence
{
    [Serializable, QueryEntity]
    public class DbMigrationHistoryQueryCriteria : Criteria
    {
        public static readonly Property<string> DatabaseProperty = P<DbMigrationHistoryQueryCriteria>.Register(e => e.Database);
        public string Database
        {
            get { return this.GetProperty(DatabaseProperty); }
            set { this.SetProperty(DatabaseProperty, value); }
        }

        public static readonly Property<DateTime?> StartTimeProperty = P<DbMigrationHistoryQueryCriteria>.Register(e => e.StartTime);
        public DateTime? StartTime
        {
            get { return this.GetProperty(StartTimeProperty); }
            set { this.SetProperty(StartTimeProperty, value); }
        }

        public static readonly Property<DateTime?> EndTimeProperty = P<DbMigrationHistoryQueryCriteria>.Register(e => e.EndTime);
        public DateTime? EndTime
        {
            get { return this.GetProperty(EndTimeProperty); }
            set { this.SetProperty(EndTimeProperty, value); }
        }
    }
}