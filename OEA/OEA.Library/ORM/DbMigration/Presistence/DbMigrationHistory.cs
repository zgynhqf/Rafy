using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleCsla;
using OEA.Library;
using OEA.MetaModel;
using OEA.MetaModel.Attributes;

namespace OEA.Library.ORM.DbMigration.Presistence
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
        [EntityProperty]
        public string Database
        {
            get { return this.GetProperty(DatabaseProperty); }
            set { this.SetProperty(DatabaseProperty, value); }
        }

        public static readonly Property<DateTime> TimeIdProperty = P<DbMigrationHistory>.Register(e => e.TimeId);
        [EntityProperty]
        public DateTime TimeId
        {
            get { return this.GetProperty(TimeIdProperty); }
            set { this.SetProperty(TimeIdProperty, value); }
        }

        public static readonly Property<bool> IsGeneratedProperty = P<DbMigrationHistory>.Register(e => e.IsGenerated);
        [EntityProperty]
        public bool IsGenerated
        {
            get { return this.GetProperty(IsGeneratedProperty); }
            set { this.SetProperty(IsGeneratedProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<DbMigrationHistory>.Register(e => e.Description);
        [EntityProperty]
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
            set { this.SetProperty(DescriptionProperty, value); }
        }

        public static readonly Property<string> MigrationClassProperty = P<DbMigrationHistory>.Register(e => e.MigrationClass);
        [EntityProperty]
        public string MigrationClass
        {
            get { return this.GetProperty(MigrationClassProperty); }
            set { this.SetProperty(MigrationClassProperty, value); }
        }

        public static readonly Property<string> MigrationContentProperty = P<DbMigrationHistory>.Register(e => e.MigrationContent);
        [EntityProperty]
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

            Meta.MapTable().HasColumns(
                DbMigrationHistory.DatabaseProperty,
                DbMigrationHistory.TimeIdProperty,
                DbMigrationHistory.IsGeneratedProperty,
                DbMigrationHistory.DescriptionProperty,
                DbMigrationHistory.MigrationClassProperty,
                DbMigrationHistory.MigrationContentProperty
                );
        }
    }
}