﻿using System;
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
using Rafy.Domain.ORM.Query;

namespace UT
{
    [Serializable]
    public abstract class UnitTestEntity : IntEntity
    {
        #region 构造函数

        protected UnitTestEntity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected UnitTestEntity(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 兼容老接口

        /// <summary>
        /// 获取指定引用 id 属性对应的 id 的可空类型返回值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public new int? GetRefNullableId(IRefIdProperty property)
        {
            return (int?)base.GetRefNullableId(property);
        }

        /// <summary>
        /// 获取指定引用 id 属性对应的 id 的返回值。
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public new int GetRefId(IRefIdProperty property)
        {
            return (int)base.GetRefId(property);
        }

        #endregion
    }

    [Serializable]
    public abstract class UnitTestEntityList : EntityList { }

    public abstract class UnitTestEntityRepository : EntityRepository//PropertyQueryRepository
    {
        protected UnitTestEntityRepository() { }

        protected QueryFactory f
        {
            get { return QueryFactory.Instance; }
        }

        //internal EntityList QueryList(Action<IPropertyQuery> queryBuider, Predicate<Entity> filter = null, EntityList entityList = null)
        //{
        //    var query = this.CreatePropertyQuery();
        //    if (queryBuider != null) queryBuider(query);
        //    return this.QueryList(new PropertyQueryArgs
        //    {
        //        PropertyQuery = query,
        //        Filter = filter,
        //        EntityList = entityList
        //    });
        //}
    }

    [DataProviderFor(typeof(UnitTestEntityRepository))]
    public class UnitTestEntityRepositoryDataProvider : RdbDataProvider
    {
        public static string DbSettingName = "Test_RafyUnitTest";
        public static string DbSettingName_Duplicate = "Test_RafyUnitTest_Duplicate";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    public abstract class UnitTestEntityConfig<TEntity> : EntityConfig<TEntity> { }
}