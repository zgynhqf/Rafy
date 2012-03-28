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
    public class DbVersion : Entity
    {
        protected internal override string ConnectionStringSettingName
        {
            get { return ConnectionStringNames.DbMigrationHistory; }
        }

        protected DbVersion() { }

        public static readonly Property<string> DatabaseProperty = P<DbVersion>.Register(e => e.Database);
        [EntityProperty]
        public string Database
        {
            get { return this.GetProperty(DatabaseProperty); }
            set { this.SetProperty(DatabaseProperty, value); }
        }

        public static readonly Property<DateTime> VersionProperty = P<DbVersion>.Register(e => e.Version);
        [EntityProperty]
        public DateTime Version
        {
            get { return this.GetProperty(VersionProperty); }
            set { this.SetProperty(VersionProperty, value); }
        }
    }

    [Serializable]
    public class DbVersionList : EntityList
    {
        protected DbVersionList() { }

        protected void DataPortal_Fetch(string database)
        {
            this.QueryDb(q => q.Constrain(DbVersion.DatabaseProperty).Equal(database));
        }
    }

    public class DbVersionRepository : EntityRepository
    {
        protected DbVersionRepository() { }

        public DbVersion GetByDb(string database)
        {
            return this.FetchFirstAs<DbVersion>(database);
        }
    }

    internal class DbVersionConfig : EntityConfig<DbVersion>
    {
        protected override void ConfigMeta()
        {
            base.ConfigMeta();

            //Meta.MapTable().HasColumns(
            //    DbVersion.DatabaseProperty,
            //    DbVersion.VersionProperty
            //    );
        }
    }
}
