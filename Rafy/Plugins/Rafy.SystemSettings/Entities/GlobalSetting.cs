/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20171104
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20171104 11:09
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

namespace Rafy.SystemSettings
{
    /// <summary>
    /// 当前系统的全局配置项实体
    /// </summary>
    [RootEntity, Serializable]
    public partial class GlobalSetting : SystemSettingsEntity
    {
        #region 引用属性

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> KeyProperty = P<GlobalSetting>.Register(e => e.Key);
        /// <summary>
        /// 配置项键值（名称）
        /// </summary>
        public string Key
        {
            get { return this.GetProperty(KeyProperty); }
            set { this.SetProperty(KeyProperty, value); }
        }

        public static readonly Property<string> ValueProperty = P<GlobalSetting>.Register(e => e.Value);
        /// <summary>
        /// 配置项-值
        /// </summary>
        public string Value
        {
            get { return this.GetProperty(ValueProperty); }
            set { this.SetProperty(ValueProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<GlobalSetting>.Register(e => e.Description);
        /// <summary>
        /// 配置项-描述
        /// </summary>
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
            set { this.SetProperty(DescriptionProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 当前系统的全局配置项实体 列表类。
    /// </summary>
    [Serializable]
    public partial class GlobalSettingList : SystemSettingsEntityList { }

    /// <summary>
    /// 当前系统的全局配置项实体 仓库类。
    /// 负责 当前系统的全局配置项实体 类的查询、保存。
    /// </summary>
    public partial class GlobalSettingRepository : SystemSettingsEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected GlobalSettingRepository() { }

        /// <summary>
        /// 通过 Key 来查找唯一的配置项。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public GlobalSetting GetByKey(string key)
        {
            return this.GetFirstBy(new CommonQueryCriteria
            {
                new PropertyMatch(GlobalSetting.KeyProperty, key),
            });
        }
    }

    /// <summary>
    /// 当前系统的全局配置项实体 配置类。
    /// 负责 当前系统的全局配置项实体 类的实体元数据的配置。
    /// </summary>
    internal class GlobalSettingConfig : SystemSettingsEntityConfig<GlobalSetting>
    {
        /// <summary>
        /// 配置实体的元数据
        /// </summary>
        protected override void ConfigMeta()
        {
            //配置实体的所有属性都映射到数据表中。
            Meta.MapTable().MapAllProperties();
        }

        protected override void AddValidations(IValidationDeclarer rules)
        {
            base.AddValidations(rules);

            rules.AddRule(GlobalSetting.KeyProperty, new RequiredRule());
            rules.AddRule(GlobalSetting.KeyProperty, new NotDuplicateRule());
        }
    }
}