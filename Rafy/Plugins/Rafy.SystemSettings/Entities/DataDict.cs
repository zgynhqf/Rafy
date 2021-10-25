/*******************************************************
 * 
 * 作者：胡庆访
 * 创建日期：20171106
 * 说明：此文件只包含一个类，具体内容见类型注释。
 * 运行环境：.NET 4.0
 * 版本号：1.0.0
 * 
 * 历史记录：
 * 创建文件 胡庆访 20171106 18:54
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
    /// 数据字典
    /// </summary>
    [RootEntity]
    public partial class DataDict : SystemSettingsEntity
    {
        #region 引用属性

        #endregion

        #region 组合子属性

        public static readonly ListProperty<DataDictItemList> DataDictItemListProperty = P<DataDict>.RegisterList(e => e.DataDictItemList);
        public DataDictItemList DataDictItemList
        {
            get { return this.GetLazyList(DataDictItemListProperty); }
        }

        #endregion

        #region 一般属性

        public static readonly Property<string> CodeProperty = P<DataDict>.Register(e => e.Code);
        /// <summary>
        /// 数据字典的编码
        /// </summary>
        public string Code
        {
            get { return this.GetProperty(CodeProperty); }
            set { this.SetProperty(CodeProperty, value); }
        }

        public static readonly Property<string> NameProperty = P<DataDict>.Register(e => e.Name);
        /// <summary>
        /// 类型名称
        /// </summary>
        public string Name
        {
            get { return this.GetProperty(NameProperty); }
            set { this.SetProperty(NameProperty, value); }
        }

        public static readonly Property<string> DescriptionProperty = P<DataDict>.Register(e => e.Description);
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get { return this.GetProperty(DescriptionProperty); }
            set { this.SetProperty(DescriptionProperty, value); }
        }

        #endregion

        #region 只读属性

        #endregion

        /// <summary>
        /// 通过编码查找指定的 item。
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DataDictItem FindItem(string code)
        {
            return this.DataDictItemList.Concrete().FirstOrDefault(i => i.Code == code);
        }
    }

    /// <summary>
    /// 数据字典 列表类。
    /// </summary>
    public partial class DataDictList : SystemSettingsEntityList { }

    /// <summary>
    /// 数据字典 仓库类。
    /// 负责 数据字典 类的查询、保存。
    /// </summary>
    public partial class DataDictRepository : SystemSettingsEntityRepository
    {
        /// <summary>
        /// 单例模式，外界不可以直接构造本对象。
        /// </summary>
        protected DataDictRepository() { }

        /// <summary>
        /// 通过唯一的编码来查找对应的数据字典
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public DataDict GetByCode(string code)
        {
            return this.GetFirstBy(new CommonQueryCriteria
            {
                new PropertyMatch(DataDict.CodeProperty, code),
                //new PropertyMatch(DataDict.Property2Property, PropertyMatchOperator.Equal, parameter2),
            });
        }
    }

    /// <summary>
    /// 数据字典 配置类。
    /// 负责 数据字典 类的实体元数据的配置。
    /// </summary>
    internal class DataDictConfig : SystemSettingsEntityConfig<DataDict>
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