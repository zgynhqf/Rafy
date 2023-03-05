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
    /// 股票投资组合
    /// </summary>
    [RootEntity]
    public partial class StockCombination : UnitTestMongoDbEntity
    {
        #region 引用属性

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<StockCombination>.Register(e => e.Code);
        /// <summary>
        /// 策略的编号
        /// </summary>
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> AdjustTimeProperty = P<StockCombination>.Register(e => e.AdjustTime);
        /// <summary>
        /// 调仓时间点
        /// </summary>
        public string AdjustTime
        {
            get { return this.GetProperty(AdjustTimeProperty); }
            set { this.SetProperty(AdjustTimeProperty, value); }
        }

        public static readonly Property<double> CurrentCashProperty = P<StockCombination>.Register(e => e.CurrentCash);
        /// <summary>
        /// 当前可用的现金
        /// </summary>
        public double CurrentCash
        {
            get { return this.GetProperty(CurrentCashProperty); }
            set { this.SetProperty(CurrentCashProperty, value); }
        }

        public static readonly Property<DateTime> LastTradeDateProperty = P<StockCombination>.Register(e => e.LastTradeDate);
        /// <summary>
        /// 程序最后的一个自动交易日期。
        /// </summary>
        public DateTime LastTradeDate
        {
            get { return this.GetProperty(LastTradeDateProperty); }
            set { this.SetProperty(LastTradeDateProperty, value); }
        }

        public static readonly Property<bool> EnabledProperty = P<StockCombination>.Register(e => e.Enabled, true);
        /// <summary>
        /// 返回是否被启用。
        /// </summary>
        public bool Enabled
        {
            get { return this.GetProperty(EnabledProperty); }
            set { this.SetProperty(EnabledProperty, value); }
        }

        public static readonly Property<bool> RearrangePositionProperty = P<StockCombination>.Register(e => e.RearrangePosition);
        /// <summary>
        /// 重新整理所有股票的仓位。
        /// 默认为 false，如果手工设置为 true，则在下次调仓时：
        /// 会先根据资金和当前真实的仓位来生成仓位同步指令，然后再合并当天策略的买卖指令，再根据合并后的指令来进行买卖。
        /// </summary>
        public bool RearrangePosition
        {
            get { return this.GetProperty(RearrangePositionProperty); }
            set { this.SetProperty(RearrangePositionProperty, value); }
        }

        public static readonly Property<bool> SellIfHitHighLimitProperty = P<StockCombination>.Register(e => e.SellIfHitHighLimit);
        /// <summary>
        /// 组合指令中，涨停的股票，是否需要卖出。
        /// </summary>
        public bool SellIfHitHighLimit
        {
            get { return this.GetProperty(SellIfHitHighLimitProperty); }
            set { this.SetProperty(SellIfHitHighLimitProperty, value); }
        }

        public static readonly Property<bool> BuyIfHitLowLimitProperty = P<StockCombination>.Register(e => e.BuyIfHitLowLimit);
        /// <summary>
        /// 组合指令中，跌停的股票，是否需要买入。
        /// </summary>
        public bool BuyIfHitLowLimit
        {
            get { return this.GetProperty(BuyIfHitLowLimitProperty); }
            set { this.SetProperty(BuyIfHitLowLimitProperty, value); }
        }

        public static readonly Property<string> GrIdProperty = P<StockCombination>.Register(e => e.GrId);
        /// <summary>
        /// 策略的 Id
        /// </summary>
        public string GrId
        {
            get { return this.GetProperty(GrIdProperty); }
            set { this.SetProperty(GrIdProperty, value); }
        }

        #endregion

        #region 组合子属性

        public static readonly ListProperty<StockCombinationItemList> StocksProperty = P<StockCombination>.RegisterList(e => e.Stocks);
        public StockCombinationItemList Stocks
        {
            get { return this.GetLazyList(StocksProperty); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 股票投资组合 列表类。
    /// </summary>
    public partial class StockCombinationList : UnitTestMongoDbEntityList { }

    /// <summary>
    /// 股票投资组合 仓库类。
    /// 负责 股票投资组合 类的查询、保存。
    /// </summary>
    public partial class StockCombinationRepository : UnitTestMongoDbEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected StockCombinationRepository() { }

        public StockCombination GetByCode(string code)
        {
            return this.GetFirstBy(new CommonQueryCriteria
            {
                new PropertyMatch(StockCombination.CodeProperty, code),
            });
        }

        public StockCombinationList SearchByCodeCompare(PropertyOperator op, string code)
        {
            return this.GetBy(new CommonQueryCriteria
            {
                new PropertyMatch(StockCombination.CodeProperty, op, code),
            });
        }
    }

    /// <summary>
    /// 股票投资组合 配置类。
    /// 负责 股票投资组合 类的实体元数据的配置。
    /// </summary>
    internal class StockCombinationConfig : UnitTestMongoDbEntityConfig<StockCombination>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            ////配置实体的所有属性都映射到数据表中。
            Meta.MapTable("StockCombination2").MapAllProperties();
        }
    }
}