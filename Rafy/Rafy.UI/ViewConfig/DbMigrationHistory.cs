/*******************************************************
 * 
 * 作者：胡庆访
 * 创建时间：201111
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 201111
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
using Rafy.MetaModel.View;
using Rafy.MetaModel.Attributes;
using Rafy.Domain.ORM.Query;

namespace Rafy.Domain.ORM.DbMigration.Presistence
{
    internal class DbMigrationHistoryConfig_WPF : WPFViewConfig<DbMigrationHistory>
    {
        protected internal override void ConfigView()
        {
            //View.UseWPFCommands(WPFCommandNames.Delete, WPFCommandNames.SaveList, WPFCommandNames.Cancel);

            using (View.OrderProperties())
            {
                View.Property(DbMigrationHistory.DatabaseProperty).HasLabel("数据库名").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.DescriptionProperty).HasLabel("描述").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.TimeStringProperty).HasLabel("时间").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.IsGeneratedProperty).HasLabel("是否生成").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.MigrationClassProperty).HasLabel("升级类").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.MigrationContentProperty).HasLabel("内容").ShowIn(ShowInWhere.List).Readonly()
                    .UseEditor(WPFEditorNames.Memo);
                View.Property(DbMigrationHistory.TimeIdProperty).HasLabel("时间Id").ShowIn(ShowInWhere.List).Readonly();
            }
        }
    }

    internal class DbMigrationHistoryConfig_Web : WebViewConfig<DbMigrationHistory>
    {
        protected internal override void ConfigView()
        {
            using (View.OrderProperties())
            {
                View.Property(DbMigrationHistory.DatabaseProperty).HasLabel("数据库名").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.DescriptionProperty).HasLabel("描述").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.TimeStringProperty).HasLabel("时间").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.IsGeneratedProperty).HasLabel("是否生成").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.MigrationClassProperty).HasLabel("升级类").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.MigrationContentProperty).HasLabel("内容").ShowIn(ShowInWhere.List).Readonly();
                View.Property(DbMigrationHistory.TimeIdProperty).HasLabel("时间Id").ShowIn(ShowInWhere.List).Readonly();
            }
        }
    }
}