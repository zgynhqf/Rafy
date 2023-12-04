/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160318
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160318 09:30
 * 
*******************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
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

namespace Rafy.SerialNumber
{
    /// <summary>
    /// 生成的流水号的具体值
    /// </summary>
    [ChildEntity]
    public partial class SerialNumberValue : SerialNumberEntity
    {
        #region 引用属性

        public static readonly Property<long> SerialNumberInfoIdProperty =
            P<SerialNumberValue>.Register(e => e.SerialNumberInfoId);
        public long SerialNumberInfoId
        {
            get { return this.GetProperty(SerialNumberInfoIdProperty); }
            set { this.SetProperty(SerialNumberInfoIdProperty, value); }
        }
        public static readonly RefEntityProperty<SerialNumberInfo> AutoCodeInfoProperty =
            P<SerialNumberValue>.RegisterRef(e => e.SerialNumberInfo, SerialNumberInfoIdProperty, ReferenceType.Parent);
        /// <summary>
        /// 所使用的自动编码规则
        /// </summary>
        public SerialNumberInfo SerialNumberInfo
        {
            get { return this.GetRefEntity(AutoCodeInfoProperty); }
            set { this.SetRefEntity(AutoCodeInfoProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> TimeKeyProperty = P<SerialNumberValue>.Register(e => e.TimeKey);
        /// <summary>
        /// 根据所属规则中定义的时间格式来生成的时间键。
        /// 如果 <see cref="SerialNumberInfo.TimeGroupFormat"/> 中定义的是 yyyyMM，那么这里就是存储具体的时间值：201408。
        /// </summary>
        public string TimeKey
        {
            get { return this.GetProperty(TimeKeyProperty); }
            set { this.SetProperty(TimeKeyProperty, value); }
        }

        public static readonly Property<int> RollValueProperty = P<SerialNumberValue>.Register(e => e.RollValue);
        /// <summary>
        /// 当前滚动的流水号值。
        /// </summary>
        public int RollValue
        {
            get { return this.GetProperty(RollValueProperty); }
            set { this.SetProperty(RollValueProperty, value); }
        }

        public static readonly Property<DateTime> LastUpdatedTimeProperty = P<SerialNumberValue>.Register(e => e.LastUpdatedTime);
        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdatedTime
        {
            get { return this.GetProperty(LastUpdatedTimeProperty); }
            set { this.SetProperty(LastUpdatedTimeProperty, value); }
        }

        #endregion

        #region 只读属性

        public static readonly Property<string> RD_TimeKeyFormatProperty = P<SerialNumberValue>.RegisterRedundancy(e => e.RD_TimeKeyFormat,
            new RedundantPath(AutoCodeInfoProperty, SerialNumberInfo.TimeGroupFormatProperty));
        public string RD_TimeKeyFormat
        {
            get { return this.GetProperty(RD_TimeKeyFormatProperty); }
        }

        public static readonly Property<string> RD_FormatProperty = P<SerialNumberValue>.RegisterRedundancy(e => e.RD_Format,
            new RedundantPath(AutoCodeInfoProperty, SerialNumberInfo.FormatProperty));
        /// <summary>
        /// 从 AutoCodeInfo 冗余过来的 Format 属性。
        /// </summary>
        public string RD_Format
        {
            get { return this.GetProperty(RD_FormatProperty); }
        }

        public static readonly Property<int> RD_RollValueStartProperty = P<SerialNumberValue>.RegisterRedundancy(e => e.RD_RollValueStart,
            new RedundantPath(AutoCodeInfoProperty, SerialNumberInfo.RollValueStartProperty));
        public int RD_RollValueStart
        {
            get { return this.GetProperty(RD_RollValueStartProperty); }
        }

        public static readonly Property<int> RD_RollValueStepProperty = P<SerialNumberValue>.RegisterRedundancy(e => e.RD_RollValueStep,
            new RedundantPath(AutoCodeInfoProperty, SerialNumberInfo.RollValueStepProperty));
        public int RD_RollValueStep
        {
            get { return this.GetProperty(RD_RollValueStepProperty); }
        }

        #endregion
    }

    /// <summary>
    /// 生成的流水号的具体值 列表类。
    /// </summary>
    public partial class SerialNumberValueList : SerialNumberEntityList { }

    /// <summary>
    /// 生成的流水号的具体值 仓库类。
    /// 负责 生成的流水号的具体值 类的查询、保存。
    /// </summary>
    public partial class SerialNumberValueRepository : SerialNumberEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected SerialNumberValueRepository() { }

        /// <summary>
        /// 获取指定规则下指定时间对应的当前值。
        /// </summary>
        /// <param name="info"></param>
        /// <param name="specificTime"></param>
        /// <returns></returns>
        public SerialNumberValue GetByTime(SerialNumberInfo info, DateTime specificTime)
        {
            var timeGroupKey = SerialNumberInfo.GetTimeGroupKey(specificTime, info.TimeGroupFormat);
            return this.GetByKey(info.Name, timeGroupKey);
        }

        /// <summary>
        /// 通过规则名、时间键来获取当前值。
        /// </summary>
        /// <param name="infoName"></param>
        /// <param name="timeKey"></param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual SerialNumberValue GetByKey(string infoName, string timeKey)
        {
            var f = QueryFactory.Instance;
            var tValue = f.Table<SerialNumberValue>();
            var tInfo = f.Table<SerialNumberInfo>();
            var q = f.Query(
                from: tValue.Join(tInfo),
                where: f.And(
                    tInfo.Column(SerialNumberInfo.NameProperty).Equal(infoName),
                    tValue.Column(SerialNumberValue.TimeKeyProperty).Equal(timeKey)
                )
            );

            return (SerialNumberValue)this.QueryData(q);
        }

        /// <summary>
        /// 获取某个规则下最新的一个值。
        /// </summary>
        /// <param name="infoName"></param>
        /// <returns></returns>
        [RepositoryQuery]
        public virtual SerialNumberValue GetLastValue(string infoName)
        {
            var f = QueryFactory.Instance;
            var t = f.Table<SerialNumberValue>();
            var t2 = f.Table<SerialNumberInfo>();
            var q = f.Query(
                from: t.Join(t2),
                where: t2.Column(SerialNumberInfo.NameProperty).Equal(infoName),
                orderBy: new List<IOrderBy> { f.OrderBy(t.Column(SerialNumberValue.LastUpdatedTimeProperty), OrderDirection.Descending) }
            );

            return (SerialNumberValue)this.QueryData(q);
        }
    }

    [DataProviderFor(typeof(SerialNumberValueRepository))]
    public partial class SerialNumberValueRepositoryDataProvider : SerialNumberEntityRepositoryDataProvider
    {
        protected override void Insert(Entity entity)
        {
            var bdEntity = entity as SerialNumberValue;
            bdEntity.LastUpdatedTime = DateTime.Now;

            base.Insert(entity);
        }

        protected override void Update(Entity entity)
        {
            var bdEntity = entity as SerialNumberValue;
            bdEntity.LastUpdatedTime = DateTime.Now;

            base.Update(entity);
        }
    }

    /// <summary>
    /// 生成的流水号的具体值 配置类。
    /// 负责 生成的流水号的具体值 类的实体元数据的配置。
    /// </summary>
    internal class SerialNumberValueConfig : SerialNumberEntityConfig<SerialNumberValue>
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