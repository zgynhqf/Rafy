using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;

namespace Rafy.Domain.ORM.DbMigration.Presistence
{
    [RootEntity, Serializable]
    public partial class DbVersion : IntEntity
    {
        #region 构造函数

        public DbVersion() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected DbVersion(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        public static readonly Property<string> DatabaseProperty = P<DbVersion>.Register(e => e.Database);
        public string Database
        {
            get { return this.GetProperty(DatabaseProperty); }
            set { this.SetProperty(DatabaseProperty, value); }
        }

        public static readonly Property<DateTime> VersionProperty = P<DbVersion>.Register(e => e.Version);
        public DateTime Version
        {
            get { return this.GetProperty(VersionProperty); }
            set { this.SetProperty(VersionProperty, value); }
        }
    }

    [Serializable]
    public partial class DbVersionList : EntityList
    {
        protected DbVersionList() { }
    }

    public partial class DbVersionRepository : EntityRepository
    {
        protected DbVersionRepository() { }

        public DbVersion GetByDb(string database)
        {
            return this.FetchFirst(database) as DbVersion;
        }

        protected EntityList FetchBy(string database)
        {
            var table = qf.Table(this);
            var q = qf.Query(
                table,
                where: qf.Constraint(table.Column(DbVersion.DatabaseProperty), database)
            );

            return this.QueryList(q);
        }

        [DataProviderFor(typeof(DbVersionRepository))]
        private class DbVersionRepositoryDataProvider : RdbDataProvider
        {
            internal protected override string ConnectionStringSettingName
            {
                get { return DbSettingNames.DbMigrationHistory; }
            }
        }
    }

    internal class DbVersionConfig : EntityConfig<DbVersion>
    {
        //protected internal override void ConfigMeta()
        //{
        //    base.ConfigMeta();

        //    //Meta.MapTable().HasColumns(
        //    //    DbVersion.DatabaseProperty,
        //    //    DbVersion.VersionProperty
        //    //    );
        //}
    }
}
