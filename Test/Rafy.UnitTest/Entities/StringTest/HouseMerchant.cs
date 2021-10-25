using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;

namespace UT
{
    /// <summary>
    /// 房产商人
    /// </summary>
    [RootEntity, Serializable]
    public partial class HouseMerchant : StringTestEntity
    {
        #region 引用属性

        #endregion

        #region 组合子属性

        public static readonly ListProperty<MerchantItemList> MerchantItemListProperty = P<HouseMerchant>.RegisterList(e => e.MerchantItemList);
        public MerchantItemList MerchantItemList
        {
            get { return this.GetLazyList(MerchantItemListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<HouseMerchant>.Register(e => e.Name);
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 房产商人 列表类。
    /// </summary>
    [Serializable]
    public partial class HouseMerchantList : StringTestEntityList { }

    /// <summary>
    /// 房产商人 仓库类。
    /// 负责 房产商人 类的查询、保存。
    /// </summary>
    public partial class HouseMerchantRepository : StringTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected HouseMerchantRepository() { }
    }

    /// <summary>
    /// 房产商人 配置类。
    /// 负责 房产商人 类的实体元数据的配置。
    /// </summary>
    internal class HouseMerchantConfig : StringTestEntityConfig<HouseMerchant>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }
    }
}