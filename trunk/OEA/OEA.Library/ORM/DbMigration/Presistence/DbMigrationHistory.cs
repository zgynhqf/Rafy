using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OEA;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;

namespace OEA.ORM.DbMigration.Presistence
{
    [Serializable]
    [RootEntity]
    public class DbMigrationHistory : Entity
    {
        protected internal override string ConnectionStringSettingName
        {
            get { return ConnectionStringNames.DbMigrationHistory; }
        }

        public static readonly Property<string> DatabaseProperty = P<DbMigrationHistory>.Register(e => e.Database);
        public string Database
        {
            get { return this.GetProperty(DatabaseProperty); }
            set { this.SetProperty(DatabaseProperty, value); }
        }

        public static readonly Property<long> TimeIdProperty = P<DbMigrationHistory>.Register(e => e.TimeId);
        /// <summary>
        /// SqlCE 数据库的 DateTime 类型的精度不够，会造成数据丢失，使得历史记录的时间对比出错。
        /// </summary>
        public long TimeId
        {
            get { return this.GetProperty(TimeIdProperty); }
            set { this.SetProperty(TimeIdProperty, value); }
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
    }

    [Serializable]
    public class DbMigrationHistoryList : EntityList
    {
        protected DbMigrationHistoryList() { }

        protected void QueryBy(string database)
        {
            this.QueryDb(q => q.Constrain(DbMigrationHistory.DatabaseProperty).Equal(database));
        }
    }

    public class DbMigrationHistoryRepository : EntityRepository
    {
        protected DbMigrationHistoryRepository() { }

        public DbMigrationHistoryList GetByDb(string database)
        {
            return this.FetchListCast<DbMigrationHistoryList>(database);
        }
    }

    internal class DbMigrationHistoryConfig : EntityConfig<DbMigrationHistory>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            Meta.MapTable().MapProperties(
                DbMigrationHistory.DatabaseProperty,
                DbMigrationHistory.TimeIdProperty,
                DbMigrationHistory.IsGeneratedProperty,
                DbMigrationHistory.DescriptionProperty,
                DbMigrationHistory.MigrationClassProperty,
                DbMigrationHistory.MigrationContentProperty
                );

            Meta.Property(DbMigrationHistory.DatabaseProperty).MapColumn().HasColumnName("Db");
        }
    }
}