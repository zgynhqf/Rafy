using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace FM
{
    [Serializable]
    public abstract class FMEntity : IntEntity
    {
        #region 构造函数

        protected FMEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected FMEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 扩展字段

        public static readonly Property<string> Extend1Property = P<FMEntity>.Register(e => e.Extend1);
        public string Extend1
        {
            get { return this.GetProperty(Extend1Property); }
            set { this.SetProperty(Extend1Property, value); }
        }

        public static readonly Property<string> Extend2Property = P<FMEntity>.Register(e => e.Extend2);
        public string Extend2
        {
            get { return this.GetProperty(Extend2Property); }
            set { this.SetProperty(Extend2Property, value); }
        }

        public static readonly Property<string> Extend3Property = P<FMEntity>.Register(e => e.Extend3);
        public string Extend3
        {
            get { return this.GetProperty(Extend3Property); }
            set { this.SetProperty(Extend3Property, value); }
        }

        #endregion
    }

    [Serializable]
    public abstract class FMEntityList : EntityList { }

    public abstract class FMEntityRepository : EntityRepository
    {
        public static string DbSettingName = "FinanceManagement";

        protected FMEntityRepository() { }
    }

    [DataProviderFor(typeof(FMEntityRepository))]
    public class FMEntityDataProvider : RdbDataProvider
    {
        protected override string ConnectionStringSettingName
        {
            get { return FMEntityRepository.DbSettingName; }
        }
    }

    public abstract class FMEntityConfig<TEntity> : EntityConfig<TEntity> { }
}