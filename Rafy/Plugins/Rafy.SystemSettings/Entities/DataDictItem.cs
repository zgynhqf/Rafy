/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20171106
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20171106 19:07
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
    /// 数据字典项
    /// </summary>
    [ChildEntity, Serializable]
    public partial class DataDictItem : SystemSettingsEntity
    {
        #region 构造函数

        public DataDictItem() { }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected DataDictItem(SerializationInfo info, StreamingContext context) : base(info, context) { }

        #endregion

        #region 引用属性

        public static readonly IRefIdProperty DataDictIdProperty =
            P<DataDictItem>.RegisterRefId(e => e.DataDictId, ReferenceType.Parent);
        public long DataDictId
        {
            get { return (long)this.GetRefId(DataDictIdProperty); }
            set { this.SetRefId(DataDictIdProperty, value); }
        }
        public static readonly RefEntityProperty<DataDict> DataDictProperty =
            P<DataDictItem>.RegisterRef(e => e.DataDict, DataDictIdProperty);
        public DataDict DataDict
        {
            get { return this.GetRefEntity(DataDictProperty); }
            set { this.SetRefEntity(DataDictProperty, value); }
        }

        #endregion

        #region 组合子属性

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<DataDictItem>.Register(e => e.Code);
        /// <summary>
        /// 数据字典项的编码
        /// </summary>
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<DataDictItem>.Register(e => e.Name);
        /// <summary>
        /// 数据字典项的名称
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<DataDictItem>.Register(e => e.Description);
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
            set { this.SetProperty(DescriptionProperty, value); }
        }

        public static readonly Property<string> ValueProperty = P<DataDictItem>.Register(e => e.Value);
        /// <summary>
        /// 该项对应的值。
        /// </summary>
        public string Value
        {
            get { return this.GetProperty(ValueProperty); }
            set { this.SetProperty(ValueProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion
    }

    /// <summary>
    /// 数据字典项 列表类。
    /// </summary>
    [Serializable]
    public partial class DataDictItemList : SystemSettingsEntityList { }

    /// <summary>
    /// 数据字典项 仓库类。
    /// 负责 数据字典项 类的查询、保存。
    /// </summary>
    public partial class DataDictItemRepository : SystemSettingsEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected DataDictItemRepository() { }
    }

    /// <summary>
    /// 数据字典项 配置类。
    /// 负责 数据字典项 类的实体元数据的配置。
    /// </summary>
    internal class DataDictItemConfig : SystemSettingsEntityConfig<DataDictItem>
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