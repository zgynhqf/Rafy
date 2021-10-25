using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using Rafy;
using Rafy.Data;
using Rafy.Domain.ORM;
using Rafy.Domain;
using Rafy.Domain.Validation;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using Rafy.MetaModel.View;
using Rafy.ManagedProperty;

namespace UT
{
    /// <summary>
    /// 建筑
    /// </summary>
    [RootEntity, Serializable]
    public partial class Building : UnitTestEntity
    {
        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<Building>.Register(e => e.Name);
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
    /// 建筑 列表类。
    /// </summary>
    [Serializable]
    public partial class BuildingList : UnitTestEntityList { }

    /// <summary>
    /// 建筑 仓库类。
    /// 负责 建筑 类的查询、保存。
    /// </summary>
    public partial class BuildingRepository : UnitTestEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected BuildingRepository() { }
    }

    /// <summary>
    /// 建筑 配置类。
    /// 负责 建筑 类的实体元数据的配置。
    /// </summary>
    internal class BuildingConfig : UnitTestEntityConfig<Building>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();

            //Id 属性不再是主键，主键改为 Name 属性。
            var setting = DbSetting.FindOrCreate(UnitTestEntityRepositoryDataProvider.DbSettingName);
            Meta.Property(Building.NameProperty).MapColumn().IsPrimaryKey(true);
            var idColumn = Meta.Property(Building.IdProperty).MapColumn().IsPrimaryKey(false);
            if (DbSetting.IsOracleProvider(setting) || setting.ProviderName.Contains("MySql"))
            {
                idColumn.IsIdentity(false);
            }
        }
    }
}