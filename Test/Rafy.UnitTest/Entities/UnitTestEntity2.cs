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

namespace UT
{
    [Serializable]
    public abstract class UnitTest2Entity : IntEntity
    {
        #region 构造函数

        protected UnitTest2Entity() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected UnitTest2Entity(SerializationInfo info, StreamingContext context) : base(info, context) { }

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
    public abstract class UnitTest2EntityList : EntityList { }

    public abstract class UnitTest2EntityRepository : EntityRepository
    {
        protected UnitTest2EntityRepository() { }
    }

    [DataProviderFor(typeof(UnitTest2EntityRepository))]
    public class UnitTest2EntityRepositoryDataProvider : RdbDataProvider
    {
        public static string DbSettingName = "Test_RafyUnitTest2";

        protected override string ConnectionStringSettingName
        {
            get { return DbSettingName; }
        }
    }

    public abstract class UnitTest2EntityConfig<TEntity> : EntityConfig<TEntity> { }
}