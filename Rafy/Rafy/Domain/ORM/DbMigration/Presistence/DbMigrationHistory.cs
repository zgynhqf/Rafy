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
    [RootEntity, Serializable]
    [ConditionQueryType(typeof(DbMigrationHistoryQueryCriteria))]
    public partial class DbMigrationHistory : IntEntity
    {
        #region 构造函数

        public DbMigrationHistory() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected DbMigrationHistory(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> DatabaseProperty = P<DbMigrationHistory>.Register(e => e.Database);
        public string Database
        {
            get { return this.GetProperty(DatabaseProperty); }
            set { this.SetProperty(DatabaseProperty, value); }
        }

        public static readonly Property<long> TimeIdProperty = P<DbMigrationHistory>.Register(e => e.TimeId, new PropertyMetadata<long>
        {
            PropertyChangedCallBack = (o, e) => (o as DbMigrationHistory).OnTimeIdChanged(e)
        });
        /// <summary>
        /// SqlCE 数据库的 DateTime 类型的精度不够，会造成数据丢失，使得历史记录的时间对比出错。
        /// </summary>
        public long TimeId
        {
            get { return this.GetProperty(TimeIdProperty); }
            set { this.SetProperty(TimeIdProperty, value); }
        }
        protected virtual void OnTimeIdChanged(ManagedPropertyChangedEventArgs e)
        {
            var time = new DateTime((long)e.NewValue);
            this.TimeString = time.ToString("yyyy/MM/dd hh:mm:ss:ms");
        }

        public static readonly Property<bool> IsGeneratedProperty = P<DbMigrationHistory>.Register(e => e.IsGenerated);
        public bool IsGenerated
        {
            get { return this.GetProperty(IsGeneratedProperty); }
            set { this.SetProperty(IsGeneratedProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<DbMigrationHistory>.Register(e => e.Description);
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
            set { this.SetProperty(DescriptionProperty, value); }
        }

        public static readonly Property<string> MigrationClassProperty = P<DbMigrationHistory>.Register(e => e.MigrationClass);
        public string MigrationClass
        {
            get { return this.GetProperty(MigrationClassProperty); }
            set { this.SetProperty(MigrationClassProperty, value); }
        }

        public static readonly Property<string> MigrationContentProperty = P<DbMigrationHistory>.Register(e => e.MigrationContent);
        public string MigrationContent
        {
            get { return this.GetProperty(MigrationContentProperty); }
            set { this.SetProperty(MigrationContentProperty, value); }
        }

        public static readonly Property<string> TimeStringProperty = P<DbMigrationHistory>.Register(e => e.TimeString);
        /// <summary>
        /// 由于 TimeId 不利于显示，所以添加一个 TimeString 冗余字符专用于显示时间。
        /// </summary>
        public string TimeString
        {
            get { return this.GetProperty(TimeStringProperty); }
            set { this.SetProperty(TimeStringProperty, value); }
        }
    }

    [Serializable]
    public partial class DbMigrationHistoryList : EntityList { }

    public partial class DbMigrationHistoryRepository : EntityRepository
    {
        protected DbMigrationHistoryRepository() { }

        [RepositoryQuery]
        public virtual DbMigrationHistoryList GetByDb(string database)
        {
            var table = qf.Table(this);
            var q = qf.Query(
                table,
                where: qf.Constraint(table.Column(DbMigrationHistory.DatabaseProperty), database)
            );

            return (DbMigrationHistoryList)this.QueryData(q);
        }

        [RepositoryQuery]
        public virtual DbMigrationHistoryList GetBy(DbMigrationHistoryQueryCriteria criteria)
        {
            var q = qf.Query(this);
            q.AddConstraintIf(DbMigrationHistory.DatabaseProperty, PropertyOperator.Contains, criteria.Database);
            if (criteria.StartTime.HasValue)
            {
                q.AddConstraintIf(DbMigrationHistory.TimeIdProperty, PropertyOperator.GreaterEqual, criteria.StartTime.Value.Ticks);
            }
            if (criteria.EndTime.HasValue)
            {
                q.AddConstraintIf(DbMigrationHistory.TimeIdProperty, PropertyOperator.LessEqual, criteria.EndTime.Value.Ticks);
            }

            return (DbMigrationHistoryList)this.QueryData(q);
        }

        [DataProviderFor(typeof(DbMigrationHistoryRepository))]
        private class DbMigrationHistoryRepositoryDataProvider : RdbDataProvider
        {
            internal protected override string ConnectionStringSettingName
            {
                get { return DbSettingNames.DbMigrationHistory; }
            }

            public DbMigrationHistoryRepositoryDataProvider()
            {
                this.DataQueryer = new DbMigrationHistoryQueryer();
            }

            private class DbMigrationHistoryQueryer : RdbDataQueryer
            {
                internal protected override void OnQuerying(EntityQueryArgs args)
                {
                    var f = QueryFactory.Instance;

                    var q = args.Query;
                    q.OrderBy.Add(
                        f.OrderBy(q.MainTable.Column(DbMigrationHistory.TimeIdProperty), OrderDirection.Descending)
                        );

                    base.OnQuerying(args);
                }
            }
        }
    }

    internal class DbMigrationHistoryConfig : EntityConfig<DbMigrationHistory>
    {
        protected internal override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                DbMigrationHistory.DatabaseProperty,
                DbMigrationHistory.TimeIdProperty,
                DbMigrationHistory.IsGeneratedProperty,
                DbMigrationHistory.DescriptionProperty,
                DbMigrationHistory.MigrationClassProperty,
                DbMigrationHistory.MigrationContentProperty,
                DbMigrationHistory.TimeStringProperty
                );

            Meta.Property(DbMigrationHistory.DatabaseProperty).MapColumn().HasColumnName("Db");
        }
    }

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