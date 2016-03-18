/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20160318
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20160318 09:27
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
using Rafy.MetaModel.View;

namespace Rafy.SerialNumber
{
    /// <summary>
    /// 流水号生成规则信息
    /// </summary>
    [RootEntity, Serializable]
    public partial class SerialNumberInfo : SerialNumberEntity
    {
        #region 构造函数

        public SerialNumberInfo() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected SerialNumberInfo(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        #endregion

        #region 组合子属性

        public static readonly ListProperty<SerialNumberValueList> AutoCodeValueListProperty = P<SerialNumberInfo>.RegisterList(e => e.AutoCodeValueList);
        public SerialNumberValueList AutoCodeValueList
        {
            get { return this.GetLazyList(AutoCodeValueListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> NameProperty = P<SerialNumberInfo>.Register(e => e.Name);
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> TimeGroupFormatProperty = P<SerialNumberInfo>.Register(e => e.TimeGroupFormat);
        /// <summary>
        /// 规则中的时间分组格式。
        /// 如果此属性不为空，则表示本规则需要根据时间的某个格式来进行分组编号。
        /// 例如：yyyyMM 表示每一个月使用一组新的流水号。yyyy 表示一年一级，yyyyMMdd 表示一天一组。
        /// 如果此属性为空，则表示不按时间分组。
        /// </summary>
        public string TimeGroupFormat
        {
            get { return this.GetProperty(TimeGroupFormatProperty); }
            set { this.SetProperty(TimeGroupFormatProperty, value); }
        }

        public static readonly Property<string> FormatProperty = P<SerialNumberInfo>.Register(e => e.Format, "yyyyMMdd******");
        /// <summary>
        /// 最终的格式化字符串。目前接受以下通配符：
        /// 1. 使用 '*' 表示滚动的流水号值，个数表示需要的位数。
        /// 2. 接受使用 DateTime 的格式化。
        /// 例如：
        /// yyyyMMdd****** 表示：以日期开头，紧接 6 位流水号数字的格式。
        /// </summary>
        public string Format
        {
            get { return this.GetProperty(FormatProperty); }
            set { this.SetProperty(FormatProperty, value); }
        }

        public static readonly Property<int> RollValueStartProperty = P<SerialNumberInfo>.Register(e => e.RollValueStart, 1);
        /// <summary>
        /// 每一组流水号从这个数字开始。
        /// </summary>
        public int RollValueStart
        {
            get { return this.GetProperty(RollValueStartProperty); }
            set { this.SetProperty(RollValueStartProperty, value); }
        }

        public static readonly Property<int> RollValueStepProperty = P<SerialNumberInfo>.Register(e => e.RollValueStep, 1);
        /// <summary>
        /// 每个新流水号的步长。
        /// </summary>
        public int RollValueStep
        {
            get { return this.GetProperty(RollValueStepProperty); }
            set { this.SetProperty(RollValueStepProperty, value); }
        }

        public static readonly Property<string> CommentProperty = P<SerialNumberInfo>.Register(e => e.Comment);
        /// <summary>
        /// 备注
        /// </summary>
        public string Comment
        {
            get { return this.GetProperty(CommentProperty); }
            set { this.SetProperty(CommentProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 流水号生成规则信息 列表类。
    /// </summary>
    [Serializable]
    public partial class SerialNumberInfoList : SerialNumberEntityList { }

    /// <summary>
    /// 流水号生成规则信息 仓库类。
    /// 负责 流水号生成规则信息 类的查询、保存。
    /// </summary>
    public partial class SerialNumberInfoRepository : SerialNumberEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected SerialNumberInfoRepository() { }

        /// <summary>
        /// 获取指定名称的编码规则。
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public SerialNumberInfo GetByName(string name)
        {
            return this.FetchFirst(new CommonQueryCriteria
            {
                new PropertyMatch(SerialNumberInfo.NameProperty, name)
            });
        }
    }

    /// <summary>
    /// 流水号生成规则信息 配置类。
    /// 负责 流水号生成规则信息 类的实体元数据的配置。
    /// </summary>
    internal class SerialNumberInfoConfig : SerialNumberEntityConfig<SerialNumberInfo>
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