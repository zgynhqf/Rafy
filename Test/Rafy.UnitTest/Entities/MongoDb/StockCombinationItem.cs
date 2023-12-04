using Rafy;
using Rafy.ComponentModel;
using Rafy.Data;
using Rafy.Domain;
using Rafy.Domain.ORM;
using Rafy.Domain.ORM.Query;
using Rafy.Domain.Validation;
using Rafy.ManagedProperty;
using Rafy.MetaModel;
using Rafy.MetaModel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using static Rafy.Domain.ORM.Query.FactoryMethods;

namespace UT
{
    /// <summary>
    /// 持仓项
    /// </summary>
    [ChildEntity]
    public partial class StockCombinationItem : UnitTestMongoDbEntity
    {
        #region 引用属性

        public static readonly Property<string> StockCombinationIdProperty =
            P<StockCombinationItem>.Register(e => e.StockCombinationId);
        public string StockCombinationId
        {
            get { return this.GetProperty(StockCombinationIdProperty); }
            set { this.SetProperty(StockCombinationIdProperty, value); }
        }
        public static readonly RefEntityProperty<StockCombination> StockCombinationProperty =
            P<StockCombinationItem>.RegisterRef(e => e.StockCombination, StockCombinationIdProperty, ReferenceType.Parent);
        public StockCombination StockCombination
        {
            get { return this.GetRefEntity(StockCombinationProperty); }
            set { this.SetRefEntity(StockCombinationProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> StockCodeProperty = P<StockCombinationItem>.Register(e => e.StockCode);
        /// <summary>
        /// 持仓股票编码
        /// </summary>
        public string StockCode
        {
            get { return this.GetProperty(StockCodeProperty); }
            set { this.SetProperty(StockCodeProperty, value); }
        }

        public static readonly Property<int> HoldNumberProperty = P<StockCombinationItem>.Register(e => e.HoldNumber);
        /// <summary>
        /// 持仓股票数。
        /// 卖出时，最多只会卖出此数量。
        /// </summary>
        public int HoldNumber
        {
            get { return this.GetProperty(HoldNumberProperty); }
            set { this.SetProperty(HoldNumberProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 持仓项 列表类。
    /// </summary>
    public partial class StockCombinationItemList : UnitTestMongoDbEntityList { }

    /// <summary>
    /// 持仓项 仓库类。
    /// 负责 持仓项 类的查询、保存。
    /// </summary>
    public partial class StockCombinationItemRepository : UnitTestMongoDbEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected StockCombinationItemRepository() { }
    }

    /// <summary>
    /// 持仓项 配置类。
    /// 负责 持仓项 类的实体元数据的配置。
    /// </summary>
    internal class StockCombinationItemConfig : UnitTestMongoDbEntityConfig<StockCombinationItem>
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